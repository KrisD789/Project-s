using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static enemy_stage;


// สร้างคลาสเก็บข้อมูลงานเล็กๆ
[System.Serializable]
public class WorkTask
{
    public Vector3 position;
    public MonoBehaviour targetObject;
    public enum TaskType { TLight, TDoor, wakeUp, alarm, Slight }
    public TaskType currentType;
}

public class Enemy_Task : MonoBehaviour
{
    enemy_stage enemyMain_script;
    NavMeshAgent agent;
    public List<WorkTask> todoList = new List<WorkTask>();

    public LayerMask FriendNeraByMask;
    private bool TaskActivate = false;

    private Coroutine wakeUpCoroutine;


    // --- ตัวแปรสำหรับระบบจับเวลาปลุกเพื่อน ---
    private bool isWakingUpFriend = false;
    private float wakeUpTimer = 0f;
    //private float wakeUpDuration = 2.0f; // ใช้เวลาปลุก 2 วินาที
    private enemy_stage friendToWake;    // เก็บเป้าหมายว่ากำลังปลุกใครอยู่
    Enemy_Report enemy_report_script;


    private void Awake()
    {

    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyMain_script = GetComponent<enemy_stage>();
        enemy_report_script = GetComponent<Enemy_Report>();
        FriendNeraByMask = LayerMask.GetMask("enemy");
    }



    void Update()
    {
        if (enemyMain_script.currentState == enemy_stage.EnemyState.faint ||
             enemyMain_script.currentState == enemy_stage.EnemyState.dead ||
             enemyMain_script.currentState == enemy_stage.EnemyState.OnGrab)
        {
            if (wakeUpCoroutine != null) StopCoroutine(wakeUpCoroutine);

            if (todoList.Count > 0)
            {
                todoList.Clear();
            }

            TaskActivate = false; // รีเซ็ตตัวล็อค
        }

        // 2. ลอจิกการนับเวลาปลุกเพื่อน
        //if (isWakingUpFriend)
        //{
        // เซฟตี้: ดักไว้เผื่อเพื่อนตาย/หรือตื่นเองไปแล้วระหว่างที่เรากำลังยืนปลุก
        //if (friendToWake == null || friendToWake.currentState != enemy_stage.EnemyState.faint)
        //{
        //CancelWakeUpTimer();
        //return;
        //}

        // เริ่มนับเวลา
        //wakeUpTimer += Time.deltaTime;

        // ถ้านับครบ 2 วินาทีแล้ว
        //if (wakeUpTimer >= wakeUpDuration)
        //{
        //FinishWakingUpFriend();
        //}
        //}
    }

    public void StartDoingTask()
    {
        // 1. ถ้าไม่มีงาน ก็ไม่ต้องทำอะไร (ป้องกัน Error)
        if (todoList.Count == 0) return;

        var currentTask = todoList[0];

        // 2. สั่งเดินไปที่เป้าหมาย
        if (agent.destination != currentTask.position)
        {
            agent.SetDestination(currentTask.position);
        }
        // 3. เช็คระยะห่างด้วย sqrMagnitude เพื่อความลื่นไหลของเกม
        Vector3 offset = transform.position - currentTask.position;
        // ตัดแกน Y ทิ้งก่อนคำนวณ (ถ้าจำเป็น) เพื่อให้วัดแค่ระยะแนวราบ ป้องกัน AI ยืนอยู่คนละความสูงแล้วหาไม่เจอ
        offset.y = 0;

        float sqrDistToTask = offset.sqrMagnitude;
        float stopDistance = 2.0f; // ระยะที่ต้องการให้เบรก (2 เมตร)

        // เพิ่มเงื่อนไข !TaskActivate เพื่อเช็คว่าตอนนี้ไม่ได้กำลังทำงานอื่นอยู่
        if (!agent.pathPending && sqrDistToTask < (stopDistance * stopDistance))
        {
            if (!TaskActivate)
            {
                TaskActivate = true; // ล็อคทันที! ป้องกันการรันซ้ำในเฟรมถัดไป
                Debug.Log("เดินมาถึงแล้ว! ลงมือทำงาน");
                ExecuteTask(currentTask);
            }
        }

    }

    public void AddToTodoList(Vector3 pos, MonoBehaviour script, WorkTask.TaskType typ)
    {
        // 1. เช็คก่อนว่างานนี้มีในลิสต์หรือยัง จะได้ไม่จดซ้ำ
        if (!todoList.Exists(x => x.targetObject == script))
        {
            WorkTask newTask = new WorkTask { position = pos, targetObject = script, currentType = typ };

            // 2. ถ้าเป็นงานปลุกเพื่อน (wakeUp) ให้ "แซงคิว" ไปอยู่อันดับ 1 ทันที
            if (typ == WorkTask.TaskType.wakeUp)
            {
                todoList.Insert(0, newTask); // แทรกที่ตำแหน่ง 0 (หน้าสุด)
                Debug.Log("Priority: ตรวจพบเพื่อนสลบ! แซงคิวงานอื่นทันที");
            }
            else
            {
                todoList.Add(newTask); // งานทั่วไป (ไฟ/ประตู) ต่อท้ายแถวตามปกติ
            }
        }
    }

