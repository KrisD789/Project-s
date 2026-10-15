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
        // 1. ดึงค่าที่เคยเซฟไว้มาอัปเดตให้แถบ Slider ตรงกับความเป็นจริง
        masterSlider.value = PlayerPrefs.GetFloat("Saved_MasterVol", 1f);
        bgmSlider.value = PlayerPrefs.GetFloat("Saved_BGMVol", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("Saved_SFXVol", 1f);

        // 2. ผูก Event เมื่อผู้เล่นลาก Slider ให้ไปเรียก AudioManager
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMasterVolume(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
    }

    public void SetBGMVolume(float value)
    {
        AudioManager.Instance.SetBGMVolume(value);
    }

    public void SetSFXVolume(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }
}