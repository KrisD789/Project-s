using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static enemy_stage;

public class EnemyRaycast : MonoBehaviour
{
    enemy_stage Enemy_script;
    Enemy_Investigate Enemy_Investigate_script;
    Enemy_Alert enemt_alert_Script;
    LightZone lightZone;
    Door DoorTarget;
    NavMeshAgent agent;
    Enemy_Task enemy_task_script;
    //Enemy_Weapon enemy_weapon_script;

    [Header("ตั้งค่าการมองเห็น")]
    public float viewRadius = 10f;
    [Range(0, 360)]
    public float viewAngle = 90f;
    public int raycastCount = 6;
    public bool foundPlayer = false;

    public LayerMask targetMask;
    public LayerMask obstacleMask;
    public LayerMask EnMask;
    public LayerMask AllieMask;

    [Header("อ้างอิง")]
    GameObject playerObj;
    LightDetect P_Light_detect;
    Player Player_state;

    public Transform EnemyHeadRaycast;
    Transform player;
    Transform bodyToRotate;

    private Quaternion lastSeenRotation;
    public float turnSpeed = 5f;

    //float timer = 0;
    //private bool actuallySeePlayer = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (Player.Instance != null)
        {
            playerObj = Player.Instance.gameObject;
            P_Light_detect = Player.Instance.player_Light_Detect;
            Player_state = Player.Instance;

            Debug.Log("EnemyRaycast: เชื่อมต่อกับ Player.Instance สำเร็จ!");
        }

        else
        {
            Debug.LogError("EnemyRaycast: !!!! หา Player.Instance ไม่เจอ! ");
        }

        player = playerObj.transform;
        bodyToRotate = transform;

        if (!TryGetComponent<enemy_stage>(out Enemy_script))
            Debug.LogWarning("EnemyRaycast:  หา enemy_stage ไม่เจอ!");

        if (!TryGetComponent<Enemy_Investigate>(out Enemy_Investigate_script))
            Debug.LogWarning("EnemyRaycast:  หา Enemy_Investigate ไม่เจอ!");

        if (!TryGetComponent<Enemy_Alert>(out enemt_alert_Script))
            Debug.LogWarning("EnemyRaycast:  หา Enemy_Alert ไม่เจอ!");

