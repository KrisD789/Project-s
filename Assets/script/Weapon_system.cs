using UnityEngine;

public class Weapon_system : MonoBehaviour
{
    public static Weapon_system Instance { get; private set; }

    public Weapon_Item Player_Primary_weapon;
    public Weapon_Item Player_Secondary_weapon;

    public Weapon_Item currentWeapon;
    public Load_out_manager LoadoutManager;

    [Header("การเชื่อมต่อกับร่างกาย")]
    public Transform weaponMount; // ลาก WeaponMount ที่มือขวามาใส่ช่องนี้

    // สิ่งที่ซ่อนไว้ทำงานหลังบ้าน
    private GameObject currentWeaponModel; // โมเดล 3D ที่ถืออยู่ตอนนี้
    public Transform currentFirePoint;     // จุดยิงกระสุนของปืนปัจจุบัน

    Weapon_raycast weapon_Raycast;

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

        GameObject loadOutScript = GameObject.FindGameObjectWithTag("LoadoutManager");
        weapon_Raycast = GetComponent<Weapon_raycast>();

        if (loadOutScript != null)
        {
            LoadoutManager = loadOutScript.GetComponent<Load_out_manager>();
            Debug.Log("Found LoadOutManager_script");
        }
        else Debug.Log("LoadOutManager_script Not Found");
    }

    void Start()
    {


        if (Player_Primary_weapon != null && Player_Secondary_weapon != null)
        {
            Player_Primary_weapon = LoadoutManager.selectedPrimaryWeapon;
            Player_Secondary_weapon = LoadoutManager.selectedSecondaryWeapon;

            currentWeapon = Player_Primary_weapon;

            Player_Primary_weapon.Current_Ammo = Player_Primary_weapon.Max_Ammo;
            Player_Secondary_weapon.Current_Ammo = Player_Secondary_weapon.Max_Ammo;

            Debug.Log("เริ่มเกม: สวมใส่อาวุธ " + currentWeapon.itemName);
        }

        else 
        {
            Player_Primary_weapon = null;
            Player_Secondary_weapon = null;


            Debug.Log("ไม่พบอาวุธใน Loadout ของ Player");
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
            // ถ้ายังมีกระสุนเหลือในแม็ก และมีการกดปุ่มยิง ให้ยกเลิกรีโหลดเลย!
            if (currentWeapon.Current_Ammo > 0 && (isClicking || isHolding))
            {
                Cancel_Reload();
            }
            else
            {
                // แต่ถ้ากระสุนเกลี้ยงแม็ก 0 นัดจริงๆ ก็ต้องปล่อยให้มันรีโหลดต่อไป (ยิงไม่ได้)
                return;
            }
        }

        // 1. ถ้ายังไม่หมดเวลาคูลดาวน์ปืน ให้ข้ามการยิงไปเลย
        if (Time.time < nextTimeToFire) return;

        // 2. เช็คโหมด Full-Auto (กดค้างก็ยิงได้)
        if (current_Weapon_FireMode == CurrentFireMode.full_Auto && isHolding)
        {
            CreateGunshotNoise();
            Shoot();
        }
        // 3. เช็คโหมด Semi-Auto (ต้องเป็นการคลิกใหม่เท่านั้น)
        else if (current_Weapon_FireMode == CurrentFireMode.Semi_Auto && isClicking)
        {
            CreateGunshotNoise();
            Shoot();
        }
    }

    public void Shoot()
    {
        // 1. เช็คกระสุน (โค้ดเดิมของคุณ)
        if (currentWeapon.Current_Ammo <= 0)
        {
            Debug.Log("กระสุนหมด! ต้องรีโหลด!");
            Start_Reload();
            return;
        }

        currentWeapon.Current_Ammo--;
        nextTimeToFire = Time.time + currentWeapon.FireRate;

        // ------------------------------------------------------------------
        // ส่วนที่ 1: ลอจิกจาก Player_raycast (ดวงตา) หาว่าเป้าเล็งชี้ไปที่ไหน
        // ------------------------------------------------------------------
        // ใช้ ViewportPointToRay แบบที่คุณเขียนไว้เป๊ะๆ!
        Ray cameraRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;
        LayerMask CombineMask = Target_mask | Obtacle_mask;

        // ถ้ายิงเลเซอร์จากกล้องไปชนอะไรสักอย่าง ให้จำพิกัดจุดนั้นไว้
        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, currentWeapon.weaponRange, CombineMask))
        {
            targetPoint = cameraHit.point;
        }
        else
        {
            // ถ้าไม่ชนอะไรเลย (เช่น ชี้ขึ้นฟ้า) ให้เป้าหมายคือจุดที่ไกลที่สุดของระยะปืน
            targetPoint = cameraRay.GetPoint(currentWeapon.weaponRange);
        }

        // ------------------------------------------------------------------
        // ส่วนที่ 2: ลอจิกจาก Weapon_system (มือ) สั่งให้ปืนยิงไปหาเป้านั้น
        // ------------------------------------------------------------------
        // คำนวณทิศทาง: เอาพิกัดเป้าหมาย ลบด้วย พิกัดปลายกระบอกปืน
        Vector3 bulletDirection = targetPoint - currentFirePoint.position;

        // ยิง Raycast ของจริงจากปลายปืน พุ่งไปหาพิกัดที่กล้องมองเห็น!
        if (Physics.Raycast(currentFirePoint.position, bulletDirection.normalized, out RaycastHit weaponHit, currentWeapon.weaponRange, CombineMask))
        {
            Debug.Log("กระสุนพุ่งไปโดน: " + weaponHit.collider.name);

            // วาดเส้นสีแดงจากปลายปืนไปหาเป้า (โชว์ 2 วินาที)
            Debug.DrawLine(currentFirePoint.position, weaponHit.point, Color.red, 2f);

            // สร้างเสียงปืนเรียกศัตรู
            CreateGunshotNoise();

            // ทำดาเมจ
            if (weaponHit.collider.TryGetComponent<Enemy>(out Enemy enemy))
            {
                DoDamage(weaponHit);
            }
        }
        else
        {
            // กรณียิงวืด ไม่โดนอะไรเลย
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
            SpawnWeaponModel(currentWeapon.itemPrefab); // สั่งเสกโมเดลปืน!
        }
    }

    public void EquipSecondary()
    {
        Cancel_Reload();

        if (Player_Secondary_weapon != null)
        {
            currentWeapon = Player_Secondary_weapon;
            SpawnWeaponModel(currentWeapon.itemPrefab); // สั่งเสกโมเดลปืน!
        }
    }

    public void DoDamage(RaycastHit targetHit)
    {
        targetHit.collider.TryGetComponent<Enemy>(out Enemy TargetEnemy);

        TargetEnemy.TakeDamage(currentWeapon.weaponDamage);
    }

    void SpawnWeaponModel(GameObject prefabToSpawn)
    {
        // 1. ถ้ามีปืนเก่าถืออยู่ในมือ ให้ทำลายทิ้งก่อน
        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
        }

        // 2. ถ้ามีข้อมูล Prefab ปืนใหม่ ให้เสกออกมา
        if (prefabToSpawn != null)
        {
            // เสกปืนใหม่ ให้อยู่ที่ตำแหน่งและองศาของ weaponMount และให้มันเป็นลูก (Child) ของมือขวาเลย
            currentWeaponModel = Instantiate(prefabToSpawn, weaponMount.position, weaponMount.rotation, weaponMount);

            // บังคับสเกลให้กลับมาเป็น 1:1:1 
            currentWeaponModel.transform.localScale = new Vector3(5f, 5f, 5f);

            // 3. ค้นหาปลายกระบอกปืน (FirePoint) จากโมเดลที่เพิ่งเสก
            // **กฎสำคัญ:** ใน Prefab ปืนทุกกระบอกของคุณ ต้องมี GameObject ลูกที่ชื่อว่า "FirePoint" แปะอยู่ปลายปืนนะครับ!
            Transform foundFirePoint = currentWeaponModel.transform.Find("FirePoint");

            if (foundFirePoint != null)
            {
                currentFirePoint = foundFirePoint; // ส่งให้ระบบ Raycast เอาไปใช้ยิงต่อ
            }
            else
            {
                Debug.LogWarning("ลืมใส่ FirePoint ไว้ในโมเดลปืนหรือเปล่า?");
            }
        }
    }

    public void Switch_FireMode()
    {
        // เช็คก่อนว่าปืนกระบอกนี้อนุญาตให้สลับโหมดไหม?
        if (currentWeapon.fireMode == Weapon_Item.FireMode.Select_Fire_Weapon)
        {
            // ถ้าเป็น Semi อยู่ ให้เปลี่ยนเป็น Full
            if (current_Weapon_FireMode == CurrentFireMode.Semi_Auto)
            {
                current_Weapon_FireMode = CurrentFireMode.full_Auto;
                Debug.Log("เปลี่ยนโหมด: Full-Auto");
            }
            // ถ้าเป็น Full อยู่ ให้เปลี่ยนกลับเป็น Semi
            else
            {
                current_Weapon_FireMode = CurrentFireMode.Semi_Auto;
                Debug.Log("เปลี่ยนโหมด: Semi-Auto");
            }
        }
        else
        {
            Debug.Log("ปืนกระบอกนี้ล็อกโหมดไว้ เปลี่ยนไม่ได้!");
            current_Weapon_FireMode = CurrentFireMode.Semi_Auto;
        }
    }

    public void Start_Reload()
    {
        // ถ้ายิงได้ปกติ และ กระสุนในแม็กน้อยกว่ากระสุนสูงสุด ถึงจะยอมให้รีโหลด
        if (currentWeapon != null && current_Weapon_Status == Weapon_Status.ready)
        {
            if (currentWeapon.Current_Ammo < currentWeapon.Max_Ammo)
            {
                current_Weapon_Status = Weapon_Status.reload;

                //ตั้งเวลาเป้าหมายว่าจะรีโหลดเสร็จตอนไหน (เซ็ตแค่ครั้งเดียวตอนเริ่มกด)
                nextTimeToReload = Time.time + currentWeapon.ReloadTime;
                Debug.Log("กำลังรีโหลด...");
            }
        }
    }

    public void HandleReload()
    {
        // ถ้าระบบกำลังอยู่ในสถานะรีโหลด
        if (currentWeapon != null && current_Weapon_Status == Weapon_Status.reload)
        {
            //เช็คว่าเวลาปัจจุบัน เดินมาถึงเวลาเป้าหมายที่ตั้งไว้หรือยัง
            if (Time.time >= nextTimeToReload)
            {
                currentWeapon.Current_Ammo = currentWeapon.Max_Ammo; // เติมกระสุน
                current_Weapon_Status = Weapon_Status.ready; //  ปลดล็อกสถานะให้กลับมายิงได้
                Debug.Log("รีโหลดเสร็จสิ้น!");
            }
        }
    }

    public void Cancel_Reload()
    {
        // ถ้าระบบกำลังรีโหลดอยู่ ให้สั่งยกเลิกและกลับสู่สถานะพร้อมยิงทันที
        if (current_Weapon_Status == Weapon_Status.reload)
        {
            current_Weapon_Status = Weapon_Status.ready;
            Debug.Log("ยกเลิกการรีโหลดฉุกเฉิน!");
        }
    }

    public void CreateGunshotNoise()
    {
        // 1. กำหนดจุดกำเนิดเสียง (ปลายปืน) และ รัศมีความดัง (ดึงจากอาวุธปัจจุบัน)
        Vector3 soundOrigin = currentFirePoint.position;
        float noiseRadius = currentWeapon.noiseLevel;

        // 2. ใช้ Physics.OverlapSphere กางทรงกลมล่องหนออกไป
        // มันจะจับเฉพาะวัตถุที่อยู่ใน Target_mask (เลเยอร์ของศัตรู) เท่านั้น
        Collider[] enemiesInHearingRange = Physics.OverlapSphere(soundOrigin, noiseRadius, Target_mask);

        // 3. วนลูปแจ้งเตือนศัตรูทุกคนที่หูดีพอและอยู่ในระยะ
        foreach (Collider hitCollider in enemiesInHearingRange)
        {
            // เช็คว่าศัตรูตัวนั้นมีสคริปต์ Enemy แปะอยู่ไหม
            if (hitCollider.TryGetComponent<Enemy>(out Enemy enemyAI))
            {
                // คำนวณหาระยะห่างที่แท้จริงระหว่างปืนกับศัตรู (เอาไว้ให้ AI ประเมินความอันตรายได้)
                float distanceToEnemy = Vector3.Distance(soundOrigin, hitCollider.transform.position);

                //Debug.Log($" ศัตรูชื่อ {hitCollider.name} ได้ยินเสียงปืน! (ห่างออกไป {distanceToEnemy:F1} เมตร)");

                // ถ้าระบบ AI ของนายพร้อมแล้ว สามารถส่งพิกัดเสียงไปให้มันเดินมาตรวจดูได้เลย เช่น:
                // enemyAI.InvestigateSound(soundOrigin); 
            }
        }
    }
}
