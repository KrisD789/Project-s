using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Report : MonoBehaviour
{
    NavMeshAgent agent;
    enemy_stage enemy_script;

    public LayerMask friendMask ; // ระบุ Layer ของ Enemy
    public float scanRadius = 30f; // รัศมีการมองหาเพื่อน

    // เก็บเป็นสคริปต์เพื่อนเลย จะได้สั่งงานต่อได้ง่ายครับ
    private enemy_stage closestFriend;

    private void Awake()
    {
        friendMask = LayerMask.GetMask("enemy");
    }
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy_script = GetComponent<enemy_stage>();
    }

    void Update()
    {
        if (enemy_script.currentState == enemy_stage.EnemyState.report)
        {
            FindClosestFriend();

            // ทดสอบ: ลากเส้นสีเขียวไปหาเพื่อนที่ใกล้ที่สุด
            if (closestFriend != null && enemy_script.currentState == enemy_stage.EnemyState.report)
            {
                Debug.DrawLine(transform.position, closestFriend.transform.position, Color.green);
                goToFriend();
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    StartCoroutine(ReportToFriend());
                }
            }
        }
    }

    void FindClosestFriend()
    {
        Debug.Log("ค้นหาคนที่อยู่ไกล้ที่สุด");
        // 1. กวาดหา Collider รอบตัวในระยะที่กำหนด
        Collider[] friendsInArea = Physics.OverlapSphere(transform.position, scanRadius, friendMask);

        float minDistanceSqr = Mathf.Infinity;
        closestFriend = null; // รีเซ็ตทุกครั้งที่หา

        foreach (var col in friendsInArea)
        {
            // 2. ป้องกันไม่ให้เจอ "ตัวเอง" (เพราะตัวเองใกล้สุดคือ 0 เมตรเสมอ)
            if (col.gameObject == this.gameObject) continue;

            // 3. ดึงสคริปต์เพื่อนออกมา (เผื่อจะเช็กสถานะ เช่น ต้องไม่สลบ)
            enemy_stage friendScript = col.GetComponent<enemy_stage>();

            if (friendScript != null && friendScript.wasFaint != true)
            {
                if (friendScript.currentState != enemy_stage.EnemyState.faint || friendScript.currentState != enemy_stage.EnemyState.dead)
                {
                    // คำนวณระยะทางแบบยกกำลังสอง (sqrMagnitude) เพื่อความเร็ว
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

    void goToFriend()
    {
        agent.SetDestination(closestFriend.transform.position);
    }

    IEnumerator ReportToFriend()
    {
        yield return new WaitForSeconds(5);

        Debug.Log("Alert!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    }
}
