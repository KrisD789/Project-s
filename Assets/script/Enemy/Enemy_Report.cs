using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum IncidentType
{
    PlayerBump,         // ชนผู้เล่นตัวเป็นๆ
    FoundUnconscious,   // เจอเพื่อนสลบ
    FoundDead           // เจอศพเพื่อน
}

public class Enemy_Report : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float radioCallDuration = 2.0f; // เวลาที่ยืนคุยวิทยุ
    [SerializeField] private float stepBackDistance = 2f;
    [SerializeField] private float recoilDuration = 0.4f;
    [SerializeField] private float shoutRadius = 50f; // รัศมีเสียงตะโกน (ปรับให้ได้ยินข้ามห้องได้)
    public LayerMask FriendNeraByMask;

    enemy_stage enemy_Stage_script;
    Coroutine ReportSequence_coroutine;

    private void Start()
    {
        
        FriendNeraByMask = LayerMask.GetMask("enemy");
        if(!TryGetComponent<enemy_stage>(out enemy_Stage_script))
        {
            Debug.LogWarning("EnemyReport.cs NotFound enemy_Stage_script ");
        }

        if (!TryGetComponent<NavMeshAgent>(out agent))
        {
            Debug.LogWarning("EnemyReport.cs NotFound agent ");
        }
    }

    private void Update()
    {
        if (enemy_Stage_script.currentState != enemy_stage.EnemyState.report)
        {
            if (ReportSequence_coroutine != null) // เพิ่มบรรทัดนี้ดักไว้
            {
                StopCoroutine(ReportSequence_coroutine);
                ReportSequence_coroutine = null; // คืนค่าความว่างเปล่าหลังจากหยุดแล้ว
            }
        }
    }

    // ฟังก์ชันนี้เรียกเมื่อ: ชนผู้เล่น หรือ เจอศพ
    // ฟังก์ชันนี้จะเป็นตัวรับค่าจากภายนอก (เมื่อชนผู้เล่น หรือ Sensor ตาเห็นศพ)
    public void StartReportState(IncidentType incident, Vector3 targetPos, GameObject bodyFound = null)
    {
        if (enemy_Stage_script.currentState == enemy_stage.EnemyState.report && ReportSequence_coroutine == null)
        { 
            ReportSequence_coroutine = StartCoroutine(ReportSequence(incident, targetPos, bodyFound));

        }
    }

    private IEnumerator ReportSequence(IncidentType incident, Vector3 targetPos, GameObject bodyFound)
    {
        // ลบเป้าหมายเดิมทิ้งและเหยียบเบรกก่อน
        agent.ResetPath();
        agent.isStopped = true;

        // เฟส 1: ตกใจผงะถอยหลัง (ใช้ agent.Move)
        yield return StartCoroutine(RecoilRoutine(targetPos));

        // เฟส 2: ยืนคุยวิทยุสื่อสาร
        Debug.Log($"Enemy: ศูนย์กลาง! ขอรายงานเหตุการณ์ประเภท: {incident}");
        yield return new WaitForSeconds(radioCallDuration);

        // เฟส 3: กระจายข่าวปลุกเพื่อน
        BroadcastAlert(incident, targetPos);

        agent.isStopped = false;
    }

    private IEnumerator RecoilRoutine(Vector3 playerPos)
    {
        // หาเวกเตอร์ทิศทางชี้ออกจากตัวผู้เล่น (เพื่อถอยหลัง)
        Vector3 pushDir = (transform.position - playerPos).normalized;
        pushDir.y = 0;

        // คำนวณความเร็วในการถอยหลัง (ระยะทาง / เวลา)
        float speed = stepBackDistance / recoilDuration;
        float elapsed = 0f;

        // 🚨 ปิดการควบคุมการหมุนของ NavMesh ชั่วคราว เพราะเราจะหมุนหัวมันเอง
        agent.updateRotation = false;

        while (elapsed < recoilDuration)
        {
            // 1. บังคับหันหน้าจ้องผู้เล่นตลอดเวลาที่ถอย (Slerp เพื่อความสมูท)
            Vector3 lookPos = new Vector3(playerPos.x, transform.position.y, playerPos.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos - transform.position), Time.deltaTime * 10f);

            // 2. ใช้คำสั่ง agent.Move() แทนการขยับ Transform ดิบๆ เพื่อความปลอดภัยบน NavMesh
            agent.Move(pushDir * speed * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // คืนสิทธิ์การหันหน้าให้ NavMesh กลับไปจัดการตามปกติหลังถอยเสร็จ
        agent.updateRotation = true;
    }
    private void BroadcastAlert(IncidentType incident, Vector3 knownPosition)
    {
        if (enemy_Stage_script.currentState != enemy_stage.EnemyState.report) return;

        // โค้ดส่งสัญญาณแจ้งศัตรูตัวอื่นในสเตจ (เช่น อัปเดตตัวแปร Global Alert)
        Debug.Log("BroadcastAlert ส่งพิกัดผู้เล่นให้ศัตรูทุกตัวในพื้นที่ทราบแล้ว!");

        if (incident == IncidentType.FoundDead || incident == IncidentType.PlayerBump)
        {
            enemy_Stage_script.currentState = enemy_stage.EnemyState.Alert;
        }
        else if (incident == IncidentType.FoundUnconscious)
        {
            enemy_Stage_script.currentState = enemy_stage.EnemyState.alertSearching;
        }

        // กางอาณาเขตวงกลมหาเพื่อนที่อยู่ในระยะ
        Collider[] friendsNearby = Physics.OverlapSphere(transform.position, shoutRadius, FriendNeraByMask);

        foreach (Collider friend in friendsNearby)
        {
            // เช็คว่าไม่ใช่ตัวเอง
            if (friend.gameObject != this.gameObject )
            {
                if (friend.gameObject.CompareTag("enemy"))
                {
                    // ใช้ TryGetComponent เช็คว่าเป็นศัตรูไหม พร้อมกับดึงสคริปต์มาในบรรทัดเดียว!
                    if (friend.TryGetComponent<enemy_stage>(out enemy_stage friendStage))
                    {
                        if (friendStage.currentState == enemy_stage.EnemyState.faint
                            || friendStage.currentState == enemy_stage.EnemyState.dead
                            || friendStage.currentState == enemy_stage.EnemyState.Dummy)
                        {
                            continue;
                        }


                        if (incident == IncidentType.FoundDead || incident == IncidentType.PlayerBump)
                        {
                            // ถ้าเพื่อนยังไม่ได้อยู่ในโหมด Alert
                            if (friendStage.currentState != enemy_stage.EnemyState.Alert)
                            {
                                // 1. ปลุกเพื่อนให้ตื่นตัว
                                friendStage.currentState = enemy_stage.EnemyState.Alert;

                                // 2. โยนพิกัดไปให้เพื่อน
                                if (friend.TryGetComponent<Enemy_Alert>(out Enemy_Alert friendAlert) && Player.Instance != null)
                                {
                                    friendAlert.HandleNoiseAlert(knownPosition);
                                }

                                //enemy_Stage_script.currentState = enemy_stage.EnemyState.Alert;
                            }
                        }

                        if (incident == IncidentType.FoundUnconscious)
                        {
                            friendStage.currentState = enemy_stage.EnemyState.alertSearching;
                            //enemy_Stage_script.currentState = enemy_stage.EnemyState.alertSearching;
                        }

                    }
                }
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        // 1. ตั้งสีของเส้นวงกลม (ใส่สีอะไรก็ได้ตามใจชอบ)
        Gizmos.color = Color.yellow;

        // 2. กำหนดรัศมีให้ตรงกับที่คุณใช้ใน Physics.OverlapSphere
        float shoutRadius = 40f;

        // 3. สั่งวาดเส้นขอบวงกลม โดยอิงจากตำแหน่งเดียวกับศูนย์กลางของ OverlapSphere
        Gizmos.DrawWireSphere(transform.position, shoutRadius);
    }

}