using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using System.IO;

/// <summary>
/// Exemple of usage inspirated from https://github.com/keijiro/AsyncCaptureTest/blob/master/Assets/AsyncCapture.cs
/// </summary>
public class UsePlugin : MonoBehaviour {

	Queue<AsyncGPUReadbackPluginRequest> _requests = new Queue<AsyncGPUReadbackPluginRequest>();
	
	void Update()
    {
        while (_requests.Count > 0)
        {
            var req = _requests.Peek();
			req.Update();

            if (req.hasError)
            {
                Debug.Log("GPU readback error detected.");
                _requests.Dequeue();
            }
            else if (req.done)
            {
                if (Time.frameCount % 60 == 0) {
                    Camera cam = GetComponent<Camera>();
                    SaveBitmap(req.GetRawData(), cam.pixelWidth, cam.pixelHeight);
                }
                _requests.Dequeue();
            }
            else
            {
                break;
            }
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_requests.Count < 8)
            _requests.Enqueue(AsyncGPUReadbackPlugin.Request(source));
        else
            Debug.LogWarning("Too many requests.");

        Graphics.Blit(source, destination);
    }

    void SaveBitmap(byte[] buffer, int width, int height)
    {
        Debug.Log("Write to file");
        var tex = new Texture2D(width, height, TextureFormat.RGBAHalf, false);
        tex.LoadRawTextureData(buffer);
        tex.Apply();
        File.WriteAllBytes("test.png", ImageConversion.EncodeToPNG(tex));
        Destroy(tex);
	}
}
