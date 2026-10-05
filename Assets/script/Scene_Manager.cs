using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Manager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Goto_LoadOut()
    {
        SceneManager.LoadScene("LoadOut");
    }

    public void ReStart()
    {
        // คืนค่าเวลาให้กลับเป็นปกติก่อนโหลดฉากใหม่ (เผื่อเกมถูก Time.timeScale = 0 ไว้ตอนตาย)
        Time.timeScale = 1f;

        // สั่งโหลดฉากปัจจุบันที่กำลังเล่นอยู่ขึ้นมาใหม่
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Menu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void Exit_Game()
    {
        Application.Quit();
    }
}
