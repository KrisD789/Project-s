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
                agent.speed = E_waklSpeed;
                headRenderer.material.color = Color.white;
                break;

            case EnemyState.Investigate:
                Investigate();
                agent.speed = E_waklSpeed;
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
                DisableAllHitboxes();
                break;

            case EnemyState.awake:
                WakeUp();
                break;

            case EnemyState.alertSearching:
                headRenderer.material.color = Color.gray;
                agent.speed = E_waklSpeed;
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
        // เพิ่มตัวดักตัวใหญ่ที่สุด: ถ้าสถานะคือ OnGrab ห้ามทำ SafetyCheck เด็ดขาด!
        if (currentState == EnemyState.OnGrab) return;

        // หรือถ้าศัตรูติดอยู่กับผู้เล่น (กลายเป็น Child) ก็ห้ามทำเช่นกัน!
        if (transform.parent != null) return;

        if (currentState == EnemyState.Patrol || currentState == EnemyState.Investigate ||
            currentState == EnemyState.Alert || currentState == EnemyState.alertSearching ||
            currentState == EnemyState.awake || currentState == EnemyState.idle)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null && rb.isKinematic) rb.isKinematic = false; // ปรับให้ถูกต้อง

            Collider col = GetComponent<Collider>();
            if (col != null && !col.enabled) col.enabled = true;

            if (!agent.enabled) agent.enabled = true;
            if (agent.isActiveAndEnabled && agent.isStopped) agent.isStopped = false;
        }
    }

    public void ChangeState(EnemyState newState)
    {
        // เพิ่มท่าไม้ตาย: ถ้าโดนผู้เล่นล็อคคออยู่ ห้าม AI ตัวไหนมาเปลี่ยนสถานะเด็ดขาด!
        if (currentState == EnemyState.OnGrab)
        {
            // อนุญาตให้เปลี่ยนได้แค่ตอนตายหรือสลบ (จากคำสั่ง FinishTakedown เท่านั้น)
            if (newState != EnemyState.dead && newState != EnemyState.faint) return;
        }

        if (currentState == newState) return;
        currentState = newState;

        Enemy_Anime_Controller enemyAnim = GetComponent<Enemy_Anime_Controller>();
        Rigidbody rb = GetComponent<Rigidbody>();

        switch (newState)
        {
            case EnemyState.dead:
            case EnemyState.faint:
                // --- 1. สั่งเบรกและล้างเส้นทาง (เปิด AI ทิ้งไว้เพื่อกันศพติดบั๊กลอย) ---
                if (agent.isActiveAndEnabled)
                {
                    agent.isStopped = true; // เหยียบเบรก ไม่ให้ขยับ
                    agent.ResetPath();      // ล้างเป้าหมายในหัวทิ้ง
                    agent.velocity = Vector3.zero;
                }
                // ลบคำสั่ง agent.enabled = false; ออกไปแล้วครับ

                // --- 2. จัดการฟิสิกส์ให้ศพหยุดนิ่งสนิท ---
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true; // ล็อกฟิสิกส์ไว้ ศพจะได้ไม่ไถล
                }

                //if (enemyAnim != null) enemyAnim.PlayKill();
                OnDown();
                break;

            case EnemyState.awake:
                if (gameObject.CompareTag("ScamCommander") || baseState == EnemyState.Dummy)
                {
                    Debug.Log("ลูกน้องพยายามปลุกบอส... แต่บอสสลบเหมือด ปลุกไม่ตื่น!");

                    // สลับสถานะกลับเป็นสลบเหมือนเดิมเพื่อไม่ให้ระบบค้าง
                    currentState = EnemyState.faint;
                    return; // เตะออกจากฟังก์ชันทันที ไม่ต้องรันโค้ดปลุกให้ตื่นด้านล่าง
                }

                // --- ปลดเบรกตอนฟื้นคืนสติ ---
                if (rb != null) rb.isKinematic = true;

                if (agent.isActiveAndEnabled)
                {
                    agent.isStopped = false; // ปลดเบรกให้กลับมาเดินได้
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

    public void DisableAllHitboxes()
    {
        Enemy_Hitbox[] allHitboxes = GetComponentsInChildren<Enemy_Hitbox>();
        foreach (var hitbox in allHitboxes)
        {
            if (hitbox.TryGetComponent<Collider>(out Collider col))
            {
                col.enabled = false;
            }
        }
    }
}