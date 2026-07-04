using UnityEngine;

public class Enemy_Weapon : MonoBehaviour
{
    [Header("อ้างอิงสคริปต์อื่น")]
    private enemy_stage enemy_main;
    private EnemyRaycast enemy_raycast;
    private GameObject Player_Obj;
    private Player player_Script;

    [Header("ตั้งค่าปืน")]
    public float fireRate = 0.5f;     // ความเร็วในการยิง (วินาทีต่อนัด)
    private float nextFireTime = 0f;  // ตัวนับเวลารอยิงนัดถัดไป
    public float weaponDamage = 10f;  // ดาเมจกระสุน
    //public Transform gunBarrel;       // ปลายกระบอกปืน (เอาไว้ใส่ Muzzle Flash ในอนาคต)

    void Awake()
    {
        // ดึงคอมโพเนนต์จากตัวศัตรูมาเก็บไว้
        enemy_main = GetComponent<enemy_stage>();
        enemy_raycast = GetComponent<EnemyRaycast>();
        Player_Obj = GameObject.FindGameObjectWithTag("Player");

        if(Player_Obj.TryGetComponent<Player>(out player_Script))
        {
            Debug.Log("Enemy_Weapon  !!Found ---> Player_Obj!!");
        }

        else
        {
            Debug.Log("!!-----Warning-----!!  Enemy_Weapon  !! Not Found !! Player_Obj!!");
        }
    }

    void Update()
    {
        // 1. เช็คก่อนว่าตอนนี้ตายหรือสลบอยู่ไหม ถ้าใช่ก็หยุดทำงาน
        if (enemy_main.currentState == enemy_stage.EnemyState.dead ||
            enemy_main.currentState == enemy_stage.EnemyState.faint)
        {
            return;
        }

        // 2. เงื่อนไขการยิง: ต้องอยู่ในโหมด Alert "และ" เรดาร์ต้องเห็นผู้เล่นชัดเจน
        if (enemy_main.currentState == enemy_stage.EnemyState.Alert && enemy_raycast.foundPlayer)
        {
            // 3. หันหน้าตรงเป้าหรือยัง? (ถ้าหันตรงแล้วค่อยยิง) 
            // ตรงนี้ทำง่ายๆ คือถ้าระบบ Raycast อนุญาตแล้ว ก็ลั่นไกเลย
            TryShoot();
            print("Shoot !!!");
        }
    }

    void TryShoot()
    {
        // เช็ค Fire Rate (คูลดาวน์กระสุน)
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate; // ตั้งเวลารอนัดต่อไป

            Fire();
        }
    }

    void Fire()
    {
        Debug.Log("ปัง! ศัตรูยิงผู้เล่น 1 นัด โดนดาเมจ: " + weaponDamage);

        // TODO: ใส่ลูกเล่นเพิ่มตรงนี้ได้เลย
        // 1. เล่นเสียงปืน: AudioSource.PlayClipAtPoint(...)
        // 2. เล่นแสงปลายปืน: muzzleFlash.Play();
        // 3. ส่งดาเมจให้ Player: 
        if (player_Script != null) player_Script.Player_TakeDamage(weaponDamage);
    }
}
