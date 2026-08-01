using UnityEngine;

public class LightZone : MonoBehaviour
{
    public bool lightZoneState = true;
    public light_switch masterSwitch;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TryGetComponent<light_switch>(out masterSwitch);
    }

    // Update is called once per frame
    void Update()
    {
        //lightZoneState = masterSwitch.lightSW_Status;
    }

   

    

}
