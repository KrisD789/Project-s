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
    // เพิ่มตัวนี้เข้าไปเพื่อกันผู้เล่นกดเมาส์รัวๆ ตอนแอนิเมชันกำลังเล่น
    private bool isExecutingFinish = false;

    private enemy_stage.EnemyState pendingEnemyState;

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
            //grabbedEnemy.transform.localRotation = Quaternion.identity;
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

            // เช็กองศาว่าควรทุ่มหน้า หรือ ล็อคหลัง! (กลับมาใช้โค้ดเดิมของคุณ)
            Vector3 dirToEnemy = (targetAliveEnemy.transform.position - transform.position).normalized;
            float playerFacingDot = Vector3.Dot(transform.forward, dirToEnemy);

            if (playerFacingDot > 0.5f) // ผู้เล่นมองไปที่ศัตรู
            {
                // เทียบหน้าผู้เล่นกับหน้าศัตรูแบบเดิม
                float enemyFacingDot = Vector3.Dot(transform.forward, targetAliveEnemy.transform.forward);

                if (enemyFacingDot > 0.5f)
                {
                    GrabEnemy(); // 🟢 หันหลังให้ = เรียกล็อคคอ
                }
                else if (enemyFacingDot < -0.5f)
                {
                    FrontalTakedown(); // 🔴 หันหน้าชนกัน = เรียกทุ่มด้านหน้า
                }
                else
                {
                    Debug.Log("เข้าผิดเหลี่ยม! ต้องเข้าหน้าตรง หรือหลังตรงเท่านั้น");
                }
            }
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

        // 1. บังคับให้ผู้เล่น "ลุกขึ้นยืน" ทันที เพื่อให้ AI ศัตรูมองเห็นตัวเราได้เต็มๆ
        Player.Instance.currentMovementState = Player.MovementState.Standing;
        isCrouching = false; // ซิงค์ตัวแปรภายในให้ตรงกันด้วย

        // 2. สั่งเล่นแอนิเมชันก้มเก็บศพ
        if (PlayerAnimator.Instance != null)
        {
            PlayerAnimator.Instance.PlayPickUpBody();
        }

        Enemy_Anime_Controller enemyAnim = NearbyBody.GetComponentInChildren<Enemy_Anime_Controller>(true);
        if (enemyAnim != null)
        {
            enemyAnim.SetCarriedState(true); // บอกศัตรูให้เล่นท่า "โดนแบก"
        }

        carriedBody = NearbyBody;
        NearbyBody = null;

        carriedBody.GetComponent<Rigidbody>().isKinematic = true;

        foreach (Collider col in carriedBody.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        carriedBody.transform.SetParent(carryPosition);
        carriedBody.transform.localPosition = Vector3.zero;

        Debug.Log("เก็บศพขึ้นบ่า บังคับลุกขึ้นยืนแล้ว!");
    }

    void DropBody()
    {
        Player.Instance.currentState = Player.PlayerState.Idle;

        // สั่งเล่นแอนิเมชันทิ้งศพ
        if (PlayerAnimator.Instance != null)
        {
            PlayerAnimator.Instance.PlayDropBody();
        }

        //  วิธีแก้ศพเด้งทะลุชั้น (เจาะจงแก้ปัญหา NavMeshAgent ดูดผิดชั้น)
        if (carriedBody.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
        {
            // 1. ปิด NavMeshAgent ชั่วคราวก่อนปล่อยศพ เพื่อไม่ให้มันเผลอดูดขึ้นเพดาน
            agent.enabled = false;

            // 2. ปล่อยศพจากบ่า
            carriedBody.transform.SetParent(null);

            // 3. ดึงระดับความสูง (แกน Y) ของผู้เล่น มาใช้กับศพ (รับประกันว่าอยู่ชั้นเดียวกัน 100%)
            Vector3 dropPosition = new Vector3(carriedBody.transform.position.x, transform.position.y, carriedBody.transform.position.z);
            carriedBody.transform.position = dropPosition;

            // 4. เปิด NavMesh กลับขึ้นมา มันจะเกาะพื้นชั้นที่เรายืนอยู่ทันที!
            agent.enabled = true;
        }
        else
        {
            // เผื่อไว้กรณีศัตรูตัวนั้นไม่มี NavMesh
            carriedBody.transform.SetParent(null);
        }

        carriedBody.GetComponent<Rigidbody>().isKinematic = true;

        // สแกนเปิด Collider อย่างระมัดระวัง 
        foreach (Collider col in carriedBody.GetComponentsInChildren<Collider>())
        {
            if (col.GetComponent<Enemy_Hitbox>() != null)
            {
                col.enabled = false;
            }
            else
            {
                col.enabled = true;
            }
        }

        Enemy_Anime_Controller enemyAnim = carriedBody.GetComponentInChildren<Enemy_Anime_Controller>(true);
        if (enemyAnim != null)
        {
            enemyAnim.SetCarriedState(false); // บอกศัตรูว่า "ปล่อยแล้ว กลับไปนอนซะ"
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
            // 1. เปลี่ยนสถานะและล็อกขาผู้เล่น (ยืนนิ่งเล่นท่าตะปบ)
            Player.Instance.currentState = Player.PlayerState.GrabbingEnemy;
            Player.Instance.movement.SetMovementLock(true);
            isExecutingFinish = false;

            grabbedEnemy = targetAliveEnemy;
            targetAliveEnemy = null;

            // 2. บอกศัตรูว่าโดนจับแล้วนะ
            if (grabbedEnemy.TryGetComponent<enemy_stage>(out enemy_stage Target_grabbedEnemy))
            {
                Target_grabbedEnemy.currentState = enemy_stage.EnemyState.OnGrab;
            }

            // 3. ดักจับเควส (ถ้ามี)
            if (grabbedEnemy.TryGetComponent<MissionTrigger>(out MissionTrigger missionTrigger))
            {
                if (missionTrigger.Mission_Data.type == MissionType.InteractObject && !missionTrigger.Mission_Data.isCompleted)
                {
                    missionTrigger.OnInteractionQuest();
                    Debug.Log("ล็อคคอเป้าหมาย! ภารกิจจับกุมสำเร็จทันที");
                }
            }

            // 4. ปิดสมอเรือ (AI เดิน) เพื่อไม่ให้มันดึงผู้เล่นไว้ตอนลาก
            if (grabbedEnemy.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
            {
                agent.enabled = false;
            }

            // 5. ปิดการชน (Collider) ทั้งหมดของศัตรู! (โค้ดดั้งเดิมที่หายไป)
            foreach (Collider col in grabbedEnemy.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            // 6. ปิด CharacterController เผื่อไว้ (กันเหนียว)
            if (grabbedEnemy.TryGetComponent<CharacterController>(out var charController))
            {
                charController.enabled = false;
            }

            // 7. ล็อกฟิสิกส์ให้อยู่นิ่งๆ
            grabbedEnemy.GetComponent<Rigidbody>().isKinematic = true;
            grabbedEnemy.GetComponent<Collider>().enabled = false;

            // 8. ดึงศัตรูมาไว้ในมือ (เปลี่ยน Parent) และจัดท่าให้ตรง
            grabbedEnemy.transform.SetParent(grabPosition);
            grabbedEnemy.transform.localPosition = Vector3.zero;
            grabbedEnemy.transform.localRotation = Quaternion.identity;

            // 9. สั่งเล่นแอนิเมชัน ทั้งผู้เล่นและศัตรู
            PlayerAnimator.Instance.PlayBehideTakedown();

            Enemy_Anime_Controller enemyAnim = grabbedEnemy.GetComponentInChildren<Enemy_Anime_Controller>();
            if (enemyAnim != null)
            {
                Debug.Log("เล่น Animtion ขแง enemy");
                enemyAnim.PlayBehindTakedown();
            }

            Debug.Log("ล็อคคอสำเร็จ รอคำสั่ง (Kill/Knockout)");
        }
    }

    void FrontalTakedown()
    {
        if (Player.Instance.currentState == Player.PlayerState.Idle)
        {
            Player.Instance.currentState = Player.PlayerState.GrabbingEnemy;
            Player.Instance.movement.SetMovementLock(true);
            isExecutingFinish = true;

            // 1. ดึงเป้าหมายมาใส่ตัวแปร (ส่วนที่หายไป)
            grabbedEnemy = targetAliveEnemy;
            targetAliveEnemy = null;

            if (grabbedEnemy.TryGetComponent<MissionTrigger>(out MissionTrigger missionTrigger))
            {
                if ((missionTrigger.Mission_Data.type == MissionType.InteractObject || missionTrigger.Mission_Data.type == MissionType.CaptureTarget)
                    && !missionTrigger.Mission_Data.isCompleted)
                {
                    missionTrigger.OnInteractionQuest();
                    Debug.Log("CQC เป้าหมายด้านหน้า! ภารกิจจับกุมสำเร็จทันที");
                }
            }

            if (grabbedEnemy.TryGetComponent<enemy_stage>(out enemy_stage Target_grabbedEnemy))
            {
                Target_grabbedEnemy.currentState = enemy_stage.EnemyState.OnGrab;
                if (grabbedEnemy.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                }
            }

            foreach (Collider col in grabbedEnemy.GetComponentsInChildren<Collider>())
                col.enabled = false;

            if (grabbedEnemy.TryGetComponent<CharacterController>(out var charController))
            {
                charController.enabled = false;
            }

            grabbedEnemy.GetComponent<Rigidbody>().isKinematic = true;
            grabbedEnemy.GetComponent<Collider>().enabled = false;

            // 2. จัดท่าให้อยู่ในมือ และหันหน้าชนกัน
            grabbedEnemy.transform.SetParent(grabPosition);
            grabbedEnemy.transform.localPosition = Vector3.zero;
            grabbedEnemy.transform.localRotation = Quaternion.Euler(0, 180, 0);

            // 3. สั่งเล่นแอนิเมชัน
            PlayerAnimator.Instance.PlayFrontTakedown();

            Enemy_Anime_Controller enemyAnim = grabbedEnemy.GetComponentInChildren<Enemy_Anime_Controller>();
            if (enemyAnim != null)
            {
                enemyAnim.PlayFrontTakedown();
            }

            pendingEnemyState = enemy_stage.EnemyState.faint;

            // 4. สั่งให้เรียกจบการทุ่มอัตโนมัติ (ปรับเวลา 2.5f ตามความยาวแอนิเมชันได้เลยครับ)
            Invoke("FinishTakedown", 2.5f);

            Debug.Log("CQC ด้านหน้า! บังคับศัตรูสลบทันที!");
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
        if (grabbedEnemy != null && !isExecutingFinish)
        {
            if (grabbedEnemy.CompareTag("ScamCommander"))
            {
                NotificationManager.Instance.ShowNotification("<color=red>คำเตือน</color> เป้าหมายสำคัญไม่อนุญาตให้ฆ่า");
                return;
            }

            isExecutingFinish = true;
            pendingEnemyState = enemy_stage.EnemyState.dead;

            //  ล็อกขาผู้เล่นทันทีที่กดปุ่ม เพื่อยืนนิ่งๆ เล่นท่าปาดคอ
            Player.Instance.movement.SetMovementLock(true);

            PlayerAnimator.Instance.PlayKill();

            Enemy_Anime_Controller enemyAnim = grabbedEnemy.GetComponentInChildren<Enemy_Anime_Controller>();
            if (enemyAnim != null)
            {
                Debug.Log("เล่น Animtion ขแง enemy");
                enemyAnim.PlayKill();
            }
        }
    }

    public void ChooseToKnockout()
    {
        if (grabbedEnemy != null && !isExecutingFinish)
        {
            isExecutingFinish = true;
            pendingEnemyState = enemy_stage.EnemyState.faint;

            //  ล็อกขาผู้เล่นทันทีที่กดปุ่ม เพื่อยืนนิ่งๆ เล่นท่ารัดคอ
            Player.Instance.movement.SetMovementLock(true);

            PlayerAnimator.Instance.PlayKnockOut();


            Enemy_Anime_Controller enemyAnim = grabbedEnemy.GetComponentInChildren<Enemy_Anime_Controller>();
            if (enemyAnim != null)
            {
                Debug.Log("เล่น Animtion ขแง enemy");
                enemyAnim.PlayKnockOut();
            }
        }
    }

    public void FinishTakedown()
    {
        if (grabbedEnemy == null) return;

        // เปลี่ยนสถานะศัตรูให้ลงไปนอน
        if (grabbedEnemy != null)
        {
            grabbedEnemy.GetComponent<enemy_stage>().ChangeState(pendingEnemyState);
        }

        //  ปลดล็อกทุกอย่าง คืนการควบคุมให้ผู้เล่นเดินได้ปกติ
        Player.Instance.currentState = Player.PlayerState.Idle;
        Player.Instance.movement.SetMovementLock(false); //  ปลดล็อกขากลับมาเดินปกติ
        isExecutingFinish = false; //  รีเซ็ตตัวแปรปุ่มกด

        grabbedEnemy.transform.SetParent(null);
        grabbedEnemy.GetComponent<Rigidbody>().isKinematic = true;

        foreach (Collider col in grabbedEnemy.GetComponentsInChildren<Collider>())
        {
            if (col.GetComponent<Enemy_Hitbox>() != null) col.enabled = false;
            else col.enabled = true;
        }

        NearbyBody = grabbedEnemy;
        grabbedEnemy = null;
    }

    public void HandleCrouch()
    {
        // 3. ดักไว้เลย! ถ้ากำลังแบกศพอยู่ ไม่อนุญาตให้นั่งยองเด็ดขาด
        if (Player.Instance.currentState == Player.PlayerState.CarryingBody)
        {
            Debug.Log("กำลังแบกศพอยู่ นั่งยองไม่ได้!");
            return; // เตะออกเลย ไม่ยอมให้เปลี่ยนสถานะ
        }

        isCrouching = !isCrouching;
        Debug.Log(isCrouching ? "ย่อตัวลง!" : "ลุกขึ้นยืน!");

        // ส่งค่าไปที่ MovementState แทน เพื่อไม่ให้ไปกวนกับการเล็ง/การกระทำอื่นๆ ของมือ
        if (isCrouching)
            Player.Instance.currentMovementState = Player.MovementState.Crouch;
        else
            Player.Instance.currentMovementState = Player.MovementState.Standing;
    }


    //  สร้างฟังก์ชันใหม่ เอาไว้ให้ StateMachineBehaviour เรียกตอนท่าจับด้านหลังเล่นจบ
    public void OnGrabAnimationFinished()
    {
        // ถ้าอยู่ในสถานะกำลังจับศัตรูอยู่ ให้ปลดล็อกขา
        if (Player.Instance.currentState == Player.PlayerState.GrabbingEnemy)
        {
            Player.Instance.movement.SetMovementLock(false); //  ปลดล็อกให้ลากศัตรูเดินได้
            Debug.Log("แอนิเมชันจับเสร็จแล้ว เริ่มลากศัตรูได้!");
        }
    }
}