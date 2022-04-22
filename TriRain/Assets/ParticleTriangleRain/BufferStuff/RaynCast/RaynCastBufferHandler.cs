using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class RaynCastBufferHandler : MonoBehaviour
{
	public static RaynCastBufferHandler inst;
	[SerializeField] ComputeShader bufferIdsToTextureCompute;

	[SerializeField] VisualEffect vfxToTrigger;
	[SerializeField] string vfxSpawnCountProperty = "_NumToSpawn";


	public const string _ab2tKernelName = "CSMain";
	public int _ab2tmkernel
	{
		get
		{
			return bufferIdsToTextureCompute.FindKernel(_ab2tKernelName);
		}
	}


	[SerializeField] RenderTexture _outIds = null;
	RenderTexture _tempOutIds;

	ComputeBuffer spawnIdBuffer;
	Vector2[] idBufferData;
	void Awake()
	{
		RaynCastBufferHandler.inst = this;
	}

	private void Start()
	{
		spawnIdBuffer = new ComputeBuffer(2048, sizeof(float) * 2);
		idBufferData = new Vector2[spawnIdBuffer.count];
	}


	public void TransposeSpawnIntsToTexture2D(List<int> indexToSpawnRainEffectsAt, int frameDelay)
	{
		IdListToVector2Array(indexToSpawnRainEffectsAt, frameDelay);

		spawnIdBuffer.SetData(idBufferData);
		TransposeToTexture2D(spawnIdBuffer, indexToSpawnRainEffectsAt.Count);


		//TODO: delay on CPU the spawner command,
	}


	void IdListToVector2Array(List<int> indecies, float delay)
	{
		Vector2 iddelay = new Vector2(0f, delay);
		for (int i = 0; i < idBufferData.Length; i++)
		{
			if (indecies.Count > 0)
			{
				int i1 = i % indecies.Count;
				iddelay.x = (float)indecies[i1];
				idBufferData[i] = iddelay;
			}
			else
				idBufferData[i] = Vector2.zero;
		}
	}


	void TransposeToTexture2D(ComputeBuffer buffer, int count)
	{
		if (_tempOutIds != null && _tempOutIds.width != _outIds.width)
		{
			Destroy(_tempOutIds);
			_tempOutIds = null;
		}

		if (_tempOutIds == null)
		{
			_tempOutIds = Smrvfx.Utility.CreateRenderTexture(_outIds);
		}

		bufferIdsToTextureCompute.SetVector("_Dimensions", new Vector4(_tempOutIds.width, _tempOutIds.height, 1f, _tempOutIds.width * _tempOutIds.height));
		bufferIdsToTextureCompute.SetVector("_InvDimensions", new Vector4(1f / _tempOutIds.width, 1f / _tempOutIds.height, 1f, 1f / (_tempOutIds.width * _tempOutIds.height)));

		bufferIdsToTextureCompute.SetInt("_SpawnCount", count);
		bufferIdsToTextureCompute.SetBuffer(_ab2tmkernel, "_AppendedSpawnIds", buffer);
		bufferIdsToTextureCompute.SetTexture(_ab2tmkernel, "_TextureSpawnIds", _tempOutIds);
		bufferIdsToTextureCompute.Dispatch(_ab2tmkernel, _tempOutIds.width / 8, _tempOutIds.height / 8, 1);

		Graphics.CopyTexture(_tempOutIds, _outIds);
		vfxToTrigger.SetUInt(vfxSpawnCountProperty, (uint)count);
	}


	void DebugRainSpawnPositions(List<int> indexToSpawnEffects)
	{

	}


	private void OnDestroy()
	{
		Smrvfx.Utility.TryDestroy(_tempOutIds);
		_tempOutIds = null;
	}
}
