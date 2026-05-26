using UnityEngine;
using UnityEngine.AI;
using static enemy;

public class EnemyRaycast : MonoBehaviour
{
    //public static EnemyRaycast enemyRaycast { get; private set; }

    enemy Enemy_script;
    Enemy_Investigate Enemy_Investigate_script;
    Enemy_Alert enemt_alert_Script;
    LightZone lightZone;
    Door DoorTarget;
    NavMeshAgent agent;
    Enemy_Task enemy_task_script;

    [Header("ตั้งค่าการมองเห็น")]
    public float viewRadius = 10f;      // ระยะมองเห็นไกลแค่ไหน
    [Range(0, 360)]
    public float viewAngle = 90f;       // มุมมองกว้างแค่ไหน (FOV)
    public int raycastCount = 6;
    bool foundPlayer = false;

    public LayerMask targetMask;        // Layer ของผู้เล่น (เช่น "Player")
    public LayerMask obstacleMask;      // Layer ของสิ่งกีดขวาง (เช่น "Wall", "Default")
    public LayerMask EnMask;
    public LayerMask AllieMask;

    //[Header("อ้างอิง")]
    GameObject playerObj;
    LightDetect P_Light_detect;

    public Transform EnemyHeadRaycast;
    Transform player;            // ตัวผู้เล่น (ควรกำหนดหรือหาอัตโนมัติ)
    Transform bodyToRotate;

    private Quaternion lastSeenRotation;
    public float turnSpeed = 5f; // ความเร็วในการหันตัว

    float timer = 0;



    //float player_stBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Awake()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player");
        P_Light_detect = playerObj.GetComponent<LightDetect>();

