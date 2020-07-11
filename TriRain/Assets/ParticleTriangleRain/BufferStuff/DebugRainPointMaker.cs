﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugRainPointMaker : MonoBehaviour
{
	public ComputeShader debugRainMakerCompute;

	ComputeBuffer[] idsToSpawnABuffer=new ComputeBuffer[2];
	ComputeBuffer countBuffer;

	public const string _drmKernelName = "CSMain";
	public int _drmkernel
	{
		get
		{
			return debugRainMakerCompute.FindKernel(_drmKernelName);
		}
	}


	// Start is called before the first frame update
	void Start()
    {
		idsToSpawnABuffer[BufferTools.READ] = new ComputeBuffer(64, sizeof(int), ComputeBufferType.Append);
		idsToSpawnABuffer[BufferTools.WRITE] = new ComputeBuffer(64, sizeof(int), ComputeBufferType.Append);
		countBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
	}

	// Update is called once per frame
	void Update()
    {
		UpdateRainSpawner();
	}


	void UpdateRainSpawner()
	{
		BufferTools.Swap(idsToSpawnABuffer);
		idsToSpawnABuffer[BufferTools.WRITE].SetCounterValue(0);
		debugRainMakerCompute.SetBuffer(_drmkernel,"_OutSpawnIndecies", idsToSpawnABuffer[BufferTools.WRITE]);

		debugRainMakerCompute.Dispatch(_drmkernel, RainMakerManager.inst.vertexThreadGroupCount, 1, 1);
		debugRainMakerCompute.SetInt("_VertexCount", RainMakerManager.inst.vertexCount);
		debugRainMakerCompute.SetFloat("_Time", Time.time);
	
		int[] args = BufferTools.GetArgs(idsToSpawnABuffer[BufferTools.WRITE], countBuffer);
		int appendcount = args[0];
		Debug.Log("appendCount: " + appendcount);
		RainSpawnAppBuffToTexture.inst.TransposeToTexture2D(idsToSpawnABuffer[BufferTools.WRITE], appendcount);

	} 


	private void OnDisable()
	{
		idsToSpawnABuffer[BufferTools.READ].Dispose();
		idsToSpawnABuffer[BufferTools.WRITE].Dispose();
		countBuffer.Dispose();
	}

}
