#include <cstddef>
#include "Unity/IUnityInterface.h"
#include "Unity/IUnityGraphics.h"

static IUnityInterfaces* unityInterfaces = NULL;
static IUnityGraphics* graphics = NULL;
static UnityGfxRenderer renderer = kUnityGfxRendererNull;


static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

/**
 * Unity plugin load event
 */
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
    UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
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
extern "C" bool IsCompatible() {
	return (renderer == kUnityGfxRendererOpenGLCore);
}


extern "C" {

	int PrintANumber(){
		if (IsCompatible())
			return 43;
		else
			return 666;
	}
}



