using UnityEngine;
using UnityEngine.AI;

public class Enemy_Anime_Controller : MonoBehaviour
{
    private Animator anim;
    private NavMeshAgent agent;

    // อ้างอิงสคริปต์จัดการสถานะ (เช่น เลือด, การมองเห็น)
    private enemy_stage enemyScript;

    void Start()
    {
        // ดึงคอมโพเนนต์มาเก็บไว้
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        enemyScript = GetComponent<enemy_stage>();
    }

    void Update()
    {
        if (anim == null) return;

        // 1. ควบคุมเลเยอร์ล่างสุด (Base Layer) - การเดิน/วิ่ง
        if (agent != null && agent.isActiveAndEnabled)
        {
            // ส่งค่าความเร็วของ NavMesh ไปที่พารามิเตอร์ Speed เพื่อให้ Blend Tree ทำงาน
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }

        // 2. ควบคุมสถานะสลบ/ตาย (ควบคุมตัวแปร unconscious สำหรับ Full Body Layer)
        if (enemyScript != null)
        {
            // เช็กว่าศัตรูอยู่ในสถานะ สลบ (faint) หรือ ตาย (dead)
            bool isDown = (enemyScript.currentState == enemy_stage.EnemyState.faint ||
                           enemyScript.currentState == enemy_stage.EnemyState.dead);

            // อัปเดต Bool unconscious (ถ้าเป็น true มันจะวิ่งไปนอน DeadPose ตลอดกาล)
            anim.SetBool("unconscious", isDown);
        }
    }

    // ==========================================
    // ส่วนที่ 3: ฟังก์ชันสำหรับเลเยอร์บน (Upper Layer - ถือปืน)
    // ==========================================

    // เรียกเมื่อศัตรูระวังตัว ให้ยกปืนขึ้น (Alert)
    public void SetAlert(bool isAlert)
    {
        if (anim != null) anim.SetBool("Alert", isAlert);
    }

    // เรียกเมื่อศัตรูกำลังยิง
    public void SetFiring(bool isFiring)
    {
        if (anim != null) anim.SetBool("IsFiring", isFiring);
    }

    // ==========================================
    // ส่วนที่ 4: ฟังก์ชันสำหรับระบบ Takedown (Full Body Layer)
    // สคริปต์ของผู้เล่น (Hex) จะเป็นคนเรียกใช้ฟังก์ชันพวกนี้
    // ==========================================

    // โดนเทคดาวน์จากด้านหน้า
    public void PlayFrontTakedown()
    {
        if (anim != null) anim.SetTrigger("FrontTakeDown");
    }

    // โดนเทคดาวน์จากด้านหลัง (ล็อกคอ)
    public void PlayBehindTakedown()
    {
        // ใช้ชื่อ Trigger ตามที่คุณตั้งไว้ในรูปที่ 4 (Behide)
        if (anim != null) anim.SetTrigger("Behide");
    }

    // ผู้เล่นตัดสินใจ: สังหารศัตรู
    public void PlayKill()
    {
        if (anim != null) anim.SetTrigger("Kill");
    }

    // ผู้เล่นตัดสินใจ: ทำให้สลบ
    public void PlayKnockOut()
    {
        if (anim != null) anim.SetTrigger("KnockOut");
    }
}