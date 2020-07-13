using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AppendFromGraph : MonoBehaviour
{
	ComputeBuffer compy;
	ComputeBuffer appy;
    // Start is called before the first frame update
    void Awake()
    {
		compy = new ComputeBuffer(1024, sizeof(float) * 3);

		List<Vector3> compydat = new List<Vector3>();

		for(int i = 0; i < 1024; i++)
		{
			compydat.Add(Vector3.right);//new Vector3(Random.value, Random.value, 1f).normalized*2f);
		}

		compy.SetData(compydat.ToArray());
		Shader.SetGlobalBuffer("_Compy", compy);



		appy = new ComputeBuffer(65536, sizeof(int), ComputeBufferType.Append);
		Shader.SetGlobalBuffer("_Appy", appy);
	}

	// Update is called once per frame
	void Update()
    {
		Shader.SetGlobalBuffer("_Compy", compy);

		ComputeBuffer argbuff = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
		int[] dat = BufferTools.GetArgs(appy, argbuff);

		Debug.Log("appy counter AT: " + dat[0] + ", " + dat[1] + ", " + dat[2] + ", " + dat[3]);
		Shader.SetGlobalBuffer("_Appy", appy);
		//appy.SetCounterValue(0);
		Graphics.ClearRandomWriteTargets();
		Graphics.SetRandomWriteTarget(7, appy, false);
		argbuff.Dispose();
	}


	private void OnDestroy()
	{
		compy.Dispose();
		appy.Dispose();
	}
}
