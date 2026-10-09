using UnityEngine;

public class MainMenu_Manager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject loadSavePanel;
    public GameObject settingsPanel;

    private void Start()
    {
        // บังคับให้เปิดมาเจอหน้า Main Menu เสมอตอนเริ่มเกม และปิดหน้าอื่นทิ้ง
        OpenMainMenu();
        CloseAllPanels();
    }

    // ฟังก์ชันนี้จะทำหน้าที่ "กวาดล้าง" ปิดทุกหน้าต่างให้เกลี้ยงก่อน
    public void CloseAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (loadSavePanel != null) loadSavePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // --- ฟังก์ชันสำหรับเอาไปผูกกับ OnClick ของแต่ละปุ่ม ---

    public void OpenMainMenu()
    {
        CloseAllPanels();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void OpenLevelSelect()
    {
        CloseAllPanels();
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
    }

    public void OpenLoadSave()
    {
        CloseAllPanels();
        if (loadSavePanel != null) loadSavePanel.SetActive(true);
    }

    public void OpenSettings()
    {
        CloseAllPanels();
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
}