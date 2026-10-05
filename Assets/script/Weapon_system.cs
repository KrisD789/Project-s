using UnityEngine;

public class Weapon_system : MonoBehaviour
{
    public static Weapon_system Instance { get; private set; }

    public Weapon_Item Player_Primary_weapon;
    public Weapon_Item Player_Secondary_weapon;

    public Weapon_Item currentWeapon;
    public Load_out_manager LoadoutManager;

    [Header("Ammo Tracking (ในแม็กกาซีน)")]
    public int primary_CurrentAmmo;
    public int secondary_CurrentAmmo;

    [Header("Reserve Ammo Tracking (กระสุนสำรอง)")]
    public int primary_ReserveAmmo;   // กระสุนสำรองอาวุธหลัก
    public int secondary_ReserveAmmo; // กระสุนสำรองอาวุธรอง

    [Header("Recoil Settings")]
    public float recoilUp;
    public float recoilSide;

    public CameraControl cameraControlScript;

    // Property สำหรับจัดการกระสุนในแม็กกาซีน
    public int CurrentAmmo
    {
        get
        {
            if (currentWeapon == Player_Primary_weapon) return primary_CurrentAmmo;
            if (currentWeapon == Player_Secondary_weapon) return secondary_CurrentAmmo;
            return 0;
        }
        set
        {
            if (currentWeapon == Player_Primary_weapon) primary_CurrentAmmo = value;
            if (currentWeapon == Player_Secondary_weapon) secondary_CurrentAmmo = value;
        }
    }

    // Property สำหรับจัดการกระสุนสำรอง
    public int CurrentReserveAmmo
    {
        get
        {
            if (currentWeapon == Player_Primary_weapon) return primary_ReserveAmmo;
            if (currentWeapon == Player_Secondary_weapon) return secondary_ReserveAmmo;
            return 0;
        }
        set
        {
            if (currentWeapon == Player_Primary_weapon) primary_ReserveAmmo = value;
            if (currentWeapon == Player_Secondary_weapon) secondary_ReserveAmmo = value;
        }
    }

    [Header("การเชื่อมต่อกับร่างกาย")]
    public Transform weaponMount;

    private GameObject currentWeaponModel;
    public Transform currentFirePoint;

    public LayerMask Target_mask;
    public LayerMask Obtacle_mask;

    public enum CurrentFireMode { Semi_Auto, full_Auto }
    public CurrentFireMode current_Weapon_FireMode;

    public enum Weapon_Status { ready, reload }
    public Weapon_Status current_Weapon_Status = Weapon_Status.ready;

    private float nextTimeToFire = 0f;
    private float nextTimeToReload = 0f;

    public Camera playerCamera;

   

    private void Awake()
    {
        Instance = this;
        
    }

    void Start()
    {
        //LoadoutManager = Load_out_manager.Instance;
        if(!TryGetComponent(out cameraControlScript))
        {
            Debug.LogWarning("Weapond_System Notfound cameraControlScript ");
        }

        Player_Primary_weapon = Load_out_manager.Instance.selectedPrimaryWeapon;
        Player_Secondary_weapon = Load_out_manager.Instance.selectedSecondaryWeapon;

        

        if (Player_Primary_weapon != null && Player_Secondary_weapon != null)
        {
            

            currentWeapon = Player_Primary_weapon;

            // ดึงกระสุนจากสเปคปืนมาเก็บไว้ตอนเริ่มเกม ทั้งในแม็กและกระสุนสำรอง
            primary_CurrentAmmo = Player_Primary_weapon.Max_Ammo;
            primary_ReserveAmmo = Player_Primary_weapon.Max_Reserve_Ammo;

            secondary_CurrentAmmo = Player_Secondary_weapon.Max_Ammo;
            secondary_ReserveAmmo = Player_Secondary_weapon.Max_Reserve_Ammo;

        }
        else
        {
            Player_Primary_weapon = null;
            Player_Secondary_weapon = null;
        }

        EquipPrimary();
    }

