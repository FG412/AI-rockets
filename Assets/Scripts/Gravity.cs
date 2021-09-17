using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Gravity : MonoBehaviour
{
    CelestialBody[] cb;   
    void Awake(){
        cb = FindObjectsOfType<CelestialBody>();
    }

    
    void FixedUpdate () {
        for (int i = 0; i < cb.Length; i++) {
            cb[i].updateMotion(cb);
        }
    }
}
