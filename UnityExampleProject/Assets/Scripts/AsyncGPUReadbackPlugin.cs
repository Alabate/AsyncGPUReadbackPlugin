using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public class AsyncGPUReadbackPlugin
{
	/// <summary>
	/// /// Create an AsyncGPUReadbackPluginRequest.
	/// Use official AsyncGPUReadback.Request if possible.
	/// If not, it tries to use OpenGL specific implementation
	/// Warning! Can only be called from render thread yet (not main thread)
	/// </summary>
	/// <param name="src"></param>
	/// <returns></returns>
	public static AsyncGPUReadbackPluginRequest Request(Texture src)
	{
		return new AsyncGPUReadbackPluginRequest(src);
	}
}

public class AsyncGPUReadbackPluginRequest
{
	
	/// <summary>
	/// Tell if we are using the plugin api or the official api
	/// </summary>
	private bool usePlugin;

	/// <summary>
	/// Official api request object used if supported
	/// </summary>
	private AsyncGPUReadbackRequest gpuRequest;

	/// <summary>
	/// Event Id used to tell what texture is targeted to the render thread
	/// </summary>
	private int eventId;

	/// <summary>
	/// Check if the request is done
	/// </summary>
	public bool done
    {
        get
        {
			if (usePlugin) {
				return isRequestDone(eventId);
			}
			else {
				return gpuRequest.done;
			}
        }
    }

	/// <summary>
	/// Check if the request has an error
	/// </summary>
	public bool hasError
    {
        get
        {
			if (usePlugin) {
				return isRequestError(eventId);
			}
			else {
				return gpuRequest.hasError;
			}
        }
    }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="src"></param>
	public AsyncGPUReadbackPluginRequest(Texture src)
	{
		Debug.Log("request.");
		if (SystemInfo.supportsAsyncGPUReadback) {
			usePlugin = false;
			gpuRequest = AsyncGPUReadback.Request(src);
		}
		else if(isCompatible()) {
			usePlugin = true;
			int textureId = (int)(src.GetNativeTexturePtr());
			this.eventId = makeRequest_mainThread(textureId, 0);
			// GL.IssuePluginEvent(getfunction_makeRequest_renderThread(), this.eventId);
			makeRequest_renderThread(this.eventId);
		}
		else {
			Debug.LogError("AsyncGPUReadback is not supported on your system.");
		}
	}

	/// <summary>
	/// Fetches the data of a successful request.
	/// Warning layer id not supported on OpenGL
	/// </summary>
	/// <returns></returns>
	public unsafe NativeArray<T> GetData<T>() where T : struct
	{
			Debug.Log("Get data.");
			if (usePlugin) {
				// uint size = getDataSize_mainThread(this.eventId);
				// byte[] buffer = new byte[size];
				// getData_mainThread(this.eventId, buffer, size);
				NativeArray<T> rtn = new NativeArray<T>();
				// rtn = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>((byte*)buffer, (int)size, Allocator.Temp);
				return rtn;
			}
			else {
				return gpuRequest.GetData<T>();
			}
	}

	public byte[] GetRawData()
	{
		Debug.Log("Get raw data .");
		if (usePlugin) {
			uint size = getDataSize_mainThread(this.eventId);
			byte[] buffer = new byte[size];
			getData_mainThread(this.eventId, buffer, size);
			return buffer;
		}
		else {
			return gpuRequest.GetData<byte>().ToArray();
		}
	}

	/// <summary>
	/// Has to be called regularly to update request status.
	/// Call this from Update() or from a corountine
	/// </summary>
	/// <param name="force">Update is automatic on official api,
	/// so we don't call the Update() method except on force mode.</param>
	public void Update(bool force = false)
	{
		Debug.Log("Update.");
		if (usePlugin) {
			GL.IssuePluginEvent(getfunction_update_renderThread(), this.eventId);
		}
		else if(force) {
			gpuRequest.Update();
		}
	}
	
	// Destructor
	~AsyncGPUReadbackPluginRequest()
	{
		Debug.Log("Clean.");
		// GL.IssuePluginEvent(getfunction_dispose_renderThread(), this.eventId);
		getfunction_dispose_renderThread();
	}

	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern bool isCompatible();
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern int makeRequest_mainThread(int texture, int miplevel);
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern IntPtr getfunction_makeRequest_renderThread();
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern void makeRequest_renderThread(int event_id);
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern IntPtr getfunction_update_renderThread();
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern uint getDataSize_mainThread(int event_id);
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern uint getData_mainThread(int event_id, byte[] buffer, uint max_length);
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern IntPtr getfunction_dispose_renderThread();
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern bool isRequestError(int event_id);
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern bool isRequestDone(int event_id);
}

