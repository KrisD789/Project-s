using UnityEngine;

public class EnemyInteraction : MonoBehaviour
{
    enemy_stage enemy_main;
    Enemy_Investigate investigate_script;
    Enemy_Alert alert_script;
    Enemy_AlertSearching Enemy_AlertSearching_script;
    Enemy_Task enemy_task;

    Door Door_Obj;

    void Awake()
    {
        enemy_main = GetComponent<enemy_stage>();
        investigate_script = GetComponent<Enemy_Investigate>();
        alert_script = GetComponent<Enemy_Alert>();
        Enemy_AlertSearching_script = GetComponent<Enemy_AlertSearching>();
        enemy_task = GetComponent<Enemy_Task>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (enemy_main.currentState == enemy_stage.EnemyState.dead ||
            enemy_main.currentState == enemy_stage.EnemyState.faint ||
            enemy_main.currentState == enemy_stage.EnemyState.Dummy ||
            enemy_main.currentState == enemy_stage.EnemyState.OnGrab)
        {
            return;
        }

        noiColliderCheck(other);
    }

    void noiColliderCheck(Collider col)
    {
        // 1. ถ้าชนผู้เล่น (ความสำคัญสูงสุด) ให้แจ้งเตือนทันที ไม่ว่าจะอยู่สเตทไหน
        if (col.CompareTag("Player"))
        {
            enemy_main.currentState = enemy_stage.EnemyState.Alert;
            Debug.Log("Alert !!!!");
            return; // เจอผู้เล่นแล้วไม่ต้องประมวลผลอย่างอื่นต่อ
        }

        // 2. แยกการประมวลผลเสียงและเพื่อน ตามสถานะของ AI
        switch (enemy_main.currentState)
        {
            case enemy_stage.EnemyState.Alert:
                // เฟสต่อสู้: ได้ยินเสียงก็แค่หันไปหา, ไม่สนเพื่อนที่สลบ
                if (col.CompareTag("Noi"))
                {
                    alert_script.HandleNoiseAlert(col.transform.position);
                }
                break;

            case enemy_stage.EnemyState.report:
                // เฟสกำลังวิทยุรายงาน: ไม่สนใจเสียงรบกวนย่อยๆ
                break;

            default:
                // เฟสปกติ หรือ กำลังเดินหา (Investigate)
                HandleNormalInteraction(col);
                break;
        }
    }

    void HandleNormalInteraction(Collider col)
    {
        // จัดการเรื่องเสียงรบกวน
        if (col.CompareTag("Noi") && enemy_main.currentState != enemy_stage.EnemyState.Alert)
        {
            enemy_task.ClearAllTasks();
            enemy_main.currentState = enemy_stage.EnemyState.Investigate;
            investigate_script.searcingLastHearPosition(col.transform.position);
        }

        // จัดการเรื่องปฏิสัมพันธ์กับเพื่อน
        if (col.CompareTag("enemy") && col.gameObject != this.gameObject)
        {
            var otherEnemy = col.GetComponent<enemy_stage>();
            if (otherEnemy != null)
            {
                if (otherEnemy.currentState == enemy_stage.EnemyState.faint)
                {
                    enemy_task.AddToTodoList(col.transform.position, otherEnemy, WorkTask.TaskType.wakeUp);
                    enemy_main.currentState = enemy_stage.EnemyState.Investigate;
                }
                else if (otherEnemy.currentState == enemy_stage.EnemyState.dead)
                {
                    enemy_main.currentState = enemy_stage.EnemyState.Alert;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Door"))
        {
            // ใช้ TryGetComponent เพื่อป้องกัน NullReferenceException
            if (other.TryGetComponent<Door>(out Door_Obj))
            {
                Door_Obj.ToggleDoor(true, Door.DoorState.Open);
                Debug.Log("open the Door");
            }
            else
            {
                // แจ้งเตือนไว้ เผื่อลืมแปะสคริปต์ Door ไว้ที่ Collider
                Debug.LogWarning($"AI ชนวัตถุชื่อ {other.gameObject.name} ที่มี Tag 'Door' แต่หาคอมโพเนนต์ Door ไม่เจอ!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Door"))
        {
            Door door = other.GetComponent<Door>();
            if (door != null)
            {
                door.ToggleDoor(true, Door.DoorState.Closed);
                Debug.Log("close the Door");
            }
            Door_Obj = null;
        }
    }
}