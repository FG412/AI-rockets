using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.MLAgents;
public class UIManager : MonoBehaviour
{
    // Start is called before the first frame update


    public Rocket rocket;

    private TMP_Text[] texts; 
    private Agent agent;
    void Start()
    {
        texts = this.GetComponentsInChildren<TMP_Text>();
        agent = rocket.GetComponent<Agent>();
    }

    // Update is called once per frame
    void Update()
    {
        texts[0].text="Thrust: " + (rocket.getEngineThrust() * 100).ToString("F3") + "%";
        texts[1].text="Velocity X: " + rocket.getRocketSpeed().x.ToString("F3");
        texts[2].text="Velocity Y: " + rocket.getRocketSpeed().y.ToString("F3");
        texts[3].text="Velocity Z: " + rocket.getRocketSpeed().z.ToString("F3");
        texts[4].text="Reward: " + agent.GetCumulativeReward().ToString("F3");
        texts[5].text="EngineX: " + rocket.getEngineX().ToString("F3");
        texts[6].text="EngineZ: " + rocket.getEngineZ().ToString("F3");
        texts[7].text="Altitude: " + rocket.getAltitude().ToString("F3");
        //texts[8].text="Orbiting: " + agent.getIsOrbiting();
        
    }
}
