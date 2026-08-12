using UnityEngine;

public class LightZone : MonoBehaviour
{
    public bool lightZoneState = true;
    public light_switch masterSwitch;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //if(TryGetComponent<light_switch>(out masterSwitch)) Debug.LogWarning("lightZone:  หา Light_Switch ไม่เจอ!"); ;
    }

    // Update is called once per frame
    void Update()
    {
        //lightZoneState = masterSwitch.lightSW_Status;
    }

   

    

}
