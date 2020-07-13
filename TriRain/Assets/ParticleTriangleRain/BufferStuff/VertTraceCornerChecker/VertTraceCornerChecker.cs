﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertTraceCornerChecker : MonoBehaviour
{
	public static VertTraceCornerChecker inst;



	public ComputeShader cornerCheckerCompute;
	public const string _ccKernelName = "CSMain";
	public int _cckernel {
		get {
			return cornerCheckerCompute.FindKernel(_ccKernelName);
		}
	}



	[Space]
	[SerializeField] RenderTexture meshVertPositionTex;
	[SerializeField] RenderTexture meshVertVelocityTex;
	[SerializeField] RenderTexture meshVertNormalsTex;
	ComputeBuffer argsBuffer;

	ComputeBuffer adjacentVertIndexBuffer;
	int maxAdjacentVertCount;
	int totalVertcount;

	public ComputeBuffer cornersToCheck;



	void Awake()
    {
		VertTraceCornerChecker.inst = this;
	}

	int[] GetAdjacentVertsFromTriangles(List<List<int>> includedTris, List<int> triangleVertInds, out int maxAdjacentVerts)
	{
		List<List<int>> allAdjacentVerts = new List<List<int>>();
		maxAdjacentVerts = 0;
		for (int i = 0; i < includedTris.Count; i++)
		{
			List<int> adjVerts = new List<int>();
			for (int t = 0; t < includedTris[i].Count; t++)
			{

				int triInd = includedTris[i][t] * 3;
				for (int ti = 0; ti < 3; ti++)
				{
					int potentialAdjacentVert = triangleVertInds[ti + triInd];
					if (!adjVerts.Contains(potentialAdjacentVert) && potentialAdjacentVert != i)//if we haven't already included the vert in this vert's adjacent list, AND the vert isn't equal to the vert in question.
						adjVerts.Add(potentialAdjacentVert);
				}
			}

			allAdjacentVerts.Add(adjVerts);

			if (adjVerts.Count > maxAdjacentVerts)
				maxAdjacentVerts = adjVerts.Count;
		}

		Debug.Log("Processed " + includedTris.Count + " verts and had a max adjacent vertex count of: " + maxAdjacentVerts);
		int[] adjacentVertsCompressed = new int[allAdjacentVerts.Count * maxAdjacentVerts];
		totalVertcount = includedTris.Count;
		//compresses the list of lists down into a single int array so it can be sent to a buffer easily. 
		for (int k = 0; k < allAdjacentVerts.Count; k++)
		{
			List<int> adjacentVerts = allAdjacentVerts[k];
			int originalCount = adjacentVerts.Count;
			for (int j = 0; j < maxAdjacentVerts; j++)
			{
				int j1 = j % originalCount;//modulo to fill vertecies with less than the max number of adjacent verts with looping valid data
				adjacentVertsCompressed[k * maxAdjacentVerts + j] = adjacentVerts[j1];
			}
		}

		return adjacentVertsCompressed;
	}

	public void ConvertIncludedTrisToAdjacentVertBuffer(List<List<int>> includedTris, List<int> triangleVertInds, int vertCount)
	{
		int[] adjacentVerts = GetAdjacentVertsFromTriangles(includedTris, triangleVertInds, out int maxAdjacetVerts);
		maxAdjacentVertCount = maxAdjacetVerts;
		adjacentVertIndexBuffer = new ComputeBuffer(includedTris.Count, sizeof(int) * maxAdjacetVerts);
		adjacentVertIndexBuffer.SetData(adjacentVerts);
	}


	private void Start()
	{
		cornersToCheck = new ComputeBuffer(4096, sizeof(float) * 4, ComputeBufferType.Append);
		argsBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
	}


	// Update is called once per frame
	void Update()
	{
		cornerCheckerCompute.SetFloat("_Time", Time.time);
		cornerCheckerCompute.SetInt("_VertCount", totalVertcount);


		cornerCheckerCompute.SetInt("_AdjacentVertBufferStride", maxAdjacentVertCount);
		cornerCheckerCompute.SetBuffer(_cckernel, "_AdjacentVertBuffer", adjacentVertIndexBuffer);

		cornerCheckerCompute.SetBuffer(_cckernel, "_ParticlesToCheck", cornersToCheck);
		cornerCheckerCompute.SetBuffer(_cckernel, "_ParticlesInTransit", MeshEdgeParticleTracer.inst.GetWriteBuffer());

		if(Time.time - lastSpawn > 2f)
		{
			lastSpawn = Time.time;
			cornerCheckerCompute.Dispatch(_cckernel, 10, 1, 1);

		}
		cornersToCheck.SetCounterValue(0);
	}

	float lastSpawn = -3f;


	private void OnDestroy()
	{
		Smrvfx.Utility.TryDispose(adjacentVertIndexBuffer);
		Smrvfx.Utility.TryDispose(cornersToCheck);
		Smrvfx.Utility.TryDispose(argsBuffer);
	}

}
