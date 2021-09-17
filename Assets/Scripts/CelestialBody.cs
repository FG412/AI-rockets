using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent (typeof (Rigidbody))]
public class CelestialBody : MonoBehaviour
{
    public bool isAPlanet;
    public float surfaceGravity;
    public float radius;
    public float mass;
    public float rotationSpeed;
    public float axisInclination;
    public Vector3 v0;
    private Vector3 force;

    private Vector3 readableForce;


    // Start is called before the first frame update
    void Start(){
        GetComponent<Rigidbody>().angularVelocity = new Vector3(0, this.rotationSpeed, 0);
    }
    void OnValidate() {
        #pragma warning disable
        GetComponent<Rigidbody>().mass = mass;
        if (!isAPlanet) {
            radius = 0;
            surfaceGravity = 0;
            //this.tag="celestial body";
        }
        if(isAPlanet) {
            //this.tag="planet";
            mass = (surfaceGravity*radius*radius)/Universe.gravitationalConstant;
            this.transform.localScale = new Vector3(radius *2, radius*2, radius*2);
            this.transform.rotation = Quaternion.Euler(0,0,axisInclination);
        }
        #pragma warning restore
    }
    void Awake()
    {
        GetComponent<Rigidbody>().AddForce(v0, ForceMode.VelocityChange);
        if (isAPlanet) {
            mass = (surfaceGravity*radius*radius)/Universe.gravitationalConstant;
        }
        GetComponent<Rigidbody>().mass = mass;
        force = Vector3.zero;

    }
    public void updateMotion(CelestialBody[] allBodies) {
        foreach (var item in allBodies)
        {
            if(item != this && item.tag.Equals("planet")){
                float sqrtDst = (item.GetComponent<Rigidbody>().position - GetComponent<Rigidbody>().position).sqrMagnitude;
                Vector3 forceDir = (item.GetComponent<Rigidbody>().position - GetComponent<Rigidbody>().position).normalized;
                force += forceDir * Universe.gravitationalConstant*mass*item.mass / sqrtDst;

            }
        }
        readableForce = force;
        GetComponent<Rigidbody>().AddForceAtPosition(force, GetComponent<Rigidbody>().worldCenterOfMass, ForceMode.Force);
        force=Vector3.zero;
    }

    public Vector3 getGravityForce() {
        return readableForce;
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "landingSite" && this.name != "Ship")
        {
            Physics.IgnoreCollision(collision.gameObject.GetComponent<Collider>(), this.GetComponentInChildren<Collider>());
        }
    }

    
}



