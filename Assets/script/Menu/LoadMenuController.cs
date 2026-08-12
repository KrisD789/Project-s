using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class LoadMenuController : MonoBehaviour
{
    [Header("ตั้งค่าการสร้างปุ่ม (ฝั่งขวา)")]
    public GameObject slotPrefab; 
    public Transform slotsContainer; 

    [Header("หน้าต่างรายละเอียด (ฝั่งซ้าย)")]
    public TextMeshProUGUI detailFileNameText;
    public TextMeshProUGUI detailFile_SceneNameText;
    public TextMeshProUGUI detailSaveTimeText;
    public RawImage detailScreenshotImage; // เพิ่มตัวแปรกรอบรูปภาพ (ใช้ Raw Image)

    [Header("ปุ่มส่วนกลาง (ลากปุ่มหลักมาใส่)")]
    public Button mainLoadButton; 

    private SaveSlotUI currentSelectedSlot = null; 
    private List<SaveSlotUI> allSlots = new List<SaveSlotUI>(); 

    private void OnEnable()
    {
        GenerateSlots(); 
        ClearSelection(); 
    }

    public void GenerateSlots()
    {
        foreach (Transform child in slotsContainer) Destroy(child.gameObject); 
        allSlots.Clear(); 

        // 1. ค้นหาไฟล์ .json ทั้งหมดในโฟลเดอร์เซฟ
        string savePath = Application.persistentDataPath;
        string[] saveFiles = Directory.GetFiles(savePath, "*.json");

        // 2. ลูปสร้างปุ่มตามจำนวนไฟล์ที่เจอจริงๆ
        foreach (string filePath in saveFiles)
        {
            // อ่านเวลาจากในไฟล์
            string json = File.ReadAllText(filePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            GameObject newSlot = Instantiate(slotPrefab, slotsContainer); 
            SaveSlotUI slotUI = newSlot.GetComponent<SaveSlotUI>(); 

            // ดึงชื่อไฟล์แล้วส่งค่าไปที่ Prefab
            string fileName = Path.GetFileName(filePath);
            slotUI.SetupSlot(fileName, data.saveTime, this);
            allSlots.Add(slotUI); 
        }
    }

    public void SelectSlot(SaveSlotUI clickedSlot)
    {
        currentSelectedSlot = clickedSlot;

        // อัปเดตสีฝั่งขวา
        foreach (SaveSlotUI slot in allSlots) 
        {
            slot.SetHighlight(slot == currentSelectedSlot); 
        }

        // อัปเดตรายละเอียดฝั่งซ้าย
        if (detailFileNameText != null)
            detailFileNameText.text = "File: " + Path.GetFileNameWithoutExtension(currentSelectedSlot.myFileName);

        if (detailSaveTimeText != null)
            detailSaveTimeText.text = "Date: " + currentSelectedSlot.saveDateStr;

        DisplaySceneNameFromSave(currentSelectedSlot.myFileName);

        mainLoadButton.interactable = true; // มีไฟล์ให้โหลดแน่นอนเพราะดึงจากของจริง

        //โหลดรูปภาพ Screen-Shot ตอน Save
        if (detailScreenshotImage != null)
        {
            // สร้างพิกัดไฟล์รูป โดยเอานามสกุล .json ออก แล้วเติม .png เข้าไปแทน
            string imagePath = Application.persistentDataPath + "/" + clickedSlot.myFileName.Replace(".json", ".png");

            if (File.Exists(imagePath))
            {
                // อ่านไฟล์รูปจากในเครื่องคอมพิวเตอร์
                byte[] fileData = File.ReadAllBytes(imagePath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); // แปลงข้อมูลให้กลายเป็นรูปภาพ

                // แปะรูปภาพลงบน UI
                detailScreenshotImage.texture = tex;
                detailScreenshotImage.color = Color.white; // เปิดให้เห็นรูปชัดๆ
            }
            else
            {
                // ถ้าไม่เจอรูปภาพ (เช่น เซฟเก่าที่ยังไม่มีรูป) ให้ซ่อนกรอบรูปไว้ หรือใส่รูปสีดำแทน
                detailScreenshotImage.texture = null;
                detailScreenshotImage.color = Color.black;
            }
        }
    }

    private void ClearSelection()
    {
        currentSelectedSlot = null; 
        foreach (SaveSlotUI slot in allSlots) slot.SetHighlight(false); 

        // เคลียร์ข้อความฝั่งซ้าย
        if (detailFileNameText != null) detailFileNameText.text = "Select Save File...";
        if (detailSaveTimeText != null) detailSaveTimeText.text = "";

        mainLoadButton.interactable = false; 
    }

    public void OnClick_MainLoadButton()
    {
        if (currentSelectedSlot != null) 
        {
            //  โหลดเกมด้วยชื่อไฟล์
            SaveManager.Instance.LoadGame(currentSelectedSlot.myFileName); 
            FindAnyObjectByType<GameMenuManager>().ResumeGame(); 
        }
    }

    public void DisplaySceneNameFromSave(string fileName)
    {
        string saveFilePath = Application.persistentDataPath + "/" + fileName;

        // 1. เช็คก่อนว่ามีไฟล์เซฟนี้อยู่จริงไหม
        if (File.Exists(saveFilePath))
        {
            // 2. อ่านข้อความ JSON ทั้งหมดออกมา
            string json = File.ReadAllText(saveFilePath);

            // 3. แปลงร่าง JSON กลับมาเป็นคลาส SaveData ของคุณ
            SaveData loadedData = JsonUtility.FromJson<SaveData>(json);

            // 4. ดึงชื่อด่าน (currentSceneName) ออกมาใช้!
            if (detailFile_SceneNameText != null)
            {
                detailFile_SceneNameText.text = "Chapter: " + loadedData.currentSceneName;
            }

            Debug.Log("ดึงชื่อด่านจากเซฟสำเร็จ: " + loadedData.currentSceneName);
        }
        else
        {
            if (detailFile_SceneNameText != null)
            {
                detailFile_SceneNameText.text = "Chapter: ";
            }
        }
    }
}