using TMPro;
using UnityEngine;

public class Player_UI_Manager : MonoBehaviour
{
    [Header("Weapon UI")]
    public TextMeshProUGUI weaponName;
    public TextMeshProUGUI Ammo;

    [Header("Player Status UI")]
    public TextMeshProUGUI Player_Health;
    public TextMeshProUGUI Player_Light_Meter;
    public TextMeshProUGUI Player_armor;

    [Header("Action Popups")]
    public GameObject takedownPopup; // เพิ่มตัวแปรมารับ Panel ล็อคคอ

    public Weapon_system weapon_system;
    public Player Player_Script;

    void Start()
    {
        weapon_system = Weapon_system.Instance;
        Player_Script = Player.Instance;

        // บังคับปิดป๊อปอัปไว้ก่อนตอนเริ่มเกม
        if (takedownPopup != null) takedownPopup.SetActive(false);
    }

    void Update()
    {
        // --- 1. อัปเดตสถานะผู้เล่น (เลือด, เกราะ, แสง) ---
        // เอาออกมาไว้นอกเงื่อนไขปืน เพื่อให้มันอัปเดตตลอดเวลา
        if (Player_Script != null)
        {
            Player_Health.text = "Health: " + Player_Script.currentHP.ToString() + " / " + Player_Script.MaxHP;
            Player_armor.text = Player_Script.currentArmorDurability.ToString();
        }

        if (LightDetect.Instance != null)
        {
            Player_Light_Meter.text = "Light Meter: " + LightDetect.Instance.light_meter.ToString();
        }

        // --- 2. อัปเดตระบบอาวุธ ---
        // เปลี่ยนมาเช็ก currentWeapon แทน เพื่อรองรับทั้งปืนหลักและปืนรอง
        if (weapon_system != null && weapon_system.currentWeapon != null)
        {
            weaponName.text = weapon_system.currentWeapon.itemName;
            Ammo.text = weapon_system.CurrentAmmo.ToString() + " / " + weapon_system.CurrentReserveAmmo;
        }
        else
        {
            weaponName.text = "Unarmed";
            Ammo.text = "-";
        }

        // --- 3. อัปเดตระบบป๊อปอัป (ล็อคคอศัตรู) ---
        CheckTakedownPopup();
    }

    void CheckTakedownPopup()
    {
        if (Player_Script != null && takedownPopup != null)
        {
            // ถ้าสถานะปัจจุบันคือ GrabbingEnemy ให้เป็น true
            bool isGrabbing = (Player_Script.currentState == Player.PlayerState.GrabbingEnemy);

            // ใช้ activeSelf เช็กก่อนเปิด/ปิด เพื่อประหยัดทรัพยากรเครื่อง
            if (takedownPopup.activeSelf != isGrabbing)
            {
                takedownPopup.SetActive(isGrabbing);
            }
        }
    }
}