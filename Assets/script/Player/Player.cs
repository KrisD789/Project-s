using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    [Header("สถานะพลังชีวิต")]
    public float currentHP = 100f;
    public float MaxHP = 100f;
    public float PermanentDamage = 3;
    public bool isDead = false;

    [Header("Equipped Armor")]
    public Armor_Item currentArmorProfile; // ช่องสำหรับลากไฟล์เกราะ (ScriptableObject) มาใส่
    public float currentArmorDurability;
    

    // ย้าย PlayerState จาก Player_Action มาไว้ที่นี่ (เป็นศูนย์กลาง)
    public enum PlayerState
    {
        Idle,           // ว่างเปล่า (เดิน/วิ่ง/ยิงปืน ปกติ)
        CarryingBody,   // กำลังแบกศพ
        GrabbingEnemy,  // กำลังล็อคคอศัตรู
        Crouch          // กำลังนั่งย่อ
    }

    [Header("สถานะปัจจุบันของผู้เล่น")]
    public PlayerState currentState = PlayerState.Idle;

    [Header("ระบบ Stealth")]
    public bool OnDark = false;

    [Header("อ้างอิง Component ต่างๆ (เพื่อให้คนอื่นเรียกใช้)")]
    public Player_Action action;
    public Player_moveMent movement;
    public Weapon_system weaponSystem;
    public CameraControl cameraControl;
    public LightDetect player_Light_Detect;
    public Player_Inventory player_Inventory;

    private void Awake()
    {
        Instance = this;

        // ให้มันดึงสคริปต์ในตัวเองมาเก็บไว้เลย
        if (!TryGetComponent<Player_Action>(out action))
            Debug.LogWarning("Player: หา Player_Action ไม่เจอ!");

        if (!TryGetComponent<LightDetect>(out player_Light_Detect))
            Debug.LogWarning("Player: หา CameraControl ไม่เจอ!");

        if (!TryGetComponent<Player_moveMent>(out movement))
            Debug.LogWarning("Player: หา Player_moveMent ไม่เจอ!");

        if (!TryGetComponent<Weapon_system>(out weaponSystem))
            Debug.LogWarning("Player: หา Weapon_system ไม่เจอ!");

        if (!TryGetComponent<CameraControl>(out cameraControl))
            Debug.LogWarning("Player: หา CameraControl ไม่เจอ!");

        if (!TryGetComponent<Player_Inventory>(out player_Inventory))
            Debug.LogWarning("Player: หา Player_Inventory ไม่เจอ!");
    }
    private void Start()
    {
        EquipArmor(currentArmorProfile);
    }

    public void Player_TakeDamage(float incomingDamage)
    {
        // 1. ตรวจสอบว่าใส่เกราะอยู่ และเกราะยังไม่แตก
        if (currentArmorProfile != null && currentArmorDurability > 0)
        {
            // ใช้เปอร์เซ็นต์จาก ScriptableObject มาคำนวณ
            float damageToArmor = incomingDamage ;

            float damageToHP = incomingDamage * (1f - currentArmorProfile.Armor_Block_Percentage);

            if (currentArmorDurability >= damageToArmor)
            {
                currentArmorDurability -= damageToArmor; // ลดความทนทานเกราะ
                currentHP -= damageToHP;                 // ลดเลือดจริง
            }
            else
            {
                //กรณีเกราะเหลือน้อยก็ซับดาเมจเท่าที่เหลือ
                float percentAbsorbed = currentArmorDurability / damageToArmor;

                // ดาเมจส่วนที่เกราะซับไว้ทัน (ลดทอนแล้ว)
                float mitigatedHP = (incomingDamage * percentAbsorbed) * (1f - currentArmorProfile.Armor_Block_Percentage);

                // ดาเมจส่วนที่ทะลุเกราะเข้ามาแบบเต็มๆ 100%
                float rawSpilloverHP = incomingDamage * (1f - percentAbsorbed);

                currentArmorDurability = 0;
                currentHP -= (mitigatedHP + rawSpilloverHP);

                Debug.Log("เกราะแตกกระจาย!");
            }
        }
        else
        {
            // 2. ถ้าไม่มีเกราะ หรือเกราะแตกไปแล้ว รับดาเมจ 100% พร้อมกับลด MaxHP (แผลฉกรรจ์)
            currentHP -= incomingDamage;
            MaxHP -= PermanentDamage;
        }

        // บังคับไม่ให้ MaxHP ต่ำกว่า 1 และไม่ให้ currentHP ล้นเกิน MaxHP
        MaxHP = Mathf.Clamp(MaxHP, 1f, 100f);
        currentHP = Mathf.Clamp(currentHP, 0f, MaxHP);

        Debug.Log($"โดนโจมตี! HP เหลือ: {currentHP} | เกราะเหลือ: {currentArmorDurability}");

        if (currentHP <= 0)
        {
            Debug.Log("ผู้เล่นเสียชีวิต!");
        }
    }

    public void EquipArmor(Armor_Item newArmor)
    {
        currentArmorProfile = newArmor;

        if (currentArmorProfile != null)
        {
            // ดึงค่าความทนทานสูงสุด มาใส่ในตัวแปรจำลอง
            currentArmorDurability = currentArmorProfile.Max_Armor_Durability;
            Debug.Log($"สวมใส่เกราะ: {currentArmorProfile.name} | พลังป้องกัน: {currentArmorProfile.Armor_Block_Percentage * 100}%");
        }
        else
        {
            currentArmorDurability = 0f;
            Debug.Log("ไม่ได้สวมใส่เกราะ");
        }
    }
}
