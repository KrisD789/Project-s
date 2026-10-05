using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class scanner : MonoBehaviour
{
    [Header("Settings")]
    public ScriptableRendererFeature xRayFeature; // ตัวเปิด X-Ray มองทะลุกำแพง
    public Volume scannerVolume;                  // Volume สำหรับปรับแสงสว่างในที่มืด

    private bool isScanning = false;
    private enemy_stage[] cachedEnemies;

    private void Start()
    {
        if (xRayFeature != null)
            xRayFeature.SetActive(false);

        if (scannerVolume != null)
            scannerVolume.enabled = false;

        isScanning = false;

        // แคชรายชื่อศัตรูไว้ตั้งแต่เริ่มฉาก เพื่อความลื่นไหล
        cachedEnemies = Object.FindObjectsByType<enemy_stage>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isScanning = !isScanning;
            ToggleScanner(isScanning);
        }
    }

    void ToggleScanner(bool active)
    {
        // 1. เปิด/ปิด การมองเห็นทะลุกำแพง (X-Ray Feature)
        if (xRayFeature != null)
            xRayFeature.SetActive(active);

        // 2. เปิด/ปิด โหมดปรับแสงสว่างรอบตัวในที่มืด
        if (scannerVolume != null)
            scannerVolume.enabled = active;

        // 3. สั่งเปิด/ปิด ไฮไลท์ศัตรูทุกตัวที่แคชไว้ทันที
        if (cachedEnemies != null)
        {
            foreach (enemy_stage enemy in cachedEnemies)
            {
                if (enemy != null)
                {
                    // ตรงนี้สามารถเรียกใช้ฟังก์ชันเปิด/ปิด Material ไฮไลท์ของศัตรูได้ครับ
                    // เช่น enemy.SetHighlight(active);
                }
            }
        }
    }
}