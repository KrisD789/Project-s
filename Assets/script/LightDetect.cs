using TMPro;
using UnityEngine;

public class LightDetect : MonoBehaviour
{
    public static LightDetect Instance { get; private set; }

    [Header("UI & Output")]
    public float light_meter = 0;
    //public TextMeshProUGUI UI;

    [Header("Detection Settings")]
    public float radarRadius = 15f;
    public LayerMask lightLayer;
    public LayerMask obstacleLayer;
    public Vector3 raycastOffset = new Vector3(0, 1f, 0);

    [Header("Performance Settings")]
    public float scanInterval = 0.1f;
    public float uiSmoothSpeed = 5f;

    // เพิ่ม AnimationCurve ตรงนี้
    [Header("Light Falloff Curve")]
    [Tooltip("แกน X: ระยะทาง (0=กลางไฟ, 1=ขอบไฟสุด) | แกน Y: ความสว่าง (0=มืดสนิท, 1=สว่างสุด)")]
    public AnimationCurve lightFalloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private Collider[] lightsInRange = new Collider[20];
    private float timer = 0f;
    private float targetBrightness = 0f;
    public float currentBrightness = 0f;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= scanInterval)
        {
            CalculateLight();
            timer = 0f;
        }

        currentBrightness = Mathf.Lerp(currentBrightness, targetBrightness, Time.deltaTime * uiSmoothSpeed);
        light_meter = Mathf.RoundToInt(currentBrightness * 100f);

        //ui_Update();
    }

    void ui_Update()
    {
        //if (UI != null)
        //{
            //UI.text = "(..!..) " + light_meter.ToString("f1");
        //}
    }

    void CalculateLight()
    {
        float maxCalculatedBrightness = 0f;

        // 1. ดึงค่าจุดกำเนิดแสงปกติตอนยืน
        Vector3 currentOffset = raycastOffset;

        // 2. ดักเช็กสถานะ ถ้าผู้เล่นกำลังนั่งยอง ให้โหลดจุดรับแสงต่ำลงมา
        if (Player.Instance != null && Player.Instance.currentMovementState == Player.MovementState.Crouch)
        {
            // ปรับระดับการยิงเลเซอร์ตอนนั่ง (ลองปรับค่า 0.4f ดูถ้ามันยังสูงหรือต่ำไป)
            currentOffset = new Vector3(0, 0f, 0);
        }

        Vector3 rayOrigin = transform.position + currentOffset;

        int lightCount = Physics.OverlapSphereNonAlloc(transform.position, radarRadius, lightsInRange, lightLayer);

        for (int i = 0; i < lightCount; i++)
        {
            Collider lightCollider = lightsInRange[i];

            LightZone lightZoneHit = lightCollider.GetComponentInParent<LightZone>() ?? lightCollider.GetComponentInChildren<LightZone>();
            Light myLight = lightCollider.GetComponentInParent<Light>() ?? lightCollider.GetComponentInChildren<Light>();

            if (lightZoneHit != null && lightZoneHit.lightZoneState && myLight != null)
            {
                if (myLight.type == LightType.Spot)
                {
                    Vector3 dirToPlayerFromLight = (rayOrigin - myLight.transform.position).normalized;
                    if (Vector3.Angle(myLight.transform.forward, dirToPlayerFromLight) > myLight.spotAngle / 2f)
                    {
                        continue;
                    }
                }

                Vector3 lightPos = myLight.transform.position;
                Vector3 directionToLight = lightPos - rayOrigin;
                float distanceToLightReal = directionToLight.magnitude;

                //  ยิง Raycast เช็กที่กำบัง พร้อมวาดเส้น Debug
                if (Physics.Raycast(rayOrigin, directionToLight, out RaycastHit hitInfo, distanceToLightReal, obstacleLayer))
                {
                    // [โดนบัง] วาดเส้นสีแดงจากตัวผู้เล่นไปจนถึงจุดที่ชนกำแพง/กล่อง
                    Debug.DrawLine(rayOrigin, hitInfo.point, Color.red);
                }
                else
                {
                    // [ไม่โดนบัง] วาดเส้นสีเหลืองยาวไปถึงจุดศูนย์กลางดวงไฟ
                    Debug.DrawLine(rayOrigin, lightPos, Color.yellow);

                    Vector3 playerPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
                    Vector3 lightPosXZ = new Vector3(lightPos.x, 0, lightPos.z);
                    float distanceXZ = Vector3.Distance(playerPosXZ, lightPosXZ);

                    float maxRadius = myLight.range;
                    float normalizedDistance = Mathf.Clamp01(distanceXZ / maxRadius);
                    float thisLightBrightness = lightFalloff.Evaluate(normalizedDistance);

                    maxCalculatedBrightness = Mathf.Max(maxCalculatedBrightness, thisLightBrightness);
                }
            }
        }

        targetBrightness = maxCalculatedBrightness;
    }
}