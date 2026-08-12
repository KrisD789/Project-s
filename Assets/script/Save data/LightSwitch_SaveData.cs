using System.Data;
using UnityEngine;

public class LightSwitch_SaveData : MonoBehaviour, Isaveable
{
    private SaveableEntity saveEntity;

    public light_switch light_switch_Script;
    //public bool Switch_Status;

    void Awake()
    {
        // 1. สั่งให้ดึงสคริปต์ SaveableEntity ที่แปะอยู่บนตัวมันเองมาเก็บไว้
        if (!TryGetComponent<SaveableEntity>(out saveEntity))
            Debug.Log("!!!!!! LightSwitch_SaveData หา --SaveableEntity-- ไม่เจอ !!!!!!");

        if (!TryGetComponent<light_switch>(out light_switch_Script)) 
            Debug.Log("!!!!!! LightSwitch_SaveData หา --light_switch_Script-- ไม่ใช่ !!!!!!");
    }

    public string GetSaveID()
    {
        return saveEntity.uniqueID;
    }

    public string SaveState()
    {
        // เนื่องจากสถานะไฟเป็นแค่ bool (True/False) เราสามารถแปลงเป็นข้อความ (String) ตรงๆ ได้เลย ไม่ต้องใช้ JsonUtility ครับ
        return light_switch_Script.lightSW_Status.ToString();
    }

    // ตอนโหลดคืนค่า
    public void LoadState(string stateData)
    {
        if (light_switch_Script != null)
        {
            // ใช้ bool.Parse เพื่อแปลงคำว่า "True" หรือ "False" ที่เซฟไว้ กลับมาเป็นสถานะ bool ครับ
            light_switch_Script.lightSW_Status = bool.Parse(stateData);

            // สั่งให้อัปเดตแสงไฟและโซนในฉากทันที
            light_switch_Script.ApplyLightState();
        }
    }
}
