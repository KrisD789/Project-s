using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    //  1. เพิ่มตัวแปรสำหรับรับ UI เมาส์
    [Header("Mouse Settings")]
    public Slider mouseSensitivitySlider;
    public TMPro.TextMeshProUGUI mouseValueText; // ถ้าใช้ TextMeshPro ให้เปลี่ยนเป็น public TMPro.TextMeshProUGUI mouseValueText;

    private void Start()
    {
        // โหลดค่าเสียงเดิม
        if (masterSlider != null) masterSlider.value = PlayerPrefs.GetFloat("Saved_MasterVol", 1f);
        if (bgmSlider != null) bgmSlider.value = PlayerPrefs.GetFloat("Saved_BGMVol", 1f);
        if (sfxSlider != null) sfxSlider.value = PlayerPrefs.GetFloat("Saved_SFXVol", 1f);

        //  2. โหลดค่าความเร็วเมาส์ (ตั้งค่าเริ่มต้นไว้ที่ 150)
        if (mouseSensitivitySlider != null)
        {
            // 1. ถอดการเชื่อมต่อฟังก์ชันออกชั่วคราว ป้องกันไม่ให้มันทำงานเองตอนเริ่มซีน
            mouseSensitivitySlider.onValueChanged.RemoveAllListeners();

            // 2. โหลดค่าความเร็วที่เซฟไว้มาใส่ใน Slider (ถ้าไม่เคยเซฟ จะดึงค่า 150 มาแทน)
            float savedSense = PlayerPrefs.GetFloat("Saved_MouseSensitivity", 150f);
            mouseSensitivitySlider.value = savedSense;
            UpdateMouseText(savedSense);

            // 3. พอจัดค่าทุกอย่างเสร็จแล้ว ค่อยผูกฟังก์ชันกลับเข้าไป เพื่อให้ทำงานเฉพาะตอนผู้เล่นเอามือไปเลื่อนเท่านั้น
            mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        }

        if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    // ฟังก์ชันเดิม (เสียง)
    public void SetMasterVolume(float value) { if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(value); }
    public void SetBGMVolume(float value) { if (AudioManager.Instance != null) AudioManager.Instance.SetBGMVolume(value); }
    public void SetSFXVolume(float value) { if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value); }

    //  3. ฟังก์ชันใหม่สำหรับปรับความเร็วเมาส์
    public void SetMouseSensitivity(float value)
    {
        // บันทึกค่าลงระบบ
        PlayerPrefs.SetFloat("Saved_MouseSensitivity", value);
        PlayerPrefs.Save();

        // อัปเดตตัวอักษร UI
        UpdateMouseText(value);

        // ส่งค่าไปอัปเดตกล้องในฉากทันที (เผื่อกดหยุดเกมมาปรับ)
        camera camScript = FindObjectOfType<camera>();
        if (camScript != null)
        {
            camScript.mouseSensitivity = value;
        }
    }

    // ฟังก์ชันย่อยสำหรับอัปเดตตัวอักษร ให้โชว์เลขจำนวนเต็มสวยๆ
    private void UpdateMouseText(float value)
    {
        if (mouseValueText != null)
        {
            mouseValueText.text = Mathf.RoundToInt(value).ToString();
        }
    }
}