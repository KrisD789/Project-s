using UnityEngine;
using static enemy_stage;

public class Player_raycast : MonoBehaviour
{
    [Header("การตั้งค่าการยิง")]
    public Camera playerCamera; // ลาก Main Camera มาใส่ (จุดศูนย์กลางสายตา)
    public float RaycastRange = 50f; // ระยะยิงสูงสุดของปืน (ถ้ายืนไกลกว่านี้ ยิงไม่โดน)

    public LayerMask TargetMask;
    public LayerMask ObtacleMask;

    Weapon_system weapon;

    private void Start()
    {
        weapon = GetComponent<Weapon_system>();
    }
    void Update()
    {
       
    }

    void ShootRaycast()
    {
        // 1. สร้างเส้นจำลอง (Ray) พุ่งออกจากตรงกลางหน้าจอกล้อง
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // 2. เตรียมกล่องเปล่าๆ ไว้รอรับผลลัพธ์ว่ายิงไปชนอะไร
        RaycastHit hit;
        LayerMask CombineMask = TargetMask | ObtacleMask; 

        
        if (Physics.Raycast(ray.origin, ray.direction, out hit, RaycastRange, CombineMask))
        {
            Debug.DrawRay(ray.origin, ray.direction * RaycastRange, Color.red, 2f);
            
            Debug.Log("ยิงไปโดน: " + hit.collider.name);

            // ใช้ TryGetComponent เช็คว่าสิ่งที่ชนมีสคริปต์ศัตรูไหม
            if (hit.collider.TryGetComponent<Enemy>(out Enemy enemy))
            {
                weapon.DoDamage(hit);
                // ถ้ามี โยน EnemyStats ตัวนี้ไปให้สคริปต์ระบบปืนจัดการต่อได้เลย!
                // weapon_system.DoDamage(enemy);
            }
        }
        else
        {
            // --- ถ้ายิงทะลุอากาศ ไม่โดนอะไรเลย จะตกมาปีกกานี้ (ไม่ต้องใส่ก็ได้) ---
            Debug.Log("ยิงวืด!");
        }
    }
}
