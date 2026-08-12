using UnityEngine;

[CreateAssetMenu(fileName = "Armor", menuName = "Scriptable Objects/Armor")]
public class Armor_Item : Base_Item
{
    [Header("Armor Defense Stats")]
    [Tooltip("ความทนทานสูงสุดของเกราะ (Armor HP)")]
    public float Max_Armor_Durability;

    [Tooltip("เปอร์เซ็นต์การดูดซับดาเมจ (0.0 ถึง 1.0) เช่น 0.5 คือลดดาเมจ 50%")]
    [Range(0f, 1f)]
    public float Armor_Block_Percentage;

    [Header("Stealth & Movement Penalties")]
    [Tooltip("ตัวคูณความเร็วการเดิน (1.0 = เดินปกติ, 0.8 = เดินช้าลง 20% เพราะเกราะหนัก)")]
    public float Movement_Speed_Multiplier = 1f;

    [Tooltip("ตัวคูณเสียงฝีเท้า (1.0 = ปกติ, 1.5 = เสียงฝีเท้าดังขึ้น 50% ทำให้ศัตรูได้ยินง่ายขึ้น)")]
    public float Noise_Multiplier = 1f;
}
