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


	[SerializeField] RenderTexture _meshVertPositions;


	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {


		edgeTracePartSpawnerCompute.SetVector("_Dimensions", new Vector4(_meshVertPositions.width, _meshVertPositions.height, 1f, _meshVertPositions.width * _meshVertPositions.height));
		edgeTracePartSpawnerCompute.SetVector("_InvDimensions", new Vector4(1f / _meshVertPositions.width, 1f / _meshVertPositions.height, 1f, 1f / (_meshVertPositions.width * _meshVertPositions.height)));

		edgeTracePartSpawnerCompute.SetFloat("_Time", Time.time);
		edgeTracePartSpawnerCompute.SetInt("_VertCount", RainMakerManager.inst.vertexCount);

		edgeTracePartSpawnerCompute.SetBuffer(_etpsckernel, "_ParticlePositionsToCheck", VertTraceCornerChecker.inst.cornersToCheck);
		edgeTracePartSpawnerCompute.SetTexture(_etpsckernel, "_MeshVertPositions", _meshVertPositions);

		//if (Mathf.Floor(Time.time*5f) % 25f == 0)
			edgeTracePartSpawnerCompute.Dispatch(_etpsckernel,1, 1, 1);
	}
}