    public void ExecuteTask(WorkTask task)
    {
        if (task == null || task.targetObject == null) 
        {
            Debug.Log("ExecuteTask : ไม่พบสริปจาก Todo list");
            return; 
        }

        switch (task.currentType)
        {
            case WorkTask.TaskType.Slight:   //ทำงานคู่กับเงื่อนไขcase ต่อไป Tlight
                // ดึงสคริปต์ LightZone จากงานที่ AI เพิ่งเดินมาถึง
                var light = task.targetObject as LightZone;
                if (light != null && !light.lightZoneState)
                {
                    // ถ้าหลอดไฟนี้บอกเราได้ว่าสวิตช์อยู่ที่ไหน
                    if (light.masterSwitch != null)
                    {
                        Debug.Log("AI: รู้แล้วว่าสวิตช์ไฟดวงนี้อยู่ตรงไหน! กำลังเดินไป...");

                        // สั่งให้เดินไปที่ตำแหน่งของสวิตช์นั้นจริงๆ และเปลี่ยนประเภทงานเป็น TLight
                        AddToTodoList(light.masterSwitch.transform.position, light.masterSwitch, WorkTask.TaskType.TLight);
                    }
                }
                Clear_Current_Task();
                Debug.Log("AI: หาสวิตไฟที่ปิดอยู่ " + task.position);
                break;

            case WorkTask.TaskType.TLight:
                // ตัวอย่าง: แปลง MonoBehaviour กลับเป็นสคริปต์ไฟแล้วสั่งเปิด
                var lightSW = task.targetObject as light_switch;
                if (lightSW != null) lightSW.Turn();

                Clear_Current_Task();
                Debug.Log("AI: กำลังจัดการกับไฟที่ " + task.position);
                break;

            case WorkTask.TaskType.TDoor:
                // ตัวอย่าง: แปลงเป็นสคริปต์ประตูแล้วสั่งปิด
                var door = task.targetObject as Door;
                if (door != null) door.ToggleDoor(true, Door.DoorState.Closed);

                Clear_Current_Task();
                Debug.Log("AI: กำลังจัดการกับประตูที่ " + task.position);
                break;

            case WorkTask.TaskType.wakeUp:
                var friend = task.targetObject as enemy_stage;
                if (friend != null && friend.currentState == enemy_stage.EnemyState.faint)
                {
                    // เก็บค่า Coroutine ลงตัวแปร
                    if (wakeUpCoroutine != null)
                    {
                        StopCoroutine(wakeUpCoroutine);
                    }
                    wakeUpCoroutine = StartCoroutine(WakeUp(friend));
                }

                Debug.Log("ทำการปลุกเพื่อน");

                //Clear_Current_Task();
                break;

            case WorkTask.TaskType.alarm:
                Alarm();
                enemyMain_script.currentState = enemy_stage.EnemyState.Alert;

                Clear_Current_Task();
                Debug.Log("Alarmmm!!!!");
                break;
        }
    }
    IEnumerator WakeUp(enemy_stage enemyFriend_Stage_script)
    {
        var currentTask = todoList[0];

        yield return new WaitForSeconds(5);
        Debug.Log("are you okay.....");
        enemyFriend_Stage_script.currentState = enemy_stage.EnemyState.awake;
        enemyMain_script.currentState = EnemyState.report; //สั่งตัวเองให้เข้าเฟดReport
        enemy_report_script.StartReportState(IncidentType.FoundUnconscious, currentTask.position);

        ClearAllTasks();
        Debug.Log("List length =" + todoList.Count);
        //target_enemy_Script.currentState = enemy.EnemyState.awake; //สั่งให้ศัตรูคัวอื่น ให้ตื่น
        //target_enemy_Script = null;


    }

    void Alarm()
    {
        enemyMain_script.currentState = enemy_stage.EnemyState.report;

        ClearAllTasks();
    }

    void Clear_Current_Task()
    {
        if (todoList.Count > 0)
        {
            todoList.RemoveAt(0); // ทำเสร็จก็ลบทิ้ง
        }

        TaskActivate = false; // ปลดล็อคให้ AI พร้อมทำงานชิ้นต่อไป!
    }

    public void ClearAllTasks()
    {
        todoList.Clear();
    }

    void CancelWakeUpTimer()
    {
        if (isWakingUpFriend)
        {
            isWakingUpFriend = false;
            wakeUpTimer = 0f;
            friendToWake = null;
            Debug.Log("ยกเลิกการนับเวลาปลุกเพื่อนกลางคัน!");
        }
    }

    void FinishWakingUpFriend()
    {
        isWakingUpFriend = false;
        wakeUpTimer = 0f;

        if (friendToWake != null)
        {
            Debug.Log("are you okay.....");
            friendToWake.currentState = enemy_stage.EnemyState.awake;

            // สั่งตัวเองให้เข้าโหมด report หลังจากปลุกเพื่อนเสร็จ
            enemyMain_script.currentState = EnemyState.report;

            Debug.Log("List length =" + todoList.Count);
        }

        friendToWake = null; // คืนค่าเป้าหมาย
    }
}