    private void Update()
    {
        // ดักไว้เลยตอนแรก: ถ้ากำลังแบกศพอยู่ ห้ามใช้อาวุธเด็ดขาด
        if (Player.Instance.currentState == Player.PlayerState.CarryingBody)
        {
            // (ทางเลือกเสริม) คุณอาจจะสั่งซ่อนโมเดลปืนตรงนี้ก็ได้
            // weaponModel.SetActive(false); 

            // ถ้ากำลังรีโหลดค้างอยู่ตอนที่เผลอไปยกศพ ให้ยกเลิกการรีโหลดนั้นทิ้งซะ!
            if (current_Weapon_Status == Weapon_Status.reload)
            {
                Cancel_Reload();
            }

            return; // เตะออกจากฟังก์ชัน Update ทันที โค้ดยิง/เล็ง ด้านล่างจะไม่ทำงาน
        }

        if (currentWeapon.fireMode == Weapon_Item.FireMode.Semi)
        {
            current_Weapon_FireMode = CurrentFireMode.Semi_Auto;
        }

        recoilUp = currentWeapon.RecoilUp;
        recoilSide = currentWeapon.RecoilSide;

        HandleReload();
    }

    public void HandleShooting(bool isHolding, bool isClicking)
    {
        if (Player.Instance.currentState != Player.PlayerState.Aim)
        {
            return;
        }

        if (current_Weapon_Status == Weapon_Status.reload )
        {
            if (CurrentAmmo > 0 && (isClicking || isHolding))
            {
                Cancel_Reload();
            }
            else
            {
                return;
            }
        }

        if (Time.time < nextTimeToFire) return; //ดักการยิงรัวทั้ง Semi และ Full Auto

        if (current_Weapon_FireMode == CurrentFireMode.full_Auto && isHolding)
        {
            nextTimeToFire = Time.time + currentWeapon.FireRate;
            CreateGunshotNoise();
            Shoot();
        }
        else if (current_Weapon_FireMode == CurrentFireMode.Semi_Auto && isClicking)
        {
            
            nextTimeToFire = Time.time + currentWeapon.FireRate;
            CreateGunshotNoise();
            Shoot();
        }
    }

