using UnityEngine;
using System.IO;
using System.Collections.Generic;
using static UnityEngine.EventSystems.EventTrigger;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance; 

    private void Awake()
    {
        // ทำเป็น Singleton เพื่อให้เรียกใช้ง่ายๆ จากทุกที่
        if (Instance == null) 
            Instance = this; 
        else 
            Destroy(gameObject); 
    }

    // ฟังก์ชันสร้างชื่อไฟล์แยกตามชื่อที่ผู้เล่นตั้ง
    private string GetSavePath(string fileName)
    {
        // เช็คก่อนว่าชื่อที่ส่งมามี .json ต่อท้ายหรือยัง (ป้องกันการซ้ำซ้อน)
        if (!fileName.EndsWith(".json"))
        {
            fileName += ".json";
        }
        return Application.persistentDataPath + "/" + fileName;
    }

    // ----------------------------------------------------
    // ฟังก์ชัน Save (เปลี่ยนมารับค่าเป็น string ชื่อไฟล์แทน)
    // ----------------------------------------------------
    public void SaveGame(string fileName)
    {
        SaveData data = new SaveData(); 
        string saveFilePath = GetSavePath(fileName);

        // บันทึกเวลาปัจจุบันลงไปด้วย เผื่อเอาไปโชว์หน้า UI
        data.saveTime = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"); 

        // 1. กวาดหาของทุกชิ้นที่มี SaveableEntity แปะอยู่ในฉาก
        SaveableEntity[] allEntities = Object.FindObjectsByType<SaveableEntity>(FindObjectsSortMode.None); 

        // 2. ลูปเข้าไปดูทีละชิ้น
        foreach (SaveableEntity entity in allEntities) 
        {
            Isaveable saveable = entity.GetComponent<Isaveable>(); 

            // 3. ถ้าของชิ้นนั้นมีสคริปต์เซ็นสัญญา ISaveable อยู่ด้วย
            if (saveable != null) 
            {
                // ดึงรหัส ID และ ข้อมูล ยัดเก็บลงกล่อง SaveData
                data.savedObjectIDs.Add(saveable.GetSaveID()); 
                data.savedObjectStates.Add(saveable.SaveState()); 
            }
        }

        // 4. แปลงข้อมูลทั้งหมดเป็น JSON แล้วเขียนลงไฟล์
        string json = JsonUtility.ToJson(data, true); 
        File.WriteAllText(saveFilePath, json); 

        // 5. แชะภาพหน้าจอเซฟไว้คู่กัน โดยเปลี่ยนนามสกุลจาก .json เป็น .png
        string imagePath = saveFilePath.Replace(".json", ".png");
        ScreenCapture.CaptureScreenshot(imagePath); 

        Debug.Log($"<color=green>เซฟเกมในชื่อ {fileName} สำเร็จ!</color>\nพิกัดไฟล์: {saveFilePath}");
    }

    // ----------------------------------------------------
    // ฟังก์ชัน Load (เปลี่ยนมารับค่าเป็น string ชื่อไฟล์แทน)
    // ----------------------------------------------------
    public void LoadGame(string fileName)
    {
        string saveFilePath = GetSavePath(fileName);

        // เช็คก่อนว่าไฟล์นี้มีให้โหลดไหม
        if (!File.Exists(saveFilePath)) 
        {
            Debug.LogWarning("ไม่พบไฟล์เซฟชื่อ: " + fileName);
            return; 
        }

        // 1. อ่านไฟล์ JSON มาแปลงเป็นกล่อง SaveData
        string json = File.ReadAllText(saveFilePath); 
        SaveData data = JsonUtility.FromJson<SaveData>(json); 

        // 2. กวาดหาของทุกชิ้นที่มีอยู่ในฉากปัจจุบัน
        SaveableEntity[] allEntities = Object.FindObjectsByType<SaveableEntity>(FindObjectsSortMode.None); 

        // 3. ลูปเข้าไปดูทีละชิ้น
        foreach (SaveableEntity entity in allEntities) 
        {
            Isaveable saveable = entity.GetComponent<Isaveable>(); 

            if (saveable != null) 
            {
                // เอา ID ของของชิ้นนี้ ไปค้นหาในรายชื่อที่อยู่ในไฟล์เซฟ ว่าตรงกับลำดับ (Index) ที่เท่าไหร่
                int index = data.savedObjectIDs.IndexOf(saveable.GetSaveID()); 

                // ถ้าค่า Index เป็น -1 แปลว่าไม่มีรหัสนี้ในไฟล์เซฟ (ข้ามไป)
                // แต่ถ้าหาเจอ (Index >= 0)
                if (index != -1) 
                {
                    // โยนข้อมูลสถานะกลับไปให้ของชิ้นนั้นจัดการตัวเอง
                    saveable.LoadState(data.savedObjectStates[index]); 
                }
            }
        }

        Debug.Log($"<color=cyan>โหลดเกมจากไฟล์ {fileName} สำเร็จ!</color>");
    }
}