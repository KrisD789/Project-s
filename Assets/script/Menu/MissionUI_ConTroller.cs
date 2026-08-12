using UnityEngine;

public class MissionUI_ConTroller : MonoBehaviour
{
    [Header("References")]
    public GameObject missionEntryPrefab;
    public Transform contentContainer;

    private void Start()
    {
        // 1. สร้าง UI ทันทีที่เริ่มด่าน
        RefreshMissionList();

        // 2. สมัครรับข่าว: ถ้า MissionManager ประกาศว่ามีเควสต์เสร็จ ให้รันฟังก์ชัน RefreshMissionList ทันที
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionComplete += HandleMissionComplete;
        }

        // สมัครรับข่าวจาก SaveManager ว่าถ้าโหลดเกมเสร็จ ให้มารันคำสั่ง RefreshMissionList ทันที
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnGameLoaded += RefreshMissionList;
        }
    }

    private void OnDestroy()
    {
        // 3. สำคัญมาก: ต้องยกเลิกการรับข่าวตอนเปลี่ยนด่านหรือปิด UI เพื่อป้องกันบั๊ก
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionComplete -= HandleMissionComplete;
        }

        // ยกเลิกการรับข่าวตอนปิดหน้าจอด้วย ป้องกันบั๊ก
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnGameLoaded -= RefreshMissionList;
        }
    }

    // ฟังก์ชันนี้จะทำงานอัตโนมัติเมื่อเควสต์สำเร็จ
    private void HandleMissionComplete(MissionData completedMission)
    {
        Debug.Log($"HUD อัปเดต: เควสต์ {completedMission.missionName} เสร็จแล้ว!");
        RefreshMissionList();
    }

    public void RefreshMissionList()
    {
        // ล้าง UI เก่าทิ้ง
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        if (MissionManager.Instance == null) return;

        // วิ่งลูปสร้าง UI ใหม่ให้ตรงกับข้อมูลปัจจุบัน
        foreach (MissionData mission in MissionManager.Instance.activeMissions)
        {
            GameObject newEntry = Instantiate(missionEntryPrefab, contentContainer);
            MissionUI_SetUP entryUI = newEntry.GetComponent<MissionUI_SetUP>();

            if (entryUI != null)
            {
                entryUI.Setup(mission);
            }
        }
    }
}
