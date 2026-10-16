using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public static PlayerAnimator Instance { get; private set; }

    private Animator anim;

    // สคริปต์พวกนี้อยู่บน Parent
    private Player_moveMent movement;
    public Player_Action playerAction;
    private Weapon_system weaponSystem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        // 1. ดึง Animator จากตัวเอง (เพราะสคริปต์นี้แปะอยู่บนตัวโมเดล Child)
        anim = GetComponent<Animator>();

        // 2. ป้องกันกรณีลืมลากใส่ Player.Instance ก็ใช้การค้นหาจาก Parent เป็นแผนสำรอง
        // (GetComponentInParent จะวิ่งขึ้นไปหา Parent ข้างบนสุดที่สคริปต์พวกนี้แปะอยู่)
        if (Player.Instance == null)
        {
            Debug.LogWarning("หา Player.Instance ไม่เจอใน Awake, กำลังดึงจาก Parent แทน...");
            movement = GetComponentInParent<Player_moveMent>();
            playerAction = GetComponentInParent<Player_Action>();
            weaponSystem = GetComponentInParent<Weapon_system>();
        }
    }

    void Start()
    {
        // 3. ถ้าระบบศูนย์กลาง (Player.Instance) ทำงานปกติ ก็ดึงข้อมูลจากศูนย์กลางเลย (วิธีหลัก)
        if (Player.Instance != null)
        {
            movement = Player.Instance.movement;
            weaponSystem = Weapon_system.Instance; // หรือ Player.Instance.weaponSystem 

            // ใช้ GetComponentInParent ดึง playerAction มาให้ชัวร์
            playerAction = GetComponentInParent<Player_Action>();
        }
    }

    void Update()
    {
        if (anim == null) return;

        // --- 1. อัปเดตค่าพื้นฐาน (เดิน, นั่ง, แบกศพ) ---
        if (movement != null) anim.SetFloat("Speed", movement.GetCurrentVelocity());

        if (Player.Instance != null)
        {
            anim.SetBool("IsCrouching", Player.Instance.currentMovementState == Player.MovementState.Crouch);
            anim.SetBool("IsCarrying", Player.Instance.currentState == Player.PlayerState.CarryingBody);
        }

        // --- 2. อัปเดตค่าเกี่ยวกับอาวุธ ---
        // เพิ่มเงื่อนไขดักตอนกำลังฮีล เพื่อไม่ให้ WeaponType โดนบังคับกลับเป็น 0
        if (Player.Instance != null && Player.Instance.currentState == Player.PlayerState.Healing)
        {
            anim.SetBool("have weapon", false); // ถือไอเทมฮีล ไม่นับว่าเป็นปืน

            float currentCrouchWeight = (Player.Instance.currentMovementState == Player.MovementState.Crouch) ? 1f : 0f;
            float smoothCrouchWeight = Mathf.Lerp(anim.GetFloat("CrouchWeight"), currentCrouchWeight, Time.deltaTime * 10f);
            anim.SetFloat("CrouchWeight", smoothCrouchWeight);
        }
        else if (weaponSystem != null && weaponSystem.currentWeapon != null)
        {
            anim.SetBool("have weapon", true);
            anim.SetBool("IsAimimg", Player.Instance.currentState == Player.PlayerState.Aim);

            float currentCrouchWeight = (Player.Instance.currentMovementState == Player.MovementState.Crouch) ? 1f : 0f;
            float smoothCrouchWeight = Mathf.Lerp(anim.GetFloat("CrouchWeight"), currentCrouchWeight, Time.deltaTime * 10f);
            anim.SetFloat("CrouchWeight", smoothCrouchWeight);
        }
        else
        {
            anim.SetBool("have weapon", false);
            anim.SetInteger("WeaponType", 0);
        }

        // --- ตัวจับการขัดจังหวะ ---
        if (Player.Instance != null && Player.Instance.currentState == Player.PlayerState.Dead && playerAction != null && playerAction.GetGrabbedEnemy() != null)
        {
            TriggerTakedownInterrupt();
        }
    }

    // ==========================================================
    // --- คำสั่งภายนอก (เรียกผ่าน PlayerAnimator.Instance) ---
    // ==========================================================
    public void SetWeaponType(int typeIndex) { if (anim != null) anim.SetInteger("WeaponType", typeIndex); }
    public void SetFiring(bool isFiring) { if (anim != null) anim.SetBool("IsFiring", isFiring); }
    public void SetFireMode(bool isFullAuto)
    {
        if (anim != null)
        {
            anim.SetBool("IsFullAuto", isFullAuto);
            anim.SetBool("Is_SigleAction", !isFullAuto);
        }
    }
    public void TriggerReload() { if (anim != null) anim.SetTrigger("Reload"); }
    public void TriggerHeal() {
        if (anim != null)
        {
            anim.SetInteger("WeaponType", 3);
        }
    }

    // ==========================================================
    // --- Animation Events (ผูกกับไทม์ไลน์) ---
    // ==========================================================
    public void TriggerSpawnWeapon()
    {
        if (weaponSystem != null) weaponSystem.AnimEvent_SpawnCurrentWeapon();
    }

    public void TriggerDestroyWeapon()
    {
        if (weaponSystem != null) weaponSystem.AnimEvent_DestroyCurrentWeapon();
    }

    // --- Takedown Events ---
    // ยิง Trigger ตามชื่อ Parameter ในรูปเป๊ะๆ
    public void PlayBehideTakedown() { if (anim != null) anim.SetTrigger("BehideTakeDown"); }
    public void PlayFrontTakedown() { if (anim != null) anim.SetTrigger("FrontTakdown"); } //  สะกด FrontTakdown ตาม Parameter ใน Unity
    public void PlayKill() { if (anim != null) anim.SetTrigger("Kill"); }
    public void PlayKnockOut() { if (anim != null) anim.SetTrigger("KnockOut"); }

    public void TriggerTakedownWaitChoice() { Debug.Log("รอคำสั่ง Kill หรือ Knockout"); }

    public void TriggerTakedownInterrupt()
    {
        if (playerAction != null) playerAction.CancelTakedown(playerAction.GetGrabbedEnemy());
    }

    // เพิ่มฟังก์ชันใหม่: เอาไว้รับ Event จากเฟรมสุดท้ายของแอนิเมชัน
    public void AnimEvent_TakedownFinished()
    {
        if (playerAction != null) playerAction.FinishTakedown();
    }

    public void PlayPickUpBody() { if (anim != null) anim.SetTrigger("PickUpBody"); }
    public void PlayDropBody() { if (anim != null) anim.SetTrigger("DropBody"); }

}