using Unity.VisualScripting;
using UnityEngine;

public class Player_Action : MonoBehaviour
{
    private GameObject currentInteractableTarget;
    private GameObject CurrentKey_Item;

    [Header("จุดที่จะเอาศพไปวางบนบ่า")]
    public Transform carryPosition;
    private GameObject carriedBody = null;
    private GameObject NearbyBody = null;

    [Header("ระบบล็อคคอ (Takedown)")]
    public Transform grabPosition;
    private GameObject grabbedEnemy = null;
    private GameObject targetAliveEnemy = null;

    [Header("Crouch Settings")]
    public float standingHeight;
    public float crouchingHeight = 1.0f;
    public float crouchSpeed = 10f;
    private bool isCrouching = false;
    private float bottomOffset;

    private CapsuleCollider capsuleCollider;

    [Header("ระบบภารกิจ")]
    private MissionTrigger activeQuestTrigger = null;

    [Header("Interaction Settings")]
    public float interactionRadius = 1.5f; // รัศมีวงกลมกำลังพอดี
    public float forwardOffset = 1.0f;     // ระยะยื่นแขนไปข้างหน้า
    public LayerMask interactableLayers;   // อย่าลืมไปตั้งค่า Layer ใน Inspector (ติ๊ก enemy, Interactable)

