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
    public bool canMove = true; // สวิตช์เปิดปิดการขยับและหมุน

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

    // ฟังก์ชันสั่งล็อกการเดินและการหมุนแบบ 100%
    public void SetMovementLock(bool isLocked)
    {
        Debug.Log("SetMovementLock in Player controller.cs !!!!! Activate !!!!!");
        canMove = !isLocked;
        if (isLocked)
        {
            // หยุดความเร็วเดิน และหยุดความเร็วหมุน (กันผู้เล่นไถลหรือหมุนค้าง)
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            rb.angularVelocity = Vector3.zero;
        }
    }

    // ฟังก์ชันส่งความเร็วจริงไปให้ PlayerAnimator
    public float GetCurrentVelocity()
    {
        Vector3 actualVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        return actualVelocity.magnitude;
    }

    public void MoveAndRotate(Vector2 moveInput, bool isAiming)
    {
        // ถ้าระบบโดนล็อกอยู่ ให้ออกจากการคำนวณเดินและการหมุนทันที
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
        if (ScrollValue > 0f) playerState++;
        else if (ScrollValue < 0f) playerState--;

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
        Vector3 actualVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float targetRadius = 0f;

        if (actualVelocity.magnitude > 0.1f)
        {
            float speedNormalized = Mathf.InverseLerp(3f, 10f, speed);
            targetRadius = Mathf.Lerp(minNoiseRadius, maxNoiseRadius, speedNormalized);
        }
        else
        {
            targetRadius = 0f;
        }

        noiseCollider.radius = Mathf.Lerp(noiseCollider.radius, targetRadius, Time.deltaTime * 15f);
    }
}