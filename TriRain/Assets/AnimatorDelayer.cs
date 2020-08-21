using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorDelayer : MonoBehaviour
{
	[SerializeField] Animator todelay;
	[SerializeField] float secondsToDelay = 5f;

	[HideInInspector]
	public int framesToDelay {
		get
		{
			if(Time.captureDeltaTime != 0f)
				return (int)(secondsToDelay * (float)Time.captureFramerate);
			else
				return (int)(secondsToDelay * 60f);
		}
	}
    // Start is called before the first frame update
    void Start()
    {
		todelay.speed = 0;
    }

	void Resume()
	{
		todelay.speed = 1;
	}

    // Update is called once per frame
    void Update()
    {
        if(Time.frameCount == framesToDelay)
		{
			Resume();
		}
    }
}
