using UnityEngine;

public class Weapon_raycast : MonoBehaviour
{
    private Transform Barrel_Point;
    public float Weapon_Range;

    public LayerMask Target_mask;
    public LayerMask Obtacle_mask;

    Weapon_system weapon_system;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        weapon_system = GetComponent<Weapon_system>();
        
    }

    // Update is called once per frame
    void Update()
    {
        Weapon_Range = weapon_system.currentWeapon.weaponRange;
        Barrel_Point = weapon_system.currentFirePoint;
    }

    public void weaponRaycast_Shoot()
    {
        Vector3 origin = weapon_system.currentFirePoint.position;
        Vector3 direction = Barrel_Point.forward;

        RaycastHit hit;
        LayerMask CombineMask = Target_mask | Obtacle_mask;

        if(Physics.Raycast(origin, direction, out hit, Weapon_Range, CombineMask))
        {
            Debug.Log("ยิงจากปลายปืนไปโดน: " + hit.collider.name);
            Debug.Log("จุดที่กระสุนกระทบ: " + hit.point);
            Debug.DrawRay(origin, direction * Weapon_Range, Color.red);

            if (hit.collider.TryGetComponent<Enemy>(out Enemy enemy))
            {
                weapon_system.DoDamage(hit);
            }

            else
            {
                Debug.Log("ยิงติดสิ่งกีดขวาง");
            }
        }
    }
}
