using UnityEngine;

[CreateAssetMenu(fileName = "item", menuName = "Scriptable Objects/item")]
public class item : ScriptableObject
{
    [Header("Item Info")]
    public Sprite itemIcon;
    public string itemName;
    public string description;
    public int weaponDamage;
    public int weaponAmmo;
    public float ReloadSpeed;
    public enum WeaponType {Primary, Secondary}
    public WeaponType type;
    

    [Header("Stealth Mechanics")]
    public float noiseLevel; // ความดังเวลาใช้งาน
    public float lightConcealment; // เปอร์เซ็นต์การพรางตัวจากแสง

    [Header("In-Game Model")]
    public GameObject itemPrefab; // โมเดล 3D ที่จะโยนเข้าไปในฉาก
}
