using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("สถานะพลังชีวิต")]
    public float MaxHP = 100f;
    public float currentHP;
    public float currentMaxHP;
    public float PermanentDamage = 3;
    public bool isDead = false;

    [Header("Equipped Armor")]
    public Armor_Item currentArmorProfile;
    public float currentArmorDurability;

    // หมวดการเคลื่อนไหว (เพิ่มมาใหม่)
    public enum MovementState { Standing, Crouch }
    public MovementState currentMovementState = MovementState.Standing;

    // หมวดการกระทำ (ใช้ชื่อเดิม ตัวแปรเดิม แต่ลบ Crouch ออกไป)
    public enum PlayerState { Idle, CarryingBody, GrabbingEnemy, Aim, Healing, Dead }
    
    [Header("สถานะปัจจุบันของผู้เล่น")]
    public PlayerState currentState = PlayerState.Idle;

    [Header("ระบบ Stealth")]
    public bool OnDark = false;

    [Header("อ้างอิง Component ต่างๆ")]
    public Player_Action action;
    public Player_moveMent movement;
    public Weapon_system weaponSystem;
    public CameraControl cameraControl;
    public LightDetect player_Light_Detect;
    public Player_Inventory player_Inventory;

    private void Awake()
    {
        Instance = this;

        if (!TryGetComponent<Player_Action>(out action)) Debug.LogWarning("Player: หา Player_Action ไม่เจอ!");
        if (!TryGetComponent<LightDetect>(out player_Light_Detect)) Debug.LogWarning("Player: หา LightDetect ไม่เจอ!");
        if (!TryGetComponent<Player_moveMent>(out movement)) Debug.LogWarning("Player: หา Player_moveMent ไม่เจอ!");
        if (!TryGetComponent<Weapon_system>(out weaponSystem)) Debug.LogWarning("Player: หา Weapon_system ไม่เจอ!");
        if (!TryGetComponent<CameraControl>(out cameraControl)) Debug.LogWarning("Player: หา CameraControl ไม่เจอ!");
        if (!TryGetComponent<Player_Inventory>(out player_Inventory)) Debug.LogWarning("Player: หา Player_Inventory ไม่เจอ!");
    }

    private void Start()
    {
        currentArmorProfile = Load_out_manager.Instance.selectedArmor;
        EquipArmor(currentArmorProfile);
        currentHP = MaxHP;
        currentMaxHP = MaxHP;
    }

    public void Player_TakeDamage(float incomingDamage)
    {
        if (currentArmorProfile != null && currentArmorDurability > 0)
        {
            float damageToArmor = incomingDamage;
            float damageToHP = incomingDamage * (1f - currentArmorProfile.Armor_Block_Percentage);

            if (currentArmorDurability >= damageToArmor)
            {
                currentArmorDurability -= damageToArmor;
                currentHP -= damageToHP;
            }
            else
            {
                float percentAbsorbed = currentArmorDurability / damageToArmor;
                float mitigatedHP = (incomingDamage * percentAbsorbed) * (1f - currentArmorProfile.Armor_Block_Percentage);
                float rawSpilloverHP = incomingDamage * (1f - percentAbsorbed);

                currentArmorDurability = 0;
                currentHP -= (mitigatedHP + rawSpilloverHP);
                Debug.Log("เกราะแตกกระจาย!");
            }
        }
        else
        {
            currentHP -= incomingDamage;
            currentMaxHP -= PermanentDamage;
        }

        currentMaxHP = Mathf.Clamp(currentMaxHP, 1f, MaxHP);
        currentHP = Mathf.Clamp(currentHP, 0f, currentMaxHP);

        Debug.Log($"โดนโจมตี! HP เหลือ: {currentHP} | เกราะเหลือ: {currentArmorDurability}");
        if (currentHP <= 0 && !isDead)
        {
            Die();
            Debug.Log("ผู้เล่นเสียชีวิต!");
        }
    }

    public void EquipArmor(Armor_Item newArmor)
    {
        currentArmorProfile = newArmor;
        if (currentArmorProfile != null)
        {
            currentArmorDurability = currentArmorProfile.Max_Armor_Durability;
            Debug.Log($"สวมใส่เกราะ: {currentArmorProfile.name} | พลังป้องกัน: {currentArmorProfile.Armor_Block_Percentage * 100}%");
        }
        else
        {
            currentArmorDurability = 0f;
            Debug.Log("ไม่ได้สวมใส่เกราะ");
        }
    }

    public void Healing(float HpRecovery, float MaxHpRecovery)
    {
        if (currentHP >= currentMaxHP && currentMaxHP >= MaxHP) return;

        currentHP += HpRecovery;
        currentMaxHP += MaxHpRecovery;
        currentHP = Mathf.Clamp(currentHP, 0f, currentMaxHP);
        currentMaxHP = Mathf.Clamp(currentMaxHP, 1f, MaxHP);
    }

    private void Die()
    {
        isDead = true;
        currentState = PlayerState.Dead;

        Debug.Log("ผู้เล่นเสียชีวิต! ตัดการควบคุม");

        // 3. สั่งหยุดเวลา และเรียกหน้าต่าง Death Menu
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // TODO: เรียกเปิด UI Death Menu ของคุณตรงนี้
        GameMenuManager menuManager = FindAnyObjectByType<GameMenuManager>();
        if (menuManager != null)
        {
            menuManager.ShowDeadMenu();
        }
    }
}