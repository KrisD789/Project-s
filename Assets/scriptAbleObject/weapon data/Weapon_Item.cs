using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_Item", menuName = "Scriptable Objects/Weapon_Item")]
public class Weapon_Item : Base_Item
{
    [Header("Weapon Stats")]
    public int weaponDamage;
    public int Max_Ammo;
    public int Current_Ammo;
    public int Max_Reserve_Ammo;
    public float weaponRange;
    public float ReloadTime;
    public float FireRate;

    public enum WeaponType { Primary, Secondary }
    public WeaponType type;

    public enum FireMode { Semi, Select_Fire_Weapon }
    public FireMode fireMode;

    [Header("Stealth Mechanics")]
    public float noiseLevel;
    public float lightConcealment;
}
