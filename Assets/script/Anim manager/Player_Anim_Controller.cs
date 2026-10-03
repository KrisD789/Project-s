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
            anim.SetFloat("Speed", movement.GetCurrentVelocity());
        }

        if (Player.Instance != null)
        {
            anim.SetBool("isCrouching", Player.Instance.currentState == Player.PlayerState.Crouch);
            anim.SetBool("isCarrying", Player.Instance.currentState == Player.PlayerState.CarryingBody);
        }
    }

    public void PlayGrabEnemy() { if (anim != null) anim.SetTrigger("GrabEnemy"); }
    public void PlayKillEnemy() { if (anim != null) anim.SetTrigger("KillEnemy"); }
    public void PlayKnockoutEnemy() { if (anim != null) anim.SetTrigger("KnockoutEnemy"); }

    // ผูกคำสั่งนี้กับ Animation Event ในหน้าต่าง Animation คลิปเชือดคอ/รัดคอ
    public void TriggerTakedownFinish()
    {
        if (playerAction != null)
        {
           //playerAction.OnTakedownAnimationFinished();
        }
    }
}