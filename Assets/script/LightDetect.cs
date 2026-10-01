using TMPro;
using UnityEngine;

public class LightDetect : MonoBehaviour
{
    public static LightDetect Instance { get; private set; }

    [Header("UI & Output")]
    public float light_meter = 0;
    public TextMeshProUGUI UI;

    [Header("Detection Settings")]
    public float radarRadius = 15f;          // ระยะกวาดหาไฟรอบตัว
    public LayerMask lightLayer;             // เลเยอร์ของหลอดไฟ
    public LayerMask obstacleLayer;          // เลเยอร์ของกำแพง/สิ่งกีดขวาง
    public Vector3 raycastOffset = new Vector3(0, 1f, 0); // จุดยิง Raycast (เช่น ยิงจากระดับอกผู้เล่น)

    [Header("Performance Settings")]
    public float scanInterval = 0.1f;        // 0.1 วินาที = ทำงาน 10 ครั้ง/วินาที
    public float uiSmoothSpeed = 5f;         // ความเร็วในการเกลี่ยตัวเลข UI

    private Collider[] lightsInRange = new Collider[20];
    private float timer = 0f;
    private float targetBrightness = 0f;     // ค่าความสว่างดิบที่ได้จากการคำนวณ
    private float currentBrightness = 0f;    // ค่าความสว่างที่กำลังถูก Lerp ไปหาเป้าหมาย

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // 1. ระบบหน่วงเวลา (Timer) ไม่ให้สแกนทุกเฟรม
        timer += Time.deltaTime;
        if (timer >= scanInterval)
        {
            CalculateLight();
            timer = 0f;
        }

        // 2. ระบบ Lerp เกลี่ยค่าแสงให้ UI ค่อยๆ ไหลขึ้นลงอย่างนุ่มนวล
        currentBrightness = Mathf.Lerp(currentBrightness, targetBrightness, Time.deltaTime * uiSmoothSpeed);

        // แปลงเป็น 0-100 แบบที่คุณทำไว้
        light_meter = Mathf.RoundToInt(currentBrightness * 100f);

        ui_Update();
    }

    void ui_Update()
    {
        if (UI != null)
        {
            UI.text = "(..!..) " + light_meter.ToString("f1");
        }
    }

    void CalculateLight()
    {
        float maxCalculatedBrightness = 0f;
        Vector3 rayOrigin = transform.position + raycastOffset;

        // สแกนหาไฟรอบตัวด้วย OverlapSphereNonAlloc
        int lightCount = Physics.OverlapSphereNonAlloc(transform.position, radarRadius, lightsInRange, lightLayer);

        for (int i = 0; i < lightCount; i++)
        {
            Collider lightCollider = lightsInRange[i];

            // --- อัปเกรด 1: ดึง Component จากทั้งตัวแม่และตัวลูก ป้องกันหาไม่เจอ ---
            LightZone lightZoneHit = lightCollider.GetComponentInParent<LightZone>() ?? lightCollider.GetComponentInChildren<LightZone>();
            Light myLight = lightCollider.GetComponentInParent<Light>() ?? lightCollider.GetComponentInChildren<Light>();

            if (lightZoneHit != null && lightZoneHit.lightZoneState && myLight != null)
            {
                // --- อัปเกรด 2: กฎครึ่งมุม สำหรับ Spot Light ---
                if (myLight.type == LightType.Spot)
                {
                    Vector3 dirToPlayerFromLight = (rayOrigin - myLight.transform.position).normalized;
                    if (Vector3.Angle(myLight.transform.forward, dirToPlayerFromLight) > myLight.spotAngle / 2f)
                    {
                        continue; // ข้ามไฟดวงนี้ไปเลยถ้าอยู่หลังกระบอกไฟ
                    }
                }

                // --- อัปเกรด 3: วัดระยะทางแบบ 3 มิติ (คำนวณความสูงด้วย) ---
                Vector3 lightPos = myLight.transform.position;
                Vector3 directionToLight = lightPos - rayOrigin;
                float distanceToLightReal = directionToLight.magnitude;

                // --- อัปเกรด 4: ยิง Raycast เช็คกำแพงแบบมี hitInfo ออกมา ---
                if (Physics.Raycast(rayOrigin, directionToLight, out RaycastHit hitInfo, distanceToLightReal, obstacleLayer))
                {
                    // ถ้ายิงติดกำแพง จะปริ้นตัวหนังสือสีแดงประจานชื่อวัตถุที่บัง
                    //Debug.Log("<color=red>ยิงเรย์แคสต์ไม่ถึงไฟดวง " + myLight.gameObject.name + " เพราะไปชน: " + hitInfo.collider.gameObject.name + "</color>");
                }
                else
                {
                    // นำระยะ XZ (แนวราบ) กลับมาใช้เฉพาะตอนคำนวณความสว่าง
                    Vector3 playerPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
                    Vector3 lightPosXZ = new Vector3(lightPos.x, 0, lightPos.z);
                    float distanceXZ = Vector3.Distance(playerPosXZ, lightPosXZ);

                    float maxRadius = myLight.range;
                    float coreRadius = maxRadius * 0.3f;
                    float thisLightBrightness = 0f;

                    // ใช้ระยะแนวราบ (distanceXZ) มาเช็คเข้าสมการแทนระยะ 3D
                    if (distanceXZ <= coreRadius)
                    {
                        thisLightBrightness = 1f;
                    }
                    else
                    {
                        // ถ้าอยู่ในขอบแสง จะค่อยๆ หรี่ลงตามระยะแนวราบ
                        thisLightBrightness = Mathf.InverseLerp(maxRadius, coreRadius, distanceXZ);
                    }

                    maxCalculatedBrightness = Mathf.Max(maxCalculatedBrightness, thisLightBrightness);
                }
            }
        }

        // เก็บค่าที่คำนวณเสร็จแล้วไว้เป็น "เป้าหมาย" ให้ Lerp วิ่งตาม
        targetBrightness = maxCalculatedBrightness;
    }
}