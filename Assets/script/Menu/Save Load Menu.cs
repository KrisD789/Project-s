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
    public TextMeshProUGUI detailSaveTimeText;

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

        mainLoadButton.interactable = true; // มีไฟล์ให้โหลดแน่นอนเพราะดึงจากของจริง
    }

    private void ClearSelection()
    {
        currentSelectedSlot = null; 
        foreach (SaveSlotUI slot in allSlots) slot.SetHighlight(false); 

        // เคลียร์ข้อความฝั่งซ้าย
        if (detailFileNameText != null) detailFileNameText.text = "เลือกไฟล์เซฟเพื่อดูรายละเอียด";
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
}