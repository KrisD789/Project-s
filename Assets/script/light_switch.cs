using System.Collections.Generic;
using UnityEngine;

public class light_switch : MonoBehaviour
{
    [Header("Assign LightZones Here")]
    [Tooltip("ลากเฉพาะตัว LightZone ใส่ที่นี่ หลอดไฟข้างในจะถูกดึงมาผูกให้อัตโนมัติ")]
    public List<LightZone> lightZones = new List<LightZone>();

    [Header("Switch Status")]
    public bool lightSW_Status = true;

    // โครงสร้างจับคู่ LightZone กับ Light เข้าด้วยกัน
    private struct LightPair
    {
        public LightZone zone;
        public Light light;
    }

    private List<LightPair> cachedLightPairs = new List<LightPair>();

    private void Awake()
    {
        InitializeLights();
    }

    private void Start()
    {
        ApplyLightState();

        
    }

    /// <summary>
    /// ทำการค้นหาและจับคู่ Light กับ LightZone ครั้งเดียวตอนโหลดฉาก
    /// </summary>
    private void InitializeLights()
    {
        cachedLightPairs.Clear();

        if (lightZones == null) return;

        foreach (LightZone zone in lightZones)
        {
            if (zone == null) continue;

            // --- ยัดเยียดตัวเอง (light_switch) ใส่ให้ LightZone ตรงนี้เลย ---
            zone.masterSwitch = this;

            Light targetLight = null;
            if (!zone.TryGetComponent<Light>(out targetLight))
            {
                targetLight = zone.GetComponentInChildren<Light>(true);

                print("GetComponentInChildren<Light> Success !!!");
            }

            else { print("!!!Error!!!! GetComponentInChildren<Light> NotFound !!!"); }

            cachedLightPairs.Add(new LightPair
            {
                zone = zone,
                light = targetLight
            });
        }
    }

    public void Turn()
    {
        lightSW_Status = !lightSW_Status;
        ApplyLightState();
        print(lightSW_Status ? "Turn-ON" : "Turn-OFF");
    }

    public void ApplyLightState()
    {
        foreach (var pair in cachedLightPairs)
        {
            if (pair.zone != null)
            {
                pair.zone.lightZoneState = lightSW_Status;
            }

            if (pair.light != null)
            {
                pair.light.enabled = lightSW_Status;
            }
        }
    }
}