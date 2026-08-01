using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Gadget_Item", menuName = "Scriptable Objects/Gadget_Item")]
public abstract class Gadget_Item : Base_Item
{
    public float Cooldown;
    public int amount;
    //public MonoBehaviour Gadget_funtion;

    // ประกาศฟังก์ชันเปล่าๆ บังคับให้ Gadget ทุกชิ้น "ต้อง" มีระบบกดใช้
    public abstract void UseGadget(GameObject player);
}
