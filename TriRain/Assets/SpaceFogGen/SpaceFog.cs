using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/SpaceFogShader")]
public sealed class SpaceFog : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

	[Tooltip("Controls the size of the raymarch step.")]
	public ClampedFloatParameter raymarchStep = new ClampedFloatParameter(0.1f, 0f, 1f);

	[Tooltip("Noise texture for nebula.")]
	public TextureParameter noiseTexture = new TextureParameter(null, false);

	Material m_Material;

    public bool IsActive() => m_Material != null && intensity.value > 0f;

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > HDRP Default Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    const string kShaderName = "Hidden/Shader/SpaceFogShader";

    public override void Setup()
    {
        if (Shader.Find(kShaderName) != null)
            m_Material = new Material(Shader.Find(kShaderName));
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume SpaceFog is unable to load.");
    }

	public Matrix4x4 GetInvVP(Camera camera)
	{
		Matrix4x4 V = camera.worldToCameraMatrix;
		Matrix4x4 P = camera.projectionMatrix;

		bool d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;

		if(d3d)
		{
			// Scale and bias from OpenGL -> D3D depth range
			for (int i = 0; i < 4; i++) { P[2, i] = P[2, i] * 0.5f + P[3, i] * 0.5f; }
		}

		return (V*P).inverse;
	}


	public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;
		m_Material.SetFloat("_Intensity", intensity.value);
		m_Material.SetFloat("_RayMarchStepSize", raymarchStep.value);

        m_Material.SetTexture("_InputTexture", source);
		m_Material.SetTexture("_NoiseTexture", noiseTexture.value);
		
		m_Material.SetVector("_WSCameraForward", camera.camera.transform.forward);

		float verticalScreenFOVFactor = Mathf.Tan(Mathf.Deg2Rad * camera.camera.fieldOfView * 0.5f);
		m_Material.SetVector("_WSScreenVerticalDirection", camera.camera.transform.up*verticalScreenFOVFactor);
		m_Material.SetVector("_WSScreenHorizontDirection", camera.camera.transform.right*verticalScreenFOVFactor*camera.camera.aspect);

		camera.camera.depthTextureMode = DepthTextureMode.Depth;

		HDUtils.DrawFullScreen(cmd, m_Material, destination);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
}
