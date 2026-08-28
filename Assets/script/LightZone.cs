using UnityEngine;

public class LightZone : MonoBehaviour
{
    public bool lightZoneState = true;
    public light_switch masterSwitch;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!masterSwitch.TryGetComponent<light_switch>(out masterSwitch)) Debug.LogWarning("lightZone:  หา Light_Switch ไม่เจอ!"); 

        else {
            Debug.Log("หา Light_Switch ไม่เจอ! ไม่เป็นไรเพราะอันนี้คือ หรอดไปที่ไม่ต้องมีสวิต");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //lightZoneState = masterSwitch.lightSW_Status;
    }

   

    

}
