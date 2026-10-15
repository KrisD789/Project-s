using UnityEngine;

// สังเกตว่ามันสืบทอดจาก StateMachineBehaviour ไม่ใช่ MonoBehaviour นะครับ
public class TakedownEndBehaviour : StateMachineBehaviour
{
    // ฟังก์ชันนี้จะทำงานอัตโนมัติ ในเฟรมสุดท้ายที่แอนิเมชันของกล่องนี้เล่นจบ (ก่อนจะเปลี่ยนไปกล่องอื่น)
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // วิ่งหา Player_Action ที่อยู่บนตัวผู้เล่น แล้วสั่ง FinishTakedown()
        Player_Action playerAction = animator.GetComponentInParent<Player_Action>();

        if (playerAction != null)
        {
            playerAction.FinishTakedown();
        }
    }
}
