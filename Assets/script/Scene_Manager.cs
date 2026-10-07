using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scene_Manager : MonoBehaviour
{
    [Header("Transition Settings")]
    public CanvasGroup fadeCanvasGroup; // ลาก UI Panel สีดำที่มี Canvas Group มาใส่ตรงนี้
    public float fadeDuration = 0.5f;   // ระยะเวลาที่ใช้เฟด (วินาที)

    private void Start()
    {
        // ทุกครั้งที่โหลดเข้าฉากใหม่ สั่งให้จอมืดก่อน แล้วค่อยๆ เฟดสว่างขึ้น
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            StartCoroutine(FadeInRoutine());
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
        UnlockCursor(); // ปลดล็อกเมาส์ก่อนกลับเมนู
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

        UnlockCursor(); // ปลดล็อกเมาส์ตรงนี้แหละที่แก้ปัญหาเมาส์ค้าง!

        StartCoroutine(FadeAndLoadRoutine("LoadOut"));
    }

    public void StartMissionFromLoadout()
    {
        string target = PlayerPrefs.GetString("TargetMission", "Main Menu");
        Time.timeScale = 1f;
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
        // --- เพิ่มเงื่อนไขเช็กว่าจบรอบเพลย์เทสต์ (ด่าน 3) หรือยัง ---
        if (currentMissionIndex >= 3)
        {
            UnlockCursor(); // ปลดล็อกเมาส์ให้ผู้เล่นกดทำแบบสอบถามได้
            StartCoroutine(FadeAndLoadRoutine("ThankYouScene")); // โหลดไปหน้าขอบคุณ
        }
        else
        {
            // ถ้ายังไม่ถึงด่าน 3 ก็ให้ไปหน้า Loadout ปกติ
            string nextLevelName = "Level_" + (currentMissionIndex + 1).ToString();
            SelectMissionAndGoLoadout(nextLevelName);
        }
    }

    // --- แถมฟังก์ชันสำหรับเปิดหน้าเว็บแบบสอบถาม (Google Form) ---
    public void OpenSurveyLink()
    {
        // เอาลิงก์แบบฟอร์มของคุณมาใส่ในเครื่องหมายคำพูดได้เลย
        Application.OpenURL("https://forms.gle/YOUR_FORM_LINK");
    }

    public bool IsMissionUnlocked(int missionIndex)
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        return missionIndex <= unlockedLevel;
    }

    // --- Helper Functions ---

    private void UnlockCursor()
    {
        // คำสั่งคืนชีพเมาส์!
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- ระบบ Transition ---

    private IEnumerator FadeInRoutine()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            // ใช้ unscaledDeltaTime เผื่อว่าเกมถูกหยุด (TimeScale = 0) อยู่ จะได้เฟดได้ปกติ
            timer += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;

        // ปิดการบังเมาส์ของ CanvasGroup เพื่อให้ผู้เล่นคลิก UI ในฉากได้
        fadeCanvasGroup.blocksRaycasts = false;
    }

    private IEnumerator FadeAndLoadRoutine(string sceneName)
    {
        // 1. เฟดจอมืดลง
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true; // เปิดการบังเมาส์กันผู้เล่นกดปุ่มรัวๆ
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }

        // 2. โหลดฉากต่อไป
        SceneManager.LoadScene(sceneName);
    }
}