using System.Collections.Generic;
using UnityEngine;

// 1. สร้าง "แพ็กเกจ" สำหรับจับคู่เควสต์กับประตู (ต้องใส่ Serializable ถึงจะโชว์ใน Unity)
[System.Serializable]
public class QuestDoorPair
{
    public MissionData questData;
    public Door doorToOpen;
}

public class TutorialRoom : MonoBehaviour
{
    [Header("รายการเควสต์และประตูในห้องนี้")]
    // 2. ใช้ List เพื่อให้เพิ่มลดจำนวนได้อิสระในหน้า Inspector
    public List<QuestDoorPair> roomQuests = new List<QuestDoorPair>();

    private void Start()
    {
        if (MissionManager.Instance != null)
        {
            // 1. กดติดตามระบบประกาศข่าว (เหมือนเดิม)
            MissionManager.Instance.OnMissionComplete += HandleMissionCompleted;

            // 2. ลูปเอาเควสต์ทั้งหมดในห้องนี้ ไปยัดใส่ MissionManager แบบอัตโนมัติ!
            foreach (QuestDoorPair pair in roomQuests)
            {
                if (pair.questData != null)
                {
                    MissionManager.Instance.RegisterMission(pair.questData);
                }
            }
        }
    }

    private void HandleMissionCompleted(MissionData completedMission) 
    {
        // 3. ใช้ลูปเช็คว่าเควสต์ที่เพิ่งเสร็จ ตรงกับแพ็กเกจไหนใน List ของเราบ้าง
        foreach (QuestDoorPair pair in roomQuests)
        {
            if (pair.questData != null && completedMission == pair.questData)
            {
                if (pair.doorToOpen != null)
                {
                    pair.doorToOpen.Automatic_Door_for_Tutorial_Map();
                    Debug.Log($"ผ่านบททดสอบ: ประตูของเควสต์ {completedMission.missionName} เปิดออกแล้ว!");
                }
            }
        }
    }
   

    private void OnDestroy()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionComplete -= HandleMissionCompleted;
        }
    }
}