using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactPulseSpawner : MonoBehaviour
{
	[SerializeField] ComputeShader impactPulseSpawnerCompute = null;
	string _ippsKernelName = "CSMain";
	public int _ippscKernel	{
		get{
			return impactPulseSpawnerCompute.FindKernel(_ippsKernelName);
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
		impactPulseSpawnerCompute.SetVector("_Dimensions", new Vector4(_meshVertPositions.width, _meshVertPositions.height, 1f, _meshVertPositions.width * _meshVertPositions.height));
		impactPulseSpawnerCompute.SetVector("_InvDimensions", new Vector4(1f / _meshVertPositions.width, 1f / _meshVertPositions.height, 1f, 1f / (_meshVertPositions.width * _meshVertPositions.height)));

		impactPulseSpawnerCompute.SetFloat("_Time", Time.time);
		impactPulseSpawnerCompute.SetInt("_VertCount", RainMakerManager.inst.vertexCount);

		impactPulseSpawnerCompute.SetBuffer(_ippscKernel, "_PulseLocations", VertTraceCornerChecker.inst.cornersToCheck);
		impactPulseSpawnerCompute.SetTexture(_ippscKernel, "_MeshVertPositions", _meshVertPositions);

		//if (Mathf.Floor(Time.time*5f) % 25f == 0)
		impactPulseSpawnerCompute.Dispatch(_ippscKernel, 1, 1, 1);
	}
}
