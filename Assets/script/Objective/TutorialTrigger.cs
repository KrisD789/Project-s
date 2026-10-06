using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("ตั้งค่าเนื้อหาสอนเล่น")]
    public Sprite tutorialImage; // ลากรูปภาพที่อยากโชว์มาใส่ตรงนี้
    public Sprite tutorialImage2;

    [TextArea(3, 5)]
    public string tutorialMessage; // พิมพ์ข้อความสอนเล่น

    [Header("ตั้งค่าการทำงาน")]
    public bool showOnlyOnce = true; // ให้แสดงแค่รอบเดียวแล้วปิดการทำงานไปเลย
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && showOnlyOnce) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            if (TutorialManager.Instance != null)
            {
                // ส่งรูปที่ 2 พ่วงไปด้วย
                TutorialManager.Instance.ShowTutorial(tutorialImage, tutorialImage2, tutorialMessage);
            }
        }
    }
}