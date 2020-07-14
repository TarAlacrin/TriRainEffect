using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class EdgeTraceVFXHandler : MonoBehaviour
{
	public static EdgeTraceVFXHandler inst;


	[SerializeField] ComputeShader particleDataToTextureCompute =null;
	public const string _pd2vfxKernelName = "CSMain";
	public int _pd2vfxkernel {
		get	{
			return particleDataToTextureCompute.FindKernel(_pd2vfxKernelName);
		}
	}


	[SerializeField] RenderTexture outputParticlePositionTex = null;
	[SerializeField] VisualEffect edgeTraceVfx = null;
	[SerializeField] string numParticlesToSpawnPropertyName = "_NumToSpawn";

	RenderTexture _tempParticlePositions;


	ComputeBuffer argsBuffer;
    // Start is called before the first frame update
    void Awake()
    {
		EdgeTraceVFXHandler.inst = this;
    }

	private void Start()
	{
		argsBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
	}

	// Update is called once per frame
	void Update()
    {
        
    }


	void HandleTemporaryParticleDat()
	{
		if (_tempParticlePositions != null && (_tempParticlePositions.width != outputParticlePositionTex.width || _tempParticlePositions.height != outputParticlePositionTex.height))
		{
			Destroy(_tempParticlePositions);
			_tempParticlePositions = null;
		}

		if (_tempParticlePositions == null)
		{
			_tempParticlePositions = Smrvfx.Utility.CreateRenderTexture(outputParticlePositionTex);
		}

		particleDataToTextureCompute.SetVector("_Dimensions", new Vector4(_tempParticlePositions.width, _tempParticlePositions.height, 1f, _tempParticlePositions.width * _tempParticlePositions.height));
		particleDataToTextureCompute.SetVector("_InvDimensions", new Vector4(1f / _tempParticlePositions.width, 1f / _tempParticlePositions.height, 1f, 1f / (_tempParticlePositions.width * _tempParticlePositions.height)));
	}


	public void PassToSpawner(ComputeBuffer particleDataToSpawn)
	{
		int[] appargs = BufferTools.GetArgs(particleDataToSpawn, argsBuffer);
		HandleTemporaryParticleDat();

		edgeTraceVfx.SetInt(numParticlesToSpawnPropertyName, appargs[0]);

		particleDataToTextureCompute.SetInt("_SpawnCount", appargs[0]);
		particleDataToTextureCompute.SetTexture(_pd2vfxkernel, "_TextureSpawnParticleData", _tempParticlePositions);
		particleDataToTextureCompute.SetBuffer(_pd2vfxkernel, "_AppendedSpawnParticleData", particleDataToSpawn);
		particleDataToTextureCompute.Dispatch(_pd2vfxkernel, _tempParticlePositions.width / 8, _tempParticlePositions.height / 8, 1);
		Graphics.CopyTexture(_tempParticlePositions, outputParticlePositionTex);
	}


	private void OnDestroy()
	{
		Smrvfx.Utility.TryDestroy(_tempParticlePositions);
		Smrvfx.Utility.TryDispose(argsBuffer);
	}
}
