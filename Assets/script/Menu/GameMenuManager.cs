using UnityEngine;

public class GameMenuManager : MonoBehaviour
{
    [Header("หน้าต่าง UI ที่ต้องการคุม")]
    public GameObject pauseMenu; 
    public GameObject savePopup; 
    public GameObject Load_Menu; 

    private bool isMenuOpen = false; 

    void Start()
    {
        if (pauseMenu != null) pauseMenu.SetActive(false); 
        if (savePopup != null) savePopup.SetActive(false);
        if (Load_Menu != null) Load_Menu.SetActive(false);
    }

    void Update()
    {
        // เปิด/ปิดเมนูด้วยปุ่ม ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen; 
        pauseMenu.SetActive(isMenuOpen);
        
        // หากปิดเมนูหลัก ให้บังคับปิดหน้าต่าง Popup และ Load Menu ไปด้วย
        if (!isMenuOpen && savePopup != null) savePopup.SetActive(false); 
        if (!isMenuOpen && Load_Menu != null) Load_Menu.SetActive(false);

        Time.timeScale = isMenuOpen ? 0f : 1f; 
        Cursor.lockState = isMenuOpen ? CursorLockMode.None : CursorLockMode.Locked; 
        Cursor.visible = isMenuOpen;
    }

    public void ResumeGame()
    {
        if (isMenuOpen) ToggleMenu(); 
    }

    public void OpenSavePopup()
    {
        if (savePopup != null) savePopup.SetActive(true); 
    }

    // เพิ่มฟังก์ชันสำหรับปิดหน้าต่าง Popup (เอาไว้ผูกกับปุ่ม Cancel)
    public void CloseSavePopup()
    {
        if (savePopup != null) savePopup.SetActive(false);
    }

    public void OpenLoad_Menu()
    {
        if (Load_Menu != null) Load_Menu.SetActive(true);
    }

    public void CloseLoad_Menu()
    {
        if (Load_Menu != null) Load_Menu.SetActive(false);
    }
}