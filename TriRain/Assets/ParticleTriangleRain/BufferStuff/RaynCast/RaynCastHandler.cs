using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaynCastHandler : MonoBehaviour
{

	[SerializeField] Smrvfx.SkinnedMeshBaker baker;
	[SerializeField] AnimatorDelayer delayerFrameReference;
	[SerializeField] float numToSpawnPerSecond;
	float spawnsLeft = 0f;

	struct FrameSpawnInfo
	{
		public int frameLife;
		public List<int> vertIndexSpawns;
	}

	Transform casterTransform;

	Texture2D spawnRaynTexture;
	Texture2D spawnEdgeTracerTexture;

	List<FrameSpawnInfo> pastFrameSpawnInfos = new List<FrameSpawnInfo>();

	FrameSpawnInfo currentFrameInfo;

	//List<int> trivertInt = new List<int>();
	// Start is called before the first frame update
	void Start()
    {
		casterTransform = new GameObject().transform;
		casterTransform.parent = this.transform;
		casterTransform.localRotation = Quaternion.identity;
		casterTransform.localScale = Vector3.one;
		casterTransform.localPosition = Vector3.zero;
	}

	void DebugDrawCrosshair(Vector3 wpos, Color col, float lengt = 0.2f)
	{
		Debug.DrawLine(wpos - Vector3.right * lengt, wpos + Vector3.right * lengt, col);
		Debug.DrawLine(wpos - Vector3.up * lengt, wpos + Vector3.up * lengt, col);
		Debug.DrawLine(wpos - Vector3.forward * lengt, wpos + Vector3.forward * lengt, col);
	}


	void DoRaycast(Vector3 pos, Vector3 dir)
	{

		Ray ray = new Ray(pos, dir);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			Debug.DrawRay(pos, dir * hit.distance, Color.red);
			DebugDrawCrosshair(hit.transform.TransformPoint(baker._positionList[baker._triVertIndecies[hit.triangleIndex * 3]]), Color.green);

			currentFrameInfo.vertIndexSpawns.Add(baker._triVertIndecies[hit.triangleIndex * 3]);
		}
		else
			Debug.DrawRay(pos, dir * 100, Color.yellow);

	}


	void SpawnVertRain(FrameSpawnInfo fsi)
	{
		RaynCastBufferHandler.inst.TransposeSpawnIntsToTexture2D(fsi.vertIndexSpawns, delayerFrameReference.framesToDelay);
	}

	void CycleFrameSpawns()
	{
		for(int i = pastFrameSpawnInfos.Count-1; i>=0; i--)
		{
			FrameSpawnInfo fsi = pastFrameSpawnInfos[i];
			fsi.frameLife -= 1;
			pastFrameSpawnInfos[i] = fsi;
			if (fsi.frameLife <= 0)
			{
				SpawnVertRain(fsi);
				pastFrameSpawnInfos.RemoveAt(i);
			}
		}
	}

	void Update()
    {
		CycleFrameSpawns();
		currentFrameInfo = new FrameSpawnInfo();
		currentFrameInfo.vertIndexSpawns = new List<int>();
		currentFrameInfo.frameLife = delayerFrameReference.framesToDelay;

		spawnsLeft += numToSpawnPerSecond*Time.deltaTime;

		while(spawnsLeft >1f)
		{
			spawnsLeft-=1f;
			casterTransform.localPosition = new Vector3(Random.value * 2f - 1f,  Random.value * 2f - 1f, 0f);
			DoRaycast(casterTransform.position, casterTransform.forward);
		}
		//need to send data to the rayncast spawner immediately when new, and to the traditional edgeTrace spawner when finished delaying
		pastFrameSpawnInfos.Add(currentFrameInfo);
	}
}


