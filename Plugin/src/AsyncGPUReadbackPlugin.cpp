/*
	This is a simple plugin, a bunch of functions that do simple things.
*/

extern "C" {

	/**
	 * Check if plugin is compatible with this system
	 */
	bool IsCompatible() {
		return true;
	}

	int PrintANumber(){
		return 42;
	}
}



