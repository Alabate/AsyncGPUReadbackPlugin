using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using Unity.Collections;

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
		else if(IsCompatible()) {
			usePlugin = true;
			//int textureId = (int)src.GetNativeTexturePtr();
			// TODO
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
	public NativeArray<T> GetData<T>() where T : struct
	{
			if (usePlugin) {
				// ?
				return new NativeArray<T>();
			}
			else {
				return gpuRequest.GetData<T>();
			}
	}

	
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern bool IsCompatible();
}