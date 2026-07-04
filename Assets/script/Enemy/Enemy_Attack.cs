using UnityEngine;


public class Enemy_Attack : MonoBehaviour
{
    [Header("Attack Settings (ปรับแต่งอิสระ)")]
    public float damage = 10f;          // ความแรงของการโจมตี
    public float fireRate = 0.5f;       // ความเร็วในการโจมตี (0.5 คือ ยิง 2 นัดต่อ 1 วินาที)
    public float attackRange = 15f;     // ระยะโจมตีสูงสุด

    private float nextAttackTime = 0f;  // ตัวทดเวลาเพื่อคำนวณ Rate of Fire

    // อ้างอิงสคริปต์อื่นในตัว AI
    private enemy_stage enemyScript;
    private EnemyRaycast enemyRaycast;

    void Start()
    {
        enemyScript = GetComponent<enemy_stage>();
        enemyRaycast = GetComponent<EnemyRaycast>();
    }

    void Update()
    {
        // 1. เช็คว่า AI อยู่ในโหมดตื่นตัว (Alert) หรือไม่
        if (enemyScript.currentState == enemy_stage.EnemyState.Alert)
        {
            // 2. ทำงานร่วมกับ Raycast: เช็คว่า "มองเห็นผู้เล่นชัดเจนจริงๆ" ใช่ไหม?
            if (enemyRaycast.foundPlayer)
            {
                // 3. เช็คว่าผู้เล่นอยู่ในระยะโจมตีหรือไม่
                float distToPlayer = Vector3.Distance(transform.position, enemyScript.playerTransform.position);
                if (distToPlayer <= attackRange)
                {
                    // 4. เช็ค Cooldown การโจมตี (Rate of Fire)
                    if (Time.time >= nextAttackTime)
                    {
                        PerformAttack();

                        // ตั้งเวลาสำหรับการโจมตีครั้งถัดไป
                        nextAttackTime = Time.time + fireRate;
                    }
                }
            }
        }
    }

    void PerformAttack()
    {
        // บริเวณนี้คือจุดที่คุณสามารถใส่ เอฟเฟกต์ปืนพ่นไฟ, เสียงยิงปืน, หรือแอนิเมชันโจมตีได้
        Debug.Log($"<color=red><b>{gameObject.name} ยิง/โจมตี ผู้เล่น! (ดาเมจ: {damage})</b></color>");

        // TODO: ส่งค่าดาเมจไปหักเลือดของผู้เล่น
        // ตัวอย่างการเรียกใช้ (ถ้าคุณมีสคริปต์จัดการเลือดที่ตัวผู้เล่น):
        /*
        if (enemyScript.playerTransform.TryGetComponent<Player_Health>(out Player_Health playerHealth))
        {
            playerHealth.TakeDamage(damage);
        }
        */
    }
}