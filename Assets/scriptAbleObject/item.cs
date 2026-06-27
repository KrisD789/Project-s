using UnityEngine;

[CreateAssetMenu(fileName = "item", menuName = "Scriptable Objects/item")]
public class item : ScriptableObject
{
    [Header("Item Info")]
    public Sprite itemIcon;
    public string itemName;
    public string description;
    public int weaponDamage;
    public int Max_Ammo;
    public int Current_Ammo;
    public float weaponRange;
    public float ReloadTime;
    public float FireRate;
    
    
    
    public enum WeaponType {Primary, Secondary}
    public WeaponType type;

    public enum FireMode { Semi, Select_Fire_Weapon }
    public FireMode fireMode;

    [Header("Stealth Mechanics")]
    public float noiseLevel; // ความดังเวลาใช้งาน
    public float lightConcealment; // เปอร์เซ็นต์การพรางตัวจากแสง

    [Header("In-Game Model")]
    public GameObject itemPrefab; // โมเดล 3D ที่จะโยนเข้าไปในฉาก
}
