using UnityEngine;
using static enemy_stage;

public class Player_raycast : MonoBehaviour
{
    [Header("การตั้งค่าการยิง")]
    public Camera playerCamera; // ลาก Main Camera มาใส่ (จุดศูนย์กลางสายตา)
    public float RaycastRange = 100f; // ระยะยิงสูงสุดของปืน (ถ้ายืนไกลกว่านี้ ยิงไม่โดน)

    public LayerMask TargetMask;
    public LayerMask ObtacleMask;

    Weapon_system weapon;
    Weapon_raycast weapon_Raycast;

    private void Start()
    {
        weapon = GetComponent<Weapon_system>();
        weapon_Raycast = GetComponent<Weapon_raycast>();
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
                weapon_Raycast.weaponRaycast_Shoot();
            }

            
        }
        
    }
}
