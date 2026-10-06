using UnityEngine;
using UnityEngine.UI;

public class LightMeterUI : MonoBehaviour
{
    [Header("UI Reference")]
    public Image lightCircleUI;

    [Header("Settings")]
    public float maxUIScale = 1f;
    public Gradient lightColorGradient;

    private void Update()
    {
        // เช็กก่อนว่ามี LightDetect ในฉากและใส่ UI ไว้หรือยัง
        if (LightDetect.Instance == null || lightCircleUI == null) return;

        // ดึงค่าความสว่างแบบ Real-time (0.0 ถึง 1.0) จาก LightDetect
        float currentBrightness = LightDetect.Instance.currentBrightness;

        // ควบคุมการขยาย-หด
        float currentScale = currentBrightness * maxUIScale;
        lightCircleUI.rectTransform.localScale = new Vector3(currentScale, currentScale, 1f);

        // เปลี่ยนสีวงกลม
        lightCircleUI.color = lightColorGradient.Evaluate(currentBrightness);
    }
}