using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer mainMixer; // ลาก Audio Mixer มาใส่ตรงนี้

    [Header("Audio Sources")]
    public AudioSource bgmSource; // ลำโพงสำหรับเล่นเพลงพื้นหลัง
    public AudioSource sfxSource; // ลำโพงกลางสำหรับเล่นเสียง UI หรือเสียงทั่วไป

    private void Awake()
    {
        // จัดการ Singleton ให้มี AudioManager แค่ตัวเดียวในเกม
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // (ใส่หรือไม่ใส่ก็ได้) ทำให้เสียงไม่ขาดตอนเวลาเปลี่ยน Scene
    }

    private void Start()
    {
        // โหลดค่าความดังเสียงที่ผู้เล่นเคยตั้งไว้ตอนเข้าเกม (ถ้าไม่เคยตั้ง ค่าเริ่มต้นคือ 1f)
        LoadSettings();
    }

    // ==========================================
    // ส่วนที่ 1: ระบบควบคุมความดัง (สำหรับหน้า Settings UI)
    // ==========================================

    // ข้อควรระวัง: Slider ใน UI ต้องตั้งค่า Min Value = 0.0001 (ห้ามเป็น 0 เด็ดขาด ไม่งั้นสมการ Log10 จะพัง) และ Max Value = 1

    public void SetMasterVolume(float sliderValue)
    {
        // แปลงค่าจาก 0-1 เป็นหน่วยเดซิเบล (dB) ของ AudioMixer
        float dbValue = Mathf.Log10(sliderValue) * 20f;
        mainMixer.SetFloat("MasterVol", dbValue);
        PlayerPrefs.SetFloat("Saved_MasterVol", sliderValue); // บันทึกค่าลงเครื่อง
    }

    public void SetBGMVolume(float sliderValue)
    {
        float dbValue = Mathf.Log10(sliderValue) * 20f;
        mainMixer.SetFloat("BGMVol", dbValue);
        PlayerPrefs.SetFloat("Saved_BGMVol", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        float dbValue = Mathf.Log10(sliderValue) * 20f;
        mainMixer.SetFloat("SFXVol", dbValue);
        PlayerPrefs.SetFloat("Saved_SFXVol", sliderValue);
    }

    private void LoadSettings()
    {
        float masterVol = PlayerPrefs.GetFloat("Saved_MasterVol", 1f);
        float bgmVol = PlayerPrefs.GetFloat("Saved_BGMVol", 1f);
        float sfxVol = PlayerPrefs.GetFloat("Saved_SFXVol", 1f);

        SetMasterVolume(masterVol);
        SetBGMVolume(bgmVol);
        SetSFXVolume(sfxVol);
    }

    // ==========================================
    // ส่วนที่ 2: ระบบสั่งเล่นเสียง (ให้สคริปต์อื่นเรียกใช้)
    // ==========================================

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip == clip) return; // ถ้าเล่นเพลงเดิมอยู่แล้วก็ไม่ต้องเปลี่ยน
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip); // เล่นเสียงทับซ้อนกันได้ (เหมาะกับเสียงปืน/UI)
        }
    }
}
