using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class SphereLandingAgent : Agent
{
    private Rocket rocket;
    private float currentDistance;
    private float previousDistance;
    private float baseReward;
    private bool inside;
    private Vector3 startPosition;
    private Vector3 startRotation;
    public GameObject target;
    public Transform tangentPlane;
    public override void Initialize()
    {
        rocket = this.GetComponent<Rocket>();
        baseReward = 1f/MaxStep;
        tangentPlane.position = new Vector3(0,0,0);
    }

    public override void OnEpisodeBegin()
    {
        
        this.randomRelocateOnPlanet(Academy.Instance.EnvironmentParameters.GetWithDefault("landing_randomization", 2f));

        rocket.setIgnite(true);
        inside=false;
        previousDistance = currentDistance = Mathf.Abs((rocket.transform.position - target.transform.position).magnitude);
        startPosition = rocket.transform.position;
        startRotation = getBaseRotation().eulerAngles;
        rocket.setTriggerLegsDeploy(true);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rocket.getAltitude());
        sensor.AddObservation(transform.InverseTransformDirection(target.transform.position - rocket.transform.position));
        sensor.AddObservation(transform.InverseTransformDirection(rocket.getRocketSpeed()));
        sensor.AddObservation(transform.InverseTransformDirection(rocket.getRocketAngularSpeed()));
        sensor.AddObservation(Vector3.Angle(rocket.transform.up, (tangentPlane.right)));
        sensor.AddObservation(Vector3.Angle(rocket.transform.up, (rocket.transform.position - rocket.startingPlanet.transform.position)));
        sensor.AddObservation(rocket.getRocketMass());
        sensor.AddObservation(transform.InverseTransformDirection(rocket.getEngineForce()));
        sensor.AddObservation(transform.InverseTransformDirection(rocket.getRocketForce()));
        sensor.AddObservation(inside);
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        rocket.setEngineThrust(actions.DiscreteActions[0]);
        rocket.setEngineX(actions.DiscreteActions[1]);
        rocket.setEngineZ(actions.DiscreteActions[2]);
        currentDistance = Mathf.Abs((rocket.transform.position - target.transform.position).magnitude);

        if (rocket.getIsExploded()|| currentDistance > 100f) {
            AddReward(-1f);
            EndEpisode();
        }

        if (currentDistance < previousDistance && !rocket.getIsLanded()) {
            AddReward(baseReward);
        }else{
            AddReward(-0.75f * baseReward);
        }

        if (rocket.getIsLanded() && inside){
            AddReward(baseReward * 5);
        }
        previousDistance = currentDistance;

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
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.D)){
            discreteActionsOut[2] = 2;
        }
        if (Input.GetKey(KeyCode.P)){
            rocket.setTriggerLegsDeploy(true);
        }   
        
    }

    
    private void OnTriggerEnter(Collider collider){
        if (collider.gameObject.GetInstanceID() == target.gameObject.GetInstanceID())
            inside = true;
    }

    private void OnTriggerExit(Collider collider) {
        if (collider.gameObject.GetInstanceID() == target.gameObject.GetInstanceID())
            inside = false;
    }

    public Rocket getRocket(){
        return rocket;
    }
    public void randomRelocateOnPlanet(float randomness) { //randomize rotation, initial speed, initial angular speed, mass 
        rocket.restart();
        
        //randomized mass/fuel
        rocket.setFuelPercentage(Random.Range(80, 100));

        //randomize rocket position
        Vector3 randomPoint = Random.onUnitSphere;
        Vector3 randomStartingPoint = new Vector3(randomPoint.x, randomPoint.y,  randomPoint.z) * (rocket.startingPlanet.radius + 10f + (randomness * 12f));
        this.transform.position = rocket.startingPlanet.transform.position + randomStartingPoint;
        this.transform.rotation = getBaseRotation();
        this.transform.rotation = Quaternion.Euler(this.transform.rotation.eulerAngles.x + Random.Range(-randomness,randomness), this.transform.rotation.eulerAngles.y + Random.Range(-randomness,randomness), this.transform.rotation.eulerAngles.z + Random.Range(-randomness,randomness));
        
        //randomize target position
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Ground");
        if (Physics.Raycast(rocket.transform.position, -rocket.transform.up, out hit, mask))
        {
            if (hit.rigidbody != null)
            {
                target.transform.position = hit.point;
            }
        }        
        //randomize initial rocket speed
        rocket.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f)) * randomness;
        
        //randomize initial angular speed
        rocket.GetComponent<Rigidbody>().angularVelocity = new Vector3(Random.Range(-0.006f, 0.006f), Random.Range(-0.006f, 0.006f), Random.Range(-0.006f, 0.006f)) * randomness;
    }
    public Quaternion getBaseRotation() {
        //RaycastHit hit;
        Vector3 normal = (rocket.startingPlanet.transform.position - this.transform.position).normalized;

        return Quaternion.FromToRotation (new Vector3(0, -1, 0), normal);
    }

    void FixedUpdate(){
        tangentPlane.LookAt(rocket.transform, -Vector3.up);
    }
}
