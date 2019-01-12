# AsyncGPUReadbackPlugin
On Unity 2018.2 was introduced a really neat feature: being able get a frame from the gpu to the cpu without blocking the rendering. This feature is really useful for screenshot or network stream of a game camera because we need the frame on the cpu, but we don't care if there is a little delay.

However this feature is only available on platform supporting DirectX (Windows) and Metal (Apple), but not OpenGL and it's not planned. (source: https://forum.unity.com/threads/graphics-asynchronous-gpu-readback-api.529901/#post-3487735).

This plugin aim to provide this feature for OpenGL platform. It tries to match the official AsyncGPUReadback as closes as possible to let you easily switch between the plugin or the official API. Under the hood, it use the official API if available on the current platform.

I created this plugin for a specific use case of running our Unity project under linux, so the plugin is only built for Linux and the plugin API doen't provide all the feature that the official one does (like callback, choosing mipIndex, the destination format, etc.), but if you need one of them, contact me, I will see what we can do.

## Use it
### Install
This plugin has two parts that you need to copy inside your project :

* `libAsyncGPUReadbackPlugin.so`: It's the linux native plugin that talk directly to OpenGL.
  * Copy it from `NativePlugin/build/libAsyncGPUReadbackPlugin.so` to `/Assets/Plugins` in your project.
* `AsyncGPUReadbackPlugin.dll`: It's the C# part of the plugin that interface with the C++ native plugin. (Yes, that dll work under linux, it's just C# inside)
  * Copy it from `ManagedPlugin/bin/Release/netstandard2.0/AsyncGPUReadbackPlugin.dll` to anywhere under your `/Assets` folder.

### The API
Once you copied the plugin, add `using AsyncGPUReadbackPluginNs` at the beginning of the script where you want to use it.

#### `static AsyncGPUReadbackPluginRequest AsyncGPUReadbackPlugin.Request(Texture src)`
Same as the official API except that it doesn't implement all the other form. It request the texture from the gpu and return a `AsyncGPUReadbackPluginRequest` object to let you watch the state of the operation and get data back.

#### `AsyncGPUReadbackPluginRequest`
This object let you see if the request is done and get the data you asked for.

##### Attributes

* `hasError`: True if the request failed
* `done`: True if the request is done and data available

##### Methods

* `NativeArray<T> GetData<T>()`: This let you get the data you asked for in the format you want once it is available.
* `void Update(bool force = false)`: This method has to be called regularly to refresh request state. It differs from the official API because you have to call it manualy if you want the request to finish. It will do nothing if the official API is used and if `force == false`.
* `void Dispose()`: This plugin doens't free its buffer automatically, you have to call this methode manually once you finished working on the data you received from `GetData()`.

### Example
To see a working example you can open `UnityExampleProject` with the Unity editor. It saves screenshot of the camera every 60 frames. The script taking screenshot is in `UnityExampleProject/Scripts/UsePlugin.cs`

### Differences with the official API
There is two major differences:

* Update(): You have to manually call Update() regularly
* Dispose(): You have to manually call Dispose() when you finished with the data to avoid memory leak.

## Troubleshoots

### The type or namespace name 'AsyncGPUReadbackPluginNs' could not be found. Are you missing an assembly reference?
If you click on `AsyncGPUReadbackPlugin.dll` under Unity Editor, you will see 

```
Plugin targets .NET 4.x and is marked as compatible with Editor, Editor can only use assemblies targeting .NET 3.5 or lower, please unselect Editor as compatible platform
```

In this case a solution is to change you runtime version in:

```
Editor > Project Settings > Player > Other Settings > Scripting Runtime Version : Set it to '.NET 4.x Equivalent'.
```

## Build it!

### Native plugin
```
cd NativePlugin
make # The makefile only work for linux, but you could add other target inside if you want
```
You can find the built file under
```
NativePlugin/build/libAsyncGPUReadbackPlugin.so
```

### Managed plugin
You have to install the .Net SDK first to get the `dotnet` command: https://dotnet.microsoft.com/download/linux-package-manager/ubuntu18-04/sdk-current

```
cd ManagedPlugin
dotnet build -c Release
```
You can then find the built file under

```
ManagedPlugin/bin/Release/netstandard2.0/AsyncGPUReadbackPlugin.dll
```

## Thanks
This project was my first Unity plugin and the first time that I played with OpenGL, so I used a lot of internet ressources to do it. Here is some work that helped me:

* https://github.com/keijiro/AsyncCaptureTest
* https://github.com/SlightlyMad/AsyncTextureReader
* http://www.songho.ca/opengl/gl_pbo.html#pack
