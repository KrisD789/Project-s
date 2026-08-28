using TMPro;
using UnityEngine;

public class Demo_end : MonoBehaviour
{
    public GameObject demo_ui;
    private bool isDemoEnded = false; // ตัวแปรป้องกันการสั่งงานซ้ำทุกเฟรม

    void Start()
    {
        if (demo_ui != null)
        {
            demo_ui.SetActive(false);
        }
    }

    void Update()
    {
        // ทำงานเฉพาะตอนที่ยังไม่จบ Demo
        if (!isDemoEnded)
        {
            CheckDemoEnd();
        }
    }

    public void CheckDemoEnd()
    {
        if (MissionManager.Instance != null && MissionManager.Instance.All_Mission_Complete())
        {
            isDemoEnded = true; // ล็อกไว้ไม่ให้เข้ามาทำซ้ำ

            if (demo_ui != null)
            {
                demo_ui.SetActive(true);
            }

            // 1. หยุดเวลาในเกม
            Time.timeScale = 0f;

            // 2. ปลดล็อกเมาส์ และโชว์เคอร์เซอร์ เพื่อให้ผู้เล่นกด UI ได้
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            /* 
            // หมายเหตุ: แต่ถ้าต้องการ "ล็อกเมาส์ไว้กลางจอ" จริงๆ (เช่น จบเกมแบบล็อคมุมกล้อง) ให้ใช้ชุดนี้แทน:
            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible = false;
            */
        }
    }
}