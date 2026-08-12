using UnityEngine;

public class light_switch : MonoBehaviour
{
    public Light light;
    public LightZone lightZone_script;
    public bool lightSW_Status = true;


    private void Start()
    {
        ApplyLightState();
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
    public void ApplyLightState()
    {
        if (lightZone_script != null)
        {
            lightZone_script.lightZoneState = lightSW_Status;
        }

        if (light != null)
        {
            light.enabled = lightSW_Status;
        }
    }

}
