using UnityEngine;

public class Weapon_system : MonoBehaviour
{
    public item Player_Primary_weapon;
    //public item Player_Secondary_weapon;

    public item currentWeapon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Player_Primary_weapon != null)
        {
            currentWeapon = Player_Primary_weapon;
            Debug.Log("เริ่มเกม: สวมใส่อาวุธ " + currentWeapon.itemName);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoDamage(RaycastHit targetHit)
    {
        targetHit.collider.TryGetComponent<Enemy>(out Enemy TargetEnemy);

        TargetEnemy.TakeDamage(currentWeapon.weaponDamage);
    }
}
