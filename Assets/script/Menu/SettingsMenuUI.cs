using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    private void Start()
    {
        // กำหนดค่าเริ่มต้นให้ Slider ดึงค่ามาจาก PlayerPrefs
        if (masterSlider != null) masterSlider.value = PlayerPrefs.GetFloat("Saved_MasterVol", 1f);
        if (bgmSlider != null) bgmSlider.value = PlayerPrefs.GetFloat("Saved_BGMVol", 1f);
        if (sfxSlider != null) sfxSlider.value = PlayerPrefs.GetFloat("Saved_SFXVol", 1f);

        // ผูก Event เมื่อลาก Slider
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMasterVolume(float value)
    {
        // ป้องกัน Error กรณีเปิดเทสต์ข้ามซีนแล้วไม่มี AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value);
        }
    }

    public void SetBGMVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(value);
        }
    }

    public void SetSFXVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }
}