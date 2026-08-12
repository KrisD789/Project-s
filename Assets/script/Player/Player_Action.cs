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
        //if (other.gameObject == CurrentKey_Item) CurrentKey_Item = null;
    }

    public void Interaction()
    {
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
        carriedBody.GetComponent<Collider>().enabled = false;
        carriedBody.transform.SetParent(carryPosition);
        carriedBody.transform.localPosition = Vector3.zero;
    }

    void DropBody()
    {
        Player.Instance.currentState = Player.PlayerState.Idle;
        carriedBody.transform.SetParent(null);
        carriedBody.GetComponent<Rigidbody>().isKinematic = false;
        carriedBody.GetComponent<Collider>().enabled = true;
        carriedBody = null;
    }

    void GrabEnemy()
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

    public void ChooseToKill()
    {
        if (grabbedEnemy != null)
        {
            grabbedEnemy.GetComponent<enemy_stage>().currentState = enemy_stage.EnemyState.dead;
            FinishTakedown();
        }
    }

    public void ChooseToKnockout()
    {
        if (grabbedEnemy != null)
        {
            grabbedEnemy.GetComponent<enemy_stage>().currentState = enemy_stage.EnemyState.faint;
            FinishTakedown();
        }
    }

    void FinishTakedown()
    {
        Player.Instance.currentState = Player.PlayerState.Idle;
        grabbedEnemy.transform.SetParent(null);
        grabbedEnemy.GetComponent<Rigidbody>().isKinematic = false;
        grabbedEnemy.GetComponent<Collider>().enabled = true;
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