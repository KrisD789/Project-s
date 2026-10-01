using UnityEngine;

public class LightZone : MonoBehaviour
{
    public bool lightZoneState = true;
    public light_switch masterSwitch; // จะถูก light_switch ยัดค่ามาให้ตอน Awake()
    public float lightRadius;
    public float visualRadiusMultiplier = 0.3f;

    void Awake()
    {
        Light myLight = GetComponentInChildren<Light>();
        SphereCollider myCollider = GetComponent<SphereCollider>();

        if (myLight != null && myCollider != null)
        {
            // 1. ดึงค่ารัศมีแสงจริงมาเก็บไว้
            lightRadius = myLight.range * visualRadiusMultiplier;

            // 2. ปรับขนาด Collider ให้เท่ากับรัศมีแสงอัตโนมัติ
            myCollider.radius = lightRadius;
            myCollider.isTrigger = true;
        }
    }

    void Start()
    {
        // เช็คว่ามีสวิตช์คุมหรือไม่ (เผื่อกรณีเป็นหลอดไฟเปิดถาวรที่ไม่มีสวิตช์)
        if (masterSwitch == null)
        {
            Debug.Log($"{gameObject.name}: ไม่มีสวิตช์ควบคุม (หลอดไฟติดถาวร)");
        }
    }
}