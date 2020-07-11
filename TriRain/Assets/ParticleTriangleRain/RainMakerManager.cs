using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainMakerManager : MonoBehaviour
{
	public static RainMakerManager inst;

	[HideInInspector]
	public int vertexCount;

	const int threadGroupSize = 64;
	public int vertexThreadGroupCount
	{
		get
		{
			return Mathf.CeilToInt((float)vertexCount /(float)threadGroupSize);
		}
	}

    // Start is called before the first frame update
    void Awake()
    {
		RainMakerManager.inst = this;       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