        if (!TryGetComponent<Enemy_Task>(out enemy_task_script))
            Debug.LogWarning("EnemyRaycast:  หา Enemy_Task ไม่เจอ!");
    }

    

   

    void Update()
    {
        if (Enemy_script.currentState == enemy_stage.EnemyState.dead || Enemy_script.currentState == enemy_stage.EnemyState.faint 
            || Enemy_script.currentState == enemy_stage.EnemyState.Dummy || Enemy_script.currentState == enemy_stage.EnemyState.OnGrab)
            return;

        Raycast();
        HandleHeadAndBodyRotation();
    }

    // แยกเรื่องการหมุนหัวและการมองตามผู้เล่นมาไว้ที่นี่เพื่อให้ Update ดูสะอาดขึ้น
    void HandleHeadAndBodyRotation()
    {
        if (Enemy_script.currentState != enemy_stage.EnemyState.Alert)
        {
            // 1. โหมดปกติ (Idle/Investigate)
            // เปิดให้ NavMesh คุมพวงมาลัยเดินตามปกติ และให้หัวส่ายไปมา
            agent.updateRotation = true;
            float angle = Mathf.Sin(Time.time * 2f) * (viewAngle / 2f);
            EnemyHeadRaycast.localRotation = Quaternion.Euler(0, angle, 0);
        }
        else
        {
            // 2. โหมด Alert
            // เช็คว่าผู้เล่นอยู่ในที่สว่างหรือไม่ (แสง >= 50)
            bool isPlayerInLight = (P_Light_detect != null && P_Light_detect.light_meter >= 50);

            if (isPlayerInLight)
            {
                // [ผู้เล่นอยู่ในสว่าง] -> ล็อคเป้า!
                // ปิด NavMesh ชั่วคราว เพื่อไม่ให้มันมาแย่งคุมหน้าตอน AI กำลัง Side Step หรือเล็งปืน
                agent.updateRotation = false;

                // หมุนตัวและหัวหาผู้เล่นแบบสมูท (Slerp)
                Vector3 directionToPlayer = player.position - transform.position;
                directionToPlayer.y = 0;

                if (directionToPlayer != Vector3.zero)
                {
                    lastSeenRotation = Quaternion.LookRotation(directionToPlayer);
                    bodyToRotate.rotation = Quaternion.Slerp(bodyToRotate.rotation, lastSeenRotation, Time.deltaTime * turnSpeed);

                    Vector3 headDir = player.position - EnemyHeadRaycast.position;
                    EnemyHeadRaycast.rotation = Quaternion.Slerp(EnemyHeadRaycast.rotation, Quaternion.LookRotation(headDir), Time.deltaTime * turnSpeed);
                }
            }
            else
            {
                // [ผู้เล่นอยู่ในที่มืด] หรือ [หาไม่เจอ]
                // ลบการล็อคคอ lastSeenRotation ของเก่าทิ้งไป!
                // แล้วเปิด NavMesh คืน เพื่อให้ AI สามารถเดินเลี้ยวซ้ายขวาตามทางเดินได้อย่างอิสระ
                agent.updateRotation = true;

                // ส่วนหัวก็ส่ายค้นหาตามปกติ
                float angle = Mathf.Sin(Time.time * 2f) * (viewAngle / 2f);
                EnemyHeadRaycast.localRotation = Quaternion.Euler(0, angle, 0);

                //Enemy_script.onAlert = true;
            }
        }
    }

    void Raycast()
    {
        foundPlayer = false; // ปรับการมองเห็นเป็นfalseไว้ตลอด เพื่อแก้ปัญหาAIค่าfoundplayer ไม่กลับไปเป็นfalse *******

        float startAngle = -viewAngle / 2;
        float angleStep = viewAngle / raycastCount;
        LayerMask combinedMask = targetMask | EnMask | AllieMask | obstacleMask;

        for (int i = 0; i <= raycastCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * EnemyHeadRaycast.forward;
            Debug.DrawRay(EnemyHeadRaycast.position, direction * viewRadius, Color.cyan);

            if (Physics.Raycast(EnemyHeadRaycast.position, direction, out RaycastHit hit, viewRadius, combinedMask))
            {
                if (((1 << hit.collider.gameObject.layer) & obstacleMask) != 0) continue;
                RayCastHit_Process(direction, hit);
            }
        }
    }

    void RayCastHit_Process(Vector3 direction, RaycastHit hit)
    {
        // -------------------------------------------------------------
        // หัวใจหลักของการจัดระเบียบ: แยกพฤติกรรมการมองเห็นตามสถานะปัจจุบัน
        // -------------------------------------------------------------
        switch (Enemy_script.currentState)
        {
            case enemy_stage.EnemyState.Alert:
                // โหมดตื่นตัวสุดขีด สนใจแค่ตามล่าผู้เล่นอย่างเดียว ไม่ต้องสนไฟหรือเพื่อน
                CheckPlayerPresence(hit, direction);
                break;

            case enemy_stage.EnemyState.Investigate:
                CheckEnvironment(hit, direction);
                break ;

            case enemy_stage.EnemyState.alertSearching:
                // โหมดค้นหา สนใจผู้เล่น และเช็คสภาพแวดล้อม (ประตู, ไฟ) 
                CheckPlayerPresence(hit, direction);
                CheckEnvironment(hit, direction);
                break;

            default:
                // โหมดเดินลาดตระเวนปกติ (Idle/Patrol) สนใจทุกอย่างรอบตัว
                CheckPlayerPresence(hit, direction);
                CheckEnvironment(hit, direction);
                CheckTeammates(hit);
                break;
        }
    }

    // --- แยกฟังก์ชันย่อยเพื่อให้แก้ไขง่ายในอนาคต ---

    void CheckPlayerPresence(RaycastHit hit, Vector3 direction)
    {
        if (!hit.collider.CompareTag("Player")) return;

        bool isLineOfSightClear = false;

        if (!Physics.Linecast(EnemyHeadRaycast.position, hit.point, obstacleMask) || hit.collider.CompareTag("Door"))
        {
            isLineOfSightClear = true;
        }

        if (isLineOfSightClear)
        {
            if (Enemy_script.currentState != enemy_stage.EnemyState.Alert)
            {
                // -- กรณี 1: ยืนเดินปกติ --
                if (Player_state != null && Player_state.currentState != Player.PlayerState.Crouch)
                {
                    if (P_Light_detect != null && P_Light_detect.light_meter >= 70)
                    {
                        Enemy_script.currentState = enemy_stage.EnemyState.Alert;
                        foundPlayer = true;
                    }
                    else if (P_Light_detect != null && P_Light_detect.light_meter >= 50)
                    {
                        Enemy_script.currentState = enemy_stage.EnemyState.Investigate;
                        Enemy_Investigate_script.searcingLastHearPosition(playerObj.transform.position);
                        //foundPlayer = true;
                    }
                }
                // -- กรณี 2: ผู้เล่นนั่งหมอบ (Crouch) --
                else if (Player_state != null && Player_state.currentState == Player.PlayerState.Crouch)
                {
                    // จุดที่ 2: ใช้ crouchTargetPos มาเช็คอีกรอบว่าระดับการหมอบมีลัง/กล่องเตี้ยๆ บังมิดไหม
                    Vector3 crouchTargetPos = playerObj.transform.position + new Vector3(0, -2f, 0);

                    if (!Physics.Linecast(EnemyHeadRaycast.position, crouchTargetPos, obstacleMask))
                    {
                        // ถ้าไม่มีอะไรบังตอนหมอบ ก็มาเช็คแสงต่อ
                        if (P_Light_detect != null && P_Light_detect.light_meter >= 70)
                        {
                            Enemy_script.currentState = enemy_stage.EnemyState.Alert;
                            foundPlayer = true;
                        }
                        else if (P_Light_detect != null && P_Light_detect.light_meter >= 50)
                        {
                            Enemy_script.currentState = enemy_stage.EnemyState.Investigate;
                            //foundPlayer = true;
                        }
                    }
                }
            }
            else
            {
                // -- จุดที่ 3: ศัตรูอยู่ในโหมด Alert กำลังไล่ยิง --
                // ในเมื่ออยู่ในโหมดต่อสู้แล้ว ถ้ามองเห็นตัว (isLineOfSightClear = true) 
                // ก็ควรจะรู้ตำแหน่งผู้เล่นทันที โดยไม่ต้องสนว่าแสงจะ < 50 ก็ตาม
                if (P_Light_detect != null && P_Light_detect.light_meter >= 50)
                {
                    Enemy_script.currentState = enemy_stage.EnemyState.Alert; // ล็อคสถานะ Alert
                    foundPlayer = true; // ยืนยันว่ามองเห็น
                }
            }
        }
    }

    void CheckEnvironment(RaycastHit hit, Vector3 direction)
    {
        if (hit.collider.CompareTag("Light"))
        {
            var obj = hit.collider.GetComponent<LightZone>();
            if (obj != null && !obj.lightZoneState)
            {
                lightZone = obj;
                enemy_task_script.AddToTodoList(hit.point, obj, WorkTask.TaskType.Slight);
                Enemy_script.currentState = enemy_stage.EnemyState.Investigate;
            }
            CheckBehindObject(hit.point + direction * 0.1f, direction, hit);
        }
        else if (hit.collider.CompareTag("Door"))
        {
            var obj = hit.collider.GetComponent<Door>();
            if (obj != null && obj.currentState == Door.DoorState.Open)
            {
                if (Enemy_script.currentState == enemy_stage.EnemyState.alertSearching)
                {
                    Enemy_script.currentState = enemy_stage.EnemyState.Investigate;
                    GetComponent<Enemy_Investigate>().searcingLastHearPosition(obj.transform.position);
                }

                if (!obj.openByAi)
                {
                    DoorTarget = obj;
                    agent.SetDestination(hit.point);
                    enemy_task_script.AddToTodoList(hit.point, obj, WorkTask.TaskType.TDoor);
                    Enemy_script.currentState = enemy_stage.EnemyState.Investigate;
                }
            }
            CheckBehindObject(hit.point + direction * 0.1f, direction, hit);
        }
    }

    void CheckTeammates(RaycastHit hit)
    {
        if (!hit.collider.CompareTag("enemy")) return;

        var obj = hit.collider.GetComponent<enemy_stage>();
        if (obj != null && obj.E_lightMeter >= 50)
        {
            if (obj.currentState == enemy_stage.EnemyState.faint)
            {
                enemy_task_script.AddToTodoList(hit.point, obj, WorkTask.TaskType.wakeUp);
                Enemy_script.currentState = enemy_stage.EnemyState.Investigate;
            }
            else if (obj.currentState == enemy_stage.EnemyState.dead)
            {
                enemy_task_script.AddToTodoList(hit.point, obj, WorkTask.TaskType.alarm);
                Enemy_script.currentState = enemy_stage.EnemyState.Investigate;
            }
        }
    }

    void CheckBehindObject(Vector3 newOrigin, Vector3 dir, RaycastHit FirstHit)
    {
        float distToFirstHit = Vector3.Distance(EnemyHeadRaycast.position, FirstHit.point);
        float remainingDist = viewRadius - distToFirstHit;

        if (remainingDist <= 0) return;

        if (Physics.Raycast(newOrigin, dir, out RaycastHit secondHit, remainingDist, targetMask | AllieMask | obstacleMask | EnMask))
        {
            RayCastHit_Process(dir, secondHit);
        }
    }
}