using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class LightDetect : MonoBehaviour
{
    //public static LightDetect lightDetect { get; private set; }

    public float light_meter = 0;
    float brightness = 0;
    //public bool lightDetect = false;

    public TextMeshProUGUI UI;
    //public TextMeshProUGUI UI2;

    //public enemy enemy_lightMeter;
    private LightZone lightZoneHit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        //lightDetect = this;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ui_Update();

        //if (lightDetect == false) stealthBar -= 1;

        //if(stealthBar < 0) stealthBar = 0;

        //if (lightDetect)
        //{
          //  stealthBar += 0.1f;

        //}
    }

   
    void ui_Update() 
    {
        
        UI.text = "(..!..) " + light_meter.ToString("f1");
        //UI2.text = "(Enemy light_Meter :) " + enemy_lightMeter.E_lightMeter.ToString("f1");

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Light"))
        {
            lightZoneHit = other.GetComponent<LightZone>();

            if (lightZoneHit != null && lightZoneHit.lightZoneState)
            {
                // 1. คำนวณระยะห่างเฉพาะแนวราบ (XZ) เพื่อความแม่นยำ 100%
                Vector3 playerPos = new Vector3(transform.position.x, 0, transform.position.z);
                Vector3 lightPos = new Vector3(other.transform.position.x, 0, other.transform.position.z);
                float distance = Vector3.Distance(playerPos, lightPos);

                float maxRadius;
                // 2. ดึงรัศมีตามแบบที่นายพิสูจน์แล้วว่า Smooth
                if (!other.TryGetComponent<SphereCollider>(out SphereCollider sphere))
                {
                    maxRadius = Mathf.Max(other.bounds.extents.x, other.bounds.extents.z);
                }
                else
                {
                    maxRadius = other.bounds.extents.x;
                }

                // 3. กำหนดพื้นที่สว่างสูงสุด (Core Radius)
                // เช่น 30% ของรัศมีทั้งหมดให้เป็น 100% เสมอ
                float coreRadius = maxRadius * 0.3f;

                if (distance <= coreRadius)
                {
                    // ถ้าอยู่ในเขต Core ให้สว่างเต็มทันที
                    brightness = 1f;
                }
                else
                {
                    // 4. ส่วนที่สำคัญที่สุด: ค่อยๆ ไล่จาก 0 (ที่ขอบ maxRadius) ไปหา 1 (ที่ขอบ coreRadius)
                    // วิธีนี้จะทำให้มันค่อยๆ เพิ่มจาก 0 แบบที่นายชอบ และเต็ม 100 ก่อนถึงจุดศูนย์กลาง
                    brightness = Mathf.InverseLerp(maxRadius, coreRadius, distance);
                }

                light_meter = Mathf.RoundToInt(brightness * 100f);
            }
            else light_meter = 0f;
        }
    }
}
