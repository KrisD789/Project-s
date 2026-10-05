using UnityEngine;
using UnityEngine.UI; // ต้องใช้สำหรับ Image
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI Elements")]
    public GameObject tutorialPanel;
    public Image tutorialImage;
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
    public void ShowTutorial(string message, Sprite imageSprite)
    {
        if (tutorialPanel == null) return;

        // 1. เซตข้อความ
        if (tutorialText != null) tutorialText.text = message;

        // 2. เซตรูปภาพ (ถ้ามีรูปก็โชว์ ถ้าไม่มีก็ซ่อนกรอบรูปไปเลย)
        if (tutorialImage != null)
        {
            if (imageSprite != null)
            {
                tutorialImage.sprite = imageSprite;
                tutorialImage.gameObject.SetActive(true);
            }
            else
            {
                tutorialImage.gameObject.SetActive(false);
            }
        }

        // 3. เปิดหน้าต่าง
        tutorialPanel.SetActive(true);

        // 4. หยุดเวลาและเปิดเมาส์ให้กดปุ่มปิดได้
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