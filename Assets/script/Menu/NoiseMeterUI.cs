using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoiseMeterUI : MonoBehaviour
{
    public static NoiseMeterUI Instance { get; private set; }

    [Header("UI Reference")]
    public Slider noiseSlider;
    // เพิ่มตัวแปรสำหรับควบคุมสีของหลอด
    public Image fillImage;
    public TextMeshProUGUI movementStateText;

    [Header("Settings")]
    public float maxNoiseRadius = 10f;
    public float decaySpeed = 20f;
    //  เพิ่มตัวแปรสำหรับตั้งค่าการไล่สี
    public Gradient noiseColorGradient;

    private float currentVisualNoise = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        float baseNoise = 0f;
        string currentModeName = "WALK"; // ตั้งค่าเริ่มต้น

        if (Player.Instance != null && Player.Instance.movement != null)
        {
            baseNoise = Player.Instance.movement.noiseCollider.radius;

            // ดึงโหมดที่ผู้เล่นเลือกไว้ (จากการกลิ้งเมาส์) มาแสดงผลทันที ไม่ต้องสนว่ายืนนิ่งหรือเดินอยู่
            int pState = Player.Instance.movement.playerState;

            if (pState == 2) currentModeName = "SPRINT";
            else if (pState == 1) currentModeName = "RUN";
            else if (pState == 0) currentModeName = "WALK";
            else if (pState == -1) currentModeName = "Stalk";
            else if (pState == -2) currentModeName = "SNEAK";
        }

        // เช็กว่าเกจเสียงสูงกว่าปกติหรือไม่ (เช่น เพิ่งยิงปืน)
        if (currentVisualNoise > baseNoise + 0.1f)
        {
            currentVisualNoise -= Time.deltaTime * decaySpeed;
            currentModeName = "NOISE!"; // ถ้ามีเสียงดังแทรกเข้ามา ให้โชว์คำเตือนทับโหมดปัจจุบันไปก่อน
        }
        else
        {
            currentVisualNoise = baseNoise;
        }

        // อัปเดต UI หลอดเสียง
        if (noiseSlider != null)
        {
            // ใช้ InverseLerp ช่วยเกลี่ยค่า โดยดึงค่า minNoiseRadius มาจาก Player.Instance
            float minNoise = (Player.Instance != null && Player.Instance.movement != null) ? Player.Instance.movement.minNoiseRadius : 0f;
            float noisePercent = Mathf.InverseLerp(minNoise, maxNoiseRadius, currentVisualNoise);
            noiseSlider.value = noisePercent;

            if (fillImage != null)
            {
                fillImage.color = noiseColorGradient.Evaluate(noisePercent);
            }
        }

        // อัปเดตข้อความโหมดการเดินลงบนหน้าจอ
        if (movementStateText != null)
        {
            movementStateText.text = currentModeName;
        }
    }

    public void RegisterGunshot(float weaponNoise)
    {
        if (weaponNoise > currentVisualNoise)
        {
            currentVisualNoise = weaponNoise;
        }
    }
}