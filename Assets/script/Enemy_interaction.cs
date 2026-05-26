using UnityEngine;

public class EnemyInteraction : MonoBehaviour
{
    enemy enemy_main;
    Enemy_Investigate investigate_script;
    Enemy_Alert alert_script;
    Enemy_AlertSearching Enemy_AlertSearching_script;
    Enemy_Task enemy_task;

    Door Door_Obj;
    void Awake()
    {
        // ดึง Script อื่นๆ มาเก็บไว้เพื่อสั่งการ
        enemy_main = GetComponent<enemy>();
        investigate_script = GetComponent<Enemy_Investigate>();
        alert_script = GetComponent<Enemy_Alert>();
        Enemy_AlertSearching_script = GetComponent<Enemy_AlertSearching>();
        enemy_task = GetComponent<Enemy_Task>();

    }

    private void OnTriggerStay(Collider other)
    {
        if (enemy_main.currentState == enemy.EnemyState.dead || 
            enemy_main.currentState == enemy.EnemyState.faint || 
            enemy_main.currentState == enemy.EnemyState.Alert)
        {
            return;
        }

        else noiColliderCheck(other);       
    }

    void noiColliderCheck(Collider col)
    {

        if (col.CompareTag("Noi"))
        {
            if (enemy_main.currentState == enemy.EnemyState.report) return;

            if (enemy_main.currentState == enemy.EnemyState.Alert)
            {

            }

            else
            {
                // 1. เคลียร์งานเก่าทิ้ง (หยุดเดินค้นรอบๆ หรือหยุดเข้าที่กำบัง)
                enemy_task.ClearAllTasks();

                // 2. สลับลงมา Investigate ได้เลย! (เพราะเดี๋ยวทำเสร็จมันก็เด้งกลับไปเอง)
                enemy_main.currentState = enemy.EnemyState.Investigate;
                investigate_script.searcingLastHearPosition(col.transform.position);
            }

           
        }

        if (col.CompareTag("Player"))
        {
            // ถ้าศัตรูเห็นผู้เล่น ให้เปลี่ยนสถานะและสั่งให้ Alert Script ทำงาน
            enemy_main.currentState = enemy.EnemyState.Alert;

            // นายสามารถสั่งตั้งค่า Behavior ใน Alert Script ได้จากตรงนี้เลย
            // เช่น ถ้าเจอระยะประชิด ให้เข้า Cover ถ้าเจอไกลให้ Flank
            //if (Vector3.Distance(transform.position, other.transform.position) < 5f)
            //alert_script.Behavior = Enemy_Alert.AlertBehave.cover;
            //else
            //alert_script.Behavior = Enemy_Alert.AlertBehave.flank;
        }

        // 3. การปฏิสัมพันธ์กับเพื่อน (Enemy)
        if (col.CompareTag("enemy") && col.gameObject != this.gameObject)
        {
            var otherEnemy = col.GetComponent<enemy>();
            if (otherEnemy != null)
            {
                // ถ้าเจอเพื่อนสลบ -> ส่งงานไปให้ระบบ TodoList ของ Investigate
                if (otherEnemy.currentState == enemy.EnemyState.faint)
                {
                    enemy_task.AddToTodoList(col.transform.position, otherEnemy, WorkTask.TaskType.wakeUp);
                    enemy_main.currentState = enemy.EnemyState.Investigate;
                }

                // ถ้าเจอเพื่อนตาย -> สั่ง Alert ทันที (Man Down!)
                if (otherEnemy.currentState == enemy.EnemyState.dead)
                {
                    enemy_main.currentState = enemy.EnemyState.Alert;
                }
            }
        }


    }
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Door"))
        {
            Door_Obj = other.GetComponent<Door>();

            Door_Obj.ToggleDoor(true, Door.DoorState.Open);
            Debug.Log("open the Door");
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Door"))
        {
            Door door = other.GetComponent<Door>(); // ดึงใหม่เพื่อความชัวร์
            if (door != null)
            {
                door.ToggleDoor(true, Door.DoorState.Closed);
                Debug.Log("close the Door");
            }
            Door_Obj = null;
        }
    }
}