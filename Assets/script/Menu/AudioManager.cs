using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer mainMixer;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource playerSource; // ลำโพงใหม่ สำหรับเล่นเสียงผู้เล่นโดยเฉพาะ

    [Header("BGM Tracks")]
    public AudioClip patrolMusic;
    public AudioClip alertMusic;

    [Header("Player SFX Clips")]
    public AudioClip[] footstepClips; // เสียงเดิน (ใส่หลายๆ ไฟล์ได้เพื่อสุ่ม)
    public AudioClip shootClip;       // เสียงยิงปืน
    public AudioClip reloadClip;      // เสียงรีโหลด
    public AudioClip healClip;        // เสียงปั๊มยา

    private bool isPlayingAlertMusic = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadSettings();
        PlayBGM(patrolMusic);
        isPlayingAlertMusic = false;
    }

    private void Update()
    {
        // ระบบเปลี่ยนเพลงตามการ Alert
        if (Enemy_Alert.AlertTimer > 0 && Enemy_Alert.AlertTimer <= Enemy_Alert.MaxAlertTimer)
        {
            if (!isPlayingAlertMusic)
            {
                PlayBGM(alertMusic);
                isPlayingAlertMusic = true;
                Debug.Log("?? AudioManager: ตรวจพบการ Alert, เปลี่ยนเป็นเพลงต่อสู้!");
            }
        }
        else
        {
            if (isPlayingAlertMusic)
            {
                PlayBGM(patrolMusic);
                isPlayingAlertMusic = false;
                Debug.Log("?? AudioManager: ปลอดภัยแล้ว, เปลี่ยนเป็นเพลง Patrol");
            }
        }
    }

    // ==========================================
    // ส่วนควบคุมความดัง (Settings)
    // ==========================================
    public void SetMasterVolume(float sliderValue)
    {
        float dbValue = Mathf.Log10(sliderValue) * 20f;
        mainMixer.SetFloat("MasterVol", dbValue);
        PlayerPrefs.SetFloat("Saved_MasterVol", sliderValue);
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
    // ฟังก์ชันเล่นเสียง BGM / SFX ทั่วไป
    // ==========================================
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource.clip == clip) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // ==========================================
    // ฟังก์ชันเล่นเสียงของผู้เล่นโดยเฉพาะ (Player Actions)
    // ==========================================
    public void PlayFootstep()
    {
        if (footstepClips != null && footstepClips.Length > 0 && playerSource != null)
        {
            // สุ่มเลือกเสียงเดินจาก Array
            int randomIndex = Random.Range(0, footstepClips.Length);

            // สุ่มความทุ้มแหลม (Pitch) นิดหน่อย เพื่อให้เสียงก้าวเท้าฟังดูเป็นธรรมชาติ ไม่แข็งเหมือนหุ่นยนต์
            playerSource.pitch = Random.Range(0.9f, 1.1f);

            playerSource.PlayOneShot(footstepClips[randomIndex]);
        }
    }

    public void PlayShoot()
    {
        if (shootClip != null && playerSource != null)
        {
            playerSource.pitch = 1f; // คืนค่า Pitch เป็นปกติกันเหนียว (เผื่อติดมาจากการเดิน)
            playerSource.PlayOneShot(shootClip);
        }
    }

    public void PlayReload()
    {
        if (reloadClip != null && playerSource != null)
        {
            playerSource.pitch = 1f;
            playerSource.PlayOneShot(reloadClip);
        }
    }

    public void PlayHeal()
    {
        if (healClip != null && playerSource != null)
        {
            playerSource.pitch = 1f;
            playerSource.PlayOneShot(healClip);
        }
    }
}