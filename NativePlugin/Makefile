

# Linux build
linux: build/libAsyncGPUReadbackPlugin.so
build/libAsyncGPUReadbackPlugin.so: src/AsyncGPUReadbackPlugin.cpp
	g++ -fPIC -std=c++11 -shared src/AsyncGPUReadbackPlugin.cpp -o build/libAsyncGPUReadbackPlugin.so
