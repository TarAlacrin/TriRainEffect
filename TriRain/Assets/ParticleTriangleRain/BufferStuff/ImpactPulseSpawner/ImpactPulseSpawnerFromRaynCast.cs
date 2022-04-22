using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactPulseSpawnerFromRaynCast : MonoBehaviour
{
	public static ImpactPulseSpawnerFromRaynCast inst;
	[SerializeField] ComputeShader impactPulseSpawnerAppenderCompute = null;
	string _ipsmKernelName = "CSMain";
	public int _etpsckernel
	{
		get
		{
			return impactPulseSpawnerAppenderCompute.FindKernel(_ipsmKernelName);
		}
	}

	//Dictionary<int, List<int>> indeciesToSpawnByFrameNumber = new Dictionary<int, List<int>>();
	ComputeBuffer indeciesToSpawnBuffer;

	private void Awake()
	{
		ImpactPulseSpawnerFromRaynCast.inst = this;
	}

	// Start is called before the first frame update
	void Start()
	{
		indeciesToSpawnBuffer = new ComputeBuffer(2048, sizeof(int));
	}



	public void TransposeSpawnIntsToVertTrace(List<int> indexToSpawnRainEffectsAt, int frameDelay)
	{
		CheckAndSpawnVertTracersAtIndecies(indexToSpawnRainEffectsAt);
		//indeciesToSpawnByFrameNumber.Add(Time.frameCount+frameDelay, indexToSpawnRainEffectsAt);
	}


	void SetAndDispatch(int vertcount)
	{
		impactPulseSpawnerAppenderCompute.SetInt("_VertCount", vertcount);
		impactPulseSpawnerAppenderCompute.SetBuffer(_etpsckernel, "_ParticlePositionsToCheck", VertTraceCornerChecker.inst.cornersToCheck);
		impactPulseSpawnerAppenderCompute.SetBuffer(_etpsckernel, "_SpawnParticlesAtVertecies", indeciesToSpawnBuffer);
		impactPulseSpawnerAppenderCompute.Dispatch(_etpsckernel, vertcount, 1, 1);
	}


	void CheckAndSpawnVertTracersAtIndecies(List<int> vertsToSpawn)
	{
			//Debug.Log("First Spawn At Time:" + Time.time);
			indeciesToSpawnBuffer.SetData(vertsToSpawn);

			if(vertsToSpawn.Count >0)
				SetAndDispatch(vertsToSpawn.Count);
	}

	// Update is called once per frame
	void Update()
	{
		//CheckAndSpawnVertTracersAtIndecies();

		//edgeTracePartSpawnerCompute.SetVector("_Dimensions", new Vector4(_meshVertPositions.width, _meshVertPositions.height, 1f, _meshVertPositions.width * _meshVertPositions.height));
		//edgeTracePartSpawnerCompute.SetVector("_InvDimensions", new Vector4(1f / _meshVertPositions.width, 1f / _meshVertPositions.height, 1f, 1f / (_meshVertPositions.width * _meshVertPositions.height)));

		//edgeTracePartSpawnerCompute.SetFloat("_Time", Time.time);
		//edgeTracePartSpawnerCompute.SetInt("_VertCount", RainMakerManager.inst.vertexCount);

		//edgeTracePartSpawnerCompute.SetBuffer(_etpsckernel, "_ParticlePositionsToCheck", VertTraceCornerChecker.inst.cornersToCheck);
		//edgeTracePartSpawnerCompute.SetTexture(_etpsckernel, "_MeshVertPositions", _meshVertPositions);

		//edgeTracePartSpawnerCompute.Dispatch(_etpsckernel, 1, 1, 1);
	}


	private void OnDestroy()
	{
		indeciesToSpawnBuffer.Release();
	}
}
