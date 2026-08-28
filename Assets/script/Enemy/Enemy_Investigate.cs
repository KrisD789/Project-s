using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static enemy_stage;
using Random = UnityEngine.Random;




public class Enemy_Investigate : MonoBehaviour
{
    NavMeshAgent agent;
    enemy_stage Enemy_script;
    enemy_stage target_enemy_Script;
    light_switch targetSW;
    Door TargetDoor;
    Enemy_Task Enemy_Task;
    //public LayerMask LayerMask;
    //public LayerMask FriendNeraByMask;
    //enemy Enemy_script = enemy.enemy_script;

    Vector3 enemy_late_Position;
    Vector3 lastHeardPosition;
                                                                                                                                                                                                  
    float timer = 0;
    float waitTime = 2;

    int coverLayerMask;

    //public List<WorkTask> todoList = new List<WorkTask>();

    [Header("การสำรวจ (Search Settings)")]
    public float searchRadius = 10f;  // รัศมีที่จะเดินวนดูรอบๆ
    public int maxSearchPoints = 5;  // จะเดินสุ่มกี่จุดก่อนจะเลิก
    public int currentSearchCount = 0;
    private bool isSearching = false;
    private bool hearSound = false;
    //private bool isFriendCheck = false;
    //public bool isSearchingLight = false;

    Vector3 soundPosition;
    
    //Start stepback
    [SerializeField] private float stepBackDistance = 1.5f;
    [SerializeField] private float recoilDuration = 0.4f;
    private bool isStartled = false;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        Enemy_script = GetComponent<enemy_stage>();
        //FriendNeraByMask = LayerMask.GetMask("enemy");
        Enemy_Task = GetComponent<Enemy_Task>();
    }


    void Update()
    {
        
        if (Enemy_script.currentState == EnemyState.Investigate)
        {
            if (Enemy_Task.todoList.Count > 0)
            {
                StopSearchingState(); // เลิกค้นหาสะเปะสะปะก่อน

                Enemy_Task.StartDoingTask();
            }


            else if (hearSound)  //เช็กว่าไปถึงจุดที่ได้ยินเสียงรึยัง ถ้าถึงแล้วให้ค้นดูรอบๆ
            {
                // ตัดแกน Y ทิ้งก่อนวัดระยะ เพื่อป้องกันบั๊กความสูง
                Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);
                Vector3 targetPos = new Vector3(soundPosition.x, 0, soundPosition.z);

                // ถ้าระยะห่างน้อยกว่า 2 เมตร ค่อยถือว่าถึงจุดเกิดเหตุจริงๆ
                if (Vector3.Distance(myPos, targetPos) <= 2.0f)
                {
                    Debug.Log("เดินมาถึงจุดที่เกิดเสียงแล้ว! เริ่มค้นหา...");
                    hearSound = false;
                    StartSearching();
                }
            }   

            else if (Enemy_Task.todoList.Count == 0)
            {
                StartSearching();
                    
                    if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                    {
                        //Debug.Log("กำลังเดินสุ่มตรวจรอบๆ.....");
                        timer += Time.deltaTime;
                        if (timer >= waitTime)
                        {
                            GoToNextSearchPoint();
                            timer = 0;
                        }

                    }
                //Debug.Log("เข้าเงื่อนไข else if (Enemy_Task.todoList.Count == 0)");
            }

            else
            {
                Enemy_script.currentState = Enemy_script.baseState;
                print("หลุดจาก investigate");
            }
        }
        
    }

    

   
   
    Vector3 GetRandomPoint(Vector3 center, float radius)
    {
        Vector3 randomDir = Random.insideUnitSphere * radius; // สุ่มทิศทางในวงกลม
        randomDir += center; // อ้างอิงจากจุดศูนย์กลาง (เช่น จุดที่เห็นไฟดับ)

        NavMeshHit hit;
        // หาจุดที่ใกล้ที่สุดบน NavMesh ในระยะ radius
        if (NavMesh.SamplePosition(randomDir, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center; // ถ้าหาไม่ได้ให้กลับที่เดิม
    }

    public void StartSearching()
    {
        if (!isSearching)
        {
            isSearching = true;
            currentSearchCount = 0;
            timer = 0;
            GoToNextSearchPoint();

            //Debug.Log("StartSearching...");
        }

        
    }

    void GoToNextSearchPoint()
    {
        if (currentSearchCount < maxSearchPoints && isSearching == true)
        {
            Vector3 nextPoint = GetRandomPoint(transform.position, searchRadius);
            agent.SetDestination(nextPoint);
            currentSearchCount++;
            //Debug.Log($"กำลังสำรวจจุดที่ {currentSearchCount}");
        }

        else if (Enemy_script.wasFaint ) // if(Enemy_script.baseState == enemy.EnemyState.investigate) 
        {
            //print("wasFaint = "+Enemy_script.wasFaint);
            //print("OnAlert = " + Enemy_script.onAlert);
            currentSearchCount = 0;
            Debug.Log("ไอ่คนที่มันทุบหันฉันมันอยู่ไหนว่ะ...");
        }
        else
        {
            // สำรวจครบแล้ว กลับไป Patrol
            isSearching = false;
            Enemy_script.currentState = Enemy_script.baseState;
            Debug.Log("สำรวจเสร็จแล้ว ไม่เจออะไร... กลับไปเดินยามต่อ");
        }
    }

    public void searcingLastHearPosition(Vector3 lastHearPosition)
    {
        soundPosition = lastHearPosition;
        isSearching = false;
        hearSound = true;
        agent.SetDestination(soundPosition);
    }

    void StopSearchingState()
    {
        isSearching = false;
        hearSound = false;
        
    }

    ////////////////////////////////////////////////////////////////////////////!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!//////////////////////////////////////////////////////////////////////////////////
   
}
