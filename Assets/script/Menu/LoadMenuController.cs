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
    public RawImage detailScreenshotImage;

    [Header("ปุ่มส่วนกลาง (ลากปุ่มหลักมาใส่)")]
    public Button mainLoadButton;

    private SaveSlotUI currentSelectedSlot = null;
    private List<SaveSlotUI> allSlots = new List<SaveSlotUI>();

    private void OnEnable()
    {
        GenerateSlots();
        ClearSelection();
    }

    // เพิ่มฟังก์ชันนี้เพื่อเคลียร์แรมทันทีที่ผู้เล่นกดปิดหน้าต่าง Load UI
    private void OnDisable()
    {
        if (detailScreenshotImage != null && detailScreenshotImage.texture != null)
        {
            Destroy(detailScreenshotImage.texture);
            detailScreenshotImage.texture = null;
        }
    }

    public void GenerateSlots()
    {
        foreach (Transform child in slotsContainer) Destroy(child.gameObject);
        allSlots.Clear();

        string savePath = Application.persistentDataPath;
        string[] saveFiles = Directory.GetFiles(savePath, "*.json");

        foreach (string filePath in saveFiles)
        {
            string json = File.ReadAllText(filePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            GameObject newSlot = Instantiate(slotPrefab, slotsContainer);
            SaveSlotUI slotUI = newSlot.GetComponent<SaveSlotUI>();

            string fileName = Path.GetFileName(filePath);
            slotUI.SetupSlot(fileName, data.saveTime, this);
            allSlots.Add(slotUI);
        }
    }

    public void SelectSlot(SaveSlotUI clickedSlot)
    {
        currentSelectedSlot = clickedSlot;

        foreach (SaveSlotUI slot in allSlots)
        {
            slot.SetHighlight(slot == currentSelectedSlot);
        }

        if (detailFileNameText != null)
            detailFileNameText.text = "File: " + Path.GetFileNameWithoutExtension(currentSelectedSlot.myFileName);

        if (detailSaveTimeText != null)
            detailSaveTimeText.text = "Date: " + currentSelectedSlot.saveDateStr;

        DisplaySceneNameFromSave(currentSelectedSlot.myFileName);

        mainLoadButton.interactable = true;

        if (detailScreenshotImage != null)
        {
            //  หัวใจสำคัญ: ก่อนจะโหลดรูปใหม่ ต้องทำลายรูปเก่าในแรมทิ้งก่อนเสมอ! 
            if (detailScreenshotImage.texture != null)
            {
                Destroy(detailScreenshotImage.texture);
            }

            string imagePath = Application.persistentDataPath + "/" + clickedSlot.myFileName.Replace(".json", ".png");

            if (File.Exists(imagePath))
            {
                byte[] fileData = File.ReadAllBytes(imagePath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);

                detailScreenshotImage.texture = tex;
                detailScreenshotImage.color = Color.white;
            }
            else
            {
                detailScreenshotImage.texture = null;
                detailScreenshotImage.color = Color.black;
            }
        }
    }

    private void ClearSelection()
    {
        currentSelectedSlot = null;
        foreach (SaveSlotUI slot in allSlots) slot.SetHighlight(false);

        if (detailFileNameText != null) detailFileNameText.text = "Select Save File...";
        if (detailSaveTimeText != null) detailSaveTimeText.text = "";

        //  เคลียร์รูปภาพในแรมตอนกดล้างการเลือกด้วย 
        if (detailScreenshotImage != null)
        {
            if (detailScreenshotImage.texture != null)
            {
                Destroy(detailScreenshotImage.texture);
            }
            detailScreenshotImage.texture = null;
            detailScreenshotImage.color = Color.black;
        }

        mainLoadButton.interactable = false;
    }

    public void OnClick_MainLoadButton()
    {
        if (currentSelectedSlot != null)
        {
            SaveManager.Instance.LoadGame(currentSelectedSlot.myFileName);
            FindAnyObjectByType<GameMenuManager>().ResumeGame();
        }
    }

    public void DisplaySceneNameFromSave(string fileName)
    {
        string saveFilePath = Application.persistentDataPath + "/" + fileName;

        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData loadedData = JsonUtility.FromJson<SaveData>(json);

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