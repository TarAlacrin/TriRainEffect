using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshEdgeParticleTracer : MonoBehaviour
{
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
    }


	//STRETCH GOAL
	//Have TriggerEventOnDie create a particle for a single frame which uses a shader that adds something to a buffer (this is a way to get the info of when a 
    //VSPro_HDIndirect might have the path forward towards getting a custom function which writes things to an append buffer. Itll be a wild ride though
    void Update()
    {
        
    }
}
