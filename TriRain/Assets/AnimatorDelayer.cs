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
			{
				Time.captureFramerate = 60;
				return (int)(secondsToDelay * 60);
			}
		}
	}

	private void Awake()
	{
		if (Time.captureDeltaTime == 0f)
			Time.captureFramerate = 60;
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
			Debug.Log("resuming at Time = " + Time.time + " seconds");
			Resume();
		}
    }
}