    public void Shoot()
    {
        if (CurrentAmmo <= 0)
        {
            Debug.Log("กระสุนหมดแม็ก! ต้องรีโหลด!");
            Start_Reload();
            return;
        }

        CurrentAmmo--;
        //nextTimeToFire = Time.time + currentWeapon.FireRate;

        if (cameraControlScript != null) //สั่ง camera controll ให้ตัวRecoilทำงาน
        {
            cameraControlScript.ApplyRecoil(recoilUp, recoilSide);
        }

        Ray cameraRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;
        LayerMask CombineMask = Target_mask | Obtacle_mask;

        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, currentWeapon.weaponRange, CombineMask))
        {
            targetPoint = cameraHit.point;
        }
        else
        {
            targetPoint = cameraRay.GetPoint(currentWeapon.weaponRange);
        }

        Vector3 bulletDirection = targetPoint - currentFirePoint.position;

        if (Physics.Raycast(currentFirePoint.position, bulletDirection.normalized, out RaycastHit weaponHit, currentWeapon.weaponRange, CombineMask))
        {
            Debug.DrawLine(currentFirePoint.position, weaponHit.point, Color.red, 2f);
            CreateGunshotNoise();

            if (weaponHit.collider.TryGetComponent<Enemy_Hitbox>(out Enemy_Hitbox enemy))
            {
                DoDamage(weaponHit);
            }
        }
        else
        {
            Debug.DrawRay(currentFirePoint.position, bulletDirection.normalized * currentWeapon.weaponRange, Color.yellow, 2f);
            CreateGunshotNoise();
        }
    }

    public void EquipPrimary()
    {
        Cancel_Reload();

        if (Player_Primary_weapon != null)
        {
            currentWeapon = Player_Primary_weapon;
            SpawnWeaponModel(currentWeapon.itemPrefab);
        }
    }

    public void EquipSecondary()
    {
        Cancel_Reload();

        if (Player_Secondary_weapon != null)
        {
            currentWeapon = Player_Secondary_weapon;
            SpawnWeaponModel(currentWeapon.itemPrefab);
        }
    }

    public void DoDamage(RaycastHit targetHit)
    {
        // 1. เช็กก่อนว่ากระสุนชนกล่อง Hitbox (หัว/ตัว) ที่เราแยกชิ้นไว้หรือเปล่า
        if (targetHit.collider.TryGetComponent<Enemy_Hitbox>(out Enemy_Hitbox hitbox))
        {
            // ถ้าโดน ให้ส่งดาเมจของปืนกระบอกนี้ไปให้กล่อง Hitbox จัดการคูณดาเมจ
            hitbox.OnHit(currentWeapon.weaponDamage);
        }
        // 2. ถ้าไม่มี Hitbox (เผื่อยิงโดนศัตรูตัวเก่าๆ ที่ยังไม่ได้ใส่ Hitbox แยกชิ้น) ให้ทำดาเมจปกติ
        else if (targetHit.collider.TryGetComponent<Enemy>(out Enemy TargetEnemy))
        {
            TargetEnemy.TakeDamage(currentWeapon.weaponDamage);
        }
    }

    void SpawnWeaponModel(GameObject prefabToSpawn)
    {
        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
        }

        if (prefabToSpawn != null)
        {
            currentWeaponModel = Instantiate(prefabToSpawn, weaponMount.position, weaponMount.rotation, weaponMount);
            currentWeaponModel.transform.localScale = new Vector3(5f, 5f, 5f);

            Transform foundFirePoint = currentWeaponModel.transform.Find("FirePoint");

            if (foundFirePoint != null)
            {
                currentFirePoint = foundFirePoint;
            }
        }
    }

    public void Switch_FireMode()
    {
        if (currentWeapon.fireMode == Weapon_Item.FireMode.Select_Fire_Weapon)
        {
            if (current_Weapon_FireMode == CurrentFireMode.Semi_Auto)
            {
                current_Weapon_FireMode = CurrentFireMode.full_Auto;
            }
            else
            {
                current_Weapon_FireMode = CurrentFireMode.Semi_Auto;
            }
        }
        else
        {
            current_Weapon_FireMode = CurrentFireMode.Semi_Auto;
        }
    }

    public void Start_Reload()
    {
        if (currentWeapon != null && current_Weapon_Status == Weapon_Status.ready)
        {
            // เช็คว่า: กระสุนในแม็กยังไม่เต็ม และ มีกระสุนสำรองเหลืออยู่!
            if (CurrentAmmo < currentWeapon.Max_Ammo && CurrentReserveAmmo > 0)
            {
                current_Weapon_Status = Weapon_Status.reload;
                nextTimeToReload = Time.time + currentWeapon.ReloadTime;
                Debug.Log($"กำลังรีโหลด... (กระสุนสำรองเหลือ: {CurrentReserveAmmo})");
            }
            else if (CurrentReserveAmmo <= 0 && CurrentAmmo < currentWeapon.Max_Ammo)
            {
                Debug.Log("กระสุนสำรองหมดเกลี้ยง! รีโหลดไม่ได้แล้ว!");
            }
        }
    }

    public void HandleReload()
    {
        if (currentWeapon != null && current_Weapon_Status == Weapon_Status.reload)
        {
            if (Time.time >= nextTimeToReload)
            {
                // คำนวณว่าแม็กกาซีนพร่องไปกี่นัด
                int ammoNeeded = currentWeapon.Max_Ammo - CurrentAmmo;

                if (CurrentReserveAmmo >= ammoNeeded)
                {
                    // กรณีที่ 1: มีกระสุนสำรองเหลือเฟือ ให้เติมเต็มแม็กไปเลย
                    CurrentAmmo += ammoNeeded;
                    CurrentReserveAmmo -= ammoNeeded;
                }
                else
                {
                    // กรณีที่ 2: กระสุนสำรองเหลือน้อยกว่าที่ขาดไป ให้เทกระสุนที่มีทั้งหมดลงแม็ก
                    CurrentAmmo += CurrentReserveAmmo;
                    CurrentReserveAmmo = 0;
                }

                current_Weapon_Status = Weapon_Status.ready;
                Debug.Log($"รีโหลดเสร็จ! ตอนนี้มีกระสุน: {CurrentAmmo}/{currentWeapon.Max_Ammo} (สำรอง: {CurrentReserveAmmo})");
            }
        }
    }

    public void Cancel_Reload()
    {
        if (current_Weapon_Status == Weapon_Status.reload)
        {
            current_Weapon_Status = Weapon_Status.ready;
            Debug.Log("ยกเลิกการรีโหลดฉุกเฉิน!");
        }
    }

    public void CreateGunshotNoise()
    {
        // 1. กำหนดจุดกำเนิดเสียง (ตำแหน่งผู้เล่น หรือ ปลายกระบอกปืน)
        Vector3 soundOrigin = transform.position;

        // 2. เช็กว่าปืนใส่ที่เก็บเสียงหรือไม่ เพื่อกำหนดความกว้างของรัศมี
        float currentNoiseRadius = currentWeapon.noiseLevel;

        // 3. กางวงกลมหาศัตรูในระยะ (ใช้ LayerMask ของศัตรู เพื่อความรวดเร็วในการประมวลผล)
        int enemyLayer = LayerMask.GetMask("enemy");
        Collider[] enemiesInHearingRange = Physics.OverlapSphere(soundOrigin, currentNoiseRadius, enemyLayer);

        // 4. ส่งสัญญาณเตือน AI ทุกตัวที่อยู่ในระยะ
        foreach (Collider hitCollider in enemiesInHearingRange)
        {
            if (hitCollider.TryGetComponent<enemy_stage>(out enemy_stage enemyAI))
            {
                // ดักความปลอดภัย: ถ้าเป็นศพหรือสลบอยู่ ให้ข้ามไปเลย! (ศพจะได้ไม่เด้งตื่นเพราะเสียงปืน)
                if (enemyAI.currentState == enemy_stage.EnemyState.dead ||
                    enemyAI.currentState == enemy_stage.EnemyState.faint)
                {
                    continue;
                }

                if (hitCollider.TryGetComponent<Enemy_Alert>(out Enemy_Alert enemyAlert))
                {
                    // --- แยกการตอบสนองตามประเภทปืน ---
                    if (currentWeapon.isSuppressed)
                    {
                        // กรณีปืนเก็บเสียง: ถ้ายังไม่รู้ตัว ให้เปลี่ยนแค่สถานะสงสัย (Investigate)
                        if (enemyAI.currentState != enemy_stage.EnemyState.Alert)
                        {
                            enemyAI.currentState = enemy_stage.EnemyState.Investigate;
                        }
                        enemyAlert.HandleNoiseAlert(soundOrigin);
                    }
                    else
                    {
                        // กรณีปืนเสียงดังลั่น: รู้เลยว่าโดนบุก! บังคับเข้าโหมด Alert ทันที
                        enemyAI.currentState = enemy_stage.EnemyState.Alert;
                        enemyAlert.HandleNoiseAlert(soundOrigin);

                        // สำคัญ: สั่งให้มันตะโกนปลุกเพื่อนรอบๆ ให้ตื่นตัวตามไปด้วย!
                        enemyAlert.Start_TriggerGroupAlert();
                    }
                }
            }
        }
    }
}