using UnityEngine;
using TMPro;
using System.Collections; // เพิ่มเข้ามาเพื่อใช้งาน Coroutine

public class SavePopupController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField fileNameInput;

    [Header("ระบบแจ้งเตือน")]
    // ลากป้ายเตือนข้อความสีแดงด้านล่าง (Cannot save while grabbing an enemy) มาใส่ช่องนี้
    public GameObject saveWarningPopup;

    private void OnEnable()
    {
        // ปิดป้ายเตือนไว้ก่อนเสมอตอนหน้าต่างเซฟถูกเปิดขึ้นมาใหม่
        if (saveWarningPopup != null) saveWarningPopup.SetActive(false);

        if (fileNameInput != null)
        {
            fileNameInput.text = "";
            fileNameInput.ActivateInputField();
        }
    }

    public void OnClick_ConfirmSave()
    {
        // 1. ย้ายการเช็กสถานะมาไว้ตรงจังหวะกดปุ่ม Save
        if (Player.Instance != null)
        {
            if (Player.Instance.currentState == Player.PlayerState.GrabbingEnemy || 
            Player.Instance.currentState == Player.PlayerState.CarryingBody)
            {
                // โชว์ป้ายเตือน
                if (saveWarningPopup != null)
                {
                    saveWarningPopup.SetActive(true);

                    // หยุด Coroutine เก่าก่อนเผื่อผู้เล่นกดปุ่ม Save รัวๆ
                    StopAllCoroutines();
                    // สั่งให้นับเวลาถอยหลัง 2.5 วินาทีแล้วซ่อนป้ายเตือน
                    StartCoroutine(HideWarningAfterDelay(2.5f));
                }

                Debug.LogWarning("ระบบปฏิเสธการเซฟ: ผู้เล่นกำลังล็อคคอ หรือ แบกศพ!");
                return; //  เตะออก ไม่ให้คำสั่งเซฟด้านล่างทำงาน
            }        
        }

        // 2. ถ้าสถานะปลอดภัย (ไม่ได้ล็อคคอ) ค่อยทำการเซฟตามปกติ
        string fileName = fileNameInput.text.Trim();

        if (!string.IsNullOrEmpty(fileName))
        {
            SaveManager.Instance.SaveGame(fileName);
            FindAnyObjectByType<GameMenuManager>().CloseSavePopup();
        }
        else
        {
            Debug.LogWarning("กรุณาตั้งชื่อไฟล์ก่อนเซฟ!");
        }
    }

    public void OnClick_Cancel()
    {
        FindAnyObjectByType<GameMenuManager>().CloseSavePopup();
    }

    // ฟังก์ชันสำหรับซ่อนป้ายเตือนอัตโนมัติ
    private IEnumerator HideWarningAfterDelay(float delay)
    {
        // ต้องใช้ WaitForSecondsRealtime เพราะเวลาในเกมถูกหยุดไว้ (Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(delay);

        if (saveWarningPopup != null) saveWarningPopup.SetActive(false);
    }
}