    private void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        standingHeight = capsuleCollider.height;
        bottomOffset = capsuleCollider.center.y - (capsuleCollider.height / 2f);
    }

    private void Update()
    {
        // ยังใช้ currentState แบบเดิมได้เลย ไม่กระทบระบบอื่น
        if (Player.Instance.currentState == Player.PlayerState.GrabbingEnemy && grabbedEnemy != null)
        {
            grabbedEnemy.transform.localPosition = Vector3.zero;
            grabbedEnemy.transform.localRotation = Quaternion.identity;
        }

        if (Player.Instance.currentState == Player.PlayerState.CarryingBody && carriedBody != null)
        {
            carriedBody.transform.localPosition = Vector3.zero;
            carriedBody.transform.localRotation = Quaternion.identity;
        }

        if (activeQuestTrigger != null && activeQuestTrigger.OnInteract)
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                activeQuestTrigger.cancel_HackQuest();
                activeQuestTrigger = null;
                Debug.Log("ขยับตัว! ยกเลิกการแฮ็กอัตโนมัติ");
            }
        }
    }

    public void Interaction()
    {
        if (Player.Instance.currentState == Player.PlayerState.Aim) return;
        if (activeQuestTrigger != null && activeQuestTrigger.OnInteract)
        {
            activeQuestTrigger.cancel_HackQuest();
            activeQuestTrigger = null;
            return;
        }

        if (carriedBody != null) { DropBody(); return; }

        // 2. เทคนิคยื่นแขน: เลื่อนจุดศูนย์กลางวงกลมไปข้างหน้าผู้เล่น
        Vector3 interactCenter = transform.position + (transform.forward * forwardOffset);

        Collider[] hits = Physics.OverlapSphere(interactCenter, interactionRadius, interactableLayers);

        GameObject bestAliveEnemy = null;
        GameObject bestBody = null;
        GameObject bestInteractable = null;

        float closestAliveDist = Mathf.Infinity;
        float closestBodyDist = Mathf.Infinity;
        float closestIntDist = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);

            if (hit.gameObject.layer == LayerMask.NameToLayer("enemy"))
            {
                if (hit.TryGetComponent<enemy_stage>(out enemy_stage stage))
                {
                    if (stage.currentState == enemy_stage.EnemyState.dead || stage.currentState == enemy_stage.EnemyState.faint)
                    {
                        if (dist < closestBodyDist) { closestBodyDist = dist; bestBody = hit.gameObject; }
                    }
                    else
                    {
                        if (dist < closestAliveDist) { closestAliveDist = dist; bestAliveEnemy = hit.gameObject; }
                    }
                }
            }
            else if (hit.gameObject.layer == LayerMask.NameToLayer("Interactable"))
            {
                if (dist < closestIntDist) { closestIntDist = dist; bestInteractable = hit.gameObject; }
            }
        }

        // 3. เรียงลำดับความสำคัญ สิ่งไหนควรทำงานก่อน
        if (bestAliveEnemy != null)
        {
            targetAliveEnemy = bestAliveEnemy;
            GrabEnemy();
            return;
        }

        if (bestBody != null)
        {
            NearbyBody = bestBody;
            PickUpBody();
            return;
        }

        if (bestInteractable != null)
        {
            currentInteractableTarget = bestInteractable;
            ProcessInteractable(bestInteractable);
            return;
        }
    }

    void ProcessInteractable(GameObject target)
    {
        if (target.TryGetComponent<MissionTrigger>(out MissionTrigger missionTrigger))
        {
            if (missionTrigger.Mission_Data.type == MissionType.Hack && !missionTrigger.Mission_Data.isCompleted)
            {
                missionTrigger.startHackQuest();
                activeQuestTrigger = missionTrigger;
            }
            else if ((missionTrigger.Mission_Data.type == MissionType.InteractObject || missionTrigger.Mission_Data.type == MissionType.CaptureTarget)
                && !missionTrigger.Mission_Data.isCompleted)
            {
                missionTrigger.OnInteractionQuest();
            }
        }
        else if (target.TryGetComponent<light_switch>(out light_switch target_light_Switch))
        {
            target_light_Switch.Turn();
        }
        else if (target.TryGetComponent<Door>(out Door DoorTarget))
        {
            if (DoorTarget.currentState == Door.DoorState.Closed)
                DoorTarget.ToggleDoor(false, Door.DoorState.Open);
            else
                DoorTarget.ToggleDoor(false, Door.DoorState.Closed);
        }
    }


    void PickUpBody()
    {
        Player.Instance.currentState = Player.PlayerState.CarryingBody;
        carriedBody = NearbyBody;
        NearbyBody = null;

        carriedBody.GetComponent<Rigidbody>().isKinematic = true;

        foreach (Collider col in carriedBody.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        carriedBody.transform.SetParent(carryPosition);
        carriedBody.transform.localPosition = Vector3.zero;
    }

    void DropBody()
    {
        Player.Instance.currentState = Player.PlayerState.Idle;
        carriedBody.transform.SetParent(null);
        carriedBody.GetComponent<Rigidbody>().isKinematic = true;

        foreach (Collider col in carriedBody.GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }

        carriedBody = null;
    }


    public GameObject GetGrabbedEnemy()
    {
        return grabbedEnemy;
    }

    void GrabEnemy()
    {
        if (Player.Instance.currentState == Player.PlayerState.Idle)
        {
            Vector3 dirToEnemy = (targetAliveEnemy.transform.position - transform.position).normalized;
            float playerFacingDot = Vector3.Dot(transform.forward, dirToEnemy);

            if (playerFacingDot > 0.5f) 
            {
                float enemyFacingDot = Vector3.Dot(transform.forward, targetAliveEnemy.transform.forward);

                bool isFrontal = enemyFacingDot < -0.5f; 
                bool isStealth = enemyFacingDot > 0.5f;  

                if (isStealth || isFrontal)
                {
                    Player.Instance.currentState = Player.PlayerState.GrabbingEnemy;
                    grabbedEnemy = targetAliveEnemy;
                    targetAliveEnemy = null;

                    // 1. หยุดศัตรู และเคลียร์เควส (เหมือนเดิม)
                    if (grabbedEnemy.TryGetComponent<enemy_stage>(out enemy_stage Target_grabbedEnemy))
                    {
                        Target_grabbedEnemy.currentState = enemy_stage.EnemyState.OnGrab;
                        
                        if(grabbedEnemy.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
                        {
                            agent.isStopped = true;
                            agent.velocity = Vector3.zero;
                        }
                    }

                    if (grabbedEnemy.TryGetComponent<MissionTrigger>(out MissionTrigger missionTrigger))
                    {
                        if (missionTrigger.Mission_Data.type == MissionType.InteractObject && !missionTrigger.Mission_Data.isCompleted)
                        {
                            missionTrigger.OnInteractionQuest();
                            Debug.Log("ล็อคคอเป้าหมาย! ภารกิจจับกุมสำเร็จทันที");
                        }
                    }

                    // 2. จัดตำแหน่งศัตรู
                    foreach (Collider col in grabbedEnemy.GetComponentsInChildren<Collider>())
                    {
                        col.enabled = false;
                    }
                    grabbedEnemy.GetComponent<Rigidbody>().isKinematic = true;
                    grabbedEnemy.GetComponent<Collider>().enabled = false;
                    
                    grabbedEnemy.transform.SetParent(grabPosition);
                    grabbedEnemy.transform.localPosition = Vector3.zero;
                    grabbedEnemy.transform.localRotation = isFrontal ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;

                    StartCoroutine(TestTakedownDelay());

                    return;
                    // 🚨 3. สั่งเล่นแอนิเมชันผ่านตัวกลาง 🚨
                    // สมมติว่ามีตัวแปร PlayerAnimator อยู่ในคลาสนี้ หรือเรียกผ่าน GetComponent ก็ได้
                    PlayerAnimator playerAnim = GetComponent<PlayerAnimator>();
                    if(playerAnim != null)
                    {
                        if(isFrontal) playerAnim.PlayFrontTakedown();
                        else playerAnim.PlayStealthTakedown();
                    }
                }
            }
        }
    }

    // ฟังก์ชันจำลองสำหรับเทสต์
    private System.Collections.IEnumerator TestTakedownDelay()
    {
        Debug.Log("เริ่มจำลองเวลาแอนิเมชัน 2 วินาที...");
        yield return new WaitForSeconds(2.0f);

        if (Player.Instance.currentState == Player.PlayerState.GrabbingEnemy)
        {
            Debug.Log("แอนิเมชันจำลองจบแล้ว! รอรับคำสั่ง ChooseToKill หรือ ChooseToKnockout");
            // ถ้ามีระบบโชว์ UI ให้ผู้เล่นกดเลือก ก็สามารถแทรกคำสั่งเปิด UI ตรงนี้ได้เลยครับ
        }
    }

    public void CancelTakedown(GameObject enemyToRelease)
    {
        if (enemyToRelease == null) return;

        // คืนสถานะผู้เล่น
        if (Player.Instance.currentState == Player.PlayerState.GrabbingEnemy)
        {
            Player.Instance.currentState = Player.PlayerState.Idle;
        }

        // ปล่อยมือจากศัตรู
        enemyToRelease.transform.SetParent(null);
        enemyToRelease.GetComponent<Rigidbody>().isKinematic = false;

        foreach (Collider col in enemyToRelease.GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }

        if (grabbedEnemy == enemyToRelease)
        {
            grabbedEnemy = null;
        }
    }

    public void ChooseToKill()
    {
        if (grabbedEnemy != null)
        {
            if (grabbedEnemy.CompareTag("ScamCommander"))
            {
                Debug.Log("เป้าหมายสำคัญ (ScamCommander)! ระบบไม่อนุญาตให้ฆ่า บังคับทำให้สลบแทน");
                NotificationManager.Instance.ShowNotification("<color=red>คำเตือน</color> เป้าหมายสำคัญ (ScamCommander)ไม่อนุญาตให้ฆ่า บังคับทำให้สลบแทน ");
                return;
            }
            else
            {
                grabbedEnemy.GetComponent<enemy_stage>().ChangeState(enemy_stage.EnemyState.dead);
            }

            FinishTakedown();
        }
    }

    public void ChooseToKnockout()
    {
        if (grabbedEnemy != null)
        {
            grabbedEnemy.GetComponent<enemy_stage>().ChangeState(enemy_stage.EnemyState.faint);
            FinishTakedown();
        }
    }

    void FinishTakedown()
    {
        Player.Instance.currentState = Player.PlayerState.Idle;
        grabbedEnemy.transform.SetParent(null);
        grabbedEnemy.GetComponent<Rigidbody>().isKinematic = true;

        foreach (Collider col in grabbedEnemy.GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }

        NearbyBody = grabbedEnemy;
        grabbedEnemy = null;
    }

    public void HandleCrouch()
    {
        isCrouching = !isCrouching;
        Debug.Log(isCrouching ? "ย่อตัวลง!" : "ลุกขึ้นยืน!");

        //  ส่งค่าไปที่ MovementState แทน เพื่อไม่ให้ไปกวนกับการเล็ง/การกระทำอื่นๆ ของมือ
        if (isCrouching)
            Player.Instance.currentMovementState = Player.MovementState.Crouch;
        else
            Player.Instance.currentMovementState = Player.MovementState.Standing;
    }
}