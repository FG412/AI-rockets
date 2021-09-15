using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PlanePilotAgent : Agent
{

    private Rocket rocket;
    private float currentDistance;
    private float previousDistance;
    private float baseReward;
    private bool inside;

    public GameObject target;

    public override void Initialize()
    {
        rocket = this.GetComponent<Rocket>();
        baseReward = 1f/MaxStep;
    }

    public override void OnEpisodeBegin()
    {
        rocket.restart();
        rocket.transform.localPosition=Vector3.zero;
        rocket.transform.rotation=Quaternion.Euler(0,0,0);
        rocket.setIgnite(true);
        inside=false;
        previousDistance = currentDistance = Mathf.Abs((rocket.transform.position - target.transform.position).magnitude);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rocket.transform.position.y);
        sensor.AddObservation(target.transform.position - rocket.transform.position);
        sensor.AddObservation(transform.rotation.x);
        sensor.AddObservation(transform.rotation.z);
        sensor.AddObservation(rocket.getRocketSpeed());
        sensor.AddObservation(rocket.getRocketAngularSpeed());
        sensor.AddObservation(transform.up);
        sensor.AddObservation(rocket.getRocketMass());
        sensor.AddObservation(rocket.getEngineForce());
        sensor.AddObservation(inside);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        rocket.setEngineThrust(actions.DiscreteActions[0]);
        rocket.setEngineX(actions.DiscreteActions[1]);
        rocket.setEngineZ(actions.DiscreteActions[2]);
        currentDistance = Mathf.Abs((rocket.transform.position - target.transform.position).magnitude);

        if (rocket.getIsExploded()) {
            SetReward(-1);
            EndEpisode();
        }

        if (currentDistance < previousDistance) {
            AddReward(baseReward);
        }else{
            AddReward(-0.75f * baseReward);
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
        if (collider.gameObject.GetInstanceID() == target.GetInstanceID())
            inside = true;
    }

    private void OnTriggerExit(Collider collider) {
        if (collider.gameObject.GetInstanceID() == target.GetInstanceID())
            inside = false;
    }

    private void OnTriggerStay(Collider collider) {
        if (collider.gameObject.GetInstanceID() == target.GetInstanceID())
            AddReward(0.1f);
    }

    public Rocket getRocket(){
        return rocket;
    }
}
