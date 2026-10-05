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
    Enemy_Task Enemy_Task;

    float timer = 0;
    float waitTime = 2;

    [Header("การสำรวจ (Search Settings)")]
    public float searchRadius = 10f;
    public int maxSearchPoints = 5;
    public int currentSearchCount = 0;
    private bool isSearching = false;
    private bool hearSound = false;
    private bool isDoingTask = false;

    Vector3 soundPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        Enemy_script = GetComponent<enemy_stage>();
        Enemy_Task = GetComponent<Enemy_Task>();
    }

    void Update()
    {
        if (Enemy_script.currentState == EnemyState.Investigate)
        {
            if (Enemy_script.currentState == EnemyState.Investigate)
            {
                if (Enemy_Task.todoList.Count > 0)
                {
                    // ใส่ตัวล็อค: ถ้ายังไม่ได้เริ่มทำ ค่อยสั่งหยุดสำรวจและเริ่มงาน
                    if (!isDoingTask)
                    {
                        StopSearchingState();
                        isDoingTask = true; // ล็อคไว้เลย เฟรมหน้าจะได้ไม่เข้ามาเบรกซ้ำ
                    }

                    Enemy_Task.StartDoingTask(); //ให้มันทำงานทุกเฟรม เพื่อเช็กการทำTask
                }
                else
                {
                    isDoingTask = false; // ถ้าคิวงานว่างแล้ว ค่อยปลดล็อค

                    if (hearSound)
                    {
                        // (โค้ดเดินไปจุดที่เกิดเสียงเหมือนเดิม)
                        Vector3 offset = transform.position - soundPosition;
                        offset.y = 0;

                        if (!agent.pathPending && offset.sqrMagnitude <= (2.0f * 2.0f))
                        {
                            Debug.Log("เดินมาถึงจุดที่เกิดเสียงแล้ว! เริ่มค้นหาสุ่มรอบๆ...");
                            hearSound = false;
                            if (agent.isActiveAndEnabled) agent.ResetPath();
                            StartSearching();
                        }
                    }
                    else if (isSearching)
                    {
                        // (โค้ดสำรวจสุ่มเหมือนเดิม)
                        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                        {
                            timer += Time.deltaTime;
                            if (timer >= waitTime)
                            {
                                GoToNextSearchPoint();
                                timer = 0;
                            }
                        }
                    }
                    else
                    {
                        // (โค้ดกลับโหมดปกติเหมือนเดิม)
                        Debug.Log("ไม่มีอะไรให้ Investigate แล้ว กลับสู่ปกติ");
                        Enemy_script.currentState = Enemy_script.baseState;
                    }
                }
            }
            else
            {
                // เผื่อมันโดนเตะไปสเตตัสอื่น (เช่น โดนยิงเข้า Alert) ให้ปลดล็อคสวิตช์ไว้ด้วย
                isDoingTask = false;
            }
        }

        else
        {
            // ระบบเซฟตี้! ถ้าถูกเตะไปสเตตัสอื่น (เช่น โดนยิง, ชนผู้เล่น)
            // ให้ล้างความจำการสำรวจที่ค้างอยู่ทิ้งให้เกลี้ยงทันที!
            if (isSearching || hearSound || isDoingTask || currentSearchCount > 0)
            {
                isSearching = false;
                hearSound = false;
                isDoingTask = false;
                currentSearchCount = 0;
                timer = 0;

                // หมายเหตุ: ไม่ต้องสั่ง agent.ResetPath() ตรงนี้ 
                // เพราะสคริปต์สเตตัสใหม่ (เช่น Alert หรือ Report) จะเป็นคนเข้ายึดพวงมาลัย NavMesh ไปจัดการเอง

                Debug.Log("หลุดจาก Investigate -> ล้างความจำการค้นหาทิ้งเรียบร้อย ปลอดภัย 100%");
            }
        }
    }

    Vector3 GetRandomPoint(Vector3 center, float radius)
    {
        Vector3 randomDir = Random.insideUnitSphere * radius;
        randomDir += center;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDir, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center;
    }

    public void StartSearching()
    {
        // เช็กก่อนว่ายังไม่ได้อยู่ในโหมดค้นหา เพื่อป้องกันบั๊กการรันซ้ำ
        if (!isSearching)
        {
            isSearching = true;
            currentSearchCount = 0;
            timer = 0;
            GoToNextSearchPoint();
        }
    }

    void GoToNextSearchPoint()
    {
        if (currentSearchCount < maxSearchPoints && isSearching)
        {
            Vector3 nextPoint = GetRandomPoint(transform.position, searchRadius);
            agent.SetDestination(nextPoint);
            currentSearchCount++;
            Debug.Log($"กำลังสำรวจจุดที่ {currentSearchCount}");
        }
        else if (Enemy_script.wasFaint)
        {
            currentSearchCount = 0;
            Debug.Log("ไอ่คนที่มันทุบหันฉันมันอยู่ไหนว่ะ...");
        }
        else
        {
            isSearching = false;
            Enemy_script.currentState = Enemy_script.baseState;

            // สำคัญ! รีเซ็ตจุดหมายเมื่อเลิกหา เพื่อป้องกัน AI ยืนค้างแบบพยายามจะเดินต่อ
            if (agent.isActiveAndEnabled) agent.ResetPath();

            Debug.Log("สำรวจเสร็จแล้ว ไม่เจออะไร... กลับไปเดินยามต่อ");
        }
    }

    public void searcingLastHearPosition(Vector3 lastHearPosition)
    {
        soundPosition = lastHearPosition;
        isSearching = false; // ปิดโหมดค้นหาสุ่มชั่วคราว
        hearSound = true;    // เปิดโหมดพุ่งเป้าไปหาจุดเกิดเหตุ

        // สั่ง AI เดินไปจุดเกิดเหตุทันที
        if (agent.isActiveAndEnabled) agent.SetDestination(soundPosition);
    }

    void StopSearchingState()
    {
        isSearching = false;
        hearSound = false;

        // ล้างคำสั่งที่ค้างอยู่ในหัว
        if (agent.isActiveAndEnabled)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }

    ////////////////////////////////////////////////////////////////////////////!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!//////////////////////////////////////////////////////////////////////////////////

}
