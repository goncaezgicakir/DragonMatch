using UnityEngine;
using System.Collections;

public class ParticleManager : MonoBehaviour {

    public GameObject clearFXPrefab;
    public GameObject breakFXPrefab;
    public GameObject doubleBreakFXPrefab;

    //method to simulate clear game piece effect
    public void ClearPieceFXAt(int x, int y, int z=0)
    {

        //create the clear effect object
        GameObject clearFX = Instantiate(clearFXPrefab,new Vector3(x, y, z), Quaternion.identity) as GameObject;
        ParticlePlayer particlePlayer = clearFX.GetComponent<ParticlePlayer>();

        Debug.LogWarning("ClearFX -> x:" + x + " y: " + y);

        //play the effect
        if (particlePlayer != null)
        {
            particlePlayer.Play();
        }

    }


    //method to simulate break effect
    public void BreakPieceFXAt(int breakableValue, int x, int y, int z=0)
    {

        GameObject breakFX = null;
        ParticlePlayer particlePlayer = null;

        //if the tile breakable value greater than 1, it is doubleBreakable twice
        if (breakableValue > 1)
        {
            breakFX = Instantiate(doubleBreakFXPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
        }
        //if the tile breakable value less than 1, it is breaklable once
        else
        {
            breakFX = Instantiate(breakFXPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
        }

        if (breakFX != null)
        {
            particlePlayer = breakFX.GetComponent<ParticlePlayer>();

            //play the break effect
            if (particlePlayer != null)
            {
                particlePlayer.Play();
            }

        }
    }



}
