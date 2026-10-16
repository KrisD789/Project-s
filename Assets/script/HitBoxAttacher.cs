using UnityEngine;

public class HitBoxAttacher : MonoBehaviour
{
    [Header("Hitbox Component")]
    [Tooltip("ลาก GameObject ของ Hitbox ส่วนหัวมาใส่ที่นี่")]
    public Transform headHitbox;

    [Header("Target Bone")]
    [Tooltip("ลากกระดูกส่วนหัว (เช่น spine.006) มาใส่ที่นี่")]
    public Transform headBone;

    void Awake()
    {
        // ย้าย Hitbox ไปเป็นลูกของกระดูกส่วนหัว พร้อมรีเซ็ตตำแหน่ง
        if (headHitbox != null && headBone != null)
        {
            headHitbox.SetParent(headBone);
            headHitbox.localPosition = Vector3.zero; // ให้ตำแหน่งตรงกับจุดศูนย์กลางของกระดูกเป๊ะๆ
            headHitbox.localRotation = Quaternion.identity; // รีเซ็ตการหมุนให้ตรงกับกระดูก
        }
        else
        {
            Debug.LogWarning("HitboxAttacher: ลืมใส่ Head Hitbox หรือ Head Bone ใน Inspector หรือเปล่า?");
        }
    }
}
