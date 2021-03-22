using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;

public class GlobalTextureSetter : MonoBehaviour
{
	public Texture tex;
    // Start is called before the first frame update
    void Start()
    {
        
    }


	string lastTexName;
	string lastTexFileName;
    // Update is called once per frame
    void Update()
    {
		
		Volume volum = this.GetComponent<Volume>();
		if (volum != null)
		{
			SpaceFog spacefog;
			if(	volum.sharedProfile.TryGet<SpaceFog>(out spacefog))
			{
				spacefog.NoiseTexture = tex;

				if(spacefog.NoiseTexture == null)
					Debug.Log("am I null?");
				else
					Debug.Log("defo not?");

			}
			else
				Debug.LogError("N O SPACEFGOG");
		}
		else
			Debug.LogError("N O VOLOOM");
    }
}
