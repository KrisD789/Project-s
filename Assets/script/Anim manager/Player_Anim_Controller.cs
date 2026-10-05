using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator anim;
    private Player_moveMent movement;
    private Player_Action playerAction;

    void Start()
    {
        anim = GetComponent<Animator>();
        playerAction = GetComponent<Player_Action>();

        if (Player.Instance != null)
        {
            movement = Player.Instance.movement;
        }
    }

    void Update()
    {
        if (anim == null) return;

        if (movement != null)
        {
            // ดึงค่าความเร็วจากสคริปต์ Movement มาคุมแอนิเมชันวิ่ง/เดิน
            anim.SetFloat("Speed", movement.GetCurrentVelocity());
        }

        if (Player.Instance != null)
        {
            anim.SetBool("isCrouching", Player.Instance.currentState == Player.PlayerState.Crouch);
            anim.SetBool("isCarrying", Player.Instance.currentState == Player.PlayerState.CarryingBody);
        }
    }

    // --- Action Triggers ---
    public void PlayGrabEnemy() { if (anim != null) anim.SetTrigger("GrabEnemy"); }
    public void PlayKillEnemy() { if (anim != null) anim.SetTrigger("KillEnemy"); }
    public void PlayKnockoutEnemy() { if (anim != null) anim.SetTrigger("KnockoutEnemy"); }

    // เพิ่ม 2 ตัวนี้สำหรับแบกศพและทิ้งศพ
    public void PlayPickUpBody() { if (anim != null) anim.SetTrigger("PickUpBody"); }
    public void PlayDropBody() { if (anim != null) anim.SetTrigger("DropBody"); }

    // ผูกคำสั่งนี้กับ Animation Event ในหน้าต่าง Animation คลิปเชือดคอ/รัดคอ เพื่อหน่วงเวลาให้ศัตรูตายตรงจังหวะเป๊ะๆ
    public void TriggerTakedownFinish()
    {
        if (playerAction != null)
        {
            // เอาไว้รอเรียก playerAction.FinishTakedown(); ตอนที่แอนิเมชันเล่นจบ
        }
    }
}