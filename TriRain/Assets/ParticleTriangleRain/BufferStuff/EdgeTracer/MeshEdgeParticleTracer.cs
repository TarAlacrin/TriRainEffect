using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class MeshEdgeParticleTracer : MonoBehaviour
{
	public static MeshEdgeParticleTracer inst;
	[SerializeField] private ComputeShader edgeTracerCompute = null;

	[SerializeField] RenderTexture meshVertexPositionTex;
	[SerializeField] RenderTexture outputParticlePositionTex;
	[Space]
	[SerializeField] VisualEffect vfxTarget;
	[SerializeField] string numParticlesPropertyName = "_NumParts";

	public const string _etKernelName = "CSMain";
	public int _etkernel {
		get {
			return edgeTracerCompute.FindKernel(_etKernelName);
		}
	}

	RenderTexture _tempParticlePositions;
	ComputeBuffer[] particleSimBuffers = new ComputeBuffer[2];
	ComputeBuffer argsBuffer;

	private void Awake()
	{
		MeshEdgeParticleTracer.inst = this;
	}




	// Start is called before the first frame update
	void Start()
    {
		//get the triangle list from skinner, get included vertex list from skinner??

		//Actually, just get adjacent vertex list from skinner and the vert with the most connections. 
		//Develop an apppend buffer with that integer to inform the max stride per entry. 
		//Fill each vertex with less connections with looping pattern of valid indecies.

		//ONE compute shader then runs every frame to simply carry the valid particles from one vertex to their target vertex. 
		//That shader then appends any valid particle ids which have reached their destination to an append buffer to be processed by another compute shader

		//That shader then runs through each of the particles who have reached their target to see if they are at the bottom of a tri or not.
		//If they are at the bottom, (or another factor like they were heading in the direction of a drop) the shader either re-adds them to the main particle list with new destination info, or adds them to the rain spawn list.



		particleSimBuffers[0] = new ComputeBuffer(RainMakerManager.inst.VertexTracerCapacity, sizeof(float) * 4, ComputeBufferType.Append);
		particleSimBuffers[1] = new ComputeBuffer(RainMakerManager.inst.VertexTracerCapacity, sizeof(float) * 4, ComputeBufferType.Append);
		argsBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
	}

	public ComputeBuffer GetWriteBuffer()
	{
		return particleSimBuffers[BufferTools.WRITE];
	}


	//STRETCH GOAL
	//Have TriggerEventOnDie create a particle for a single frame which uses a shader that adds something to a buffer (this is a way to get the info of when a 
	//VSPro_HDIndirect might have the path forward towards getting a custom function which writes things to an append buffer. Itll be a wild ride though
	void LateUpdate()
    {
		BufferTools.Swap(particleSimBuffers);

		if(_tempParticlePositions != null && (_tempParticlePositions.width != outputParticlePositionTex.width || _tempParticlePositions.height != outputParticlePositionTex.height))
		{
			Destroy(_tempParticlePositions);
			_tempParticlePositions = null;
		}

		if(_tempParticlePositions == null)
		{
			_tempParticlePositions = Smrvfx.Utility.CreateRenderTexture(outputParticlePositionTex);
		}

		//edgeTracerCompute.SetInt("_AdjacentVertBufferStride", maxAdjacentVertCount);
		//edgeTracerCompute.SetBuffer(_etkernel, "_AdjacentVertBuffer", )
		edgeTracerCompute.SetTexture(_etkernel, "_OutputTexToVfxGraph", _tempParticlePositions);
		edgeTracerCompute.SetTexture(_etkernel, "_VertexPositions", meshVertexPositionTex);

		edgeTracerCompute.SetVector("_Dimensions", new Vector4(_tempParticlePositions.width, _tempParticlePositions.height, 1f, _tempParticlePositions.width * _tempParticlePositions.height));
		edgeTracerCompute.SetVector("_InvDimensions", new Vector4(1f / _tempParticlePositions.width, 1f / _tempParticlePositions.height, 1f, 1f / (_tempParticlePositions.width * _tempParticlePositions.height)));
		edgeTracerCompute.SetFloat("_Deltatime", Time.deltaTime);

		particleSimBuffers[BufferTools.WRITE].SetCounterValue(0);
		edgeTracerCompute.SetBuffer(_etkernel, "_CurrentParticlePositions", particleSimBuffers[BufferTools.READ]);
		edgeTracerCompute.SetBuffer(_etkernel, "_ParticlePositionsInTransit", particleSimBuffers[BufferTools.WRITE]);
		edgeTracerCompute.SetBuffer(_etkernel, "_ParticlePositionsAtTarget", VertTraceCornerChecker.inst.cornersToCheck);

		int[] appargs = BufferTools.GetArgs(particleSimBuffers[BufferTools.READ], argsBuffer);

		Debug.Log("I have this many parts simming: " + appargs[0]);
		edgeTracerCompute.Dispatch(_etkernel, appargs[0], 1, 1);

		Graphics.CopyTexture(_tempParticlePositions, outputParticlePositionTex);
		vfxTarget.SetInt(numParticlesPropertyName, appargs[0]);
	}



	private void OnDestroy()
	{
		Smrvfx.Utility.TryDestroy(_tempParticlePositions);
		Smrvfx.Utility.TryDispose(particleSimBuffers[0]);
		Smrvfx.Utility.TryDispose(particleSimBuffers[1]);
		Smrvfx.Utility.TryDispose(argsBuffer);
	}
}
