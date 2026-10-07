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

    [Header("สถานะการควบคุม")]
    public bool canMove = true;

    Rigidbody rb;
    Transform camTransform;
    public LayerMask InteracMask;

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
        setSpeedPlayer();
        setCollider();
    }

    public void SetMovementLock(bool isLocked)
    {
        Debug.Log("SetMovementLock in Player controller.cs !!!!! Activate !!!!!");
        canMove = !isLocked;
        if (isLocked)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            rb.angularVelocity = Vector3.zero;
        }
    }

    public float GetCurrentVelocity()
    {
        Vector3 actualVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        return actualVelocity.magnitude;
    }

    public void MoveAndRotate(Vector2 moveInput, bool isAiming)
    {
        if (!canMove) return;

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

        // --- ระบบอัตราเร่ง และผลกระทบจากเกราะ ---
        float armorAccelModifier = 1f;
        if (Player.Instance != null && Player.Instance.currentArmorProfile != null)
        {
            armorAccelModifier = Player.Instance.currentArmorProfile.Movement_Speed_Multiplier;
        }

        float accelerationRate = 10f;

        if (isAiming)
        {
            // เล็งปืน: ตอบสนองทันที 100% ไม่สนน้ำหนักเกราะ
            accelerationRate = 50f;
        }
        else if (playerState <= 0)
        {
            // ย่อง/เดิน/นั่งยอง: ออกตัวไวขึ้นเพื่อความแม่นยำในการตามศัตรู
            accelerationRate = 25f * armorAccelModifier;
        }
        else
        {
            // วิ่ง/Sprint: แสดงน้ำหนักและความหนืดของเกราะอย่างเต็มที่
            accelerationRate = 10f * armorAccelModifier;
        }

        // คำนวณความเร็วเป้าหมาย และใช้ Lerp ปรับความเร็วจริง
        Vector3 targetVelocity = moveDir * speed;
        targetVelocity.y = rb.linearVelocity.y;

        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * accelerationRate);

        // --- ระบบหมุนตัว ---
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
        if (ScrollValue > 0f) playerState++;
        else if (ScrollValue < 0f) playerState--;

        playerState = Mathf.Clamp(playerState, -2, 2);
    }

    void setSpeedPlayer()
    {
        int maxState = 2;
        //  เพิ่มเงื่อนไขดักตอนฮีล ให้เดินช้ามากๆ (ล็อกเพดานไว้ที่โหมดย่อง State -1)
        if (Player.Instance.currentState == Player.PlayerState.Healing)
        {
            maxState = -1;
        }
        
        if (Player.Instance.currentMovementState == Player.MovementState.Crouch ||
            Player.Instance.currentState == Player.PlayerState.CarryingBody ||
            Player.Instance.currentState == Player.PlayerState.Aim ||
            Player.Instance.currentState == Player.PlayerState.GrabbingEnemy)
        {
            maxState = 0;
        }

        playerState = Mathf.Clamp(playerState, -2, maxState);

        // คำนวณความเร็วตั้งต้น
        float baseSpeed = 0f;
        if (playerState == 0) baseSpeed = 5f;
        else if (playerState == 1) baseSpeed = 6f;
        else if (playerState == 2) baseSpeed = 10f;
        else if (playerState == -1) baseSpeed = 4f;
        else if (playerState == -2) baseSpeed = 2f;

        // คำนวณผลกระทบของเกราะต่อความเร็วสูงสุด
        float armorSpeedModifier = 1f;
        if (Player.Instance != null && Player.Instance.currentArmorProfile != null)
        {
            armorSpeedModifier = Player.Instance.currentArmorProfile.Movement_Speed_Multiplier;
        }

        speed = baseSpeed * armorSpeedModifier;
    }

    void setCollider()
    {
        Vector3 actualVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float targetRadius = 0f;

        if (actualVelocity.magnitude > 0.1f)
        {
            // --- เพิ่มตรงนี้: สร้างตัวแปรจำลองความเร็วตั้งต้น เพื่อใช้คำนวณเสียงโดยเฉพาะ ---
            float baseSpeedForNoise = 0f;
            if (playerState == 0) baseSpeedForNoise = 4f;
            else if (playerState == 1) baseSpeedForNoise = 8f;
            else if (playerState == 2) baseSpeedForNoise = 10f;
            else if (playerState == -1) baseSpeedForNoise = 3f;
            else if (playerState == -2) baseSpeedForNoise = 1f;

            // ใช้ baseSpeedForNoise (ความเร็วที่ยังไม่โดนเกราะถ่วง) มาคำนวณหลอดเสียงแทน
            float speedNormalized = Mathf.InverseLerp(1f, 10f, baseSpeedForNoise); //********************************************//
            targetRadius = Mathf.Lerp(minNoiseRadius, maxNoiseRadius, speedNormalized);
        }
        else
        {
            targetRadius = 0f;
        }

        noiseCollider.radius = Mathf.Lerp(noiseCollider.radius, targetRadius, Time.deltaTime * 15f);
    }
}