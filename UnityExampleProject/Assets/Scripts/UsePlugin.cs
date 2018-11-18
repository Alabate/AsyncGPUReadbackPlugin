using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

public class UsePlugin : MonoBehaviour {	
	void Start () {
		Debug.LogWarning(AsyncGPUReadbackPlugin.PrintANumber2());
	}
}
