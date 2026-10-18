using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    // เปลี่ยนมาใช้ Instance (I พิมพ์ใหญ่) แบบที่คุณตั้งไว้
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource playerSource;

    // เพิ่ม Loadout เข้าไปในลิสต์
    public enum MusicState { None, MainMenu, Loadout, Patrol, Investigate, Alert }
    private MusicState currentMusicState = MusicState.None;

    [Header("BGM Clips")]
    public AudioClip mainMenuBGM;
    public AudioClip loadoutBGM; // <-- เพิ่มช่องใส่เพลงหน้า Loadout
    public AudioClip patrolBGM;
    public AudioClip investigateBGM;
    public AudioClip alertBGM;

    [Header("Player SFX")]
    public AudioClip healClip;
    public AudioClip rifleClip;
    public AudioClip pistolClip;
    public AudioClip silencedPistolClip;
    public AudioClip reloadClip;
    public AudioClip hurtClip;     // เสียงตอนผู้เล่นโดนยิง
    public AudioClip hitSoundClip; // เสียง "ติ๊ก!" ตอนเรายิงโดนศัตรู

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadSettings();
        // เอาคำสั่งบังคับเล่น Patrol ออก เพื่อให้ Scene_Manager เป็นตัวสั่งแทน
    }

    // ==========================================
    // ระบบเปิดเพลงพื้นหลัง (BGM)
    // ==========================================
    public void PlayBGM(MusicState newState)
    {
        if (currentMusicState == newState) return;

        currentMusicState = newState;
        bgmSource.Stop();

        // เพิ่ม Case สำหรับ Loadout
        switch (newState)
        {
            case MusicState.MainMenu:
                bgmSource.clip = mainMenuBGM;
                break;
            case MusicState.Loadout:
                bgmSource.clip = loadoutBGM;
                break;
            case MusicState.Patrol:
                bgmSource.clip = patrolBGM;
                break;
            case MusicState.Investigate:
                bgmSource.clip = investigateBGM;
                break;
            case MusicState.Alert:
                bgmSource.clip = alertBGM;
                break;
        }

        if (bgmSource.clip != null)
        {
            bgmSource.Play();
        }
    }

    // ==========================================
    // ระบบเสียงเอฟเฟกต์ผู้เล่น (Player SFX)
    // ==========================================
    public void PlayRifleShoot()
    {
        if (rifleClip != null && playerSource != null)
        {
            playerSource.pitch = Random.Range(0.95f, 1.05f);
            playerSource.PlayOneShot(rifleClip);
        }
    }

    public void PlayPistolShoot()
    {
        if (pistolClip != null && playerSource != null)
        {
            playerSource.pitch = Random.Range(0.95f, 1.05f);
            playerSource.PlayOneShot(pistolClip);
        }
    }

    public void PlaySilencedPistol()
    {
        if (silencedPistolClip != null && playerSource != null)
        {
            playerSource.pitch = Random.Range(0.95f, 1.05f);
            playerSource.PlayOneShot(silencedPistolClip);
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

    public void PlayReload() // <-- เพิ่มบล็อกฟังก์ชันนี้
    {
        if (reloadClip != null && playerSource != null)
        {
            // รีโหลดไม่จำเป็นต้องสุ่มเสียงสูงต่ำ ให้ pitch เป็น 1 ปกติ
            playerSource.pitch = 1f;
            playerSource.PlayOneShot(reloadClip);
        }
    }

    public void PlayHurtSound()
    {
        if (hurtClip != null && playerSource != null)
        {
            // สุ่มเสียงสูงต่ำเล็กน้อย จะได้ฟังดูไม่ซ้ำซากตอนโดนรัวยิง
            playerSource.pitch = Random.Range(0.9f, 1.1f);
            playerSource.PlayOneShot(hurtClip);
        }
    }

    public void PlayHitSound()
    {
        if (hitSoundClip != null && playerSource != null)
        {
            // Hit Sound ควรจะเป๊ะๆ และหนักแน่น
            playerSource.pitch = 1f;
            playerSource.PlayOneShot(hitSoundClip);
        }
    }

    // ==========================================
    // ระบบตั้งค่าความดัง (Volume Control)
    // ==========================================
    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVol", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("MasterVol", volume);
    }

    public void SetBGMVolume(float volume)
    {
        audioMixer.SetFloat("BGMVol", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("BGMVol", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVol", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("SFXVol", volume);
    }

    public void LoadSettings()
    {
        float masterVol = PlayerPrefs.GetFloat("MasterVol", 1f);
        float bgmVol = PlayerPrefs.GetFloat("BGMVol", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVol", 1f);

        SetMasterVolume(masterVol);
        SetBGMVolume(bgmVol);
        SetSFXVolume(sfxVol);
    }


}