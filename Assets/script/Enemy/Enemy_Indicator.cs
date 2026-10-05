using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Enemy_Indicator : MonoBehaviour
{
    [Header("อ้างอิง")]
    public enemy_stage enemyScript;
    public Canvas indicatorCanvas;
    public Image iconImage;

    [Header("สไปรต์ไอคอน")]
    public Sprite questionMark;     // ? 
    public Sprite exclamationMark;  // !

    [Header("ตั้งค่าเวลา")]
    public float alertShowDuration = 2.0f; // ระยะเวลาที่จะโชว์ ! ก่อนหายไป (วินาที)

    private Camera mainCam;
    private enemy_stage.EnemyState previousState; // ตัวเก็บสถานะก่อนหน้า
    private Coroutine hideAlertCoroutine;

    void Start()
    {
        mainCam = Camera.main;
        if (enemyScript == null) enemyScript = GetComponent<enemy_stage>();

        indicatorCanvas.enabled = false;
        previousState = enemyScript.currentState; // จำสถานะตอนเริ่มเกมไว้
    }

    void LateUpdate()
    {
        if (enemyScript == null || iconImage == null) return;

        // หัน Canvas เข้าหากล้องตลอดเวลา
        indicatorCanvas.transform.LookAt(indicatorCanvas.transform.position + mainCam.transform.rotation * Vector3.forward,
                                         mainCam.transform.rotation * Vector3.up);

        // เช็กว่าสถานะ "เปลี่ยน" ไปจากเดิมหรือไม่
        if (previousState != enemyScript.currentState)
        {
            OnStateChanged(enemyScript.currentState);
            previousState = enemyScript.currentState; // อัปเดตความจำ
        }
    }

    // ฟังก์ชันนี้จะรันแค่ "ครั้งเดียว" ตอนที่ AI เปลี่ยนสถานะ
    void OnStateChanged(enemy_stage.EnemyState newState)
    {
        // ถ้ามีคำสั่งซ่อนไอคอนรออยู่ ให้ยกเลิกก่อน
        if (hideAlertCoroutine != null)
        {
            StopCoroutine(hideAlertCoroutine);
            hideAlertCoroutine = null;
        }

        switch (newState)
        {
            case enemy_stage.EnemyState.Investigate:
                // โชว์ ? ค้างไว้ตลอด (ไม่มีการเรียก Coroutine นับถอยหลัง)
                iconImage.sprite = questionMark;
                iconImage.color = Color.yellow;
                indicatorCanvas.enabled = true;
                break;

            case enemy_stage.EnemyState.Alert:
            case enemy_stage.EnemyState.alertSearching:
                // โชว์ ! สีแดง
                iconImage.sprite = exclamationMark;
                iconImage.color = Color.red;
                indicatorCanvas.enabled = true;

                // สั่งให้นับเวลาถอยหลังเพื่อซ่อนไอคอน
                hideAlertCoroutine = StartCoroutine(HideIconAfterDelay(alertShowDuration));
                break;

            default:
                // สถานะอื่นๆ (Patrol, Faint, Dead) ให้ซ่อนไอคอนทันที
                indicatorCanvas.enabled = false;
                break;
        }
    }

    IEnumerator HideIconAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        indicatorCanvas.enabled = false; // ปิดไอคอนเมื่อครบเวลา
    }
}
