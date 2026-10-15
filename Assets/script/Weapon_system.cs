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

    [Header("Fire Mode Tracking (จำโหมดการยิงแยกกระบอก)")]
    public CurrentFireMode primary_FireMode;
    public CurrentFireMode secondary_FireMode;

    public CameraControl cameraControlScript;

    private PlayerAnimator playerAnimator;

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
        if (!TryGetComponent(out cameraControlScript))
        {
            Debug.LogWarning("Weapon_System Notfound cameraControlScript");
        }

        //if (Player.Instance != null)
        //{
            // ถ้า PlayerAnimator อยู่ที่โมเดลลูก (Child) ให้ใช้ GetComponentInChildren
            //playerAnimator = Player.Instance.GetComponentInChildren<PlayerAnimator>();
        //}

        Player_Primary_weapon = Load_out_manager.Instance.selectedPrimaryWeapon;
        Player_Secondary_weapon = Load_out_manager.Instance.selectedSecondaryWeapon;

        // --- แยกเช็กปืนหลัก ---
        if (Player_Primary_weapon != null)
        {
            primary_CurrentAmmo = Player_Primary_weapon.Max_Ammo;
            primary_ReserveAmmo = Player_Primary_weapon.Max_Reserve_Ammo;
            primary_FireMode = (Player_Primary_weapon.fireMode == Weapon_Item.FireMode.Select_Fire_Weapon) ? CurrentFireMode.full_Auto : CurrentFireMode.Semi_Auto;

            // ให้ปืนหลักเป็นอาวุธเริ่มต้นไว้ก่อน (ถ้ามี)
            currentWeapon = Player_Primary_weapon;
        }

        // --- แยกเช็กปืนรอง ---
        if (Player_Secondary_weapon != null)
        {
            secondary_CurrentAmmo = Player_Secondary_weapon.Max_Ammo;
            secondary_ReserveAmmo = Player_Secondary_weapon.Max_Reserve_Ammo;
            secondary_FireMode = (Player_Secondary_weapon.fireMode == Weapon_Item.FireMode.Select_Fire_Weapon) ? CurrentFireMode.full_Auto : CurrentFireMode.Semi_Auto;

            // ถ้าเกิดไม่มีปืนหลัก ให้เอาปืนรองมาถือแทน
            if (currentWeapon == null) currentWeapon = Player_Secondary_weapon;
        }

        // ถ้ามีปืนหลักให้ควักปืนหลักก่อน ถ้าไม่มีให้ไปควักปืนรอง (กันบั๊กกรณีพกปืนกระบอกเดียว)
        if (Player_Primary_weapon != null)
            EquipPrimary();
        else if (Player_Secondary_weapon != null)
            EquipSecondary();

        // ++ เติมบรรทัดนี้เข้าไปท้ายสุดของฟังก์ชัน Start ++
        // บังคับเสกปืนเข้ามือตั้งแต่ตอนเริ่มเกม (เพราะยังไม่มี Animation Event มาสั่ง)
        if (currentWeapon != null)
        {
            SpawnWeaponModel(currentWeapon.itemPrefab);
        }
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

        //if (currentWeapon.fireMode == Weapon_Item.FireMode.Semi)
        //{
        //current_Weapon_FireMode = CurrentFireMode.Semi_Auto;
        //}

        if (currentWeapon == null) return;

        recoilUp = currentWeapon.RecoilUp;
        recoilSide = currentWeapon.RecoilSide;

        HandleReload();
    }

    public void HandleShooting(bool isHolding, bool isClicking)
    {
        if (currentWeapon == null) return;

        if (Player.Instance.currentState != Player.PlayerState.Aim)
        {
            // ++ 3. ปิดการยิงที่ Animator เมื่อผู้เล่นเลิกเล็ง ++
            if (PlayerAnimator.Instance != null) PlayerAnimator.Instance.SetFiring(false);
            
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

        if (PlayerAnimator.Instance != null)
        {
            if (current_Weapon_FireMode == CurrentFireMode.full_Auto)
            {
                PlayerAnimator.Instance.SetFiring(isHolding); // Auto: กดค้าง=ยิง, ปล่อย=หยุด
            }
            else if (current_Weapon_FireMode == CurrentFireMode.Semi_Auto)
            {
                if (isClicking) PlayerAnimator.Instance.SetFiring(true); // Single: คลิก 1 ครั้ง=ยิง
                else PlayerAnimator.Instance.SetFiring(false);           // บังคับปิดในเฟรมถัดไป
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

        if (cameraControlScript != null)
        {
            cameraControlScript.ApplyRecoil(recoilUp, recoilSide);
        }

        // 1. ยิง Raycast ออกจากกึ่งกลางหน้าจอ (กล้อง)
        Ray cameraRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        LayerMask CombineMask = Target_mask | Obtacle_mask;

        // 2. เช็กว่าเป้ากลางจอ เล็งโดนอะไรไหมในระยะปืน
        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, currentWeapon.weaponRange, CombineMask))
        {
            // ถ้าเป้าเล็งโดนอะไรบางอย่าง
            Debug.Log($"ยิงโดน: {cameraHit.collider.name}");

            // วาดเส้น Debug จากปลายปืน ไปหาจุดที่กล้องเล็งโดน (เพื่อความสวยงามเวลามอง)
            if (currentFirePoint != null)
            {
                Debug.DrawLine(currentFirePoint.position, cameraHit.point, Color.red, 2f);
            }

            // ทำดาเมจถ้าจุดที่เป้าเล็งชี้ไป เป็นศัตรู
            if (cameraHit.collider.TryGetComponent<Enemy_Hitbox>(out Enemy_Hitbox enemyHitbox))
            {
                DoDamage(cameraHit);
            }
            else if (cameraHit.collider.TryGetComponent<Enemy>(out Enemy targetEnemy))
            {
                DoDamage(cameraHit); // รองรับศัตรูแบบไม่มี Hitbox
            }
        }
        else
        {
            // ถ้าเป้าเล็งไม่โดนอะไรเลย (ยิงขึ้นฟ้า หรือยิงไกลกว่าระยะปืน)
            Vector3 farPoint = cameraRay.GetPoint(currentWeapon.weaponRange);

            if (currentFirePoint != null)
            {
                Debug.DrawLine(currentFirePoint.position, farPoint, Color.yellow, 2f);
            }
        }

        CreateGunshotNoise();
    }

    public void EquipPrimary()
    {
        Cancel_Reload();

        if (Player_Primary_weapon != null)
        {
            currentWeapon = Player_Primary_weapon;

            current_Weapon_FireMode = primary_FireMode;

            // ++ 5. บอก Animator ว่าถือปืนหลัก (WeaponType = 1) ++
            if (PlayerAnimator.Instance != null) PlayerAnimator.Instance.SetWeaponType(1);
            UpdateAnimatorFireMode();

            SpawnWeaponModel(currentWeapon.itemPrefab);
        }
    }

    public void EquipSecondary()
    {
        Cancel_Reload();

        if (Player_Secondary_weapon != null)
        {
            currentWeapon = Player_Secondary_weapon;

            current_Weapon_FireMode = secondary_FireMode;

            // ++ 6. บอก Animator ว่าถือปืนรอง (WeaponType = 2) ++
            if (PlayerAnimator.Instance != null) PlayerAnimator.Instance.SetWeaponType(2);
            UpdateAnimatorFireMode();

            SpawnWeaponModel(currentWeapon.itemPrefab);
        }
    }

    public void DoDamage(RaycastHit targetHit)
    {
        // 1. เช็กก่อนว่ากระสุนชนกล่อง Hitbox (หัว/ตัว) ที่เราแยกชิ้นไว้หรือเปล่า
        if (targetHit.collider.TryGetComponent<Enemy_Hitbox>(out Enemy_Hitbox hitbox))
        {
            // ถ้าโดน ให้ส่งดาเมจของปืนกระบอกนี้ไปให้กล่อง Hitbox จัดการคูณดาเมจ
            Debug.Log("ให้ส่งดาเมจของปืนกระบอกนี้ไปให้กล่อง Hitbox จัดการคูณดาเมจ");
            hitbox.OnHit(currentWeapon.weaponDamage);
        }
        // 2. ถ้าไม่มี Hitbox (เผื่อยิงโดนศัตรูตัวเก่าๆ ที่ยังไม่ได้ใส่ Hitbox แยกชิ้น) ให้ทำดาเมจปกติ
        else if (targetHit.collider.TryGetComponent<Enemy>(out Enemy TargetEnemy))
        {
            Debug.Log("หา HitBoxไม่เจอทำDamageแบบปกติ");
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
            currentWeaponModel.transform.localScale = new Vector3(1f, 1f, 1f);

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

            //  เพิ่มการจำค่าลงในปืนกระบอกที่กำลังถืออยู่ 
            if (currentWeapon == Player_Primary_weapon)
                primary_FireMode = current_Weapon_FireMode;
            else if (currentWeapon == Player_Secondary_weapon)
                secondary_FireMode = current_Weapon_FireMode;
        }
        else
        {
            current_Weapon_FireMode = CurrentFireMode.Semi_Auto; // ปืนทั่วไปบังคับ Semi
        }

        // ++ 7. อัปเดตโหมดการยิงที่ Animator ++
        UpdateAnimatorFireMode();
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

                // ++ 8. สั่ง Animator เล่นท่ารีโหลด ++
                if (PlayerAnimator.Instance != null) PlayerAnimator.Instance.TriggerReload();

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

        //  ส่งข้อมูลไปให้ UI 
        if (NoiseMeterUI.Instance != null)
        {
            NoiseMeterUI.Instance.RegisterGunshot(currentNoiseRadius);
        }

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
                    enemyAI.currentState == enemy_stage.EnemyState.faint ||
                    enemyAI.currentState == enemy_stage.EnemyState.Dummy)
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
                            Debug.Log("if (enemyAI.currentState != enemy_stage.EnemyState.Alert)");
                            enemyAI.currentState = enemy_stage.EnemyState.Investigate;

                            //  แก้ตรงนี้: ดึงสคริปต์ Investigate มาใช้งานแทน Alert 
                            if (hitCollider.TryGetComponent<Enemy_Investigate>(out Enemy_Investigate enemyInvestigate))
                            {
                                enemyInvestigate.searcingLastHearPosition(soundOrigin);
                            }
                        }
                       
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

    // ++ 9. ฟังก์ชันเสริมสำหรับอัปเดตโหมดปืนไปที่ Animator ++
    private void UpdateAnimatorFireMode()
    {
        if (PlayerAnimator.Instance != null)
        {
            bool isAuto = (current_Weapon_FireMode == CurrentFireMode.full_Auto);
            PlayerAnimator.Instance.SetFireMode(isAuto);
        }
    }

    // ++ 10. ฟังก์ชันให้ Animation Event เรียกตอนมือหยิบปืน ++
    public void AnimEvent_SpawnCurrentWeapon()
    {
        if (currentWeapon != null)
        {
            SpawnWeaponModel(currentWeapon.itemPrefab);
        }
    }

    // ++ 11. ฟังก์ชันให้ Animation Event เรียกตอนเก็บปืนเสร็จ ++
    public void AnimEvent_DestroyCurrentWeapon()
    {
        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
            currentWeaponModel = null;
        }
    }
}