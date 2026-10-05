using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class Enemypatro : MonoBehaviour
{
    enemy_stage enemy_script; //
    NavMeshAgent agent; //
    public float waitTime = 3; 
    float Timer = 0; 
    public int index = 0; 
    bool isWaiting = false; 
    public Transform[] wayPoint; 

    [Header("ตั้งค่าการหมุน")]
    public float turnSpeed = 5f; // เพิ่มความเร็วในการหันหน้าให้สมูท
    public float RangAngle = 45; 
    public float swingSpeed = 1; 
    float baseAngle; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); 
        enemy_script = GetComponent<enemy_stage>();

        // ---  เซฟตี้ดักไว้เผื่อลืมใส่ Waypoint จะได้ไม่พัง ---
        if (wayPoint != null && wayPoint.Length > 0)
        {
            agent.SetDestination(wayPoint[0].position);
            Debug.Log($"[Patrol AI] เริ่มต้นทำงาน! กำลังเดินไปประจำที่ Waypoint [0]");
        }
        else
        {
            Debug.LogWarning("[Patrol AI] แจ้งเตือน! ศัตรูตัวนี้ยังไม่ได้ใส่ Waypoint เลย ระบบลาดตระเวนจะไม่ทำงาน");
        }
    }

    void Update()
    {
        if (enemy_script.currentState == enemy_stage.EnemyState.Patrol) 
        {
            if (index >= wayPoint.Length) index = 0; 

            float distToTarget = Vector3.Distance(transform.position, wayPoint[index].position); 

            if (!agent.pathPending && distToTarget < 2f) 
            {
                isWaiting = true; 
                patroLogic(); 
            }
            else
            {
                patroLogic(); 
            }
        }

        else
        {
            // --- ป้องกันบั๊ก: เมื่อเปลี่ยนไปสถานะอื่น (เช่น ไล่ล่า, ค้นหา) ---

            // คืนสิทธิ์การหันหน้าให้ NavMesh ทันที
            agent.updateRotation = true;

            // เคลียร์สถานะการรอ เพื่อให้เวลากลับมา Patrol ใหม่ระบบไม่ค้าง
            isWaiting = false; 
            Timer = 0; 
        }
    }

    void patroLogic()
    {
        if (isWaiting) 
        {
            // 1. ปิดไม่ให้ NavMesh ควบคุมการหันหน้าตอนกำลังยืนรอ
            agent.updateRotation = false;

            // 2. ค่อยๆ หมุนหน้าศัตรู (แกน Z) ให้ตรงกับแกน Z ของ Waypoint ปัจจุบัน
            transform.rotation = Quaternion.Slerp(transform.rotation, wayPoint[index].rotation, turnSpeed * Time.deltaTime);

            

            //  จุดเดียวที่ดัดแปลง: ถ้ามี Waypoint มากกว่า 1 จุด ถึงจะอนุญาตให้นับเวลาและเดินสลับจุด
            if (wayPoint.Length > 1)
            {
                Timer += Time.deltaTime;

                if (Timer >= waitTime)
                {
                    index++;

                    // ดัก index เกินไว้ตรงนี้เพื่อป้องกัน error ก่อนสั่งเดิน
                    if (index >= wayPoint.Length) index = 0;

                    // 3. คืนสิทธิ์การหันหน้าให้ NavMeshAgent ตอนเริ่มเดินไปจุดใหม่
                    agent.updateRotation = true;
                    agent.SetDestination(wayPoint[index].position);

                    Timer = 0;
                    isWaiting = false;
                }
            }
        }
        else
        {
            // ปรับปรุงการสั่งเดินเพื่อไม่ให้ NavMesh คำนวณเส้นทางใหม่ทุกเฟรมจนกินสเปก
            if (agent.destination != wayPoint[index].position)
            {
                agent.SetDestination(wayPoint[index].position); 
            }
        }
    }

    void EnemyRotation() 
    {
        float offset = Mathf.Sin((Time.time * swingSpeed) * RangAngle); 
        float finalAngle = baseAngle + offset; 
        transform.rotation = quaternion.Euler(0, finalAngle, 0); 
    }
}