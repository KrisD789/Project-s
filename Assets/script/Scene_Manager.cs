using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scene_Manager : MonoBehaviour
{
    [Header("Transition Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.5f;

    private void Start()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            StartCoroutine(FadeInRoutine());
        }

        // --- จุดที่เพิ่มใหม่: เช็กว่าถ้าเปิดเกมมาตอนแรก แล้วอยู่หน้าเมนู ให้สั่งเล่นเพลงเมนู ---
        if (SceneManager.GetActiveScene().name == "Main Menu")
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBGM(AudioManager.MusicState.MainMenu);
            }
        }
    }

    public void ReStart()
    {
        Time.timeScale = 1f;
        StartCoroutine(FadeAndLoadRoutine(SceneManager.GetActiveScene().name));
    }

    public void Menu()
    {
        Time.timeScale = 1f;
        UnlockCursor();

        // --- จุดที่เพิ่มใหม่: สั่งเปลี่ยนเพลงเป็นเพลงเมนู เมื่อกดกลับเมนู ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.MusicState.MainMenu);
        }

        StartCoroutine(FadeAndLoadRoutine("Main Menu"));
    }

    public void Exit_Game()
    {
        Application.Quit();
    }

    public void SelectMissionAndGoLoadout(string missionSceneName)
    {
        PlayerPrefs.SetString("TargetMission", missionSceneName);
        PlayerPrefs.Save();

        UnlockCursor();

        // --- จุดที่เพิ่มใหม่: (เผื่อไว้) ถ้าหน้า Loadout อยากให้ใช้เพลงเมนูด้วย ก็สั่งเปลี่ยนเพลงที่นี่ ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.MusicState.MainMenu);
        }

        StartCoroutine(FadeAndLoadRoutine("LoadOut"));
    }

    public void StartMissionFromLoadout()
    {
        string target = PlayerPrefs.GetString("TargetMission", "Main Menu");
        Time.timeScale = 1f;

        // --- จุดที่เพิ่มใหม่: สั่งเปลี่ยนเพลงเป็นเพลง Patrol เมื่อเริ่มเข้าเล่นด่าน ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.MusicState.Patrol);
        }

        StartCoroutine(FadeAndLoadRoutine(target));
    }

    public void CompleteMission(int currentMissionIndex)
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (currentMissionIndex >= unlockedLevel)
        {
            PlayerPrefs.SetInt("UnlockedLevel", currentMissionIndex + 1);
            PlayerPrefs.Save();
            Debug.Log("ปลดล็อกด่านที่ " + (currentMissionIndex + 1) + " แล้ว!");
        }

        if (currentMissionIndex >= 3)
        {
            UnlockCursor();
            // --- จุดที่เพิ่มใหม่: สั่งเปลี่ยนเพลงเป็นเพลงเมนู (หรือเพลงจบเกม) หน้าขอบคุณ ---
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBGM(AudioManager.MusicState.MainMenu);
            }
            StartCoroutine(FadeAndLoadRoutine("ThankYouScene"));
        }
        else
        {
            string nextLevelName = "Level_" + (currentMissionIndex + 1).ToString();
            SelectMissionAndGoLoadout(nextLevelName);
        }
    }

    public void OpenSurveyLink()
    {
        Application.OpenURL("https://forms.gle/YOUR_FORM_LINK");
    }

    public bool IsMissionUnlocked(int missionIndex)
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        return missionIndex <= unlockedLevel;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator FadeInRoutine()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }

    private IEnumerator FadeAndLoadRoutine(string sceneName)
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }

        SceneManager.LoadScene(sceneName);
    }
}