using UnityEngine;

public class GameMenuManager : MonoBehaviour
{
    [Header("หน้าต่าง UI ที่ต้องการคุม")]
    public GameObject pauseMenu;
    public GameObject savePopup;
    public GameObject Load_Menu;
    public GameObject deathMenuUI;

    // --- เพิ่มช่องสำหรับลาก UI หน้า Setting มาใส่ ---
    public GameObject Setting_Menu;

    private bool isMenuOpen = false;

    // เพิ่มบรรทัดนี้ ให้สคริปต์อื่นเช็กได้ง่ายๆ
    public static bool IsMenuActive { get; private set; }


    void Start()
    {
        ForceResumeGame();

        // สมัครรับข่าว: ถ้า SaveManager โหลดเกมเสร็จเมื่อไหร่ ให้มารันคำสั่งล้างค่าทันที
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnGameLoaded += ForceResumeGame;
        }

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (savePopup != null) savePopup.SetActive(false);
        if (Load_Menu != null) Load_Menu.SetActive(false);

        // --- ปิดหน้า Setting ไว้ก่อนตอนเริ่มเกม ---
        if (Setting_Menu != null) Setting_Menu.SetActive(false);
    }

    private void OnDestroy()
    {
        // ยกเลิกรับข่าวตอนปิดเกม
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnGameLoaded -= ForceResumeGame;
        }
    }

    void Update()
    {
        // เปิด/ปิดเมนูด้วยปุ่ม ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // เพิ่มเงื่อนไขตรงนี้: ถ้าผู้เล่นตายแล้ว ให้หยุดการทำงานทันที (ห้ามเปิดเมนู)
            if (Player.Instance != null && Player.Instance.isDead)
            {
                Debug.Log("GameMenuManager: ผู้เล่นตายแล้ว ไม่สามารถเปิดเมนูได้!");
                return;
            }

            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;

        IsMenuActive = isMenuOpen; // อัปเดตค่าให้สคริปต์อื่นรู้ด้วย

        pauseMenu.SetActive(isMenuOpen);

        // หากปิดเมนูหลัก ให้บังคับปิดหน้าต่าง Popup, Load Menu และ Setting ไปด้วย
        if (!isMenuOpen)
        {
            if (savePopup != null) savePopup.SetActive(false);
            if (Load_Menu != null) Load_Menu.SetActive(false);
            if (Setting_Menu != null) Setting_Menu.SetActive(false); // --- เพิ่มการบังคับปิด Setting ---
        }

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

    // --- เพิ่มฟังก์ชัน เปิด/ปิด หน้า Setting ---
    public void OpenSetting_Menu()
    {
        if (Setting_Menu != null) Setting_Menu.SetActive(true);
    }

    public void CloseSetting_Menu()
    {
        if (Setting_Menu != null) Setting_Menu.SetActive(false);
    }

    public void ForceResumeGame()
    {
        isMenuOpen = false;
        IsMenuActive = false;

        // ปิด UI เมนูให้เกลี้ยง
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (savePopup != null) savePopup.SetActive(false);
        if (Load_Menu != null) Load_Menu.SetActive(false);

        // --- บังคับปิดหน้า Setting ให้เกลี้ยงด้วย ---
        if (Setting_Menu != null) Setting_Menu.SetActive(false);

        // บังคับปิดหน้าmenuตายด้วย เผื่อกรณีที่ผู้เล่นกดโหลดเซฟจากหน้า Death Menu
        if (deathMenuUI != null) deathMenuUI.SetActive(false);

        // คืนค่าเวลาและการควบคุมเมาส์
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("GameMenuManager: บังคับเคลียร์ UI และคืนค่าเวลาเป็น 1 แล้ว!");
    }

    public void ShowDeadMenu()
    {
        if (deathMenuUI != null)
        {
            deathMenuUI.SetActive(true);
        }
    }
}