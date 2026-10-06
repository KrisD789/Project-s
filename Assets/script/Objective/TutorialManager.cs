using UnityEngine;
using UnityEngine.UI; // ต้องใช้สำหรับ Image
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI Elements")]
    public GameObject tutorialPanel;
    public Image tutorialImage;   // กรอบรูปที่ 1
    public Image tutorialImage2;  // กรอบรูปที่ 2 
    public TextMeshProUGUI tutorialText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }

    // ฟังก์ชันรับทั้งข้อความและรูปภาพ
    public void ShowTutorial(Sprite img1, Sprite img2, string message)
    {
        // ใส่รูปที่ 1 และซ่อนกรอบถ้าไม่มีรูป
        if (tutorialImage != null)
        {
            tutorialImage.sprite = img1;
            tutorialImage.gameObject.SetActive(img1 != null);
        }

        // ใส่รูปที่ 2 และซ่อนกรอบถ้าไม่มีรูป
        if (tutorialImage2 != null)
        {
            tutorialImage2.sprite = img2;
            tutorialImage2.gameObject.SetActive(img2 != null);
        }

        if (tutorialText != null) tutorialText.text = message;

        if (tutorialPanel != null) tutorialPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ฟังก์ชันสำหรับปุ่มปิด
    public void CloseTutorial()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        // คืนค่าเวลาและซ่อนเมาส์
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}