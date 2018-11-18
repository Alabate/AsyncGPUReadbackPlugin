using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

public class AsyncGPUReadbackPlugin {

	public static int PrintANumber2()
	{
		return PrintANumber();
	}

	
	

	//Lets make our calls from the Plugin
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern int PrintANumber();
	
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern IntPtr PrintHello();
	
	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern int AddTwoIntegers(int i1,int i2);

	[DllImport ("AsyncGPUReadbackPlugin")]
	private static extern float AddTwoFloats(float f1,float f2);
}
