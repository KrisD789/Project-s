using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Alert : MonoBehaviour
{
    enemy_stage enemy_script;
    NavMeshAgent agent;


    [Header("Flanking Settings")]

    Vector3 sideDir; 
    Vector3 intermediatePoint ;

    bool hasFlanked = false; // ตัวแปรเช็คว่าฉีกออกไปหรือยัง
    public float flankDistance = 10f; // ระยะห่างจากตัวผู้เล่นเวลาโอบ
    public enum FlankDirection { Left, Right, Direct }
    public FlankDirection moveStyle = FlankDirection.Direct; // ตั้งค่าใน Inspector ของศัตรูแต่ละตัว

    public enum AlertBehave {flank, cover, push, surround, closeCom };
    public AlertBehave CurrentBehavior = AlertBehave.surround;

    public static float MaxAlertTimer = 30;
    public static float AlertTimer = 0;
    private bool OnRanDom = false;

    [Header("SurroundPlayer Settings")]
    public Vector3 surroundTarget;
    public float DistToPlayer;
    public float AlertZone = 10f;
    public float keepDist = 10;

    public float startSurroundRadius = 15f; // ระยะเริ่มล้อมวงนอกสุด
    public float minSurroundRadius = 5f;    // ระยะบีบเข้ามาใกล้สุด (จุดที่จะหยุดบีบวง)
    private bool isSurrounding = false;
    private float actionTimer = 0;
    private bool hasReachedOuterRing = false; // เช็กว่าไปถึงขอบนอกสุดหรือยัง
    private float strafeTimer = 0f;           // ตัวนับเวลาสลับซ้ายขวา
    private float strafeDirection = 1f;       // 1 = ขวา, -1 = ซ้าย

    [Header("AI Settings")]
    public float updateDelay = 0.5f; // หน่วงเวลา 0.5 วินาที (ปรับแต่งได้ตามความเหมาะสม)
    private float UpdateTime = 0f;
    private bool SeePlayer = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemy_script = GetComponent<enemy_stage>();
        agent = GetComponent<NavMeshAgent>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (agent != null && enemy_script != null)
        {
            if (enemy_script.currentState == enemy_stage.EnemyState.Alert)
            {
                if (!OnRanDom)
                {
                    OnRanDom = true;
                    randomBehavior();
                }
                actionTimer += Time.deltaTime;
                if (actionTimer >= 0.5f) // ให้มันคิดหาจุดเดินใหม่ทุกๆ ครึ่งวินาที
                {
                    if (CurrentBehavior == AlertBehave.flank) { flank(); }
                    else if (CurrentBehavior == AlertBehave.push) { push(); }
                    else if (CurrentBehavior == AlertBehave.cover) { FindCover(); }
                    else if (CurrentBehavior == AlertBehave.surround) { surrondPlayer(); }
                    actionTimer = 0f; // รีเซ็ตเวลา
                }
            }
            else
            {
                if (AlertTimer >= MaxAlertTimer)
                {
                    // คืนตั๋วก่อนเปลี่ยน State
                    Enemy_combatManager.Instance.ReleaseToken(this.gameObject);
                    enemy_script.currentState = enemy_script.baseState;
                    AlertTimer = 0f;
                }
            }
        }
    }

    void FindCover()
    {
        // 1. หาวัตถุรอบตัวในระยะ 10 เมตร
        int coverLayerMask = LayerMask.GetMask("Cover");
        Collider[] obstacles = Physics.OverlapSphere(transform.position, 10f, coverLayerMask);

        GameObject bestCover = null;
        float closestDist = Mathf.Infinity;

        foreach (var obj in obstacles)
        {
            if (obj.CompareTag("cover"))
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    bestCover = obj.gameObject;
                }
            }
        }

        if (bestCover != null)
        {
            // 2. คำนวณหาจุดที่ "ที่กำบัง" บังตัวผู้เล่นไว้
            // หลักการ: ทิศทางจากผู้เล่นมาที่ที่กำบัง แล้วยืดระยะออกไปอีกนิด
            Vector3 coverDir = (bestCover.transform.position - enemy_script.playerTransform.position).normalized;
            Vector3 coverPos = bestCover.transform.position + coverDir * 1.5f;

            // 3. สั่งให้ NavMeshAgent เดินไปที่จุดนั้น
            agent.SetDestination(coverPos);
        }
    }

    void flank()
    {
        Vector3 playerPos = enemy_script.playerTransform.position;

        // 1. หาตำแหน่งกำแพงซ้ายหรือขวา (อิงจาก World Space ไม่ใช่ตัวคน)
        // แทนที่จะอ้อมแค่ 10 เมตร ให้ลองขยับจุดออกไปให้กว้างที่สุดเท่าที่พื้น NavMesh จะมี
        Vector3 flankDir = (moveStyle == FlankDirection.Left) ? Vector3.left : Vector3.right;

        // 2. สร้างจุดหมายที่ "กว้าง" และ "ลึก"
        // - ยืดออกข้าง 15 เมตร (เพื่อให้ชนขอบ NavMesh/กำแพง)
        // - ยืดไปข้างหน้าเล็กน้อย (เพื่อให้มันเดินนำหน้าผู้เล่น)
        Vector3 wallPoint = playerPos + (flankDir * 10f) + (enemy_script.playerTransform.forward * 2f);

        // 3. ตรวจสอบว่าจุดนั้นอยู่บน NavMesh ไหม (ป้องกัน AI พยายามเดินทะลุกำแพง)
        NavMeshHit hit;
        if (NavMesh.SamplePosition(wallPoint, out hit, 10f, NavMesh.AllAreas))
        {
            // 4. สั่งให้เดินไปที่ขอบ NavMesh ที่ใกล้กำแพงที่สุด
            agent.SetDestination(hit.position);

            // 5. ถ้าเดินมาจนเกือบถึงขอบ หรืออยู่ระนาบเดียวกับผู้เล่นแล้ว ค่อยเข้าชาร์จ
            float distToPlayerSide = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                                     new Vector3(hit.position.x, 0, hit.position.z));

            //if (distToPlayerSide < 2f)
            //{
                //agent.SetDestination(playerPos);
                //print("ถึงขอบกำแพงแล้ว! เริ่มตีโอบเข้ากลาง");
                //CurrentBehavior = AlertBehave.cover;
            //}
        }
    }

    void chasePlayer_keepDist() // เดินหาผู้เล่นแต่ทิ้งระยะจากผู้เล่น
    {
        Vector3 playerPos = enemy_script.playerTransform.position;

        // หาเวกเตอร์ทิศทางจากผู้เล่นชี้มาหา AI 
        Vector3 dirFromPlayerToEnemy = (transform.position - playerPos).normalized;

        // จุดหมาย = ตำแหน่งผู้เล่น ดันออกไปตามระยะ keepDist
        Vector3 targetPos = playerPos + (dirFromPlayerToEnemy * keepDist);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 4f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            agent.ResetPath(); // ถ้าติดกำแพงทะลุไม่ได้ ให้หยุดเดิน
        }
    }

    void sideStep()
    {
        Vector3 playerPos = enemy_script.playerTransform.position;

        // 1. ลอจิกหน่วงเวลาสลับซ้าย-ขวา
        strafeTimer += Time.deltaTime;
        if (strafeTimer > 3f) // เปลี่ยนทิศทุกๆ 3 วินาที
        {
            strafeDirection = Random.Range(0, 2) == 0 ? 1f : -1f; // สุ่มซ้ายหรือขวา
            strafeTimer = 0f;
        }

        // 2. คำนวณทิศทางด้านข้าง
        Vector3 dirFromPlayerToEnemy = (transform.position - playerPos).normalized;
        Vector3 strafeDir = Vector3.Cross(dirFromPlayerToEnemy, Vector3.up) * strafeDirection;

        // 3. จุดหมาย = เอา "ตำแหน่งปัจจุบันของ AI" + ขยับออกข้างไปนิดเดียว (เช่น 1.5 เมตร)
        Vector3 targetPos = transform.position + (strafeDir * 1.5f);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 4f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            agent.ResetPath();
        }
    }

    void push()
    {
        agent.SetDestination(enemy_script.playerTransform.position);
    }

    void surrondPlayer()
    {
        // ถ้ายังไม่ได้กำลังเดินเลาะขอบอยู่ ให้เริ่มเดินได้
        if (!isSurrounding)
        {
            isSurrounding = true;
            StartCoroutine(SurroundSteppingRoutine());
        }
    }

    IEnumerator SurroundSteppingRoutine()
    {
        float currentRadius = keepDist;
        float dir = Random.Range(0, 2) == 0 ? 1f : -1f; // สุ่มเริ่มเดินซ้ายหรือขวา
        int stepCount = 0;
        int stepsToChangeDir = Random.Range(5, 10); // เดินไปกี่ก้าวถึงจะโยกสลับทิศ

        // 🚨 ลูปหลัก: เดินวนไปเรื่อยๆ ตราบใดที่ยังโดนสั่งให้ Surround และยังอยู่ใน Alert
        while (CurrentBehavior == AlertBehave.surround && enemy_script.currentState == enemy_stage.EnemyState.Alert)
        {
            // 1. เช็กสลับทิศทาง (เดินสับขาหลอกผู้เล่น)
            stepCount++;
            if (stepCount >= stepsToChangeDir)
            {
                dir *= -1f; // โยกกลับทิศ
                stepCount = 0;
                stepsToChangeDir = Random.Range(5, 10); // สุ่มจำนวนก้าวรอบใหม่
            }

            // 2. คำนวณพิกัดทีละ 20 องศา (เพื่อให้เดินเลาะขอบเนียนๆ)
            Vector3 playerPos = enemy_script.playerTransform.position;
            Vector3 startDirection = (transform.position - playerPos).normalized;

            float angleToStep = 20f * dir;
            Vector3 nextDir = Quaternion.Euler(0, angleToStep, 0) * startDirection;
            Vector3 nextWaypoint = playerPos + (nextDir * currentRadius);

            // 3. สั่งเดินบน NavMesh
            if (NavMesh.SamplePosition(nextWaypoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }

            // 4. ลืมตารอ: รอจนกว่าจะก้าวเกือบถึงจุดหมาย 
            // หรือ โดนเตะออกจากสถานะกลางอากาศ (เช่น ได้คิวเข้า Push)
            yield return new WaitUntil(() =>
                (!agent.pathPending && agent.remainingDistance <= 1.5f) ||
                CurrentBehavior != AlertBehave.surround ||
                enemy_script.currentState != enemy_stage.EnemyState.Alert
            );
        }

        // พอหลุดจากลูป while (เช่น ได้ตั๋ว Push แล้ว หรือ AlertTimer หมด) 
        // ก็ปลดล็อกสวิตช์ เพื่อให้ระบบอื่นทำงานต่อได้ทันที
        isSurrounding = false;
    }

    void randomBehavior()
    {
        // 1. สุ่มตัวเลขระหว่าง 0 ถึง 100
        float chance = Random.Range(0f, 100f);
        if (Enemy_combatManager.Instance.RequestAttackToken(this.gameObject))
        {
            // 2. ใช้เงื่อนไขแบ่งเปอร์เซ็นต์ (เช่น ค้นบ้าน 70% / คุมพื้นที่ 30%)
            //if (chance <= 40f)
            //{
                // โอกาส 70%: ไปค้นตามซอกตึกหรือในบ้าน
                //Debug.Log("AI flank");
                //CurrentBehavior = AlertBehave.flank;
            //}
            //else if (chance <= 60f)
            //{
                // โอกาส 30%: ยืนคุมเชิง หรือเดินลาดตระเวนรอบๆ โซน
                //Debug.Log("AI cover");
                //CurrentBehavior = AlertBehave.cover;
            //}

            //else
            //{
                CurrentBehavior = AlertBehave.push;
                Debug.Log("AI push");
            //}
        }

        else
        {
            CurrentBehavior = AlertBehave.surround;
            Debug.Log("AI Surrond Player");
        }
    }

    public void counting_AlertTimer(bool seePlayer)
    {
        SeePlayer = seePlayer;

        if (SeePlayer)
        {
            AlertTimer = 0;
        }
        else
        {
            AlertTimer += Time.deltaTime; 
        }
    }

    public void HandleNoiseAlert()
    {
        transform.LookAt(enemy_script.playerTransform.position);
    }

    Vector3 GetRing_RandomPoint(Vector3 center, float minRad, float maxRad)
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomDist = Random.Range(minRad, maxRad);

        Vector3 targetPos = center + new Vector3(randomDir.x * randomDist, 0, randomDir.y * randomDist);
        targetPos.y = center.y;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 10f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center;
    }
}
