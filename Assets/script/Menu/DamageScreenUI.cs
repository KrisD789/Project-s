using UnityEngine;
using UnityEngine.UI;

public class DamageScreenUI : MonoBehaviour
{
    public static DamageScreenUI Instance;

    [Header("UI Settings")]
    public Image bloodImage;

    [Header("Effect Settings")]
    public float flashSpeed = 5f;        // ความเร็วตอนจางหาย (ตอนโดนยิงทั่วไป)
    public float lowHealthThreshold = 40f; // เลือดต่ำกว่าเท่าไหร่ถึงจะกระพริบรัวๆ
    public float pulseSpeed = 5f;        // ความเร็วกระพริบตอนเลือดน้อย
    public float maxPulseAlpha = 0.6f;   // ความเข้มสูงสุดตอนกระพริบ (0.0 - 1.0)

    private Color bloodColor;
    private float targetAlpha = 0f;
    private float currentAlpha = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (bloodImage == null) bloodImage = GetComponent<Image>();

        if (bloodImage != null)
        {
            bloodColor = bloodImage.color;
            bloodColor.a = 0f;
            bloodImage.color = bloodColor;
        }
    }

    void Update()
    {
        if (Player.Instance == null || bloodImage == null) return;

        float currentHP = Player.Instance.currentHP;

        // เช็กว่าเลือดต่ำกว่าจุดวิกฤต (40) ไหม
        if (currentHP > 0 && currentHP <= lowHealthThreshold)
        {
            // เลือดวิกฤต: ใช้สมการคณิตศาสตร์ (Sine wave) ทำให้ค่ามันเด้งขึ้นลงเป็นจังหวะชีพจร
            targetAlpha = Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)) * maxPulseAlpha;
        }
        else
        {
            // เลือดปกติ: ให้สีแดงค่อยๆ จางหายไปเป็น 0
            targetAlpha = 0f;
        }

        // ค่อยๆ ปรับความโปร่งใสให้สมูท
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, flashSpeed * Time.deltaTime);
        bloodColor.a = currentAlpha;
        bloodImage.color = bloodColor;
    }

    // ฟังก์ชันนี้เอาไว้รับคำสั่ง "กระชากสีแดงขึ้นมา" เวลาโดนกระสุน
    public void FlashBlood()
    {
        currentAlpha = 0.8f; // ดันความแดงขึ้นมาทันที 80%
    }
}