        player = playerObj.transform;
        bodyToRotate = transform;
        Enemy_script = GetComponent<enemy>();
        Enemy_Investigate_script = GetComponent<Enemy_Investigate>();
        enemt_alert_Script = GetComponent<Enemy_Alert>();
        enemy_task_script = GetComponent<Enemy_Task>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Enemy_script.currentState != enemy.EnemyState.dead && Enemy_script.currentState != enemy.EnemyState.faint)
        {

            Raycast();

            // เช็กว่าถ้าสถานะปัจจุบันไม่ใช่ Alert ให้ส่ายหัวปกติ
            if (Enemy_script.currentState != enemy.EnemyState.Alert)
            {
                // ส่ายหัวไปมาจริงๆ (สมมติว่าหมุนแกน Y)
                float angle = Mathf.Sin(Time.time * 2f) * (viewAngle / 2f);
                // EnemyHeadRaycast คือ Transform ส่วนหัวของ AI
                EnemyHeadRaycast.localRotation = Quaternion.Euler(0, angle, 0);
            }
            else
            {
                if (foundPlayer)
                {
                    //Debug.Log("Found");
                    // โหมด Alert:
                    if (player != null && P_Light_detect != null)
                    {
                        // ถ้าเห็นผู้เล่นชัดเจน (ไฟสว่างพอ)
                        if (P_Light_detect.light_meter >= 50)
                        {
                            // 1. คำนวณทิศทางหาผู้เล่นในแนวราบ (Y คงที่) เพื่อไม่ให้ตัว AI เอียง[cite: 11]
                            Vector3 directionToPlayer = player.position - transform.position;
                            directionToPlayer.y = 0;

                            if (directionToPlayer != Vector3.zero)
                            {
                                // 2. จำตำแหน่งการหมุนนี้ไว้[cite: 11]
                                lastSeenRotation = Quaternion.LookRotation(directionToPlayer);

                                // 3. หันทั้งตัว (bodyToRotate) ไปหาผู้เล่น[cite: 11]
                                bodyToRotate.rotation = Quaternion.Slerp(bodyToRotate.rotation, lastSeenRotation, Time.deltaTime * turnSpeed);

                                // 4. ส่วนหัวก็ยังคงจ้องไปที่ตัวผู้เล่น (รวมแนวตั้งด้วย)[cite: 11]
                                Vector3 headDir = player.position - EnemyHeadRaycast.position;
                                EnemyHeadRaycast.rotation = Quaternion.Slerp(EnemyHeadRaycast.rotation, Quaternion.LookRotation(headDir), Time.deltaTime * turnSpeed);
                            }
                        }

                    }
                }
                else
                {
                    timer += Time.deltaTime;
                    if (timer <= 50)
                    {


                        // ถ้าผู้เล่นหายไปในความมืด: ให้ AI หันค้างไว้ที่ตำแหน่งล่าสุดที่เคยเห็น[cite: 11]
                        bodyToRotate.rotation = Quaternion.Slerp(bodyToRotate.rotation, lastSeenRotation, Time.deltaTime * turnSpeed);

                        float angle = Mathf.Sin(Time.time * 2f) * (viewAngle / 2f);
                        EnemyHeadRaycast.localRotation = Quaternion.Euler(0, angle, 0);

                        //Debug.Log("not Found");
                    }
                    else
                    {
                        timer = 0;
                        Enemy_script.currentState = enemy.EnemyState.Investigate;
                        Enemy_script.onAlert = true;
                    }
                }
            }

           
        }
    }

   

    void Raycast()
    {
        foundPlayer = false;

        // คำนวณหาจุดเริ่มต้น (ซ้ายสุดของกรวย)
        float startAngle = -viewAngle / 2;
        float angleStep = viewAngle / raycastCount;
        LayerMask combinedMask = targetMask | EnMask | AllieMask | obstacleMask;
        


        if (Enemy_script.currentState == enemy.EnemyState.faint || Enemy_script.currentState == enemy.EnemyState.dead) return;
        

        for (int i = 0; i <= raycastCount; i++)
        {
            // คำนวณทิศทางของ Ray แต่ละเส้น โดยอิงจากมุมหมุนของหัว
            float currentAngle = startAngle + (angleStep * i);
            Vector3 direction =  Quaternion.Euler(0, currentAngle, 0) * EnemyHeadRaycast.forward;

            Debug.DrawRay(EnemyHeadRaycast.position, direction * viewRadius, Color.cyan);

            // รวม Mask ทั้งหมดที่ AI "ควรจะมองเห็น"
           

            if (Physics.Raycast(EnemyHeadRaycast.position, direction, out RaycastHit hit, viewRadius, combinedMask))
            {
                // บรรทัดนี้จะบอกเลยว่าเลเซอร์มันไปค้างอยู่ที่อะไร
                //Debug.Log("Raycast ชนกับ: " + hit.collider.name + " ที่ Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));

                if (((1 << hit.collider.gameObject.layer) & obstacleMask) != 0) continue; //เช็กกำแพง

                RayCastHit_Process(direction, hit);
                
            }
        }
    }

    void RayCastHit_Process(Vector3 direction, RaycastHit hit)
    {
        

        if (hit.collider.CompareTag("Player"))
        {
            //var obj = hit.collider.GetComponent<LightDetect>();
            if (!Physics.Linecast(EnemyHeadRaycast.position, hit.point, obstacleMask) || hit.collider.CompareTag("Door"))
            {
                
                if (P_Light_detect != null && P_Light_detect.light_meter >= 50)
                {
                    enemt_alert_Script.counting_AlertTimer(true);
                    Enemy_script.currentState = enemy.EnemyState.Alert;
                    foundPlayer = true;
                    
                }

                else enemt_alert_Script.counting_AlertTimer(false);
            }

        }
        //else foundPlayer = false;

        if (hit.collider.CompareTag("Light") || hit.collider.CompareTag("Door"))
        {
            checkEnvirament(hit);

            CheckBehindObject(hit.point + direction * 0.1f, direction, hit);
        }

        if (Enemy_script.currentState != enemy.EnemyState.Alert)
        {
            if (hit.collider.CompareTag("enemy"))
            {
                var obj = hit.collider.GetComponent<enemy>();

                if (obj != null && obj.E_lightMeter >= 50)
                {
                    //Debug.Log("found teamMate!!!");

                    //Enemy_script.headRenderer.material.color = Color.yellow;

                    if (obj.currentState == enemy.EnemyState.faint)
                    {
                        // แทนที่จะสั่งเดินและเช็คระยะตรงนี้
                        // ให้ส่งงานไปที่ TodoList เพื่อให้ระบบจัดการคิวทำงานแทน
                        enemy_task_script.AddToTodoList(hit.point, obj, WorkTask.TaskType.wakeUp);

                        // เปลี่ยนสถานะตัวเองให้เริ่มไปตรวจสอบ
                        Enemy_script.currentState = enemy.EnemyState.Investigate;
                    }

                    if (obj.currentState == enemy.EnemyState.dead)
                    {

                        // แทนที่จะสั่งเดินและเช็คระยะตรงนี้
                        // ให้ส่งงานไปที่ TodoList เพื่อให้ระบบจัดการคิวทำงานแทน
                        enemy_task_script.AddToTodoList(hit.point, obj, WorkTask.TaskType.alarm);

                        // เปลี่ยนสถานะตัวเองให้เริ่มไปตรวจสอบ
                        Enemy_script.currentState = enemy.EnemyState.Investigate;
                    }

                }
                //else Enemy_script.headRenderer.material.color = Color.green;
            }
        }

    }
    void CheckBehindObject(Vector3 newOrigin, Vector3 dir, RaycastHit FirstHit)
    {
        //Debug.Log("CheckBehideObject");

        // คำนวณระยะที่เหลือจริง (ไม่ให้มองไกลเกินรัศมีเดิม)
        float distToFirstHit = Vector3.Distance(EnemyHeadRaycast.position, FirstHit.point);
        float remainingDist = viewRadius - distToFirstHit;

        if (remainingDist <= 0) return; // ถ้าระยะหมดแล้วก็ไม่ต้องยิงต่อ


        // เพิ่ม obstacleMask เข้าไปด้วยเพื่อกันมองทะลุกำแพงหลังไฟ
        if (Physics.Raycast(newOrigin, dir, out RaycastHit secondHit, remainingDist, targetMask | AllieMask | obstacleMask | EnMask))
        {
            RayCastHit_Process(dir, secondHit);
        }
    }

    void checkEnvirament(RaycastHit hit)
    {
        if (Enemy_script.currentState == enemy.EnemyState.Alert) return;

        if (hit.collider.CompareTag("Light"))
        {
            var obj = hit.collider.GetComponent<LightZone>();
            if (obj != null && !obj.lightZoneState)
            {
                lightZone = obj;
                //agent.SetDestination(hit.point);
                enemy_task_script.AddToTodoList(hit.point, obj, WorkTask.TaskType.Slight);
                Enemy_script.currentState = EnemyState.Investigate;
            }
        }
        else if (hit.collider.CompareTag("Door"))
        {
            //Debug.Log("ใครเปิดประตู ???");
            var obj = hit.collider.GetComponent<Door>();

            if (obj != null && obj.currentState == Door.DoorState.Open)
            {
                if (Enemy_script.currentState == enemy.EnemyState.alertSearching)
                {
                    Enemy_script.currentState = enemy.EnemyState.Investigate;
                    GetComponent<Enemy_Investigate>().searcingLastHearPosition(obj.transform.position);
                }


                //Debug.Log("ใครเปิดประตู ???");
                if (!obj.openByAi)
                {
                    DoorTarget = obj;
                    agent.SetDestination(hit.point);
                    enemy_task_script.AddToTodoList(hit.point, obj, WorkTask.TaskType.TDoor);
                    Enemy_script.currentState = EnemyState.Investigate;
                    //Debug.Log("ใครเปิดประตู ???");
                }
            }
        }
    
    }

    
    
    
}
