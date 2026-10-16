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
        // 1. หา Animator: ถ้าสคริปต์นี้แปะอยู่บนตัวโมเดลที่มี Animator ก็ใช้ GetComponent ตัวมันเองได้เลย
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>(); // เผื่อเหนียวไว้
        }

        // ดึงคอมโพเนนต์มาเก็บไว้
        agent = GetComponentInParent<NavMeshAgent>();
        enemyScript = GetComponentInParent<enemy_stage>();

        //  เพิ่ม Debug เช็กความชัวร์ (ถ้ามี Error แดงขึ้นตอนเริ่มเกม จะได้รู้ทันทีว่าลืมแปะอะไร)
        if (anim == null) Debug.LogError("Enemy_Anime_Controller: หา Animator ไม่เจอ!");
        if (agent == null) Debug.LogError("Enemy_Anime_Controller: หา NavMeshAgent บน Parent ไม่เจอ!");
        if (enemyScript == null) Debug.LogError("Enemy_Anime_Controller: หา enemy_stage บน Parent ไม่เจอ!");
    }

    void Update()
    {
        if (anim == null) return;

        // 1. ควบคุมการเดิน/วิ่ง
        if (agent != null && agent.isActiveAndEnabled)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }

        if (enemyScript != null)
        {
            // 2. ควบคุมสถานะสลบ/ตาย
            bool isDown = (enemyScript.currentState == enemy_stage.EnemyState.faint ||
                           enemyScript.currentState == enemy_stage.EnemyState.dead);
            anim.SetBool("unconscious", isDown);

            // 3. เพิ่มระบบบังคับยอมจำนน (สำหรับ Scam Commander หรือ Dummy)
            bool isSurrender = false;

            // เช็กว่าเป็น ScamCommander หรือถูกตั้งค่าเป็น Dummy หรือไม่
            if (gameObject.CompareTag("ScamCommander") || enemyScript.currentState == enemy_stage.EnemyState.Dummy)
            {
                // จะยอมจำนนตลอดเวลา *ยกเว้น* ตอนกำลังโดนล็อคคอ (OnGrab) หรือตาย/สลบไปแล้ว
                if (enemyScript.currentState != enemy_stage.EnemyState.OnGrab && !isDown)
                {
                    isSurrender = true;
                }
            }

            // ส่งค่าไปที่ Animator ของศัตรู
            anim.SetBool("is_surrender", isSurrender);
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
        Debug.Log("Play FrontTakeDown");
    }

    // โดนเทคดาวน์จากด้านหลัง (ล็อกคอ)
    public void PlayBehindTakedown()
    {
        // ใช้ชื่อ Trigger ตามที่คุณตั้งไว้ในรูปที่ 4 (Behide)
        if (anim != null) anim.SetTrigger("Behide");
        Debug.Log("Play Behide");
    }

    // ผู้เล่นตัดสินใจ: สังหารศัตรู
    public void PlayKill()
    {
        if (anim != null) anim.SetTrigger("Kill");
        Debug.Log("Play Kill");
    }

    // ผู้เล่นตัดสินใจ: ทำให้สลบ
    public void PlayKnockOut()
    {
        if (anim != null) anim.SetTrigger("KnockOut");
        Debug.Log("Play KnockOut");
    }

    public void SetCarriedState(bool carried)
    {
        if (anim != null) anim.SetBool("is_carried", carried);
    }
}