using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Report : MonoBehaviour
{
    NavMeshAgent agent;
    enemy_stage enemy_script;

    public LayerMask friendMask;
    public float scanRadius = 30f;
    private bool On_Report = false;

    private enemy_stage closestFriend;
    private Coroutine reportCoroutine; // เอาไว้เช็คและหยุด Coroutine เก่าเผื่อมีการเรียกซ้ำ

    private void Awake()
    {
        friendMask = LayerMask.GetMask("enemy");
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy_script = GetComponent<enemy_stage>();
    }

    private void Update()
    {
        if (enemy_script.currentState == enemy_stage.EnemyState.report)
        {
            if (!On_Report)
            {
                On_Report = true;
                StartReporting();   
            }
        }
        else
        {
            CancelReporting();
            On_Report = false;
        }
    }

    //1. ฟังก์ชันนี้มีไว้รับคำสั่ง! (ให้ State Machine เรียกใช้ฟังก์ชันนี้ "แค่ครั้งเดียว" ตอนเข้า State Report)
    public void StartReporting()
    {
        
            // ป้องกันการเรียกซ้อนทับกัน
            if (reportCoroutine != null) StopCoroutine(reportCoroutine);

            reportCoroutine = StartCoroutine(ReportSequence());
        
    }

    //2. Coroutine ทำงานเป็นขั้นเป็นตอน (Sequence)
    IEnumerator ReportSequence()
    {
        // --- ขั้นที่ 1: หาเพื่อน ---
        FindClosestFriend();

        if (closestFriend == null)
        {
            Debug.Log("ไม่มีเพื่อนให้ไปหา! ยกเลิกการ Report แล้วเข้า Alert ตัวเองเลย");
            enemy_script.currentState = enemy_stage.EnemyState.Alert;
            yield break; // จบการทำงานของ Coroutine ทันที
        }

        // --- ขั้นที่ 2: สั่งให้เดินไปหาเพื่อน ---
        Debug.Log("กำลังวิ่งไปหาเพื่อน: " + closestFriend.gameObject.name);

        // --- ขั้นที่ 3: รอจนกว่าจะเดินถึงตัวเพื่อน (อัปเกรดใหม่!) ---
        float safeStopDistance = 2.5f; // ระยะเบรกคุยกัน (2.5 เมตร - กันพุงชนกัน)

        while (closestFriend != null)
        {
            // อัปเดตเป้าหมายเรื่อยๆ เผื่อเพื่อนกำลังเดินลาดตระเวนอยู่ จะได้เดินตามไปคุย
            agent.SetDestination(closestFriend.transform.position);

            // วัดระยะห่างเองเพื่อป้องกันบั๊ก NavMesh ชนกัน
            Vector3 offset = transform.position - closestFriend.transform.position;
            offset.y = 0; // ตัดความสูงทิ้ง

            // ถ้าระยะห่างน้อยกว่า 2.5 เมตร ถือว่าถึงแล้ว! ให้พังลูปออกไปทำขั้นต่อไป
            if (!agent.pathPending && offset.sqrMagnitude <= (safeStopDistance * safeStopDistance))
            {
                agent.ResetPath(); // สั่งเบรกหยุดเดินทันที
                break;
            }

            yield return null;
        }

        // --- ดักบั๊ก: ระหว่างวิ่งไปหาเพื่อน เพื่อนอาจตาย/สลบ ---
        if (closestFriend == null || closestFriend.currentState == enemy_stage.EnemyState.dead || closestFriend.currentState == enemy_stage.EnemyState.faint)
        {
            Debug.Log("เพื่อนตุยระหว่างทาง! ยกเลิกการ Report");
            enemy_script.currentState = enemy_stage.EnemyState.Alert;
            yield break;
        }

        // --- ขั้นที่ 4: ถึงตัวเพื่อนแล้ว เริ่มการคุย (Report) ---
        Debug.Log("ถึงตัวเพื่อนแล้ว! เริ่มทำการ Report");
        yield return new WaitForSeconds(2f);

        // --- ขั้นที่ 5: สั่งให้เพื่อน Alert ---
        Debug.Log("เพื่อน Alert แล้ว!");
        closestFriend.currentState = enemy_stage.EnemyState.Alert;
        //closestFriend.onAlert = true;

        // --- ขั้นที่ 6: เปลี่ยนสถานะตัวเองเป็น Alert ด้วย ---
        enemy_script.currentState = enemy_stage.EnemyState.Alert;
        //enemy_script.onAlert = true;
    }

    void FindClosestFriend()
    {
        Debug.Log("ค้นหาคนที่อยู่ใกล้ที่สุด");
        Collider[] friendsInArea = Physics.OverlapSphere(transform.position, scanRadius, friendMask);

        float minDistanceSqr = Mathf.Infinity;
        closestFriend = null;

        foreach (var col in friendsInArea)
        {
            if (col.gameObject == this.gameObject) continue;

            enemy_stage friendScript = col.GetComponent<enemy_stage>();

            // จุดที่แก้บักโลจิกจากโค้ดเดิมของคุณ: ต้องใช้ && ไม่ใช่ || 
            if (friendScript != null && !friendScript.wasFaint)
            {
                if (friendScript.currentState != enemy_stage.EnemyState.faint && friendScript.currentState != enemy_stage.EnemyState.dead)
                {
                    float distSqr = (col.transform.position - transform.position).sqrMagnitude;

                    if (distSqr < minDistanceSqr)
                    {
                        minDistanceSqr = distSqr;
                        closestFriend = friendScript;
                    }
                }
            }
        }
    }

    public void CancelReporting()
    {
        // 1. หยุดวงจร Coroutine
        if (reportCoroutine != null)
        {
            StopCoroutine(reportCoroutine);
            reportCoroutine = null; // คืนค่าความว่างเปล่า
        }

        // 2. ลืมเป้าหมาย
        closestFriend = null;

        // 3. สั่งหยุดขยับขา (ถ้า Agent ยังเปิดใช้งานอยู่)
        //if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        //{
            //agent.ResetPath();
        //}

        //Debug.Log("ยกเลิกการ Report และรีเซ็ตค่าเรียบร้อยแล้ว!");
    }
}