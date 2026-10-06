using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public Slider loadingSlider;

    void Start()
    {
        // ทันทีที่เข้าฉากคั่นกลาง ให้ดึงชื่อด่านที่ฝากไว้ แล้วเริ่มโหลดเบื้องหลังเลย
        string targetScene = PlayerPrefs.GetString("TargetMission", "Main Menu");
        StartCoroutine(LoadLevelAsync(targetScene));
    }

    IEnumerator LoadLevelAsync(string targetScene)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            if (loadingSlider != null)
            {
                loadingSlider.value = progress;
            }
            yield return null;
        }
    }
}