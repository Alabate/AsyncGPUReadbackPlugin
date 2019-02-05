#include <cstddef>
#include <map>
#include <mutex>
#include <memory>
#include <cstring>
#include "Unity/IUnityInterface.h"
#include "Unity/IUnityGraphics.h"
#include <iostream>
#include "TypeHelpers.hpp"

#define DEBUG 1
#ifdef DEBUG
	#include <fstream>
	#include <thread>
#endif

struct Task {
	GLuint texture;
	GLuint fbo;
	GLuint pbo;
	GLsync fence;
	bool initialized = false;
	bool error = false;
	bool done = false;
	void* data;
	int miplevel;
	int size;
	int height;
	int width;
	int depth;
	GLint internal_format;
};

static IUnityInterfaces* unity_interfaces = NULL;
static IUnityGraphics* graphics = NULL;
static UnityGfxRenderer renderer = kUnityGfxRendererNull;

static std::map<int,std::shared_ptr<Task>> tasks;
static std::mutex tasks_mutex;
int next_event_id = 1;

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);




#ifdef DEBUG
std::ofstream logMain, logRender;

/**
 * @brief Debug log function. Log to /tmp/AsyncGPUReadbackPlugin.log
 * 
 * @param message 
 */
void logToFile(std::string message) {
	std::ofstream outfile;
	outfile.open("/tmp/AsyncGPUReadbackPlugin_main.log", std::ios_base::app);
	outfile << "GL CALLBACK: " << message  << std::endl;
	outfile.close();
}

/**
 * OpenGL debug message callback
 */
void GLAPIENTRY DebugMessageCallback( GLenum source,
                 GLenum type,
                 GLuint id,
                 GLenum severity,
                 GLsizei length,
                 const GLchar* message,
                 const void* userParam)
{
	if (type == GL_DEBUG_TYPE_ERROR) {
		logRender << "GL CALLBACK: " << message  << std::endl;
	}
}
#endif

/**
 * Unity plugin load event
 */
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
    UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
	#ifdef DEBUG
		logMain.open("/tmp/AsyncGPUReadbackPlugin_main.log", std::ios_base::app);
		logRender.open("/tmp/AsyncGPUReadbackPlugin_render.log", std::ios_base::app);

		glEnable              ( GL_DEBUG_OUTPUT );
		glDebugMessageCallback( DebugMessageCallback, 0 );
	#endif

    unityInterfaces = unityInterfaces;
    graphics = unityInterfaces->Get<IUnityGraphics>();
        
    graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
        
    // Run OnGraphicsDeviceEvent(initialize) manually on plugin load
    // to not miss the event in case the graphics device is already initialized
    OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

/**
 * Unity unload plugin event
 */
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
	graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

/**
 * Called for every graphics device events
 */
static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
	// Create graphics API implementation upon initialization
	if (eventType == kUnityGfxDeviceEventInitialize)
	{
		renderer = graphics->GetRenderer();
	}

	// Cleanup graphics API implementation upon shutdown
	if (eventType == kUnityGfxDeviceEventShutdown)
	{
		renderer = kUnityGfxRendererNull;
	}
}


/**
 * Check if plugin is compatible with this system
 * This plugin is only compatible with opengl core
 */
extern "C" bool isCompatible() {
	return (renderer == kUnityGfxRendererOpenGLCore);
}

/**
 * @brief Init of the make request action.
 * You then have to call makeRequest_renderThread
 * via GL.IssuePluginEvent with the returned event_id
 * 
 * @param texture OpenGL texture id
 * @return event_id to give to other functions and to IssuePluginEvent
 */
extern "C" int makeRequest_mainThread(GLuint texture, int miplevel) {
	// Create the task
	std::shared_ptr<Task> task = std::make_shared<Task>();
	task->texture = texture;
	task->miplevel = miplevel;
	int event_id = next_event_id;
	next_event_id++;

	// Save it (lock because possible vector resize)
	tasks_mutex.lock();
	tasks[event_id] = task;
	tasks_mutex.unlock();

	return event_id;
}

/**
 * @brief Create a a read texture request
 * Has to be called by GL.IssuePluginEvent
 * @param event_id containing the the task index, given by makeRequest_mainThread
 */
