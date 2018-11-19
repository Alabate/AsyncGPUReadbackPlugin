using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public class AsyncGPUReadbackPlugin
{
	public static int PrintANumber2()
	{
		return PrintANumber();
	}

	/// <summary>
	/// /// Create an AsyncGPUReadbackPluginRequest.
	/// Use official AsyncGPUReadback.Request if possible.
	/// If not, it tries to use OpenGL specific implementation
	/// </summary>
	/// <param name="src"></param>
	/// <returns></returns>
	public static AsyncGPUReadbackPluginRequest Request(Texture src)
	{
		return new AsyncGPUReadbackPluginRequest(src);
	}

	//Lets make our calls from the Plugin
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern int PrintANumber();
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
				// ?
				return false;
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
				// ?
				return true;
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
		if (SystemInfo.supportsAsyncGPUReadback) {
			usePlugin = false;
			gpuRequest = AsyncGPUReadback.Request(src);
		}
		else if(isCompatible()) {
			usePlugin = true;
			int textureId = (int)(src.GetNativeTexturePtr());
			this.eventId = makeRequest_mainThread(textureId, 0);
			GL.IssuePluginEvent(getfunction_makeRequest_renderThread(), this.eventId);
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
			if (usePlugin) {
				uint size = getDataSize_mainThread(this.eventId);
				byte[] buffer = new byte[size];
				NativeArray<T> rtn;
				fixed ( byte* p = buffer )
 				{
					getData_mainThread(this.eventId, p, size);
					rtn = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(p, (int)size, Allocator.Temp);
				}
				return rtn;
			}
			else {
				return gpuRequest.GetData<T>();
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
		GL.IssuePluginEvent(getfunction_dispose_renderThread(), this.eventId);
	}

	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern bool isCompatible();
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern int makeRequest_mainThread(int texture, int miplevel);
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern IntPtr getfunction_makeRequest_renderThread();
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern IntPtr getfunction_update_renderThread();
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern uint getDataSize_mainThread(int event_id);
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static unsafe extern uint getData_mainThread(int event_id, byte* buffer, uint max_length);
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern IntPtr getfunction_dispose_renderThread();
}

