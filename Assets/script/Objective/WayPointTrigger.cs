using UnityEngine;

public class WayPointTrigger : MonoBehaviour
{
    public enum WaypointMode
    {
        Proximity,
        ShowAfterMission,
        ShowWhenReadyToExit
    }

    [Header("การตั้งค่าเงื่อนไข")]
    public WaypointMode mode = WaypointMode.Proximity;

    [Tooltip("เควสต์ที่จะสั่งปิดป้ายนี้ (ถ้าปล่อยว่างไว้ มันจะปิดตัวเองตามเควสต์ใน MissionTrigger)")]
    public MissionData overrideCloseMission;

    [Tooltip("ไฟล์ MissionData ที่ต้องทำเสร็จก่อน จุดนี้ถึงจะโผล่ (ใช้สำหรับดรอปโซน)")]
    public MissionData prerequisiteMission;

    [Header("UI Object")]
    public GameObject waypointDisplay;

    // ตัวแปรนี้จะดึงมาจาก MissionTrigger อัตโนมัติ เพื่อไว้เช็กตอนปิดตัวเอง
    private MissionData myMissionData;

    private bool isDiscovered = false;
    private bool isCompleted = false;

    private void Awake()
    {
        //GetComponent<Collider>().isTrigger = true;

        // ดึง MissionData จากสคริปต์ MissionTrigger ที่แปะอยู่บน Object เดียวกันมาเก็บไว้
        MissionTrigger trigger = GetComponent<MissionTrigger>();
        if (trigger != null)
        {
            myMissionData = trigger.Mission_Data;
        }
    }

    private void Start()
    {
        if (waypointDisplay != null) waypointDisplay.SetActive(false);

        // สมัครรับข่าวจาก MissionManager (ฟังทั้งตอนเปิด และ ตอนปิด)
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionComplete += HandleMissionUpdate;
        }

        // สมัครรับข่าวโหลดเซฟ
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnGameLoaded += RefreshStateAfterLoad;
        }

        CheckCurrentMissionStatus(); // ตรวจสอบตอนเริ่มเกม
    }

    private void OnDestroy()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionComplete -= HandleMissionUpdate;
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnGameLoaded -= RefreshStateAfterLoad;
        }
    }

    private void RefreshStateAfterLoad()
    {
        isDiscovered = false;
        isCompleted = false;
        if (waypointDisplay != null) waypointDisplay.SetActive(false);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        CheckCurrentMissionStatus();
    }

    private void CheckCurrentMissionStatus()//ใช้ตอนโหลดเซฟ หรือ โหลดฉากใหม่
    {
        // --- เปลี่ยนมาใช้ตัวแปลเช็กเงื่อนไข ---
        MissionData targetCloseMission = overrideCloseMission != null ? overrideCloseMission : myMissionData;

        if (targetCloseMission != null && targetCloseMission.isCompleted)
        {
            CloseWaypoint();
            return;
        }

        // 2. เช็กเปิดตัวเอง: ตามโหมดที่ตั้งไว้
        if (mode == WaypointMode.ShowAfterMission && prerequisiteMission != null)
        {
            if (prerequisiteMission.isCompleted) ActivateWaypoint();
        }
        else if (mode == WaypointMode.ShowWhenReadyToExit)
        {
            if (MissionManager.Instance.CanPlayerExit()) ActivateWaypoint();
        }
    }

    private void HandleMissionUpdate(MissionData completedMission) //เรียกใช้ตอนทำเควสแต่ละอันเสร็จ
    {
        if (isCompleted) return;

        // --- เปลี่ยนให้เช็กตามเป้าหมายใหม่ ---
        MissionData targetCloseMission = overrideCloseMission != null ? overrideCloseMission : myMissionData;

        if (targetCloseMission != null && completedMission == targetCloseMission)
        {
            CloseWaypoint();
            return;
        }


        // ถ้าไม่ใช่เควสต์ตัวเอง ก็มาเช็กว่าตรงกับเงื่อนไขการ "เปิด" หรือไม่
        if (mode == WaypointMode.ShowAfterMission)
        {
            if (completedMission == prerequisiteMission) ActivateWaypoint();
        }
        else if (mode == WaypointMode.ShowWhenReadyToExit)
        {
            if (MissionManager.Instance.CanPlayerExit()) ActivateWaypoint();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (mode == WaypointMode.Proximity && !isDiscovered && !isCompleted && other.CompareTag("Player"))
        {
            ActivateWaypoint();
        }
    }

    private void ActivateWaypoint()
    {
        isDiscovered = true;
        if (waypointDisplay != null) waypointDisplay.SetActive(true);
    }

    // เปลี่ยนให้กลายเป็นฟังก์ชันส่วนตัว (private) เพราะมันจะสั่งปิดตัวเองอัตโนมัติแล้ว
    private void CloseWaypoint()
    {
        isCompleted = true;
        if (waypointDisplay != null) waypointDisplay.SetActive(false);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
}
