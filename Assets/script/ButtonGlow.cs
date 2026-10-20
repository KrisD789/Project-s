using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("ตั้งค่าขอบเรืองแสง")]
    public Outline buttonOutline; // ลากคอมโพเนนต์ Outline มาใส่ตรงนี้
    public float glowSpeed = 8f;
    public float maxAlpha = 1f;

    private float targetAlpha = 0f;

    void Start()
    {
        // บังคับให้โปร่งใสตอนเริ่มเกม
        if (buttonOutline != null)
        {
            Color c = buttonOutline.effectColor;
            c.a = 0f;
            buttonOutline.effectColor = c;
        }
    }

    void Update()
    {
        if (buttonOutline == null) return;

        // ค่อยๆ ปรับความเข้มของสีเส้นขอบให้สว่าง/ดับอย่างนุ่มนวล
        Color c = buttonOutline.effectColor;
        c.a = Mathf.Lerp(c.a, targetAlpha, Time.unscaledDeltaTime * glowSpeed);
        buttonOutline.effectColor = c;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetAlpha = maxAlpha;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetAlpha = 0f;
    }
}