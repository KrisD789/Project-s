using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class SaveSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI slotTitleText; 
    //public TextMeshProUGUI saveTimeText; 

    [Header("ระบบ Highlight")]
    public Image backgroundImage; 
    public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 1f); 
    public Color selectedColor = new Color(0.8f, 0.5f, 0f, 1f); 

    [HideInInspector] public string myFileName; // เปลี่ยนเป็น String
    [HideInInspector] public string saveDateStr;

    private LoadMenuController menuController; 

    // ปรับการรับค่าให้ส่งชื่อไฟล์และวันที่เข้ามาโดยตรง
    public void SetupSlot(string fileName, string dateStr, LoadMenuController controller)
    {
        myFileName = fileName;
        saveDateStr = dateStr;
        menuController = controller; 
        
        // โชว์ชื่อไฟล์โดยตัดนามสกุล .json ออก
        slotTitleText.text = Path.GetFileNameWithoutExtension(myFileName); 
        //saveTimeText.text = saveDateStr; 
    }

    public void OnClick_SelectThisSlot()
    {
        menuController.SelectSlot(this);
    }

    public void SetHighlight(bool isSelected)
    {
        if (backgroundImage != null) 
        {
            backgroundImage.color = isSelected ? selectedColor : normalColor; 
        }
    }
}