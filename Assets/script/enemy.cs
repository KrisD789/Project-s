using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class enemy : MonoBehaviour
{
    //public static enemy enemy_script {  get; private set; }
    Enemy_Alert enemy_alert_script;
    Enemy_Investigate enemy_investigate_script;
    enemy target_enemy_Script;
    public Transform playerTransform;
    //gameplay gameplay;
    NavMeshAgent agent;
    //LightDetect lightDetect_Script = LightDetect.lightDetect;
    LightZone lightZoneHit;

    float playerStealthBar;
    public bool alert = false;
    public bool lineOfSight = false;
    //public bool noisAlert = false;

    float timer = 0;
    float waitTime = 3f;
    
    bool isenemyLatePos = false;
    public bool onAlert = false;
    public bool wasFaint = false; //เป็นตัวไว้บอกว่าศัตรูตัวนี้เคยโดนplayerทำให้สลบ


    public float E_runSpeed = 7;
    public float E_waklSpeed = 3;


    public enum EnemyState
    {
        Patrol,      // เดินลาดตระเวนตามปกติ
        Investigate, // ตรวจสอบตำแหน่งที่ได้ยินเสียง
        Alert,        // ไล่ล่าผู้เล่นที่ถูกมองเห็น
        faint,
        awake,
        report,
        dead,
        alertSearching
    }

    public EnemyState baseState = EnemyState.Patrol;
    public EnemyState currentState = EnemyState.Patrol;
    private Vector3 lastHeardPosition; // ตำแหน่งที่ได้ยินเสียงล่าสุด
    private Vector3 enemy_late_Position;
    public MeshRenderer headRenderer;

    public float E_lightMeter = 0;
    float brightness = 0;

    

    private void Awake()
    {
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy_alert_script = GetComponent<Enemy_Alert>();
        enemy_investigate_script = GetComponent<Enemy_Investigate>();
        
        agent.speed = E_waklSpeed;
        
        
    }

    // Update is called once per frame
    void Update()
    {
        //playerStealthBar = LightDetect.lightDetect.light_meter;
        

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                headRenderer.material.color = Color.white;
                break;
            case EnemyState.Investigate:
                Investigate();
                
                headRenderer.material.color = Color.yellow;
                break;
            case EnemyState.Alert:
                Alert();
                headRenderer.material.color = Color.red;
                break;

            case EnemyState.faint:
                OnDown();
                break;

            case EnemyState.awake:
                WakeUp();
                break;

            case EnemyState.dead:
                OnDown();
                break;

            case EnemyState.alertSearching:
                headRenderer.material.color = Color.black;
                agent.speed = E_runSpeed;
                break;
        }

        //if(wasFaint) currentState = EnemyState.Investigate;
        //if (currentState == EnemyState.Alert  && playerStealthBar < 50) currentState = EnemyState.Investigate;

        //print("Timer = " + timer);
    }

    public void Alert()
    {
        agent.speed = E_runSpeed;
        //agent.SetDestination(playerTransform.position);
        //print("On Alert !!!!!");
    }

    public void Investigate()
    {
        agent.speed = E_waklSpeed;
    }

    private void Patrol()
    {
        //agent.SetDestination(PatroPosition);
        agent.speed = E_waklSpeed;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Light"))
        {
            lightZoneHit = other.GetComponent<LightZone>();

            if (lightZoneHit != null && lightZoneHit.lightZoneState)
            {
                // 1. คำนวณระยะห่างเฉพาะแนวราบ (XZ) เพื่อความแม่นยำ 100%
                Vector3 playerPos = new Vector3(transform.position.x, 0, transform.position.z);
                Vector3 lightPos = new Vector3(other.transform.position.x, 0, other.transform.position.z);
                float distance = Vector3.Distance(playerPos, lightPos);

                float maxRadius;
                // 2. ดึงรัศมีตามแบบที่นายพิสูจน์แล้วว่า Smooth
                if (!other.TryGetComponent<SphereCollider>(out SphereCollider sphere))
                {
                    maxRadius = Mathf.Max(other.bounds.extents.x, other.bounds.extents.z);
                }
                else
                {
                    maxRadius = other.bounds.extents.x;
                }

                // 3. กำหนดพื้นที่สว่างสูงสุด (Core Radius)
                // เช่น 30% ของรัศมีทั้งหมดให้เป็น 100% เสมอ
                float coreRadius = maxRadius * 0.2f;

                if (distance <= coreRadius)
                {
                    // ถ้าอยู่ในเขต Core ให้สว่างเต็มทันที
                    brightness = 1f;
                }
                else
                {
                    // 4. ส่วนที่สำคัญที่สุด: ค่อยๆ ไล่จาก 0 (ที่ขอบ maxRadius) ไปหา 1 (ที่ขอบ coreRadius)
                    // วิธีนี้จะทำให้มันค่อยๆ เพิ่มจาก 0 แบบที่นายชอบ และเต็ม 100 ก่อนถึงจุดศูนย์กลาง
                    brightness = Mathf.InverseLerp(maxRadius, coreRadius, distance);
                }

                E_lightMeter = Mathf.RoundToInt(brightness * 100f);
            }
            else E_lightMeter = 0f;
        }
    }

    // เพิ่ม OnTriggerExit เพื่อล้างค่าเมื่อออกจากเขตแสงแน่นอน
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Light"))
        {
            brightness = 0;
            E_lightMeter = 0;
        }
    }

    public void OnDown() // ฟังก์ชันตอนสลบ
    {
        wasFaint = true; //เป็นตัวไว้บอกว่าศัตรูตัวนี้เคยโดนplayerทำให้สลบ

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // ปิดการคำนวณหลบหลีก เพื่อไม่ให้มันขยับเอง
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        // ถ้ามี Rigidbody ให้เซ็ตเป็น Kinematic กันเพื่อนเดินมาชนแล้วกระเด็น
        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = true;
        } 

        //Debug.Log("AI: ฉันสลบแล้วนะ อย่ามาเข็นฉัน!dxes");
    }

    public void WakeUp()
    {
        currentState = EnemyState.Investigate;

        agent.isStopped = false; // สั่งให้เดินต่อได้

        // ปรับการหลบหลีกกลับเป็นแบบเดิม (ปกติคือ HighQuality หรือ LowQuality)
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        // ถ้ามี Rigidbody และเซ็ต Kinematic ไว้ ให้ปิดด้วยเพื่อให้รับแรงฟิสิกส์ได้เหมือนเดิม
        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = false;
        }

        Debug.Log("AI: ฟื้นแล้ว! กลับไปทำงานต่อ");
    }

    IEnumerator OnTalk()
    {
        yield return new WaitForSeconds(2);
        Debug.Log("are you okay.....");

        if(target_enemy_Script != null)
        {
            currentState = EnemyState.report; //สั่งตัวเองให้เข้าเฟดinvestigate

            target_enemy_Script.currentState = enemy.EnemyState.awake; //สั่งให้ศัตรูคัวอื่น ให้ตื่น
            target_enemy_Script = null;
        }
    }
} 