using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TaskSaveData
{
    public Vector3 taskPosition;
    public int taskType; // เก็บ Enum เป็น int เพื่อความปลอดภัย
    public string targetID;
}

[System.Serializable]
public struct EnemyDataBox
{
    public Vector3 position;
    public Vector3 rotation;
    public float EnemyHp;

    public int EnemyCurrentState;
    public bool enemy_WasFaint;

    // เพิ่มช่องเก็บ Index การเดินและจำนวนรอบค้นหา
    public int currentPatrolIndex;
    public int currentSearchCount;

    public List<TaskSaveData> SaveToDoList;

    public bool isHoldingAttackToken;
}
public class Enemy_SaveData : MonoBehaviour, Isaveable
{
    Enemy enemy_script;
    enemy_stage enemy_stage_System;
    Enemy_Task Enemy_Task_System;
    Enemypatro Enemy_patro_System;
    Enemy_Investigate Enemy_Investigate_System;

    private SaveableEntity saveEntity;


    private void Awake()
    {
        if(!TryGetComponent<Enemy>(out enemy_script)) Debug.LogWarning("Enemy_SaveData:  หา enemy_script ไม่เจอ!");
        if (!TryGetComponent<enemy_stage>(out enemy_stage_System)) Debug.LogWarning("Enemy_SaveData:  หา enemy_stage ไม่เจอ!");
        if (!TryGetComponent<Enemy_Task>(out Enemy_Task_System)) Debug.LogWarning("Enemy_SaveData:  หา enemy_Task ไม่เจอ!");
        if (!TryGetComponent<Enemypatro>(out Enemy_patro_System)) Debug.LogWarning("Enemy_SaveData:  หา Enemy_patro ไม่เจอ!");
        if (!TryGetComponent<Enemy_Investigate>(out Enemy_Investigate_System)) Debug.LogWarning("Enemy_SaveData:  หา Enemy_Investigate ไม่เจอ!");

        if (!TryGetComponent<SaveableEntity>(out saveEntity)) Debug.LogWarning("Enemy_SaveData:  หา saveEntity ไม่เจอ!");
    }

    public string GetSaveID()
    {
        return saveEntity.uniqueID;
    }

    public string SaveState()
    {
        EnemyDataBox dataBox = new EnemyDataBox();

        dataBox.SaveToDoList = new List<TaskSaveData>();

        dataBox.isHoldingAttackToken = Enemy_combatManager.Instance.HasToken(this.gameObject);

        dataBox.position = transform.position;
        dataBox.rotation = transform.eulerAngles;
        dataBox.EnemyHp = enemy_script.Enemy_Health;

        if(enemy_stage_System != null)
        {
            dataBox.EnemyCurrentState = (int)enemy_stage_System.currentState;
            dataBox.enemy_WasFaint = enemy_stage_System.wasFaint;
        }

        if (Enemy_patro_System != null && Enemy_Investigate_System != null)
        {
            dataBox.currentPatrolIndex = Enemy_patro_System.index;
            dataBox.currentSearchCount = Enemy_Investigate_System.currentSearchCount;
        }

        if (Enemy_Task_System != null && Enemy_Task_System.todoList.Count > 0)
        {
            foreach (WorkTask task in Enemy_Task_System.todoList)
            {
                TaskSaveData taskData = new TaskSaveData();
                taskData.taskPosition = task.position;
                taskData.taskType = (int)task.currentType;

                // ขอตรวจดูบัตรประชาชนของเป้าหมายหน่อย
                if (task.targetObject != null)
                {
                    SaveableEntity entity = task.targetObject.GetComponent<SaveableEntity>();
                    if (entity != null)
                    {
                        taskData.targetID = entity.uniqueID; // ดึงรหัสมาเก็บไว้!
                    }
                }

                dataBox.SaveToDoList.Add(taskData);
            }
        }

        return JsonUtility.ToJson(dataBox);
    }
    public void LoadState(string stateData)
    {
        EnemyDataBox dataBox = JsonUtility.FromJson<EnemyDataBox>(stateData);

        // 2. คืนค่าพิกัด หมุนตัว และพลังชีวิต
        transform.position = dataBox.position;
        transform.eulerAngles = dataBox.rotation;

        if (dataBox.isHoldingAttackToken)
        {
            Enemy_combatManager.Instance.RestoreToken(this.gameObject);
        }

        if (enemy_script != null)
        {
            enemy_script.Enemy_Health = dataBox.EnemyHp;
        }

        if (enemy_stage_System != null)
        {
            enemy_stage_System.currentState = (enemy_stage.EnemyState)dataBox.EnemyCurrentState;
            enemy_stage_System.wasFaint = dataBox.enemy_WasFaint;
        }

        if (Enemy_patro_System != null && Enemy_Investigate_System != null)
        {
            Enemy_patro_System.index = dataBox.currentPatrolIndex;
            Enemy_Investigate_System.currentSearchCount = dataBox.currentSearchCount;
        }

        // 3. คืนค่างานใน TodoList
        if (Enemy_Task_System != null && dataBox.SaveToDoList != null)
        {
            // ล้างงานเก่าในหัว AI ทิ้งก่อน เพื่อรับงานจากเซฟ
            Enemy_Task_System.todoList.Clear();

            // กวาดหา SaveableEntity ทั้งฉากมารอไว้ (ค้นหาบัตรประชาชน)
            SaveableEntity[] allEntitiesInScene = FindObjectsByType<SaveableEntity>(FindObjectsSortMode.None);

            foreach (TaskSaveData savedTask in dataBox.SaveToDoList)
            {
                // สร้างงานชิ้นใหม่เตรียมส่งให้ AI
                WorkTask newTask = new WorkTask();
                newTask.position = savedTask.taskPosition;
                newTask.currentType = (WorkTask.TaskType)savedTask.taskType; // แปลง int กลับเป็น Enum

                // เอารหัสบัตรประชาชน ไปตามหาตัวตนจริงๆ ในฉาก
                if (!string.IsNullOrEmpty(savedTask.targetID))
                {
                    foreach (SaveableEntity entity in allEntitiesInScene)
                    {
                        if (entity.uniqueID == savedTask.targetID)
                        {
                            // เจอตัวแล้ว ดึง MonoBehaviour กลับมาใส่ให้ AI
                            newTask.targetObject = entity.GetComponent<MonoBehaviour>();
                            break; // เจอแล้วก็หยุดค้นหาชิ้นนี้
                        }
                    }
                }

                // เอางานที่ประกอบร่างเสร็จแล้ว ยัดกลับเข้าหัว AI
                Enemy_Task_System.todoList.Add(newTask);
            }
        }
    }

   

    
}
