using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Alert : MonoBehaviour
{

    enemy_stage enemy_script;
    EnemyRaycast enemy_Raycast;
    NavMeshAgent agent;
    GameObject player_Obj;
    //Player_Action player_action;
    LightDetect player_LightDetect;

    [Header("Flanking Settings")]

    Vector3 sideDir;
    Vector3 intermediatePoint;

    //bool hasFlanked = false; // ตัวแปรเช็คว่าฉีกออกไปหรือยัง
    public float flankDistance = 10f; // ระยะห่างจากตัวผู้เล่นเวลาโอบ
    public enum FlankDirection { Left, Right, Direct }
    public FlankDirection moveStyle = FlankDirection.Direct; // ตั้งค่าใน Inspector ของศัตรูแต่ละตัว

    public enum AlertBehave { flank, cover, push, surround, chasePlayer_keepDist, trail };
    public AlertBehave Alert_CurrentBehavior = AlertBehave.surround;
    public AlertBehave Alert_PreviouslyBehavior;

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
    //private bool hasReachedOuterRing = false; // เช็กว่าไปถึงขอบนอกสุดหรือยัง
    private float strafeTimer = 0f;           // ตัวนับเวลาสลับซ้ายขวา
    private float strafeDirection = 1f;       // 1 = ขวา, -1 = ซ้าย

    [Header("AI Settings")]
    public float updateDelay = 0.5f; // หน่วงเวลา 0.5 วินาที (ปรับแต่งได้ตามความเหมาะสม)
    //private float UpdateTime = 0f;
    //private bool SeePlayer = false;

    //[Header("Darkness Search Settings")]
    //public float searchRadiusInDark = 5f; // ระยะการสุ่มรอบตัวผู้เล่น (ปรับใน Inspector ได้)
    //private Vector3 currentDarkTarget;    // เก็บพิกัดเป้าหมายในที่มืดปัจจุบัน
    //private bool hasDarkTarget = false;   // เช็คว่ามีเป้าหมายที่มืดหรือยัง
    //private int stuckCounter = 0;         // ตัวนับเวลา AI เดินติด

    [Header("Memory & Trail Settings")]
    public float memoryDuration = 10f; // เวลาที่จำพิกัดได้หลังหลุดสายตา
    public float hintInterval = 10f;   // เว้นช่วงการใบ้พิกัด 3 วินาที
    private float timeLostSight;
    private float hintTimer = 0f;
    private Vector3 currentTargetPos; // ศูนย์รวมพิกัดที่ AI ทุกตัวจะอ้างอิง
    public float delayBeforeTrail = 3f; // เวลาที่จะให้ AI ยืนนิ่งๆ ก่อนตามรอย
    private float waitTimer = 0f;       // ตัวนับเวลา

    [Header("Combat & Side Step Settings")]
    public float pushEngageDistance = 10f; // ระยะที่ AI โหมด Push จะเริ่มรู้สึกว่า "ใกล้พอที่จะสไลด์หามุมยิงแล้ว"
    public float sideStepMargin = 2f;      // ระยะเผื่อ (Margin) เวลารักษาระยะห่าง เช่น keepDist + sideStepMargin

    [Header("Combat & Noise Settings")]
    private bool isDistractedByNoise = false; // สวิตช์บอกว่ากำลังสนใจเสียงอยู่

    [Header("Warn Other Settings")]
    public LayerMask FriendNeraByMask;
    private bool On_TriggerGroupAlert = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemy_script = GetComponent<enemy_stage>();
        agent = GetComponent<NavMeshAgent>();
        enemy_Raycast = GetComponent<EnemyRaycast>();
        currentTargetPos = enemy_script.playerTransform.position;
        FriendNeraByMask = LayerMask.GetMask("enemy");

        player_Obj = GameObject.FindGameObjectWithTag("Player");

        if (player_Obj != null) Debug.Log("Found __player_Obj__");

        else Debug.Log("-----Warning------ !!!!! Not Found __player_Obj__");

        if (player_Obj.TryGetComponent<LightDetect>(out player_LightDetect))
        {
            Debug.Log("ตรวจพบ สคริป Light_Detect");
        }

        else Debug.Log("-----Warning------ !!!!!  ไม่พบพบ สคริป Light_Detect");

    }

    // Update is called once per frame
    void Update()
    {
        //print(player_LightDetect.light_meter);
        //print(timeLostSight);
        //print(AlertTimer);
        if (agent != null && enemy_script != null)
        {
            if (enemy_script.currentState == enemy_stage.EnemyState.Alert)
            {
                counting_AlertTimer();

                if (AlertTimer <= MaxAlertTimer)
                {
                    // ศูนย์สั่งการ: ตัดสินใจว่าจะให้พิกัดไหนกับ AI  
                    ManageTargeting();


                    // การเรียกใช้งานฟังก์ชันพฤติกรรม
                    if (!OnRanDom)
                    {
                        OnRanDom = true;
                        randomBehavior();
                        //Alert_CurrentBehavior = AlertBehave.flank;
                    }

                    actionTimer += Time.deltaTime;
                    if (actionTimer >= 0.5f)
                    {
                        // ทุกฟังก์ชันด้านล่างนี้ จะไปดึง currentTargetPos ไปใช้
                        if (Alert_CurrentBehavior == AlertBehave.flank) { flank(); }
                        else if (Alert_CurrentBehavior == AlertBehave.push) { push(); }
                        else if (Alert_CurrentBehavior == AlertBehave.cover) { FindCover(); }
                        else if (Alert_CurrentBehavior == AlertBehave.surround) { surrondPlayer(); }
                        else if (Alert_CurrentBehavior == AlertBehave.chasePlayer_keepDist) { chasePlayer_keepDist(); }
                        else if (Alert_CurrentBehavior == AlertBehave.trail) { trail(); }

                        actionTimer = 0f;
                    }
                }
                else
                {
                    EndAlertState();
            
                }
            }

            if (enemy_script.currentState == enemy_stage.EnemyState.dead || enemy_script.currentState == enemy_stage.EnemyState.faint)
            {
                // คืนตั๋วให้ Combat Manager ทันทีเมื่อตายหรือสลบ
                Enemy_combatManager.Instance.ReleaseToken(this.gameObject);

                // รีเซ็ตตัวแปรที่ใช้ใน AlertState
                OnRanDom = false;
                isSurrounding = false;
                On_TriggerGroupAlert = false;
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
        if (TrySideStepForLineOfSight(keepDist + sideStepMargin))
        {
            Debug.Log("TrySideStepForLineOfSight");
            return;
        }

        Vector3 playerPos = currentTargetPos;

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
        // ใช้ตัวแปร keepDist + sideStepMargin แทนการบวก 2f ตรงๆ
        if (TrySideStepForLineOfSight(keepDist + sideStepMargin))
        {
            Debug.Log("TrySideStepForLineOfSight");
            return;
        }

        Vector3 playerPos = currentTargetPos;

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

    bool TrySideStepForLineOfSight(float engageDistance) // ฟังก์ชันเช็คว่าควรสไลด์หามุมยิงหรือไม่ (รับค่าระยะหวังผลของแต่ละ State เข้ามา)
    {
        Vector3 playerPos = enemy_script.playerTransform.position;
        float distToTarget = Vector3.Distance(transform.position, playerPos);
        bool isMemoryFresh = (Time.time - timeLostSight) <= memoryDuration;

        if (distToTarget <= engageDistance && !enemy_Raycast.foundPlayer && isMemoryFresh)
        {
            sideStep();
            return true; // กำลังสไลด์อยู่ 
        }


        // จุดสำคัญ: ถ้าไม่ได้ทำ Side Step แล้ว ให้คืนค่าควบคุมให้ NavMesh ทันที
        agent.updateRotation = true;
        return false; // ทำงานปกติต่อ
    }

    void sideStep()
    {
        Vector3 playerPos = currentTargetPos;
        // ลอจิกสับขาซ้ายขวา
        strafeTimer += Time.deltaTime;
        if (strafeTimer > 3f)
        {
            strafeDirection = Random.Range(0, 2) == 0 ? 1f : -1f;
            strafeTimer = 0f;
        }

        Vector3 dirFromPlayerToEnemy = (transform.position - playerPos).normalized;
        Vector3 strafeDir = Vector3.Cross(dirFromPlayerToEnemy, Vector3.up) * strafeDirection;
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
        // ใช้ตัวแปร pushEngageDistance แทนตัวเลข 10f
        //if (TrySideStepForLineOfSight(pushEngageDistance))
        //{
        //return;
        //}

        agent.SetDestination(currentTargetPos);
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

        // ลูปหลัก: เดินวนไปเรื่อยๆ ตราบใดที่ยังโดนสั่งให้ Surround และยังอยู่ใน Alert
        while (Alert_CurrentBehavior == AlertBehave.surround && enemy_script.currentState == enemy_stage.EnemyState.Alert)
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
                Alert_CurrentBehavior != AlertBehave.surround ||
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
            if (chance <= 40f)
            {
                //โอกาส 70%: ไปค้นตามซอกตึกหรือในบ้าน
                Debug.Log("AI flank");
                Alert_CurrentBehavior = AlertBehave.flank;
            }
            else if (chance <= 60f)
            {

                Debug.Log("AI cover");
                Alert_CurrentBehavior = AlertBehave.cover;
            }
            else if (chance <= 80)
            {
                Debug.Log("AI chasePlayer_keepDist");
                Alert_CurrentBehavior = AlertBehave.chasePlayer_keepDist;
            }

            else
            {
                Alert_CurrentBehavior = AlertBehave.push;
                Debug.Log("AI push");
            }
        }

        else
        {
            Alert_CurrentBehavior = AlertBehave.surround;
            Debug.Log("AI Surrond Player");
        }

        Alert_PreviouslyBehavior = Alert_CurrentBehavior;
    }

    public void counting_AlertTimer()
    {

        if (enemy_Raycast.foundPlayer)
        {
            AlertTimer = 0;
        }
        else
        {
            AlertTimer += Time.deltaTime;
        }
    }

    public void HandleNoiseAlert(Vector3 P_Position)
    {
        if (enemy_Raycast.foundPlayer)
        {
            return; // ตัดจบฟังก์ชัน ไม่ต้องไปอัปเดตเป้าหมายหรือหันหน้าตามเสียง
        }

        // 1. หันขวับไปทางจุดที่เกิดเสียง
        Vector3 lookPos = new Vector3(P_Position.x, transform.position.y, P_Position.z);
        transform.LookAt(lookPos);

        // 2. อัปเดตเข็มทิศเป้าหมายหลัก
        currentTargetPos = P_Position;
        timeLostSight = Time.time;
        hintTimer = 0f;

        isDistractedByNoise = true; // เปิดสวิตช์

        agent.SetDestination(currentTargetPos);
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

    void trail()
    {
        if (Alert_CurrentBehavior == AlertBehave.trail)
        {
            // 1. หันหน้าไปทางคำใบ้ล่าสุด
            transform.LookAt(new Vector3(currentTargetPos.x, transform.position.y, currentTargetPos.z));

            // 2. เดินรักษาระยะห่าง 8 เมตรจากคำใบ้
            Vector3 dirFromTarget = (transform.position - currentTargetPos).normalized;
            Vector3 keepDistPos = currentTargetPos + (dirFromTarget * 8f);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(keepDistPos, out hit, 3f, NavMesh.AllAreas)) // ทำเงื่อนไขการทิ้งระยะห่าง เช่น surround อยู่ไกลplayer ตัวที่ได้เข้าโจมตีจะตามอยู่ไกล้player
            {
                //if()
                agent.SetDestination(currentTargetPos);
            }

            // 3. ถ้ามองเห็นผู้เล่นอีกครั้ง ให้กลับไป Push ทันที!
            if (enemy_Raycast.foundPlayer)
            {
                if (Enemy_combatManager.Instance.RequestAttackToken(this.gameObject)) Alert_CurrentBehavior = Alert_PreviouslyBehavior;
            }
        }
    }

    void ManageTargeting()
    {
        bool canSeePlayer = enemy_Raycast.foundPlayer;

        if (canSeePlayer)
        {
            timeLostSight = Time.time;
            currentTargetPos = enemy_script.playerTransform.position;
            isDistractedByNoise = false;
        }
        else
        {
            if (Time.time - timeLostSight > memoryDuration && Alert_CurrentBehavior != AlertBehave.trail)
            {
                Alert_CurrentBehavior = AlertBehave.trail;
                isDistractedByNoise = false;
            }

            // ปลดล็อคถ้าเดินไปถึงจุดเกิดเสียงแล้วไม่เจอใคร! 
            if (isDistractedByNoise)
            {
                // ถ้า NavMesh คำนวณเส้นทางเสร็จแล้ว และเดินไปถึงระยะ 1.5 เมตรจากเป้าหมาย
                if (!agent.pathPending && agent.remainingDistance <= 1.5f)
                {
                    Debug.Log("ถึงจุดเกิดเสียงแล้ว ไม่เจอใคร เลิกสนใจเสียง กลับไปรับคำใบ้ต่อ!");
                    isDistractedByNoise = false; // ปิดสวิตช์ทันที ระบบใบ้พิกัดจะได้ทำงานต่อ
                }
            }

            if (!isDistractedByNoise)
            {
                if (Alert_CurrentBehavior == AlertBehave.trail)
                {
                    hintTimer += Time.deltaTime;
                    if (hintTimer >= hintInterval)
                    {
                        currentTargetPos = enemy_script.playerTransform.position;
                        hintTimer = 0f;
                    }
                }
                else
                {
                    hintTimer += Time.deltaTime;
                    float shortHintInterval = 2f;

                    if (hintTimer >= shortHintInterval)
                    {
                        currentTargetPos = enemy_script.playerTransform.position;
                        hintTimer = 0f;
                    }
                }
            }
        }
    }

    void EndAlertState()
    {
        Debug.Log("หมดเวลา Alert! ยอมแพ้และคืนตั๋ว");
        Enemy_combatManager.Instance.ReleaseToken(this.gameObject);
        enemy_script.currentState = enemy_stage.EnemyState.alertSearching;
        //enemy_script.baseState = enemy_stage.EnemyState.alertSearching;
        OnRanDom = false;
        isSurrounding = false;
        On_TriggerGroupAlert = false;
    }

    public void Start_TriggerGroupAlert()
    {
        if (!On_TriggerGroupAlert)
        {
            TriggerGroupAlert();
            On_TriggerGroupAlert = true;
        }
    }

    public void TriggerGroupAlert()
    {
        float shoutRadius = 20f; // รัศมีเสียงตะโกน (ปรับให้ได้ยินข้ามห้องได้)

        // กางอาณาเขตวงกลมหาเพื่อนที่อยู่ในระยะ
        Collider[] friendsNearby = Physics.OverlapSphere(transform.position, shoutRadius, FriendNeraByMask);

        foreach (Collider friend in friendsNearby)
        {
            // เช็คว่าไม่ใช่ตัวเอง
            if (friend.gameObject != this.gameObject)
            {
                // ใช้ TryGetComponent เช็คว่าเป็นศัตรูไหม พร้อมกับดึงสคริปต์มาในบรรทัดเดียว!
                if (friend.TryGetComponent<enemy_stage>(out enemy_stage friendStage))
                {
                    // ถ้าเพื่อนยังไม่ได้อยู่ในโหมด Alert
                    if (friendStage.currentState != enemy_stage.EnemyState.Alert)
                    {
                        // 1. ปลุกเพื่อนให้ตื่นตัว
                        friendStage.currentState = enemy_stage.EnemyState.Alert;

                        // 2. โยนพิกัดไปให้เพื่อน
                        if (friend.TryGetComponent<Enemy_Alert>(out Enemy_Alert friendAlert) && Player.Instance != null)
                        {
                            friendAlert.HandleNoiseAlert(Player.Instance.transform.position);
                        }
                    }
                }
            }
        }
    }
}
