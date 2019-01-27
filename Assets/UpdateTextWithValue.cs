using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateTextWithValue : MonoBehaviour
{
    public Text valueText;
    private Slider slider;
    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(delegate { SliderValueUpdated(); });
        SliderValueUpdated();
    }

    public void SliderValueUpdated(){
        valueText.text = slider.value.ToString("F2");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
