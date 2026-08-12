using System.Collections.Generic;
using UnityEngine;

// กล่องจำความคืบหน้าของแต่ละภารกิจ
[System.Serializable]
public struct MissionSaveState
{
    public string missionName;
    public bool isCompleted;
    public int currentAmount;
}

// กล่องใหญ่เก็บรายการเควสต์ทั้งหมด
[System.Serializable]
public struct MissionManagerSaveData
{
    public List<MissionSaveState> savedMissions;
}

public class Mission_SaveData : MonoBehaviour, Isaveable
{
    private SaveableEntity saveEntity;
    private MissionManager missionManager_Script;

    private void Awake()
    {
        if (!TryGetComponent<SaveableEntity>(out saveEntity))
            Debug.Log("!!!!!! MissionManager_SaveData หา --SaveableEntity-- ไม่เจอ !!!!!!");

        if (!TryGetComponent<MissionManager>(out missionManager_Script))
            Debug.Log("!!!!!! MissionManager_SaveData หา --MissionManager-- ไม่เจอ !!!!!!");
    }

    public string GetSaveID()
    {
        return saveEntity.uniqueID;
    }

    public string SaveState()
    {
        MissionManagerSaveData dataBox = new MissionManagerSaveData();
        dataBox.savedMissions = new List<MissionSaveState>();

        if (missionManager_Script != null)
        {
            // ดึงข้อมูลความคืบหน้าจาก MissionManager มาแพ็คลงกล่อง
            foreach (MissionData mission in missionManager_Script.activeMissions)
            {
                MissionSaveState miniState = new MissionSaveState();
                miniState.missionName = mission.missionName;
                miniState.isCompleted = mission.isCompleted;
                miniState.currentAmount = mission.currentAmount;

                dataBox.savedMissions.Add(miniState);
            }
        }

        return JsonUtility.ToJson(dataBox);
    }

    public void LoadState(string stateData)
    {
        MissionManagerSaveData dataBox = JsonUtility.FromJson<MissionManagerSaveData>(stateData);

        if (missionManager_Script != null)
        {
            // จับคู่ชื่อเควสต์ แล้วเขียนทับความคืบหน้า
            foreach (MissionSaveState savedMission in dataBox.savedMissions)
            {
                foreach (MissionData activeMission in missionManager_Script.activeMissions)
                {
                    if (activeMission.missionName == savedMission.missionName)
                    {
                        activeMission.isCompleted = savedMission.isCompleted;
                        activeMission.currentAmount = savedMission.currentAmount;
                        break;
                    }
                }
            }
        }
    }
}