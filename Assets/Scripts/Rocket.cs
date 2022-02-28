using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class Rocket : MonoBehaviour
{
    public float maxRocketThrust;
    public float startFuel;
    public float fuelPerLiterWeight;
    public float maxConsuption;
    private bool ignite;
    private Vector3 force;
    private float consumptionPerTimeStep;
    private float xTeta;
    private float zTeta;
    public float xMaxDegRotation;
    public float zMaxDegRotation;
    private float currentThrust;
    public float explosionThreshold;
    public float angleThreshold;
    private float currentConsuption;
    private bool isExploded;
    private bool isLanded;
    private Vector3 readableForce;
    private float inpactVelocity;
    public float dryMass;
    private float wetMass;
    private Rigidbody mainStage;
    public GameObject[] legs;
    private bool triggerLegsDeploy;

    public ParticleSystem fireSmall;
    public ParticleSystem embers;
    public ParticleSystem smokeEffect;
    public CelestialBody startingPlanet;
    private ParticleSystem.MainModule fire;
    private ParticleSystem.MainModule emb;
    private ParticleSystem.MainModule smoke;
    private float fuel;

    void Awake() {
        
        mainStage = this.GetComponent<Rigidbody>();
        fire=fireSmall.main;
        emb = embers.main;
        smoke = smokeEffect.main;
    }
    void Start() //Z VELOCITY 8.79
    {   
        
        inpactVelocity = 0f;
        readableForce = Vector3.zero; 
        xTeta = 0;
        zTeta = 0;
        ignite = false;
        fuel = startFuel;
        wetMass = dryMass + (fuel * fuelPerLiterWeight);
        mainStage.mass = wetMass;
        
        isExploded = false;
        currentConsuption = maxConsuption * 0;
        consumptionPerTimeStep = currentConsuption * Time.fixedDeltaTime;
        currentThrust = 0;
        force = Vector3.zero;

        mainStage.velocity = Vector3.zero;
        mainStage.angularVelocity = Vector3.zero;
        legs[0].transform.localRotation = Quaternion.Euler(0,0,0);
        legs[1].transform.localRotation = Quaternion.Euler(0,0,270);
        legs[2].transform.localRotation = Quaternion.Euler(0,0,180);
        legs[3].transform.localRotation = Quaternion.Euler(0,0,90);
    }
    void FixedUpdate()
    {
        force = Vector3.zero;
        if (ignite && fuel - consumptionPerTimeStep > 0) {
            
            //ROCKET THRUST
            force = new Vector3(currentThrust * Mathf.Cos((90 -xTeta)*Mathf.PI / 180), 
            (currentThrust * Mathf.Sin((90 -xTeta)*Mathf.PI / 180) + currentThrust * Mathf.Sin((90 -zTeta)*Mathf.PI / 180)) / 2, 
            currentThrust * Mathf.Cos((90 -zTeta)*Mathf.PI / 180));

            readableForce = transform.TransformDirection(force);
            GetComponent<Rigidbody>().AddForceAtPosition(readableForce, transform.GetChild(0).position);
            
            fuel -= consumptionPerTimeStep;

            this.GetComponent<Rigidbody>().mass = this.GetComponent<Rigidbody>().mass - consumptionPerTimeStep;
        }

        if (triggerLegsDeploy) {
            this.deployLegs();
        }
        if (UnityEditor.TransformUtils.GetInspectorRotation(legs[3].transform).x < -130) {
            triggerLegsDeploy = false;
        }
    }

    public void deployLegs() {
        for (int i = 0; i < legs.Length; i++){
            legs[i].transform.RotateAround(legs[i].transform.position, legs[i].transform.up, 200 * Time.fixedDeltaTime); 
            legs[i].GetComponent<MeshCollider>().enabled=true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != 3) {
            isLanded = true;
            if (collision.relativeVelocity.magnitude > explosionThreshold) {
                inpactVelocity = collision.relativeVelocity.magnitude;
                this.isExploded=true;
            }
            //VERIFYING ANGLE IMPACT
            else{
                Vector3 normal = (transform.position - startingPlanet.transform.position).normalized;
                Vector3 rocketRay = this.transform.up;              
                if (Vector3.Angle(normal, rocketRay) > angleThreshold){
                    this.isExploded = true;

                }
            }
        }
    }
    void OnCollisionStay(Collision collision) {
        if (collision.gameObject.layer != 3) {
            isLanded = true;
            Vector3 normal = (transform.position - startingPlanet.transform.position).normalized;

            Vector3 rocketRay = this.transform.up;
            if (Vector3.Angle(normal, rocketRay) > angleThreshold){
                this.isExploded = true;

            }
        }
    }
    void OnCollisionExit(Collision collision) {
        isLanded = false;
    }
    

    public Vector3 getEngineForce() {
        return readableForce;
    }

    public Vector3 getEngineAcceleration() {
        return this.getEngineForce() / GetComponent<Rigidbody>().mass; 
    }
    public Vector3 getRocketForce() {
        return GetComponent<CelestialBody>().getGravityForce();
    }

    public Vector3 getRocketAcceleration() {
        return this.getRocketForce() / GetComponent<Rigidbody>().mass; 
    }

    public Vector3 getRocketSpeed(){
        return GetComponent<Rigidbody>().velocity;
    }

    public Vector3 getRocketAngularSpeed(){
        return GetComponent<Rigidbody>().angularVelocity;
    }
    
    public float getFuel() {
        return fuel;
    }

    public float getAltitude(){
        return Vector3.Distance(startingPlanet.GetComponent<Rigidbody>().position, GetComponent<Rigidbody>().position) - startingPlanet.transform.localScale.x / 2 * this.transform.localScale.x;
    }

    public bool getIsIgnited() {
        return ignite;
    }

    public float getEngineX(){
        return xTeta/xMaxDegRotation;
    }

    public float getEngineZ() {
        return zTeta/zMaxDegRotation;
    }

    public void setEngineX(int alpha) {
        switch(alpha) {
            case 1:
                if (xTeta < xMaxDegRotation)
                    xTeta += xMaxDegRotation/40;

                break;
            case 2:
                if (xTeta > -xMaxDegRotation)
                    xTeta -= xMaxDegRotation/40;
                break;
            case 3:
                xTeta=0;
                break;
        }    
    }
    public void setEngineZ(int alpha) {
        switch(alpha) {
            case 1:
                if (zTeta < zMaxDegRotation)
                    zTeta += zMaxDegRotation/40;

                break;
            case 2:
                if (zTeta > -zMaxDegRotation)
                    zTeta -= zMaxDegRotation/40;
                break;
            case 3:
                xTeta=0;
                break;
        }
    }
    public void setFuelPercentage(int percentage){
        fuel = fuel * (percentage/100f);
        wetMass = dryMass + (fuel * fuelPerLiterWeight);
        mainStage.mass = wetMass;
    }

    public bool getIsExploded() {
        return isExploded;
    }


    public void setIgnite(bool condition) {
        ignite = condition;
    }

    public void setEngineThrust(int acc) {

        switch(acc){
            case 1:
                if (currentThrust < maxRocketThrust) {
                    currentThrust += maxRocketThrust / 40;
                    currentConsuption += maxConsuption / 40;
                }

                break;
            case 2:
                if (currentThrust > 0) {
                    currentThrust -= maxRocketThrust /40;
                    currentConsuption -= maxConsuption / 40;
                }

                break;
            case 3:
                currentThrust=0;
                currentConsuption=0;
                break;
        }
        
        if (getEngineThrust() > 0) {
            fire.loop=true;
            emb.loop=true;
            smoke.loop=true;
        }
        else {
            fire.loop=false;
            emb.loop=false;
            smoke.loop=false;
        }
        
        consumptionPerTimeStep = currentConsuption * Time.fixedDeltaTime;
    }

    public float getEngineThrust() {
        return currentThrust / maxRocketThrust;
    } 

    public float getInpactVelocity() {
        return this.inpactVelocity;
    }
    
    public void restart() {
        this.Start();
    }

    public bool getIsLanded() {
        return this.isLanded;
    }

    public bool getTriggerLegsDeploy() {
        return this.triggerLegsDeploy;
    }

    public void setTriggerLegsDeploy(bool condition) {
        if (!this.triggerLegsDeploy)
            this.triggerLegsDeploy = condition;
    }

    public float getRocketMass(){
        return this.GetComponent<Rigidbody>().mass / wetMass;
    }

}
