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
    Camera cam;
    int pixelHeight;
    int pixelWidth;
    RenderTexture renderTexture;
	
    void Start()
    {
        cam = GetComponent<Camera>();
        pixelHeight = cam.pixelHeight;
        pixelWidth = cam.pixelWidth;

        renderTexture = new RenderTexture(pixelWidth, pixelHeight, 0, RenderTextureFormat.ARGB32)
        {
            antiAliasing = 1,
            autoGenerateMips = true,
            dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
            volumeDepth = 1
        };
        renderTexture.Create();
        cam.targetTexture = renderTexture;
        StartCoroutine(triggerRender());
    }

    IEnumerator triggerRender()
    {
        while (true) {
            cam.Render();
            yield return new WaitForSeconds(1.0f);
        }
    }

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
                Debug.Log("<Done.");
                SaveBitmap(req.GetRawData(), pixelWidth, pixelHeight);
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
        Debug.Log("<Request: " + source.format);
        if (_requests.Count < 8)
            _requests.Enqueue(AsyncGPUReadbackPlugin.Request(source));
        else
            Debug.Log("Too many requests.");

        Graphics.Blit(source, destination);
    }

    void SaveBitmap(byte[] buffer, int width, int height)
    {
        Debug.LogError("Write to file." + buffer.Length);
        String dbg = "";
        for(int i = 0 ; i < 10 ; i++) {
            dbg += String.Format("{0:X}", buffer[i]);
        }
        Debug.Log("Write to file: " + dbg);

        var tex = new Texture2D(width, height, TextureFormat.RGBAHalf, false);
        tex.LoadRawTextureData(buffer);
        tex.Apply();
        File.WriteAllBytes("test.png", ImageConversion.EncodeToPNG(tex));
        Destroy(tex);
	}
}
