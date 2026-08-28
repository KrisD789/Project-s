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
        if (currentWeapon.fireMode == Weapon_Item.FireMode.Semi)
        {
            current_Weapon_FireMode = CurrentFireMode.Semi_Auto;
        }

        HandleReload();
    }

    public void HandleShooting(bool isHolding, bool isClicking)
    {
        if (current_Weapon_Status == Weapon_Status.reload)
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

        if (Time.time < nextTimeToFire) return;

        if (current_Weapon_FireMode == CurrentFireMode.full_Auto && isHolding)
        {
            CreateGunshotNoise();
            Shoot();
        }
        else if (current_Weapon_FireMode == CurrentFireMode.Semi_Auto && isClicking)
        {
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
        nextTimeToFire = Time.time + currentWeapon.FireRate;

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

            if (weaponHit.collider.TryGetComponent<Enemy>(out Enemy enemy))
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
        if (targetHit.collider.TryGetComponent<Enemy>(out Enemy TargetEnemy))
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
        Vector3 soundOrigin = currentFirePoint.position;
        float noiseRadius = currentWeapon.noiseLevel;

        Collider[] enemiesInHearingRange = Physics.OverlapSphere(soundOrigin, noiseRadius, Target_mask);

        foreach (Collider hitCollider in enemiesInHearingRange)
        {
            if (hitCollider.TryGetComponent<enemy_stage>(out enemy_stage enemyAI))
            {
                float distanceToEnemy = Vector3.Distance(soundOrigin, hitCollider.transform.position);

                enemyAI.currentState = enemy_stage.EnemyState.Alert;

                if (hitCollider.TryGetComponent<Enemy_Alert>(out Enemy_Alert enemyAIAlert))
                {
                    enemyAIAlert.HandleNoiseAlert(transform.position);
                }
            }
        }
    }
}