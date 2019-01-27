using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AudioAgentManager : MonoBehaviour
{

    public Slider PreyRate;
    public Slider PredatorRate;
    public Slider PreySpeed;
    public Slider PredatorSpeed;

    private List<AudioAgent> audioAgents;

    void GetAudioAgents(){
        audioAgents = new List<AudioAgent>();

        AudioAgent[] agents = GameObject.FindObjectsOfType<AudioAgent>();
        for (int i = 0; i < agents.Length; i++){
            audioAgents.Add(agents[i]);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        PreyRate.onValueChanged.AddListener(delegate { SliderValueUpdated(); });
        PredatorRate.onValueChanged.AddListener(delegate { SliderValueUpdated(); });
        PreySpeed.onValueChanged.AddListener(delegate { SliderValueUpdated(); });
        PredatorSpeed.onValueChanged.AddListener(delegate { SliderValueUpdated(); });
        GetAudioAgents();

    }
    public void SliderValueUpdated()
    {
        foreach(AudioAgent agent in audioAgents){

            if(agent.isAgentFood){
                agent.updateFrequencyInSeconds = PreyRate.value;
                agent.stepSpeed = PreySpeed.value;
            } else {
                agent.updateFrequencyInSeconds = PredatorRate.value;
                agent.stepSpeed = PredatorSpeed.value;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
