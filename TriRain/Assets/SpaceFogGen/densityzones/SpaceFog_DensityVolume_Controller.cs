using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.PostProcessing;

public class SpaceFog_DensityVolume_Controller : MonoBehaviour
{
    DensityVolume dv;
    public RenderTexture linkedRenderTexture;


    // Start is called before the first frame update
    void Start()
    {        
        dv = this.gameObject.GetComponent<DensityVolume>();
        dv.parameters.volumeMask = linkedRenderTexture;
        //dv.parameters.volumeMask = linkedRenderTexture;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
