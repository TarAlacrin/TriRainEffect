using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class AudioSourcePlaybackManager : MonoBehaviour
{
	[SerializeField] private AudioSource src;

    // Start is called before the first frame update
    void Start()
    {
		    
    }

    // Update is called once per frame
    void Update()
    {
		//src.time = Time.time;
    }


	private void LateUpdate()
	{
	}
}
