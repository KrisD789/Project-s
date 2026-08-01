using UnityEngine;
using TMPro;

public class SavePopupController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField fileNameInput;

    private void OnEnable()
    {
        // เคลียร์ข้อความและโฟกัสช่องพิมพ์อัตโนมัติเมื่อหน้าต่างนี้เด้งขึ้นมา
        if (fileNameInput != null)
        {
            fileNameInput.text = "";
            fileNameInput.ActivateInputField();
        }
    }

    public void OnClick_ConfirmSave()
    {
        // ตัดช่องว่างหน้า-หลังทิ้ง
        string fileName = fileNameInput.text.Trim();

        if (!string.IsNullOrEmpty(fileName))
        {
            // จุดสำคัญ: SaveManager ของคุณต้องเปลี่ยนไปรับค่า string แทน int แล้ว
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
}
