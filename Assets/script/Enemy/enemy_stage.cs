using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class enemy_stage : MonoBehaviour
{
    Enemy_Alert enemy_alert_script;
    Enemy_Investigate enemy_investigate_script;
    enemy_stage target_enemy_Script;
    public GameObject player_Obj;
    public Transform playerTransform;
    NavMeshAgent agent;
    LightZone lightZoneHit;

    public bool alert = false;
    public bool lineOfSight = false;
    public bool wasFaint = false;

    public float E_runSpeed = 7;
    public float E_waklSpeed = 3;

    public enum EnemyState
    {
        Patrol,
        Investigate,
        Alert,
        faint,
        awake,
        report,
        dead,
        alertSearching,
        Dummy,
        OnGrab,
        idle
    }

    public EnemyState baseState = EnemyState.Patrol;
    public EnemyState currentState = EnemyState.Patrol;
    private Vector3 lastHeardPosition;
    private Vector3 enemy_late_Position;
    public MeshRenderer headRenderer;

    public float E_lightMeter = 0;
    float brightness = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy_alert_script = GetComponent<Enemy_Alert>();
        enemy_investigate_script = GetComponent<Enemy_Investigate>();

        agent.speed = E_waklSpeed;
        player_Obj = GameObject.FindGameObjectWithTag("Player");
        playerTransform = player_Obj.transform;
    }

    void Update()
    {
        SafetyCheckForSaveLoad();

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
                baseState = EnemyState.alertSearching;
                headRenderer.material.color = Color.red;
                break;
            case EnemyState.faint:
            case EnemyState.dead:
                headRenderer.material.color = Color.black;
                break;
            case EnemyState.awake:
                WakeUp();
                break;
            case EnemyState.alertSearching:
                headRenderer.material.color = Color.gray;
                agent.speed = E_runSpeed;
                break;
            case EnemyState.Dummy:
                headRenderer.material.color = Color.blue;
                break;
            case EnemyState.OnGrab:
                headRenderer.material.color = Color.black;
                break;
            case EnemyState.idle:
                break;
        }
    }

    public void Alert() { agent.speed = E_runSpeed; }
    public void Investigate() { agent.speed = E_waklSpeed; }
    private void Patrol() { agent.speed = E_waklSpeed; }


    // -------------------------------------------------------------
    //  ฟังก์ชัน: ตัวดักจับฟิสิกส์ค้าง
    // -------------------------------------------------------------
    private void SafetyCheckForSaveLoad()
    {
        // เช็กเฉพาะสถานะที่ศัตรู "ยังมีชีวิต" และ "ต้องเดินได้อิสระ"
        if (currentState == EnemyState.Patrol ||
            currentState == EnemyState.Investigate ||
            currentState == EnemyState.Alert ||
            currentState == EnemyState.alertSearching ||
            currentState == EnemyState.awake ||
            currentState == EnemyState.idle)
        {
            // ก. ถ้าเผลอติดอยู่กับมือ/ไหล่ใคร ให้หลุดออกทันที
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }

            // ข. คืนค่าฟิสิกส์และการชน ให้กลับมาสมบูรณ์
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null && rb.isKinematic) rb.isKinematic = true;

            Collider col = GetComponent<Collider>();
            if (col != null && !col.enabled) col.enabled = true;

            // ค. บังคับเปิด NavMeshAgent ให้เดินได้
            if (!agent.enabled) agent.enabled = true;
            if (agent.isActiveAndEnabled && agent.isStopped) agent.isStopped = false;
        }
    }


    // --- ส่วนที่ศัตรูจัดการตัวเองตอนโดนจับ ---
    public void EnterGrabState(Transform playerGrabPos)
    {
        currentState = EnemyState.OnGrab;
        Enemy_Anime_Controller enemyAnim = GetComponent<Enemy_Anime_Controller>();
        Rigidbody rb = GetComponent<Rigidbody>();
        Collider col = GetComponent<Collider>();

        if (agent.isActiveAndEnabled) agent.isStopped = true;
        agent.enabled = false;

        if (rb != null) rb.isKinematic = true;
        if (col != null) col.enabled = false;

        transform.SetParent(playerGrabPos);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (enemyAnim != null) enemyAnim.PlayGrabbed();
    }

    public void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        Enemy_Anime_Controller enemyAnim = GetComponent<Enemy_Anime_Controller>();
        Rigidbody rb = GetComponent<Rigidbody>();
        Collider col = GetComponent<Collider>();

        switch (newState)
        {
            case EnemyState.dead:
            case EnemyState.faint:
                transform.SetParent(null);

                // --- 1. สั่งเบรกและปิด AI ---
                if (agent.isActiveAndEnabled)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                }
                agent.enabled = false;

                // --- 2. จัดการฟิสิกส์ให้ศพหยุดนิ่งสนิท (ตามที่คุณต้องการ) ---
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;        // ล้างแรงเฉื่อยการพุ่ง
                    rb.angularVelocity = Vector3.zero; // ล้างแรงหมุนกลิ้ง
                    rb.isKinematic = true;             // ล็อกฟิสิกส์! ไม่ให้ไหลหรือโดนดัน
                }

                // (เปิด Collider ไว้เหมือนเดิม เพื่อให้ผู้เล่นยังเอามือไปชี้เพื่อ "กดลากศพ" ได้)
                if (col != null) col.enabled = true;

                if (enemyAnim != null) enemyAnim.PlayDie();
                OnDown();
                break;

            case EnemyState.awake:
                // --- จังหวะฟื้นคืนสติ ---
                if (col != null) col.enabled = true;
                if (rb != null) rb.isKinematic = true; // ล็อกฟิสิกส์ตอนเดินไว้กันเป๋

                // --- เปิดระบบสมอง AI กลับมาทำงาน ---
                agent.enabled = true;
                if (agent.isActiveAndEnabled)
                {
                    agent.isStopped = false;
                }

                WakeUp();
                break;
        }
    }

    public void OnDown()
    {
        wasFaint = true;
        if (agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }
    }

    public void WakeUp()
    {
        currentState = EnemyState.Investigate;
        if (agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        }
        Debug.Log("AI: ฟื้นแล้ว! กลับไปทำงานต่อ");
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Light"))
        {
            lightZoneHit = other.GetComponent<LightZone>();
            if (lightZoneHit != null && lightZoneHit.lightZoneState)
            {
                Vector3 playerPos = new Vector3(transform.position.x, 0, transform.position.z);
                Vector3 lightPos = new Vector3(other.transform.position.x, 0, other.transform.position.z);
                float distance = Vector3.Distance(playerPos, lightPos);

                float maxRadius;
                if (!other.TryGetComponent<SphereCollider>(out SphereCollider sphere)) maxRadius = Mathf.Max(other.bounds.extents.x, other.bounds.extents.z);
                else maxRadius = other.bounds.extents.x;

                float coreRadius = maxRadius * 0.2f;

                if (distance <= coreRadius) brightness = 1f;
                else brightness = Mathf.InverseLerp(maxRadius, coreRadius, distance);

                E_lightMeter = Mathf.RoundToInt(brightness * 100f);
            }
            else E_lightMeter = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Light"))
        {
            brightness = 0;
            E_lightMeter = 0;
        }
    }

    IEnumerator OnTalk()
    {
        yield return new WaitForSeconds(2);
        if (target_enemy_Script != null)
        {
            currentState = EnemyState.report;
            target_enemy_Script.currentState = enemy_stage.EnemyState.awake;
            target_enemy_Script = null;
        }
    }
}