using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertTraceSpawner : MonoBehaviour
{
	[SerializeField] ComputeShader edgeTracePartSpawnerCompute = null;
	string _etpsKernelName = "CSMain";
	public int _etpsckernel	{
		get{
			return edgeTracePartSpawnerCompute.FindKernel(_etpsKernelName);
		}
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
		edgeTracePartSpawnerCompute.SetFloat("_Time", Time.time);
		edgeTracePartSpawnerCompute.SetInt("_VertCount", RainMakerManager.inst.vertexCount);

		edgeTracePartSpawnerCompute.SetBuffer(_etpsckernel, "_ParticlePositionsToCheck", VertTraceCornerChecker.inst.cornersToCheck);

		if(Mathf.Floor(Time.time*5f) % 25f == 0)
			edgeTracePartSpawnerCompute.Dispatch(_etpsckernel,50, 1, 1);
	}
}
