using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    [Header("สถานะพลังชีวิต")]
    public float Health = 100f;
    public float MaxHealth = 100f;
    public float Armor;
    public bool isDead = false;

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

    public void Player_TakeDamage(float E_Weapon_Damage)
    {
        if (isDead) return; // ถ้าตายแล้ว ไม่ต้องลดเลือดซ้ำ

        if (Health > 0)
        {
            Health -= E_Weapon_Damage;
            if (Health <= 0)
            {
                Health = 0;
                isDead = true;
                Debug.Log("Player : กูตายแล้วยิงอะไรเยอะแยะ");
                // TODO: ใส่ฟังก์ชัน Game Over 
            }
        }
    }
}
