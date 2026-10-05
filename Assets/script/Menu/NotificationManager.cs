using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("UI Elements")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationText;

    [Header("Settings")]
    public float displayDuration = 3f; // ระยะเวลาโชว์ป๊อปอัป (วินาที)

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (notificationPanel != null) notificationPanel.SetActive(false);
    }

    // ฟังก์ชันนี้เรียกใช้ได้จากทุกที่ แค่ส่งข้อความที่อยากให้โชว์มา
    public void ShowNotification(string message)
    {
        if (notificationPanel != null && notificationText != null)
        {
            notificationText.text = message;
            notificationPanel.SetActive(true);

            // สั่งหยุดเวลานับถอยหลังอันเก่า (เผื่อเดินชน 2 เควสต์ติดกัน) แล้วเริ่มนับใหม่
            StopAllCoroutines();
            StartCoroutine(HideNotification());
        }
    }

    private IEnumerator HideNotification()
    {
        yield return new WaitForSeconds(displayDuration);
        notificationPanel.SetActive(false);
    }
}