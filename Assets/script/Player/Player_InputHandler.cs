using UnityEngine;
using UnityEngine.InputSystem;


public class Player_InputHanler : MonoBehaviour
{
    [Header("Input Actions")]
    // 2. สร้างตัวแปรสำหรับรับค่า Input จากหน้า Inspector
    public InputActionReference moveAction;     // สำหรับเดิน (W A S D)
    public InputActionReference interactAction; // สำหรับกดปุ่ม (E)
    public InputActionReference scrollAction;   // สำหรับลูกกลิ้งเมาส์
    public InputActionReference aimAction;
    public InputActionReference FireAction;

    public Player_moveMent player_MoveMent;
    private void OnEnable()
    {
        moveAction.action.Enable();
        interactAction.action.Enable();
        scrollAction.action.Enable();
        aimAction.action.Enable();
        FireAction.action.Enable();
    }

    private void OnDisable()
    {
        interactAction.action.performed -= OnInteract;
        scrollAction.action.performed -= HandleScrollInput;

        moveAction.action.Disable();
        interactAction.action.Disable();
        scrollAction.action.Disable();
        aimAction.action.Disable();
        FireAction.action.Disable();
    }

    private void Awake()
    {
        //moveAction.action.performed += move;
        interactAction.action.performed += OnInteract;
        scrollAction.action.performed += HandleScrollInput;
        //aimAction.action.performed += TakeAim;
        //FireAction.action.performed -= TakeAim;
    }

    private void FixedUpdate()
    {
        if (player_MoveMent == null) return;

        // ดึงค่าปุ่มแบบ "เช็คต่อเนื่อง" (Continuous)
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        bool isAiming = aimAction.action.IsPressed();

        // ส่งค่าการเดินและการเล็งไปให้ร่างกายจัดการทุกเฟรม!
        player_MoveMent.MoveAndRotate(moveInput, isAiming);

        if (FireAction != null && FireAction.action.IsPressed())
        {
            OnFire();
        }

        if (aimAction != null && aimAction.action.IsPressed())
        {
            TakeAim();
        }
    }

    public void OnFire()
    {
        Debug.Log("ยิงปืน!");
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("เก็บของ!");
    }

    public void move()
    {
        Debug.Log("เคลื่อนที่!");
    }

    public void HandleScrollInput(InputAction.CallbackContext context)
    {
        float scrollY = scrollAction.action.ReadValue<Vector2>().y;
        player_MoveMent.SpeedControll(scrollY); 
    }

    public void TakeAim()
    {
        Debug.Log("เล็ง!");
    }
}
