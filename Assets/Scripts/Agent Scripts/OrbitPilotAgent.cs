using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class OrbitPilotAgent : Agent
{

    private Rocket rocket;
    private float baseReward;
    private bool insideOrbit;
    private Vector3 startPosition;
    private Vector3 startRotation;
    private Vector3 startUpDirection;
    public float orbitAltitude;
    private float currentDistance;
    private float previousDistance;

    public override void Initialize()
    {
        rocket = this.GetComponent<Rocket>();
        baseReward = 1f/MaxStep;
    }

    public override void OnEpisodeBegin()
    {
        this.randomRelocateOnPlanet();
        //rocket.GetComponent<Rigidbody>().velocity=this.getTargetTangentVelocity(orbitAltitude);
        rocket.setIgnite(true);
        insideOrbit=false;
        startRotation = rocket.transform.rotation.eulerAngles;
        startUpDirection = new Vector3(transform.up.x, transform.up.y + 1, transform.up.z);
        rocket.GetComponent<Rigidbody>().freezeRotation=true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 rotation = startRotation - transform.rotation.eulerAngles;

        sensor.AddObservation(rocket.getAltitude());
        sensor.AddObservation(currentDistance);
        sensor.AddObservation(transform.InverseTransformPoint(startPosition - rocket.transform.position));
        sensor.AddObservation(rotation.y / 180);

        sensor.AddObservation(transform.InverseTransformDirection(rocket.getRocketAngularSpeed()));
        sensor.AddObservation(transform.InverseTransformDirection(rocket.getRocketSpeed()));
        sensor.AddObservation(transform.InverseTransformDirection(getTargetTangentVelocity(orbitAltitude) - rocket.getRocketSpeed()));
        sensor.AddObservation(Vector3.Dot(rocket.getRocketSpeed().normalized, getTargetTangentVelocity(orbitAltitude).normalized));
        sensor.AddObservation(startUpDirection - transform.up);
        sensor.AddObservation(rocket.getRocketMass());
        sensor.AddObservation(rocket.getEngineThrust());
        sensor.AddObservation(transform.InverseTransformDirection(rocket.getEngineForce() + rocket.getRocketForce()));
        sensor.AddObservation(insideOrbit);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        rocket.setEngineThrust(actions.DiscreteActions[0]);
        //rocket.setEngineX(actions.DiscreteActions[1]);
        rocket.setEngineZ(actions.DiscreteActions[1]);

        if (rocket.getAltitude() > 1f){
            rocket.GetComponent<Rigidbody>().freezeRotation=false;
        }
        if (rocket.getIsExploded() || rocket.getAltitude() > orbitAltitude + 50f) {
            //SetReward(-1);
            EndEpisode();
        }
        currentDistance = Mathf.Abs(orbitAltitude - rocket.getAltitude());

        if (rocket.getAltitude() >= (orbitAltitude - 8) && rocket.getAltitude() <= (orbitAltitude + 8)) {
            insideOrbit=true;
            float rewardSystem = Vector3.Dot(rocket.getRocketSpeed().normalized, getTargetTangentVelocity(orbitAltitude).normalized) / (Mathf.Abs(rocket.getRocketSpeed().magnitude - this.getTargetTangentVelocity(orbitAltitude).magnitude) + 1); 
            AddReward(rewardSystem * baseReward * 5);
            AddReward(-rocket.getEngineThrust() * baseReward * Mathf.Abs(rocket.getRocketAngularSpeed().magnitude) * 15f);
        }
        else {
            insideOrbit = false;

            if (currentDistance < previousDistance && !rocket.getIsLanded()) {
                AddReward(baseReward * 2f);
            }else{
                AddReward(-baseReward * 2f);
            }
            AddReward(-rocket.getEngineThrust() * baseReward * (rocket.getAltitude()* 2f/orbitAltitude));
            if (Vector3.Dot(rocket.getRocketSpeed().normalized, getTargetTangentVelocity(orbitAltitude).normalized) < 0) {
                AddReward(-baseReward);
            }
        }
        previousDistance = currentDistance;

        Debug.DrawRay(rocket.transform.position, rocket.getRocketSpeed(), Color.green, 0f);
        Debug.DrawRay(rocket.transform.position, this.getTargetTangentVelocity(orbitAltitude), Color.magenta, 0f);
        Debug.DrawRay(rocket.transform.position, rocket.getEngineForce(), Color.blue, 0f);

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.UpArrow)){
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.DownArrow)){
            discreteActionsOut[0] = 2;
        }
        if (Input.GetKey(KeyCode.W)){
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.S)){
            discreteActionsOut[1] = 2;
        }
        if (Input.GetKey(KeyCode.A)){
            //discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.D)){
            //discreteActionsOut[2] = 2;
        }
        if (Input.GetKey(KeyCode.P)){
            rocket.setTriggerLegsDeploy(true);
        }   
        
    }
    public Rocket getRocket(){
        return rocket;
    }
    public void randomRelocateOnPlanet() {
        rocket.restart();
        Vector2 randomPoinOnCircle = Random.insideUnitCircle.normalized ;
        Vector3 randomPoint = new Vector3(randomPoinOnCircle.x, 0, randomPoinOnCircle.y)* (rocket.startingPlanet.radius + 0.1f);
        this.transform.position = rocket.startingPlanet.transform.position + randomPoint;
        this.transform.rotation = getBaseRotation();
        rocket.transform.localEulerAngles = new Vector3(-180, rocket.transform.localEulerAngles.y, rocket.transform.localEulerAngles.z);
        if (rocket.transform.localEulerAngles.z > 100)
            rocket.transform.localEulerAngles = new Vector3(-180, rocket.transform.localEulerAngles.y, rocket.transform.localEulerAngles.z);
    }
    public Quaternion getBaseRotation() {
        //RaycastHit hit;
        Vector3 normal = (rocket.startingPlanet.transform.position - this.transform.position).normalized;

        return Quaternion.FromToRotation (new Vector3(0, -1, 0), normal);
    }
    private Vector3 getTargetTangentVelocity (float orbitRadius) {
        float tangentSpeed = Mathf.Sqrt((rocket.startingPlanet.mass * Universe.gravitationalConstant) / (orbitRadius + rocket.startingPlanet.radius));
        Vector3 tangentVelocity = Vector3.Cross(rocket.transform.position - rocket.startingPlanet.transform.position, Vector3.up).normalized * tangentSpeed;
        return tangentVelocity;
    }
    public bool getIsOrbiting() {
        return insideOrbit;
    }
}