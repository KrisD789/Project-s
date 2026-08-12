using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

// กำหนดประเภทของภารกิจ
public enum MissionType
{
    ReachLocation,  // เดินไปถึงจุดที่กำหนด (เช่น จุดหนี)
    InteractObject, //เก็บปืน ในโหมดฝึก
    Hack,// กดใช้งานของ (เช่น แฮ็กคอมพิวเตอร์, ขโมยข้อมูล)
    CaptureTarget, // จับกุมเป้าหมาย
    EliminateEnemies, //จัดการเป้าหมาย
    Extraction // หนีออกจากพื้นที่
}

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;
    public List<MissionData> activeMissions = new List<MissionData>();

    // 1. สร้างช่องทางประกาศข่าว (ส่งไฟล์ MissionData ไปกับข่าวด้วย)
    public event Action<MissionData> OnMissionComplete;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        foreach (MissionData mission in activeMissions)
        {
            if (mission != null)
            {
                mission.isCompleted = false; // เอาเครื่องหมายติ๊กถูกออกให้หมด
                mission.currentAmount = 0;   // รีเซ็ตยอดนับจำนวนศัตรู (ถ้ามี) ให้กลับเป็นศูนย์
            }
        }
    }

    public void OnMissionEventReceived(MissionData incomingMission)
    {
        if (activeMissions.Contains(incomingMission) && !incomingMission.isCompleted)
        {
            incomingMission.isCompleted = true;
            Debug.Log("ภารกิจสำเร็จ: " + incomingMission.missionName);

            // 2. ประกาศข่าวออกไป! ว่าเควสต์นี้สำเร็จแล้ว (ถ้ามีคนรอฟังอยู่)
            OnMissionComplete?.Invoke(incomingMission);
        }
    }

    // สำหรับเควสต์กำจัดศัตรู (ถ้าใช้งาน)
    public void OnEnemyEliminated()
    {
        foreach (MissionData mission in activeMissions)
        {
            if (mission.type == MissionType.EliminateEnemies && !mission.isCompleted)
            {
                mission.currentAmount++;
                if (mission.currentAmount >= mission.targetAmount)
                {
                    mission.isCompleted = true;
                    Debug.Log("ภารกิจสำเร็จ: " + mission.missionName);

                    // ประกาศข่าวออกไปเช่นกัน!
                    OnMissionComplete?.Invoke(mission);
                }
            }
        }
    }

    public void RegisterMission(MissionData newMission) // ฟังก์ชันสำหรับให้ห้องต่างๆ ส่งเควสต์มาลงทะเบียนตอนเริ่มเกม
    {
        // เช็คก่อนว่าในลิสต์มีเควสต์นี้อยู่แล้วหรือยัง (กันการใส่ซ้ำ)
        if (!activeMissions.Contains(newMission))
        {
            activeMissions.Add(newMission);
            Debug.Log("ลงทะเบียนเควสต์ใหม่เข้าระบบอัตโนมัติ: " + newMission.missionName);
        }
    }

    public bool CanPlayerExit()
    {
        foreach (MissionData mission in activeMissions)
        {
            if (mission.isRequiredForExit && !mission.isCompleted) return false;
        }
        return true;
    }
}
