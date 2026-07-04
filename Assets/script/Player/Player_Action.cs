using UnityEngine;



public class Player_Action : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,           // ว่างเปล่า (เดิน/วิ่ง/ยิงปืน ปกติ)
        CarryingBody,   // กำลังแบกศพ
        GrabbingEnemy,
        Crouch          // กำลังล็อคคอศัตรู
    }

    [Header("สถานะปัจจุบันของผู้เล่น")]
    public PlayerState currentState = PlayerState.Idle;

    private GameObject currentInteractableTarget;

    [Header("จุดที่จะเอาศพไปวางบนบ่า")]
    public Transform carryPosition; // ลาก Empty Object ที่อยู่ตรงหลัง/บ่าผู้เล่นมาใส่
    private GameObject carriedBody = null; // จำไว้ว่ากำลังแบกใครอยู่ไหม
    private GameObject NearbyBody = null;

    [Header("ระบบล็อคคอ (Takedown)")]
    public Transform grabPosition; // ลาก Empty Object ที่อยู่ "ด้านหน้า" ผู้เล่นมาใส่ (สำหรับเป็นจุดล็อคคอ)
    private GameObject grabbedEnemy = null; // จำไว้ว่ากำลังล็อคคอใครอยู่
    private GameObject targetAliveEnemy = null; // เรดาร์จับศัตรูที่เป็นๆ

    [Header("Crouch Settings")]
    public float standingHeight;
    public float crouchingHeight = 1.0f;
    public float crouchSpeed = 10f; // ความเร็วในการสมูทตอนย่อ/ลุก
    private bool isCrouching = false;
    private float bottomOffset;

    public bool OnDark = false;

    private CapsuleCollider capsuleCollider;

    private void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();

        standingHeight = capsuleCollider.height;

        // คำนวณหา "จุดต่ำสุดของเท้า" จากจุดศูนย์กลางเดิมของ Collider ตอนที่ยังยืนอยู่
        bottomOffset = capsuleCollider.center.y - (capsuleCollider.height / 2f);
    }
    private void Update()
    {
        // ล็อคตำแหน่งศัตรูตอนจับให้อยู่หมัด ห้ามเคลื่อนเด็ดขาด!
        if (currentState == PlayerState.GrabbingEnemy && grabbedEnemy != null)
        {
            // บังคับให้อยู่ตรงกลาง grabPosition เสมอ
            grabbedEnemy.transform.localPosition = Vector3.zero;

            // บังคับให้หันหน้าไปทิศทางเดียวกับผู้เล่นเสมอ
            grabbedEnemy.transform.localRotation = Quaternion.identity;
        }

        if (currentState == PlayerState.CarryingBody && carriedBody != null)
        {
            carriedBody.transform.localPosition = Vector3.zero;
            carriedBody.transform.localRotation = Quaternion.identity;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        // ถ้าของชิ้นนั้นอยู่ใน Layer ที่กำหนด
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            currentInteractableTarget = other.gameObject;
            Debug.Log("มีของให้กดอยู่ใกล้ๆ: " + currentInteractableTarget.name);
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("enemy"))
        {
            if (other.TryGetComponent<enemy_stage>(out enemy_stage target_body))
            {
                if (target_body != null)
                {
                    if (target_body.currentState == enemy_stage.EnemyState.dead || target_body.currentState == enemy_stage.EnemyState.faint)
                    {
                        NearbyBody = target_body.gameObject;
                        Debug.Log("มีศพEnemyอยู่หน้าคุณ: " + NearbyBody.name);
                    }

                    else
                    {
                        targetAliveEnemy = target_body.gameObject;
                        Debug.Log("มีEnemyอยู่หน้าคุณ: " + targetAliveEnemy.name);
                    }

                }
            }
        }
    }

    // เมื่อเราเดินออกมาจากของชิ้นนั้น
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == currentInteractableTarget)
        {
            currentInteractableTarget = null; // ลบความจำทิ้ง เพราะเดินออกมาแล้ว
        }

        if(other.gameObject == NearbyBody)
        {
            NearbyBody = null;
        }
    }


    public void Interaction()
    {
        // 1. ถ้าแบกศพอยู่ ปุ่ม E จะมีไว้เพื่อ "วางศพ" เท่านั้น (ทำอย่างอื่นไม่ได้จนกว่าจะวาง)
        if (carriedBody != null)
        {
            DropBody();
            return; // วางเสร็จ กระโดดออกจากฟังก์ชันเลย!
        }

        if (targetAliveEnemy != null)
        {
            GrabEnemy();
            return;
        }

        // 2. ถ้าไม่ได้แบกศพ และมีศพอยู่ใกล้ๆ ให้ "เก็บศพ"
        if (carriedBody == null && NearbyBody != null)
        {
            PickUpBody();
            return; // เก็บเสร็จ กระโดดออกจากฟังก์ชันเลย!
        }

        // 3. ถ้ามือว่าง และไม่มีศพ ค่อยมาเช็คการกดสวิตช์หรือเปิดประตู
        if (currentInteractableTarget != null)
        {
            if (currentInteractableTarget.TryGetComponent<light_switch>(out light_switch target_light_Switch))
            {
                target_light_Switch.Turn();
                return;
            }

            if (currentInteractableTarget.TryGetComponent<Door>(out Door DoorTarget))
            {
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
        currentState = PlayerState.CarryingBody;

        Debug.Log("แบกศพขึ้นบ่า!");
        carriedBody = NearbyBody;
        NearbyBody = null; // พอหยิบขึ้นบ่าแล้ว ศพก็ไม่ได้อยู่บนพื้นอีกต่อไป

        // 1. ปิดฟิสิกส์ศพชั่วคราว จะได้ไม่ชนกับตัวเรา
        carriedBody.GetComponent<Rigidbody>().isKinematic = true;
        carriedBody.GetComponent<Collider>().enabled = false;

        // 2. ย้ายศพมาเกาะที่บ่าผู้เล่น
        carriedBody.transform.SetParent(carryPosition);
        carriedBody.transform.localPosition = Vector3.zero; // จัดให้อยู่ตรงกลางจุดแบกเป๊ะๆ

        // TODO: ลดความเร็วการเดินของผู้เล่นลงครึ่งนึง (หนัก)
    }

    void DropBody()
    {
        currentState = PlayerState.Idle;
        Debug.Log("วางศพลงพื้น");

        // 1. ปลดออกจากบ่า
        carriedBody.transform.SetParent(null);

        // 2. เปิดฟิสิกส์ให้ศพหล่นตุบลงพื้น
        carriedBody.GetComponent<Rigidbody>().isKinematic = false;
        carriedBody.GetComponent<Collider>().enabled = true;

        carriedBody = null; // ล้างความจำว่ามือว่างแล้ว
    }

    void GrabEnemy()
    {
        currentState = PlayerState.GrabbingEnemy;

        // เช็คว่าอยู่ด้านหลังไหม (Dot Product > 0.5)
        float angleCheck = Vector3.Dot(transform.forward, targetAliveEnemy.transform.forward);

        if (angleCheck > 0.5f)
        {
            Debug.Log("ล็อคคอสำเร็จ! เลือกว่าจะ ฆ่า(คลิกซ้าย) หรือ สลบ(คลิกขวา)");

            grabbedEnemy = targetAliveEnemy;
            targetAliveEnemy = null; // เอาออกจากเรดาร์คนเป็น

            if (grabbedEnemy.TryGetComponent<enemy_stage>(out enemy_stage Target_grabbedEnemy))
            {
                Target_grabbedEnemy.currentState = enemy_stage.EnemyState.OnGrab;
            }

            // 1. ปิดฟิสิกส์ศัตรู (มันจะได้ขัดขืนไม่ได้)
            grabbedEnemy.GetComponent<Rigidbody>().isKinematic = true;
            grabbedEnemy.GetComponent<Collider>().enabled = false;

            // 2. ดึงศัตรูมาแปะไว้ที่ด้านหน้าผู้เล่น (grabPosition)
            grabbedEnemy.transform.SetParent(grabPosition);
            grabbedEnemy.transform.localPosition = Vector3.zero;

            // สั่งให้มันหันหน้าไปทางเดียวกับเรา
            grabbedEnemy.transform.localRotation = Quaternion.identity;

            // TODO: ลดความเร็วเดิน (เหมือนตอนแบกศพ)
        }
        else
        {
            Debug.Log(" ล็อคคอไม่ได้! คุณต้องอยู่ข้างหลังมัน");
        }
    }

    // ฟังก์ชันนี้เอาไปผูกกับปุ่ม "คลิกซ้าย" (หรือปุ่มอะไรก็ได้ใน Input System)
    public void ChooseToKill()
    {
        if (grabbedEnemy != null)
        {
            Debug.Log("ตัดสินใจ: เชือดคอทิ้ง!");
            grabbedEnemy.GetComponent<enemy_stage>().currentState = enemy_stage.EnemyState.dead;
            FinishTakedown();
        }
    }

    // ฟังก์ชันนี้เอาไปผูกกับปุ่ม "คลิกขวา" (หรือปุ่มอะไรก็ได้ใน Input System)
    public void ChooseToKnockout()
    {
        if (grabbedEnemy != null)
        {
            Debug.Log("ตัดสินใจ: รัดคอจนสลบ!");
            grabbedEnemy.GetComponent<enemy_stage>().currentState = enemy_stage.EnemyState.faint;
            FinishTakedown();
        }
    }

    // เคลียร์ศัตรูออกจากมือ แล้วปล่อยลงพื้นกลายเป็นศพ
    void FinishTakedown()
    {
        currentState = PlayerState.Idle;

        // ปลดออกจากตัวผู้เล่น
        grabbedEnemy.transform.SetParent(null);

        // เปิดฟิสิกส์ให้หล่นลงพื้น
        grabbedEnemy.GetComponent<Rigidbody>().isKinematic = false;
        grabbedEnemy.GetComponent<Collider>().enabled = true;

        // โยนศัตรูตัวนี้เข้าไปในเรดาร์ศพ (NearbyBody) เพื่อให้ผู้เล่นกด E แบกศพต่อได้ทันที!
        NearbyBody = grabbedEnemy;
        grabbedEnemy = null;

        // TODO: สั่งคืนความเร็วเดินให้ผู้เล่น
    }

    public void HandleCrouch()
    {
        isCrouching = !isCrouching;
        Debug.Log(isCrouching ? "ย่อตัวลง!" : "ลุกขึ้นยืน!");

        if (isCrouching) currentState = PlayerState.Crouch;
        else currentState = PlayerState.Idle;

        // 1. กำหนดความสูงเป้าหมาย
        //float targetHeight = isCrouching ? crouchingHeight : standingHeight;

        // 2. ใช้ Mathf.Lerp เพื่อให้การย่อ/ลุกดูสมูท ไม่กระตุกแข็งๆ
        //capsuleCollider.height = Mathf.Lerp(capsuleCollider.height, targetHeight, Time.deltaTime * crouchSpeed);

        //float newCenterY = bottomOffset + (capsuleCollider.height / 2f);
        //capsuleCollider.center = new Vector3(0, newCenterY, 0);
    }
}
