using UnityEngine;

public class EnemyInteraction : MonoBehaviour
{
    enemy_stage enemy_main;
    Enemy_Investigate investigate_script;
    Enemy_AlertSearching Enemy_AlertSearching_script;
    Enemy_Alert enemy_Alert_Script;
    Enemy_Task enemy_task;
    Enemy_Report enemy_report_script;
    EnemyRaycast enemy_raycast_script;
    Door Door_Obj;

    void Awake()
    {
        if (!TryGetComponent(out enemy_main)) Debug.LogWarning("ไม่พบ enemy_stage");
        if (!TryGetComponent(out investigate_script)) Debug.LogWarning("ไม่พบ Enemy_Investigate");
        if (!TryGetComponent(out enemy_Alert_Script)) Debug.LogWarning("ไม่พบ Enemy_Alert");
        if (!TryGetComponent(out Enemy_AlertSearching_script)) Debug.LogWarning("ไม่พบ Enemy_AlertSearching");
        if (!TryGetComponent(out enemy_task)) Debug.LogWarning("ไม่พบ Enemy_Task");
        if (!TryGetComponent(out enemy_report_script)) Debug.LogWarning("ไม่พบ Enemy_Report");
        if (!TryGetComponent(out enemy_raycast_script)) Debug.LogWarning("ไม่พบ enemy_raycast_script");
    }

    private void Update()
    {
        if (enemy_raycast_script.foundPlayer )
        {
            enemy_Alert_Script.Reset_AlerTimer();
        }
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

        if (other.CompareTag("Player"))
        {
            enemy_Alert_Script.Reset_AlerTimer();
            HandleNormalInteraction(other);
            return; // ตัดจบฟังก์ชันทันที! ศัตรูจะหูหนวกชั่วคราว ไม่สนใจเสียงเลย
        }

        NoisColliderCheck(other);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemy_main.currentState == enemy_stage.EnemyState.dead ||
            enemy_main.currentState == enemy_stage.EnemyState.faint ||
            enemy_main.currentState == enemy_stage.EnemyState.Dummy ||
            enemy_main.currentState == enemy_stage.EnemyState.OnGrab)
        {
            return;
        }

        NormalColliderCheck(other);

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

    void NormalColliderCheck(Collider col)
    {
        // 1. ถ้าชนผู้เล่น (ความสำคัญสูงสุด) ให้แจ้งเตือนทันที ไม่ว่าจะอยู่สเตทไหน
        if (col.CompareTag("Player"))
        {
            enemy_Alert_Script.Reset_AlerTimer();
            HandleNormalInteraction(col);
            //enemy_Alert_Script.Start_TriggerGroupAlert();
            Debug.Log("Check Player Collider");
            return; // เจอผู้เล่นแล้วไม่ต้องประมวลผลอย่างอื่นต่อ
        }

        if (col.CompareTag("enemy"))
        {
            HandleNormalInteraction(col);
            //enemy_Alert_Script.Start_TriggerGroupAlert();
            Debug.Log("Check Enemy Collider");
            return; // เจอผู้เล่นแล้วไม่ต้องประมวลผลอย่างอื่นต่อ
        }


    }

    void NoisColliderCheck(Collider col)
    {
        // 2. แยกการประมวลผลเสียงและเพื่อน ตามสถานะของ AI
        if (col.CompareTag("Noi"))
        {
            enemy_Alert_Script.Reset_AlerTimer();

            switch (enemy_main.currentState)
            {
                case enemy_stage.EnemyState.Alert:
                    // เฟสต่อสู้: ได้ยินเสียงก็แค่หันไปหา, ไม่สนเพื่อนที่สลบ                
                    enemy_Alert_Script.HandleNoiseAlert(col.transform.position);
                    break;

                case enemy_stage.EnemyState.report:
                    // เฟสกำลังวิทยุรายงาน: ไม่สนใจเสียงรบกวนย่อยๆ
                    break;

                default:
                    // เฟสปกติ หรือ กำลังเดินหา (Investigate)
                    Handle_Nois_Interactions(col);
                    break;
            }
        }
    }

    void HandleNormalInteraction(Collider col)
    {
        // 1. ถ้าชนผู้เล่น (ความสำคัญสูงสุด)
        if (col.CompareTag("Player"))
        {
            enemy_task.ClearAllTasks();

            // เช็กสถานะ: ถ้ากำลัง Alert ให้ยิงเผาขนเลย!
            if (enemy_main.currentState == enemy_stage.EnemyState.Alert)
            {
                enemy_Alert_Script.PointBlankShoot(col.transform.position);
                Debug.Log("ผู้เล่นเข้าประชิด! หยุดแล้วยิงสวนทันที!");
            }
            // ถ้าอยู่ในสถานะอื่น (เช่น เดินยาม, ค้นหา) ให้ตกใจและถอยร่นตามลอจิก Report
            else
            {
                enemy_main.currentState = enemy_stage.EnemyState.report;
                enemy_report_script.StartReportState(IncidentType.PlayerBump, col.transform.position);
                Debug.Log("ตกใจชนผู้เล่น! เข้าโหมด Report");
            }
            return;
        }

        // จัดการเรื่องปฏิสัมพันธ์กับเพื่อน
        if (col.CompareTag("enemy") && col.gameObject != this.gameObject)
        {
            // ถ้ากำลัง Alert อยู่ ให้เมินศพหรือเพื่อนที่สลบไปเลย ไม่ต้องสนใจ!
            if (enemy_main.currentState == enemy_stage.EnemyState.Alert)
            {
                return; // ตัดจบฟังก์ชัน ศัตรูจะเดินข้ามเพื่อนไปสู้ต่อ
            }

            var otherEnemy = col.GetComponent<enemy_stage>();

            if (otherEnemy != null)
            {
                if (enemy_main.currentState != enemy_stage.EnemyState.report)
                {
                    if (otherEnemy.currentState == enemy_stage.EnemyState.faint)
                    {
                        enemy_task.AddToTodoList(col.transform.position, otherEnemy, WorkTask.TaskType.wakeUp);
                        enemy_main.currentState = enemy_stage.EnemyState.Investigate;
                    }
                    else if (otherEnemy.currentState == enemy_stage.EnemyState.dead)
                    {
                        enemy_task.AddToTodoList(col.transform.position, otherEnemy, WorkTask.TaskType.alarm);
                        enemy_main.currentState = enemy_stage.EnemyState.Investigate;
                    }
                }
            }
        }
    }

    void Handle_Nois_Interactions(Collider col)
    {
        // จัดการเรื่องเสียงรบกวน
        if (col.CompareTag("Noi"))
        {
            //enemy_task.ClearAllTasks();

            if (enemy_main.currentState != enemy_stage.EnemyState.Alert && enemy_main.currentState != enemy_stage.EnemyState.alertSearching)
            {
                enemy_main.currentState = enemy_stage.EnemyState.Investigate;
                investigate_script.searcingLastHearPosition(col.transform.position);
            }
        }
    }
   
}