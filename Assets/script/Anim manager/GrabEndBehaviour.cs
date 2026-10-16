using UnityEngine;

public class GrabEndBehaviour : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //  เปลี่ยนมาใช้ Player.Instance ตรงๆ เลยครับ รับประกันว่าหาเจอแน่นอน
        if (Player.Instance != null && Player.Instance.action != null)
        {
            Player.Instance.action.OnGrabAnimationFinished();
        }
    }
}