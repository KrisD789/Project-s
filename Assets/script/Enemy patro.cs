using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class Enemypatro : MonoBehaviour
{
    enemy_stage enemy_script;
    NavMeshAgent agent;
    float waitTime = 1;
    float Timer;
    int index = 0;
    bool isWaiting = false;
    public Transform[] wayPoint;

    [Header("ตั้งค่าการหมุน")]

    public float RangAngle = 45;
    //public float maxAngle = 45;
    public float swingSpeed = 1;
    float baseAngle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy_script = GetComponent<enemy_stage>();
        agent.SetDestination(wayPoint[0].position);
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy_script.currentState == enemy_stage.EnemyState.Patrol)
        {
            if (index >= wayPoint.Length) index = 0;

            float distToTarget = Vector3.Distance(transform.position, wayPoint[index].position);

            // --- เพิ่มส่วนนี้: ถ้าไม่ได้รออยู่ และ Agent ไม่มีจุดหมาย (หรือจุดหมายไม่ใช่ Waypoint) ให้สั่งเดินใหม่ ---
            //if (!isWaiting && agent.destination != wayPoint[index].position)
            //{
                //agent.SetDestination(wayPoint[index].position);
            //}

            if (!agent.pathPending && distToTarget < 2f)
            {
                isWaiting = true;
            }

            if (isWaiting)
            {
                patroLogic();
            }
        }

        print(index);
    }

    void patroLogic()
    {
        Timer += Time.deltaTime; // นับเวลาไปเรื่อยๆ แม้จะโดนเบียด

        if (Timer >= waitTime) // เมื่อรอจนครบ 1 วินาที (เปลี่ยนจาก <= เป็น >=)
        {
            index++; // เปลี่ยนไปจุดถัดไป
            if (index >= wayPoint.Length) index = 0;

            agent.SetDestination(wayPoint[index].position);

            // --- จุดสำคัญ: รีเซ็ตทุกอย่างเพื่อเริ่มงานใหม่ ---
            Timer = 0;
            isWaiting = false; // เลิกกะพริบ/เลิกรอ แล้วออกเดินได้!
        }
    }

    void EnemyRotation()
    {
        

        float offset = Mathf.Sin((Time.time * swingSpeed) * RangAngle);

        float finalAngle = baseAngle + offset;

        transform.rotation = quaternion.Euler(0, finalAngle, 0);


        //transform.localEulerAngles = new(Vector3(0, current,0))

    }
}
