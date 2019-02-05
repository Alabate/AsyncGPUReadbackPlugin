using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace AsyncGPUReadbackPluginNs {

	// Tries to match the official API
	public class AsyncGPUReadbackPlugin
	{
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
		/// Is buffer allocated
		/// </summary>
		private bool bufferCreated = false;

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
		/// Create an AsyncGPUReadbackPluginRequest.
		/// Use official AsyncGPUReadback.Request if possible.
		/// If not, it tries to use OpenGL specific implementation
		/// Warning! Can only be called from render thread yet (not main thread)
		/// </summary>
		/// <param name="src"></param>
		/// <returns></returns>
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

		public unsafe byte[] GetRawData()
		{
			if (usePlugin) {
				// Get data from cpp plugin
				void* ptr = null;
				int length = 0;
				getData_mainThread(this.eventId, ref ptr, ref length);

				// Copy data to a buffer that we own and that will not be deleted
				byte[] buffer = new byte[length];
				Marshal.Copy(new IntPtr(ptr), buffer, 0, length);
				bufferCreated = true;

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
			if (usePlugin) {
				GL.IssuePluginEvent(getfunction_update_renderThread(), this.eventId);
			}
			else if(force) {
				gpuRequest.Update();
			}
		}

		/// <summary>
		/// Has to be called to free the allocated buffer after it has been used
		/// </summary>
		public void Dispose()
		{
			if (usePlugin && bufferCreated) {
				dispose(this.eventId);
			}
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
		private static extern unsafe void getData_mainThread(int event_id, ref void* buffer, ref int length);
		[DllImport ("AsyncGPUReadbackPlugin")]
		private static extern bool isRequestError(int event_id);
		[DllImport ("AsyncGPUReadbackPlugin")]
		private static extern bool isRequestDone(int event_id);
		[DllImport ("AsyncGPUReadbackPlugin")]
		private static extern void dispose(int event_id);
	}
}