using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static enemy;


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
    enemy enemyMain_script;
    NavMeshAgent agent;
    public List<WorkTask> todoList = new List<WorkTask>();

    public LayerMask FriendNeraByMask;
    private bool TaskActivate = false;


    private void Awake()
    {

    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyMain_script = GetComponent<enemy>();
        FriendNeraByMask = LayerMask.GetMask("enemy");
    }



    void Update()
    {
        //print(todoList.Count);
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

        // เอาระยะหยุดมายกกำลังสอง (2 * 2 = 4) เพื่อเอาไปเทียบกับ sqrDistToTask
        if (!agent.pathPending && sqrDistToTask < (stopDistance * stopDistance))
        {
            Debug.Log("เดินมาถึงแล้ว! ลงมือทำงาน");
            ExecuteTask(currentTask);
            todoList.RemoveAt(0); // ทำเสร็จก็ลบทิ้ง
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
                Debug.Log("AI: หาสวิตไฟที่ปิดอยู่ " + task.position);
                break;

            case WorkTask.TaskType.TLight:
                // ตัวอย่าง: แปลง MonoBehaviour กลับเป็นสคริปต์ไฟแล้วสั่งเปิด
                var lightSW = task.targetObject as light_switch;
                if (lightSW != null) lightSW.Turn();

                Debug.Log("AI: กำลังจัดการกับไฟที่ " + task.position);
                break;

            case WorkTask.TaskType.TDoor:
                // ตัวอย่าง: แปลงเป็นสคริปต์ประตูแล้วสั่งปิด
                var door = task.targetObject as Door;
                if (door != null) door.ToggleDoor(true, Door.DoorState.Closed);
                Debug.Log("AI: กำลังจัดการกับประตูที่ " + task.position);
                break;

            case WorkTask.TaskType.wakeUp:
                var friend = task.targetObject as enemy;
                if (friend != null && friend.currentState == enemy.EnemyState.faint) friend.currentState = enemy.EnemyState.awake;
                //if (friend != null && friend.currentState == enemy.EnemyState.dead) Enemy_script.currentState = enemy.EnemyState.Alert;

                StartCoroutine(checkMate());
                Debug.Log("ทำการปลุกเพื่อน");
                break;

            case WorkTask.TaskType.alarm:
                Alarm();
                enemyMain_script.currentState = enemy.EnemyState.Alert;
                Debug.Log("Alarmmm!!!!");
                break;
        }
    }
    IEnumerator checkMate()
    {
        yield return new WaitForSeconds(2);
        Debug.Log("are you okay.....");



        enemyMain_script.currentState = EnemyState.report; //สั่งตัวเองให้เข้าเฟดinvestigate

        Debug.Log("List length =" + todoList.Count);
        //target_enemy_Script.currentState = enemy.EnemyState.awake; //สั่งให้ศัตรูคัวอื่น ให้ตื่น
        //target_enemy_Script = null;


    }

    void Alarm()
    {
        Collider[] friendNearBy = Physics.OverlapSphere(transform.position, 50f, FriendNeraByMask);

        float AlertRange = 20f;
        //float AlertSearchingRange = 50f;

        foreach (var coll in friendNearBy)
        {
            Vector3 dist = coll.transform.position - transform.position;
            var friendScript = coll.GetComponent<enemy>();

            if (friendScript.currentState != enemy.EnemyState.dead && friendScript.currentState != enemy.EnemyState.faint)
            {
                continue;
            }

            if (friendScript != null)
            {

                if (dist.sqrMagnitude <= (AlertRange * AlertRange))
                {

                    friendScript.currentState = enemy.EnemyState.Alert;
                }

            }
        }
    }

    public void ClearAllTasks()
    {
        todoList.Clear();
    }
}