extern "C" void UNITY_INTERFACE_API makeRequest_renderThread(int event_id) {
	// Get task back
	tasks_mutex.lock();
	std::shared_ptr<Task> task = tasks[event_id];
	tasks_mutex.unlock();

	// Get texture informations
	glBindTexture(GL_TEXTURE_2D, task->texture);
	glGetTexLevelParameteriv(GL_TEXTURE_2D, task->miplevel, GL_TEXTURE_WIDTH, &(task->width));
	glGetTexLevelParameteriv(GL_TEXTURE_2D, task->miplevel, GL_TEXTURE_HEIGHT, &(task->height));
	glGetTexLevelParameteriv(GL_TEXTURE_2D, task->miplevel, GL_TEXTURE_DEPTH, &(task->depth));
	glGetTexLevelParameteriv(GL_TEXTURE_2D, task->miplevel, GL_TEXTURE_INTERNAL_FORMAT, &(task->internal_format));
	task->size = task->depth * task->width * task->height * getPixelSizeFromInternalFormat(task->internal_format);

	// Check for errors
	if (task->size == 0
		|| getFormatFromInternalFormat(task->internal_format) == 0
		|| getTypeFromInternalFormat(task->internal_format) == 0) {
		task->error = true;
		return;
	}

	// Allocate the final data buffer !!! WARNING: free, will have to be done on script side !!!!
	task->data = std::malloc(task->size);

	// Create the fbo (frame buffer object) from the given texture
	task->fbo;
	glGenFramebuffers(1, &(task->fbo));

	// Bind the texture to the fbo
	glBindFramebuffer(GL_FRAMEBUFFER, task->fbo);
	glFramebufferTexture(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, task->texture, 0);

	// Create and bind pbo (pixel buffer object) to fbo
	task->pbo;
	glGenBuffers(1, &(task->pbo));
	glBindBuffer(GL_PIXEL_PACK_BUFFER, task->pbo);
	glBufferData(GL_PIXEL_PACK_BUFFER, task->size, 0, GL_DYNAMIC_READ);

	// Start the read request
	glReadBuffer(GL_COLOR_ATTACHMENT0);
	glReadPixels(0, 0, task->width, task->height, getFormatFromInternalFormat(task->internal_format), getTypeFromInternalFormat(task->internal_format), 0);

	// Unbind buffers
	glBindBuffer(GL_PIXEL_PACK_BUFFER, 0);
	glBindFramebuffer(GL_FRAMEBUFFER, 0);

	// Fence to know when it's ready
	task->fence = glFenceSync(GL_SYNC_GPU_COMMANDS_COMPLETE, 0);
	
	// Done init
	task->initialized = true;
}
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API getfunction_makeRequest_renderThread() {
	return makeRequest_renderThread;
}

/**
 * @brief check if data is ready
 * Has to be called by GL.IssuePluginEvent
 * @param event_id containing the the task index, given by makeRequest_mainThread
 */
extern "C" void UNITY_INTERFACE_API update_renderThread(int event_id) {
	// Get task back
	tasks_mutex.lock();
	std::shared_ptr<Task> task = tasks[event_id];
	tasks_mutex.unlock();

	// Check if task has not been already deleted by main thread
	if(task == nullptr) {
		return;
	}

	// Do something only if initialized (thread safety)
	if (!task->initialized || task->done) {
		return;
	}

	// Check fence state
	GLint status = 0;
	GLsizei length = 0;
	glGetSynciv(task->fence, GL_SYNC_STATUS, sizeof(GLint), &length, &status);
	if (length <= 0) {
		task->error = true;
		task->done = true;
		return;
	}

	// When it's done
	if (status == GL_SIGNALED) {

		// Bind back the pbo
		glBindBuffer(GL_PIXEL_PACK_BUFFER, task->pbo);

		// Map the buffer and copy it to data
		void* ptr = glMapBufferRange(GL_PIXEL_PACK_BUFFER, 0, task->size, GL_MAP_READ_BIT);
		std::memcpy(task->data, ptr, task->size);

		// Unmap and unbind
		glUnmapBuffer(GL_PIXEL_PACK_BUFFER);
		glBindBuffer(GL_PIXEL_PACK_BUFFER, 0);

		// Clear buffers
		glDeleteFramebuffers(1, &(task->fbo));
		glDeleteBuffers(1, &(task->pbo));
		glDeleteSync(task->fence);

		// yeah task is done!
		task->done = true;
	}
}
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API getfunction_update_renderThread() {
	return update_renderThread;
}

/**
 * @brief Get data from the main thread
 * @param event_id containing the the task index, given by makeRequest_mainThread
 */
extern "C" void getData_mainThread(int event_id, void** buffer, size_t* length) {
	// Get task back
	tasks_mutex.lock();
	std::shared_ptr<Task> task = tasks[event_id];
	tasks_mutex.unlock();

	// Do something only if initialized (thread safety)
	if (!task->done) {
		return;
	}

	// Copy the pointer. Warning: free will have to be done on script side
	*length = task->size;
	*buffer = task->data;
}

/**
 * @brief Check if request is done
 * @param event_id containing the the task index, given by makeRequest_mainThread
 */
extern "C" bool isRequestDone(int event_id) {
	// Get task back
	tasks_mutex.lock();
	std::shared_ptr<Task> task = tasks[event_id];
	tasks_mutex.unlock();

	return task->done;
}

/**
 * @brief Check if request is in error
 * @param event_id containing the the task index, given by makeRequest_mainThread
 */
extern "C" bool isRequestError(int event_id) {
	// Get task back
	tasks_mutex.lock();
	std::shared_ptr<Task> task = tasks[event_id];
	tasks_mutex.unlock();

	return task->error;
}

/**
 * @brief clear data for a frame
 * Warning : Buffer is never cleaned, it has to be cleaned from script side 
 * Has to be called by GL.IssuePluginEvent
 * @param event_id containing the the task index, given by makeRequest_mainThread
 */
extern "C" void dispose(int event_id) {
	// Remove from tasks
	tasks_mutex.lock();
	std::shared_ptr<Task> task = tasks[event_id];
	std::free(task->data);
	tasks.erase(event_id);
	tasks_mutex.unlock();
}