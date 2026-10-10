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
        if (movement != null) anim.SetFloat("Speed", movement.GetCurrentVelocity());

        if (Player.Instance != null)
        {
            anim.SetBool("isCrouching", Player.Instance.currentMovementState == Player.MovementState.Crouch);
            anim.SetBool("isCarrying", Player.Instance.currentState == Player.PlayerState.CarryingBody);
        }

        //  ตัวจับการขัดจังหวะ (ถ้าผู้เล่นตายตอนกำลังจับศัตรู ให้สั่งยกเลิก)
        if (Player.Instance.currentState == Player.PlayerState.Dead && playerAction.GetGrabbedEnemy() != null)
        {
            TriggerTakedownInterrupt();
        }
    }

    // --- Action Triggers (ขาเข้า สั่งแอนิเมชัน) ---
    public void PlayStealthTakedown() { if (anim != null) anim.SetTrigger("StealthTakedown"); }
    public void PlayFrontTakedown() { if (anim != null) anim.SetTrigger("FrontTakedown"); }

    // ... (ฟังก์ชันเดิมของคุณ) ...

    // --- Animation Events (ขาออก จากแอนิเมชันกลับสู่โค้ด) ---

    // 1. ผูกกับเฟรมกลางๆ (หรือตอนจบ) เพื่อให้รอหน้าจอขึ้น UI ให้ผู้เล่นเลือก Kill หรือ Knockout
    public void TriggerTakedownWaitChoice()
    {
        Debug.Log("รอคำสั่ง Kill หรือ Knockout จากผู้เล่น");
        // ศัตรูจะค้างอยู่ในมือจนกว่าผู้เล่นจะกด (ตามโค้ด ChooseToKill หรือ ChooseToKnockout ของคุณ)
    }

    // 2. ถ้าโดนขัดจังหวะกลางคัน (เช่น ผู้เล่นโดนยิงตาย) ให้เรียกฟังก์ชันนี้
    public void TriggerTakedownInterrupt()
    {
        Debug.Log("โดนขัดจังหวะ ปล่อยศพ!");
        if (playerAction != null)
        {
            playerAction.CancelTakedown(playerAction.GetGrabbedEnemy());
            // อาจจะสั่ง SetTrigger("CancelAction") เพื่อให้แอนิเมชันกลับไปท่ายืนปกติด้วย
        }
    }
}