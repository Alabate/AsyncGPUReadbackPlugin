

# Linux build
linux: build/libAsyncGPUReadbackPlugin.so
build/libAsyncGPUReadbackPlugin.so: src/AsyncGPUReadbackPlugin.cpp
	mkdir -p build
	g++ -fPIC -shared src/AsyncGPUReadbackPlugin.cpp -o build/libAsyncGPUReadbackPlugin.so
