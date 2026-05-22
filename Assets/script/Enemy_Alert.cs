using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Alert : MonoBehaviour
{
    enemy enemy_script;
    NavMeshAgent agent;


    [Header("Flanking Settings")]

    Vector3 sideDir; 
    Vector3 intermediatePoint ;

    bool hasFlanked = false; // ตัวแปรเช็คว่าฉีกออกไปหรือยัง
    public float flankDistance = 10f; // ระยะห่างจากตัวผู้เล่นเวลาโอบ
    public enum FlankDirection { Left, Right, Direct }
    public FlankDirection moveStyle = FlankDirection.Direct; // ตั้งค่าใน Inspector ของศัตรูแต่ละตัว

    public enum AlertBehave {flank, cover };
    public AlertBehave Behavior = AlertBehave.flank;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemy_script = GetComponent<enemy>();
        agent = GetComponent<NavMeshAgent>();

        
    }

    // Update is called once per frame
    void Update()
    {
        if (agent != null && enemy_script != null)
        {
            if (enemy_script.currentState == enemy.EnemyState.Alert)
            {
                //FindCover(); 
                if (Behavior == AlertBehave.flank) 
                {
                    flank();
                    //print("iS Flankkkkk");}
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

            if (distToPlayerSide < 2f)
            {
                agent.SetDestination(playerPos);
                print("ถึงขอบกำแพงแล้ว! เริ่มตีโอบเข้ากลาง");
                Behavior = AlertBehave.cover;
            }
        }
    }
}
