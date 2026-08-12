using UnityEngine;
using System.IO;
using System.Collections.Generic;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    // เพิ่ม Event สำหรับแจ้งเตือนตอนโหลดเกมเสร็จ
    public event System.Action OnGameLoaded;
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
        if (!TryToSaveGame())
        {
            return;
        }

        SaveData data = new SaveData(); 
        string saveFilePath = GetSavePath(fileName);

        // บันทึกเวลาปัจจุบันลงไปด้วย เผื่อเอาไปโชว์หน้า UI
        data.saveTime = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        data.currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

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



    // ตัวแปรสำหรับพักข้อมูลไว้ชั่วคราวระหว่างรอฉากโหลด
    private SaveData pendingLoadData;

    // ----------------------------------------------------
    // จังหวะที่ 1: อ่านไฟล์ และสั่งโหลดด่าน
    // ----------------------------------------------------
    public void LoadGame(string fileName)
    {
        string saveFilePath = GetSavePath(fileName);
        if (!File.Exists(saveFilePath)) return;

        // อ่าน JSON มาเก็บพักไว้ในตัวแปรนี้ก่อน
        string json = File.ReadAllText(saveFilePath);
        pendingLoadData = JsonUtility.FromJson<SaveData>(json);

        string currentScene = SceneManager.GetActiveScene().name;

        // ล้างคิวตั๋วเก่าของ Combat Manager ทิ้งให้หมดก่อนเริ่มโหลดข้อมูลใหม่
        if (Enemy_combatManager.Instance != null)
        {
            Enemy_combatManager.Instance.ClearAllTokens();
        }

        // เช็คว่าผู้เล่นอยู่ด่านเดียวกับในเซฟไหม?
        if (!string.IsNullOrEmpty(pendingLoadData.currentSceneName) && pendingLoadData.currentSceneName != currentScene)
        {
            // ถ้าอยู่คนละด่าน ให้สมัครรับข่าว (Event) ไว้ว่า "ถ้าด่านโหลดเสร็จ ให้รันฟังก์ชัน OnSceneLoaded นะ"
            SceneManager.sceneLoaded += OnSceneLoaded;

            // สั่งโหลดด่านใหม่
            SceneManager.LoadScene(pendingLoadData.currentSceneName);
        }
        else
        {
            // ถ้าอยู่ด่านเดียวกันอยู่แล้ว ไม่ต้องโหลดใหม่ ให้คืนค่าสถานะได้เลยทันที
            ApplySaveData(pendingLoadData);
        }
    }

    // ----------------------------------------------------
    // จังหวะที่ 2: ด่านโหลดเสร็จ 100% แล้วค่อยคืนค่า
    // ----------------------------------------------------
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("ด่านใหม่โหลดเสร็จเรียบร้อย!");

        // สำคัญมาก: ยกเลิกการรับข่าว เพื่อไม่ให้มันทำงานซ้ำตอนเปลี่ยนด่านครั้งหน้า
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // สั่งเอาข้อมูลที่พักไว้ มาสวมให้สิ่งของในด่านใหม่
        ApplySaveData(pendingLoadData);
    }

    // ----------------------------------------------------
    // ฟังก์ชันย่อยสำหรับเขียนข้อมูลทับ (รวบยอดมาจากของเดิม)
    // ----------------------------------------------------
    private void ApplySaveData(SaveData data)
    {
        SaveableEntity[] allEntities = Object.FindObjectsByType<SaveableEntity>(FindObjectsSortMode.None);

        foreach (SaveableEntity entity in allEntities)
        {
            Isaveable saveable = entity.GetComponent<Isaveable>();
            if (saveable != null)
            {
                int index = data.savedObjectIDs.IndexOf(saveable.GetSaveID());
                if (index != -1)
                {
                    saveable.LoadState(data.savedObjectStates[index]);
                }
            }
        }
        Debug.Log("โหลดข้อมูลสิ่งของสำเร็จ!");

        // ประกาศข่าวให้ UI และระบบอื่นๆ รู้ว่าโหลดข้อมูลเสร็จหมดแล้ว!
        OnGameLoaded?.Invoke();
    }
    public bool TryToSaveGame()
    {
        if (Player.Instance.currentState == Player.PlayerState.CarryingBody ||
            Player.Instance.currentState == Player.PlayerState.GrabbingEnemy)
        {
            Debug.LogWarning("บันทึกเกมล้มเหลว: ไม่สามารถเซฟได้ในขณะที่กำลังแบกศพหรือล็อคคอศัตรู!");
            // TODO: สั่งโชว์ข้อความ UI สีแดงเตือนผู้เล่นบนหน้าจอตรงนี้

            return false; // ไม่ให้ผ่าน!
        }

        Debug.Log("สถานะปลอดภัย อนุญาตให้เซฟเกมได้...");
        return true; // ให้ผ่านได้!
    }
}