using UnityEngine;
using UnityEngine.InputSystem;

public class Player_moveMent : MonoBehaviour
{
    public float speed = 1f;
    public int playerState = 0;

    public SphereCollider noiseCollider;
    public float minNoiseRadius = 0.5f;
    public float maxNoiseRadius = 10f;

    [Header("Rotation Settings")]
    public float turnSpeed = 15f;

    

    Rigidbody rb;
    Transform camTransform;
    public LayerMask InteracMask;

    // 3. ระบบใหม่จำเป็นต้องเปิด (Enable) และปิด (Disable) การรับค่าเสมอ
   
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        //HandleScrollInput();
        setSpeedPlayer();
        setCollider();
        //Interaction();
    }

    public void MoveAndRotate(Vector2 moveInput, bool isAiming)
    {
        float H = moveInput.x;
        float V = moveInput.y;

        Vector3 moveDir = Vector3.zero;

        if (camTransform != null)
        {
            Vector3 camForward = camTransform.forward;
            Vector3 camRight = camTransform.right;

            camForward.y = 0;
            camRight.y = 0;

            camForward.Normalize();
            camRight.Normalize();

            moveDir = (camForward * V + camRight * H).normalized;
        }
        else
        {
            moveDir = new Vector3(H, 0, V).normalized;
        }

        Vector3 velocity = moveDir * speed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;

        if (isAiming)
        {
            if (camTransform != null)
            {
                Vector3 camForward = camTransform.forward;
                camForward.y = 0; 
                if (camForward != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(camForward);
                    rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * turnSpeed));
                }
            }
        }
        else
        {
            if (moveDir.magnitude >= 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * turnSpeed));
            }
        }
    }

    public void SpeedControll(float ScrollValue)
    {
        // 5. เช็กว่ามีการขยับลูกกลิ้งในเฟรมนี้หรือไม่ ( triggered )
            if (ScrollValue > 0f) playerState++;
            else if (ScrollValue < 0f) playerState--;

        print(playerState);
            playerState = Mathf.Clamp(playerState, -1, 2); 
    }

    void setSpeedPlayer()
    {
        if (playerState == 0) speed = 4f;
        else if (playerState == 1) speed = 6f;
        else if (playerState == 2) speed = 8f;
        else if (playerState == -1) speed = 3f;
        else if (playerState == -2) speed = 1f;
    }

    void setCollider()
    {
        // 1. เช็คความเร็วที่ขยับจริงๆ ในโลก (ตัดแกน Y ออก ป้องกันบั๊กตกจากที่สูงแล้วเกิดเสียงดัง)
        Vector3 actualVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        float targetRadius = 0f;

        // 2. ถ้าตัวละครมีการขยับจริงๆ (มากกว่า 0.1 กันค่าคลาดเคลื่อน)
        if (actualVelocity.magnitude > 0.1f)
        {
            // คำนวณรัศมีเสียงตามความเร็ว (Speed) ที่เราเลือกล็อคไว้
            float speedNormalized = Mathf.InverseLerp(3f, 10f, speed);
            targetRadius = Mathf.Lerp(minNoiseRadius, maxNoiseRadius, speedNormalized);
        }
        else
        {
            // ถ้ายืนนิ่งๆ ให้เป้าหมายรัศมีเสียงเป็น 0 ไปเลย (หรือจะให้เหลือ minNoiseRadius ก็ได้)
            targetRadius = 0f;
        }

        // 3. ใช้ Lerp เพื่อให้วงกลมสมูท ไม่กางหรือหุบทันทีใน 1 เฟรม (ดูเป็นมืออาชีพขึ้นเยอะครับ!)
        noiseCollider.radius = Mathf.Lerp(noiseCollider.radius, targetRadius, Time.deltaTime * 15f);
    }

    //void Interaction()
    //{
    //Collider[] enviroment = Physics.OverlapSphere(transform.position, 5f, InteracMask);

    //foreach (var Obj in enviroment)
    //{
    // 6. ใช้ WasPressedThisFrame() ซึ่งมีความหมายเหมือน Input.GetKeyDown() เป๊ะ
    //if (Obj.CompareTag("lightSwitch") && interactAction.action.WasPressedThisFrame())
    //{
    //Obj.GetComponent<light_switch>().Turn();
    //}

    //if (Obj.CompareTag("Door") && interactAction.action.WasPressedThisFrame())
    //{
    //var DoorTarget = Obj.GetComponent<Door>();
    //if (DoorTarget != null)
    //{
    //if (DoorTarget.currentState == Door.DoorState.Closed)
    //{
    //DoorTarget.ToggleDoor(false, Door.DoorState.Open);
    //}
    //else
    //{
    //DoorTarget.ToggleDoor(false, Door.DoorState.Closed);
    //}
    //}
    //}
    //}
    //}
}