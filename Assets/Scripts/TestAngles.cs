using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TestAngles : MonoBehaviour
{

    public Rocket rocket;
    public Transform plane;
    // Start is called before the first frame update
    void Start()
    {
        plane.position = new Vector3(0,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        
        plane.LookAt(rocket.transform, -Vector3.up);
        Debug.Log("ANGLE X " + Vector3.Angle(rocket.transform.up, (plane.right)));
        Debug.Log("ANGLE Y " + Vector3.Angle(rocket.transform.up, (rocket.transform.position - rocket.startingPlanet.transform.position)));
        //Debug.Log(getBaseRotation().eulerAngles - rocket.transform.localRotation.eulerAngles);
        Debug.DrawRay(rocket.transform.position, -rocket.transform.up * 100f, Color.green, 0f);
        Debug.DrawRay(rocket.transform.position, -(rocket.transform.position - rocket.startingPlanet.transform.position), Color.red, 0f);
        Debug.DrawRay(rocket.transform.position, plane.right * 100f, Color.magenta, 0f);
        //Debug.DrawRay(rocket.transform.position, rocket.getEngineForce(), Color.blue, 0f);
    }
    public Quaternion getBaseRotation() {
        //RaycastHit hit;
        Vector3 normal = (rocket.startingPlanet.transform.position - this.transform.position).normalized;

        return Quaternion.FromToRotation (new Vector3(0, -1, 0), normal);
    }  

}
