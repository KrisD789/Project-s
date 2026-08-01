using UnityEngine;

public class light_switch : MonoBehaviour, Isaveable
{
    public Light light;
    public LightZone lightZone_script;
    public bool lightSW_Status = true;

    private SaveableEntity saveEntity;
    void Awake()
    {
        // 1. สั่งให้ดึงสคริปต์ SaveableEntity ที่แปะอยู่บนตัวมันเองมาเก็บไว้
        saveEntity = GetComponent<SaveableEntity>();
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

    public string GetSaveID()
    {
        return saveEntity.uniqueID;
    }

    public string SaveState()
    {
        return lightSW_Status.ToString();
    }

    public void LoadState(string stateData)
    {
        lightSW_Status = bool.Parse(stateData);

        // 3. สั่งให้หลอดไฟและโซนแสงในเกม ปรับสถานะตามข้อมูลที่โหลดมา!
        light.enabled = lightSW_Status;
        if (lightZone_script != null)
        {
            lightZone_script.lightZoneState = lightSW_Status;
        }
    }
}
