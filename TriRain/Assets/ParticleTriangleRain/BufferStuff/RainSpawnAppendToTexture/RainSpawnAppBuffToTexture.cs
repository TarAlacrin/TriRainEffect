using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
public class RainSpawnAppBuffToTexture : MonoBehaviour
{
	public VisualEffect effect;
	public static RainSpawnAppBuffToTexture inst;
	public ComputeShader appendBufferIdsToTextureCompute;

	public const string _ab2tKernelName = "CSMain";
	public int _ab2tmkernel
	{
		get
		{
			return appendBufferIdsToTextureCompute.FindKernel(_ab2tKernelName);
		}
	}



	[SerializeField] RenderTexture _outIds =null;
	RenderTexture _tempOutIds;



	// Start is called before the first frame update
	void Start()
    {
		RainSpawnAppBuffToTexture.inst = this;
	}





	public void TransposeToTexture2D(ComputeBuffer appendBuffer, int count)
	{
		if(_tempOutIds != null && _tempOutIds.width != _outIds.width)
		{
			Destroy(_tempOutIds);
			_tempOutIds = null;
		}

		if(_tempOutIds ==null)
		{
			_tempOutIds = Smrvfx.Utility.CreateRenderTexture(_outIds);
		}

		appendBufferIdsToTextureCompute.SetVector("_Dimensions", new Vector4(_tempOutIds.width, _tempOutIds.height, 1f, _tempOutIds.width* _tempOutIds.height));
		appendBufferIdsToTextureCompute.SetVector("_InvDimensions", new Vector4(1f/_tempOutIds.width, 1f/_tempOutIds.height, 1f, 1f/(_tempOutIds.width* _tempOutIds.height)));


		//appendBufferIdsToTextureCompute.SetFloat("_Time", Time.time);
		appendBufferIdsToTextureCompute.SetInt("_SpawnCount", count);
		appendBufferIdsToTextureCompute.SetBuffer(_ab2tmkernel, "_AppendedSpawnIds",appendBuffer);
		appendBufferIdsToTextureCompute.SetTexture(_ab2tmkernel, "_TextureSpawnIds",_tempOutIds);

		appendBufferIdsToTextureCompute.Dispatch(_ab2tmkernel, _tempOutIds.width / 8, _tempOutIds.height / 8, 1);
	
		Graphics.CopyTexture(_tempOutIds, _outIds);

		effect.SetInt("NumToSpawn", count);
	}



	private void OnDestroy()
	{
		Smrvfx.Utility.TryDestroy(_tempOutIds);
		_tempOutIds = null;
	}
}
