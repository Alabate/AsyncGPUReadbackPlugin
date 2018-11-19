

# Linux build
linux: build/libAsyncGPUReadbackPlugin.so
build/libAsyncGPUReadbackPlugin.so: src/AsyncGPUReadbackPlugin.cpp
	g++ -fPIC -std=c++11 -shared src/AsyncGPUReadbackPlugin.cpp -o build/libAsyncGPUReadbackPlugin.so
	cp build/libAsyncGPUReadbackPlugin.so ../ExampleUnityProject/Assets/Plugins

# Windows build with mingw
windows: build/AsyncGPUReadbackPlugin.dll
build/AsyncGPUReadbackPlugin.dll: src/AsyncGPUReadbackPlugin.cpp
	g++ -fPIC -std=c++11 -shared src/AsyncGPUReadbackPlugin.cpp -o build/AsyncGPUReadbackPlugin.dll
	cp build/AsyncGPUReadbackPlugin.dll ../ExampleUnityProject/Assets/Plugins
