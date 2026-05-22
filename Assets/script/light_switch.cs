using UnityEngine;

public class light_switch : MonoBehaviour
{
    public Light light;
    public LightZone lightZone_script;
    public bool lightSW_Status = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Turn() 
    { 
        lightSW_Status = !lightSW_Status;
        if (lightSW_Status == true)
        {
            lightZone_script.lightZoneState = lightSW_Status;
            light.enabled = lightSW_Status;
        }
        else 
        {
            lightZone_script.lightZoneState = lightSW_Status;
            light.enabled = lightSW_Status; 
        }
        print(lightSW_Status ? "Turn-ON" : "Turn-OFF");
    }
}
