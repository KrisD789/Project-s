using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class scanner : MonoBehaviour
{
    [Header("Settings")]
    public ScriptableRendererFeature xRayFeature;
    public Volume scannerVolume; // ลาก ScannerVolume มาใส่ตรงนี้

    //public TextMeshProUGUI ui;

    private bool isScanning = false;

    private void Start()
    {
        //ui = GetComponent<TextMeshProUGUI>();

        xRayFeature.SetActive(false);
        scannerVolume.enabled = false;
        isScanning = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isScanning = !isScanning;
            ToggleScanner(isScanning);
        }

        //ui.text = "Scanner: " + (isScanning ? "ON" : "OFF");

    }

    void ToggleScanner(bool active)
    {
        // 1. เปิด/ปิด เห็นทะลุกำแพง
        if (xRayFeature != null)
            xRayFeature.SetActive(active);

        // 2. เปิด/ปิด หน้าจอเปลี่ยนสี
        if (scannerVolume != null)
            scannerVolume.enabled = active;

        // 3. (แถม) ใส่เสียงตอนเปิดปิดตรงนี้ได้เลย
    }


}
