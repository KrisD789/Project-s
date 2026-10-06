using System.Collections; // ต้องเพิ่มตัวนี้เพื่อใช้ Coroutine
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ต้องเพิ่มตัวนี้เพื่อใช้ Slider (หลอดโหลด)

public class Scene_Manager : MonoBehaviour
{
    //[Header("Loading UI")]
    //public GameObject loadingPanel; // หน้าต่างฉากโหลด (Panel) ที่จะให้เด้งขึ้นมาบังจอ
    //public Slider loadingSlider;    // หลอดความคืบหน้า (Progress Bar)

    public void ReStart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Menu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    public void Exit_Game()
    {
        Application.Quit();
    }

    public void SelectMissionAndGoLoadout(string missionSceneName)
    {
        PlayerPrefs.SetString("TargetMission", missionSceneName);
        PlayerPrefs.Save();
        SceneManager.LoadScene("LoadOut");
    }

    // ---------------------------------------------------------
    // ปรับปรุงฟังก์ชันนี้: เปลี่ยนจากโหลดทันที เป็นการเรียก Coroutine
    // ---------------------------------------------------------
    public void StartMissionFromLoadout()
    {
        string target = PlayerPrefs.GetString("TargetMission", "Main Menu");
        Time.timeScale = 1f;

        // เริ่มต้นการโหลดเบื้องหลัง
        //StartCoroutine(LoadAsynchronously(target));
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

        string nextLevelName = "Level_" + (currentMissionIndex + 1).ToString();
        SelectMissionAndGoLoadout(nextLevelName);
    }

    public bool IsMissionUnlocked(int missionIndex)
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        return missionIndex <= unlockedLevel;
    }

   
}