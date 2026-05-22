using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public float speed = 1f;
    public int playerState = 0; // เปลี่ยนเป็น public เผื่อเช็คใน Inspector

    public SphereCollider noiseCollider;
    public float minNoiseRadius = 0.5f;
    public float maxNoiseRadius = 10f;

    // 1. เปลี่ยนมาใช้ Rigidbody
    Rigidbody rb;

    public LayerMask InteracMask;

    void Start()
    {
        // 2. ดึง Rigidbody มาเก็บไว้
        rb = GetComponent<Rigidbody>();

        // กันไม่ให้ตัวละครล้มกลิ้งเวลาชนของ
        rb.freezeRotation = true;
    }

    void Update()
    {
        float H = Input.GetAxis("Horizontal");
        float V = Input.GetAxis("Vertical");
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");

        // จัดการเรื่อง Scroll Wheel เหมือนเดิม
        HandleScrollInput(scrollDelta);

        setSpeedPlayer();
        setCollider();
        Interaction();
    }

    // 3. ใช้ FixedUpdate สำหรับการขยับตัวด้วย Rigidbody (เพื่อความเสถียรของฟิสิกส์)
    void FixedUpdate()
    {
        float H = Input.GetAxis("Horizontal");
        float V = Input.GetAxis("Vertical");

        Vector3 moveDir = (transform.forward * V + transform.right * H).normalized;

        // ใส่แรงโน้มถ่วงเดิมของมันไว้ในแกน Y
        Vector3 velocity = moveDir * speed;
        velocity.y = rb.linearVelocity.y; // Unity 6 ใช้ linearVelocity แทน velocity

        rb.linearVelocity = velocity;
    }

    void HandleScrollInput(float scrollDelta)
    {
        if (scrollDelta != 0f)
        {
            if (scrollDelta > 0f) playerState++;
            else if (scrollDelta < 0f) playerState--;

            playerState = Mathf.Clamp(playerState, -1, 2);
        }
    }

    void setSpeedPlayer()
    {
        if (playerState == 0) speed = 5f;
        else if (playerState == 1) speed = 8f;
        else if (playerState == 2) speed = 10f;
        else if (playerState == -1) speed = 3f;
    }

    void setCollider()
    {
        // ใช้ค่าความเร็วสูงสุด-ต่ำสุดจริงๆ ในการคำนวณ (3f ถึง 10f)
        float speedNormalized = Mathf.InverseLerp(3f, 10f, speed);
        float targetRadius = Mathf.Lerp(minNoiseRadius, maxNoiseRadius, speedNormalized);
        noiseCollider.radius = targetRadius;
    }

    void Interaction()
    {
        
        Collider[] enviroment = Physics.OverlapSphere(transform.position, 5f, InteracMask);

        foreach (var Obj in enviroment)
        {
            if (Obj.CompareTag("lightSwitch") && Input.GetKeyDown(KeyCode.E))
            { 
                Obj.GetComponent<light_switch>().Turn();
                
            }

            if (Obj.CompareTag("Door") && Input.GetKeyDown(KeyCode.E))
            {
                var DoorTarget = Obj.GetComponent<Door>();
                Debug.Log("Door detected");
                if (DoorTarget != null)
                {
                    if (DoorTarget.currentState == Door.DoorState.Closed)
                    {
                        DoorTarget.ToggleDoor(false, Door.DoorState.Open);
                    }

                    else DoorTarget.ToggleDoor(false, Door.DoorState.Closed);
                }
            }
        }
       
    }
}