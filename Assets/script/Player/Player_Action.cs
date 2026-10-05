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

    //[Header("Player referent")]
    //private Player player_script;

    private void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        standingHeight = capsuleCollider.height;
        bottomOffset = capsuleCollider.center.y - (capsuleCollider.height / 2f);
        //player_script = Player.Instance;
    }

    private void Update()
    {
        // เรียกใช้ State จาก Player.Instance แทน
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
            // เช็คว่าผู้เล่นมีการกดปุ่มขยับตัว (WASD / ลูกศร) หรือไม่
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                activeQuestTrigger.cancel_HackQuest(); // สั่งยกเลิกเควสต์
                activeQuestTrigger = null;             // คืนค่าให้มือว่าง
                Debug.Log("ขยับตัว! ยกเลิกการแฮ็กอัตโนมัติ");
            }
        }

        //print(currentInteractableTarget.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            currentInteractableTarget = other.gameObject;
        }

        //if (other.gameObject.layer == LayerMask.NameToLayer("KeyItem"))
        //{
        //CurrentKey_Item = other.gameObject;
        //}


        if (other.gameObject.layer == LayerMask.NameToLayer("enemy"))
        {
            if (other.TryGetComponent<enemy_stage>(out enemy_stage target_body))
            {
                if (target_body != null)
                {
                    if (target_body.currentState == enemy_stage.EnemyState.dead || target_body.currentState == enemy_stage.EnemyState.faint)
                    {
                        NearbyBody = target_body.gameObject;
                    }
                    else
                    {
                        targetAliveEnemy = target_body.gameObject;
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == currentInteractableTarget) currentInteractableTarget = null;
        if (other.gameObject == NearbyBody) NearbyBody = null;
        if (other.gameObject == targetAliveEnemy) targetAliveEnemy = null;
    }

    public void Interaction()
    {
        if (Player.Instance.currentState == Player.PlayerState.Aim)
        {
            return;
        }

        // --- ส่วนที่เพิ่มใหม่ (1): ดักเช็คการยกเลิกเควส ---
        // ถ้าระบบจำได้ว่ามีเควสที่กำลังกดทำอยู่ และเควสนั้นมีสถานะ OnInteract เป็น true
        if (activeQuestTrigger != null && activeQuestTrigger.OnInteract)
        {
            activeQuestTrigger.cancel_HackQuest(); // เรียกใช้ฟังก์ชันยกเลิก
            activeQuestTrigger = null; // ล้างค่าในมือทิ้ง
            Debug.Log("ยกเลิกการทำเควสกลางคัน!");
            return; // จบการทำงาน ไม่ต้องไปเช็คอย่างอื่นต่อ
        }
        // ------------------------------------------

        // เพิ่มบล็อกนี้: ดักเช็กก่อนว่าคนที่อยู่ตรงหน้าตายหรือยัง ถ้าตายแล้วให้ย้ายไปเป็นศพ (NearbyBody)
        if (targetAliveEnemy != null)
        {
            if (targetAliveEnemy.TryGetComponent<enemy_stage>(out enemy_stage stage))
            {
                if (stage.currentState == enemy_stage.EnemyState.dead || stage.currentState == enemy_stage.EnemyState.faint)
                {
                    NearbyBody = targetAliveEnemy;
                    targetAliveEnemy = null; // ลบออกจากหมวดคนเป็น
                }
            }
        }

        // โค้ดเก็บกุญแจเดิม
        //if (CurrentKey_Item != null)
        //{
        //if (CurrentKey_Item.TryGetComponent<PickUp_Item>(out PickUp_Item item))
        //{
        //item.PickUp();
        //CurrentKey_Item = null;
        //}
        //}

        if (carriedBody != null) { DropBody(); return; }
        if (targetAliveEnemy != null) { GrabEnemy(); return; }
        if (carriedBody == null && NearbyBody != null) { PickUpBody(); return; }

        // ส่วนของการตรวจจับสิ่งที่ Interact ได้
        if (currentInteractableTarget != null)
        {
            print("currentInteractableTarget != null");
            // --- ส่วนที่เพิ่มใหม่ (2): ตรวจจับ Mission Trigger ---
            if (currentInteractableTarget.TryGetComponent<MissionTrigger>(out MissionTrigger missionTrigger))
            {
                print("if (currentInteractableTarget.TryGetComponent<MissionTrigger>(out MissionTrigger missionTrigger))");
                // ถ้าเป็นเควสแบบกดค้าง (InteractObject) และยังไม่ผ่าน
                if (missionTrigger.Mission_Data.type == MissionType.Hack && !missionTrigger.Mission_Data.isCompleted)
                {
                    missionTrigger.startHackQuest(); // เรียกให้เวลาเริ่มเดิน
                    activeQuestTrigger = missionTrigger; // จดจำเครื่องนี้เอาไว้ เพื่อรอกดยกเลิก
                    Debug.Log("เริ่มแฮ็กระบบ!");
                    return;
                }

            }
            // ----------------------------------------------

            // โค้ดสวิตช์ไฟและประตูเดิม
            if (currentInteractableTarget.TryGetComponent<light_switch>(out light_switch target_light_Switch))
            {
                target_light_Switch.Turn();
                return;
            }

            if (currentInteractableTarget.TryGetComponent<Door>(out Door DoorTarget))
            {
                print("แตะประตู++");
                if (DoorTarget.currentState == Door.DoorState.Closed)
                    DoorTarget.ToggleDoor(false, Door.DoorState.Open);
                else
                    DoorTarget.ToggleDoor(false, Door.DoorState.Closed);
                return;
            }
        }
    }

    void PickUpBody()
    {
        Player.Instance.currentState = Player.PlayerState.CarryingBody;
        carriedBody = NearbyBody;
        NearbyBody = null;

        carriedBody.GetComponent<Rigidbody>().isKinematic = true;

        // ปิด Collider ทั้งตัวแม่และลูก (Hitbox) ทุกชิ้น!
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
        carriedBody.GetComponent<Rigidbody>().isKinematic = true; // ล็อกฟิสิกส์ไว้ ศพจะได้ไม่ไถล

        // เปิด Collider ทุกชิ้นคืนกลับมา (ศพบนพื้นจะได้โดนยิงซ้ำได้ หรือกดหยิบซ้ำได้)
        foreach (Collider col in carriedBody.GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }

        carriedBody = null;
    }

    void GrabEnemy()
    {
        if (Player.Instance.currentState == Player.PlayerState.Idle ||
            Player.Instance.currentState == Player.PlayerState.Crouch)
        {

            float angleCheck = Vector3.Dot(transform.forward, targetAliveEnemy.transform.forward);

            if (angleCheck > 0.5f)
            {
                Player.Instance.currentState = Player.PlayerState.GrabbingEnemy;
                grabbedEnemy = targetAliveEnemy;
                targetAliveEnemy = null;

                if (grabbedEnemy.TryGetComponent<enemy_stage>(out enemy_stage Target_grabbedEnemy))
                {
                    Target_grabbedEnemy.currentState = enemy_stage.EnemyState.OnGrab;
                }

                foreach (Collider col in grabbedEnemy.GetComponentsInChildren<Collider>())
                {
                    col.enabled = false;
                }

                grabbedEnemy.GetComponent<Rigidbody>().isKinematic = true;
                grabbedEnemy.GetComponent<Collider>().enabled = false;
                grabbedEnemy.transform.SetParent(grabPosition);
                grabbedEnemy.transform.localPosition = Vector3.zero;
                grabbedEnemy.transform.localRotation = Quaternion.identity;
            }
            else
            {
                Debug.Log("ล็อคคอไม่ได้! คุณต้องอยู่ข้างหลังมัน");
            }
        }
        else
        {
            Debug.Log("ผู้เล่นไม่ได้อยู่ใน __ state __ Idle or Crouch");
        }
    }

    public void ChooseToKill()
    {
        if (grabbedEnemy != null)
        {
            // Safety: เช็กแท็กของศัตรูที่กำลังถูกล็อกคอ
            if (grabbedEnemy.CompareTag("ScamCommander"))
            {
                Debug.Log("เป้าหมายสำคัญ (ScamCommander)! ระบบไม่อนุญาตให้ฆ่า บังคับทำให้สลบแทน");

                // บังคับเปลี่ยนสเตตัสเป็นสลบ (faint) แทนการตาย
                return;
            }
            else
            {
                // ถ้าไม่ใช่บอส เป็นศัตรูทั่วไป ก็เชือดได้ตามปกติ
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

        // เปิด Collider ทุกชิ้นคืนกลับมาตอนปล่อยศพลงพื้น
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

        // อัปเดต State ไปที่ Player.Instance
        if (isCrouching) Player.Instance.currentState = Player.PlayerState.Crouch;
        else Player.Instance.currentState = Player.PlayerState.Idle;
    }
}
