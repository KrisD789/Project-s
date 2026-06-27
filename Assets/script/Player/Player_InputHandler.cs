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
    public InputActionReference Switch_To_Primary;
    public InputActionReference Switch_To_Secondary;
    public InputActionReference Switch_Fire_Mode;
    public InputActionReference reload_Action;
    public InputActionReference Crouch_Action;


    [Header("Script References")]
    public Player_moveMent player_MoveMent;
    public Weapon_system weapon_system;
    public Player_Action player_action;
    public CameraControl camera_control;


    private void OnEnable()
    {
        moveAction.action.Enable();
        interactAction.action.Enable();
        scrollAction.action.Enable();
        aimAction.action.Enable();
        FireAction.action.Enable();
        Switch_To_Primary.action.Enable();
        Switch_To_Secondary.action.Enable();
        Switch_Fire_Mode.action.Enable();
        reload_Action.action.Enable();
        Crouch_Action.action.Enable();
    }

    private void OnDisable()
    {
        // 1. ถอดสายไฟ Event ให้ครบทุกอันที่มีการใช้ += ใน Awake
        interactAction.action.performed -= OnInteract;
        scrollAction.action.performed -= HandleScrollInput;
        Switch_To_Primary.action.performed -= Activate_Primary_Weapon; 
        Switch_To_Secondary.action.performed -= Activate_Secondary_Weapon;
        Switch_Fire_Mode.action.performed -= SwitchFireMode;
        reload_Action.action.performed -= Reload;
        Crouch_Action.action.performed -= ToggleCrouch;

        // 2. ปิดการรับค่า
        moveAction.action.Disable();
        interactAction.action.Disable();
        scrollAction.action.Disable();
        aimAction.action.Disable();
        FireAction.action.Disable();
        Switch_To_Primary.action.Disable();
        Switch_To_Secondary.action.Disable();
        Switch_Fire_Mode.action.Disable();
        reload_Action.action?.Disable();
        Crouch_Action?.action?.Disable();
    }


    private void Awake()
    {
        // ใช้ TryGetComponent เดี่ยวๆ เพื่อดึงสคริปต์ที่อยู่บน GameObject เดียวกันมาใส่ในตัวแปร
        if (TryGetComponent<Player_moveMent>(out player_MoveMent))
        {
            Debug.Log("Found Player_moveMent script");
        }
        else
        {
            Debug.LogWarning("หา Player_moveMent ไม่เจอ! ลืมแปะสคริปต์ไว้ที่ตัวละครหรือเปล่า?");
        }

        if (TryGetComponent<Weapon_system>(out weapon_system))
        {
            Debug.Log("Found Weapon_system script");
        }
        else
        {
            Debug.LogWarning("หา Weapon_system ไม่เจอ!");
        }

        if(TryGetComponent<Player_Action>(out player_action))
        {
            Debug.Log("Found player_action script");
        }

        else
        {
            Debug.Log("หา player_action script ไม่เจอ");
        }

        if (TryGetComponent<CameraControl>(out camera_control))
        {
            Debug.Log("Found camera_control script");
        }

        else
        {
            Debug.Log("หา camera_control script ไม่เจอ");
        }

        // เสียบสายไฟ Event
        interactAction.action.performed += OnInteract;
        scrollAction.action.performed += HandleScrollInput;
        Switch_To_Primary.action.performed += Activate_Primary_Weapon;
        Switch_To_Secondary.action.performed += Activate_Secondary_Weapon;
        Switch_Fire_Mode.action.performed += SwitchFireMode;
        reload_Action.action.performed += Reload;
        Crouch_Action.action.performed += ToggleCrouch;
    }

    private void Update()
    {
        if (weapon_system == null) return;

        // แยกการรับค่า 2 แบบ: การยิง
        // .IsPressed() = เป็น True ตลอดเวลาที่เอานิ้วกดเมาส์ค้างไว้
        bool isHoldingFire = FireAction.action.IsPressed();

        // .WasPressedThisFrame() = เป็น True แค่เฟรมเดียวตอนที่คลิกเมาส์ลงไป (เหมาะกับ Semi-Auto)
        bool isClickingFire = FireAction.action.WasPressedThisFrame();

        if (FireAction != null && FireAction.action.IsPressed())
        {
            OnFire(isHoldingFire, isClickingFire);
        }

        //การเล็ง
        // 1. จังหวะ "เริ่มกด" คลิกขวา (ทำงานเฟรมเดียว)
        if (aimAction != null && aimAction.action.WasPressedThisFrame())
        {
            TakeAim(true, true);
        }
        // 2. จังหวะ "ปล่อยนิ้ว" จากคลิกขวา (ทำงานเฟรมเดียว)
        else if (aimAction != null && aimAction.action.WasReleasedThisFrame())
        {
            TakeAim(false, false);
        }
    }

    private void FixedUpdate()
    {
        if (player_MoveMent == null) return;

        // ดึงค่าปุ่มแบบ "เช็คต่อเนื่อง" (Continuous)
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        bool isAiming = aimAction.action.IsPressed();

        // ส่งค่าการเดินและการเล็งไปให้ร่างกายจัดการทุกเฟรม!
        player_MoveMent.MoveAndRotate(moveInput, isAiming);

        //if (FireAction != null && FireAction.action.IsPressed())
        //{
        //OnFire();
        //}

        

    }

    public void OnFire(bool isHolding, bool isClicking)
    {
        if (player_action.currentState == Player_Action.PlayerState.GrabbingEnemy)
        {
            // ให้รับแค่จังหวะคลิก (isClicking) เท่านั้น ห้ามกดค้าง
            if (isClicking)
            {
                player_action.ChooseToKill();
            }
        }
        else
        {
            //Debug.Log("ยิงปืน!");
            weapon_system.HandleShooting(isHolding, isClicking);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("interaction!");
        player_action.Interaction();
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

    public void TakeAim(bool HoldAim, bool clickAim)
    {
        if (player_action.currentState == Player_Action.PlayerState.GrabbingEnemy)
        {
            // เช็คก่อนว่าเป็นการ "กดคลิกลงไป" (Press == true) ถึงจะรัดคอ
            if (clickAim)
            {
                player_action.ChooseToKnockout();
            }
        }
        else
        {
            camera_control.HandleAim(HoldAim);
            //Debug.Log("เล็ง!");
        }
    }

    public void Activate_Primary_Weapon(InputAction.CallbackContext context)
    {
        Debug.Log("เปลี่ยนไปใช้ปืนหลัก!!!!");
        weapon_system.EquipPrimary();
    }

    public void Activate_Secondary_Weapon(InputAction.CallbackContext context)
    {
        Debug.Log("เปลี่ยนไปใช้ปืนรอง!!!!");
        weapon_system.EquipSecondary();
    }

    public void SwitchFireMode(InputAction.CallbackContext context)
    {
        weapon_system.Switch_FireMode();
    }

    public void Reload(InputAction.CallbackContext context)
    {
        weapon_system.Start_Reload();
    }

    public void ToggleCrouch(InputAction.CallbackContext context)
    {
        player_action.HandleCrouch();
    }
}
