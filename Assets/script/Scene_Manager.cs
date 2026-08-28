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

    public void Start_Game()
    {
        // สั่งโหลดโดยพิมพ์ชื่อไฟล์ซีนลงไปตรงๆ (ต้องสะกดพิมพ์เล็กพิมพ์ใหญ่ให้ตรงเป๊ะ)
        SceneManager.LoadScene("Demo chapter 1");
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
