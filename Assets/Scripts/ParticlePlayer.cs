using UnityEngine;
using System.Collections;

public class ParticlePlayer : MonoBehaviour {


    public ParticleSystem[] allParticles;
    public float lifetime = 1f;

	void Start () {
	    
        //get all particle components
        allParticles = GetComponentsInChildren<ParticleSystem>();
        //destroy the object after use
        Destroy(gameObject, lifetime);
	}
	

    public void Play()
    {

        foreach (ParticleSystem ps in allParticles)
        {   
            //as a pressing 'stop' button for particle in unity gui
            ps.Stop();
            //as a pressing 'simulate' button for particle in unity gui
            ps.Play();
        }

    }
}
