using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player_UI_Manager : MonoBehaviour
{
    [Header("Weapon UI")]
    public TextMeshProUGUI weaponName;
    public TextMeshProUGUI Ammo;
    //  เพิ่มตัวแปรสำหรับโหมดการยิงและรูปภาพปืน
    public TextMeshProUGUI fireModeText;
    public Image weaponIcon;

    [Header("Consumable UI")]
    // เพิ่มหมวดหมู่สำหรับไอเทมประเภทยา
    public GameObject consumablePopupPanel;
    public TextMeshProUGUI consumableName;
    public TextMeshProUGUI consumableAmount;
    public Image consumableIcon;

    [Header("Player Status UI")]
    public TextMeshProUGUI Player_Health;
    public TextMeshProUGUI Player_Light_Meter;
    public TextMeshProUGUI Player_armor;

    [Header("Light Circle UI")]
    public Image lightCircleUI;
    public Gradient lightColorGradient;

    [Header("Action Popups")]
    public GameObject takedownPopup;

    public Weapon_system weapon_system;
    public Player Player_Script;

    void Start()
    {
        weapon_system = Weapon_system.Instance;
        Player_Script = Player.Instance;

        if (takedownPopup != null) takedownPopup.SetActive(false);
    }

    void Update()
    {
        // --- 1. อัปเดตสถานะผู้เล่น ---
        if (Player_Script != null)
        {
            Player_Health.text = "Health: " + Player_Script.currentHP.ToString() + " / " + Player_Script.MaxHP;
            Player_armor.text = Player_Script.currentArmorProfile.name + ": " + Player_Script.currentArmorDurability.ToString();
        }

        if (LightDetect.Instance != null && lightCircleUI != null)
        {
            Player_Light_Meter.text = "Light Meter: " + LightDetect.Instance.light_meter.ToString();
            float currentBrightness = LightDetect.Instance.currentBrightness;
            lightCircleUI.color = lightColorGradient.Evaluate(currentBrightness);
        }

        // --- 2. อัปเดตระบบอาวุธ (ชื่อ, กระสุน, โหมดการยิง, รูปภาพ) ---
        if (weapon_system != null && weapon_system.currentWeapon != null)
        {
            weaponName.text = weapon_system.currentWeapon.itemName;
            Ammo.text = weapon_system.CurrentAmmo.ToString() + " / " + weapon_system.CurrentReserveAmmo;

            // แสดงโหมดการยิงปัจจุบัน
            if (fireModeText != null)
            {
                fireModeText.text = weapon_system.current_Weapon_FireMode == Weapon_system.CurrentFireMode.Semi_Auto ? "SEMI" : "AUTO";
            }

            // แสดงรูปภาพปืน (ดึงจาก itemIcon ใน Base_Item)
            if (weaponIcon != null && weapon_system.currentWeapon.itemIcon != null)
            {
                weaponIcon.sprite = weapon_system.currentWeapon.itemIcon;
                weaponIcon.enabled = true;
            }
        }
        else
        {
            weaponName.text = "Unarmed";
            Ammo.text = "-";
            if (fireModeText != null) fireModeText.text = "-";
            if (weaponIcon != null) weaponIcon.enabled = false; // ปิดรูปถ้าไม่มีปืน
        }

        // --- 3. อัปเดตระบบไอเทมกดใช้ (ยา/เกราะ) ---
        UpdateConsumableUI();

        // --- 4. อัปเดตระบบป๊อปอัป (ล็อคคอศัตรู) ---
        CheckTakedownPopup();
    }

    void UpdateConsumableUI()
    {
        // เช็กว่ามีการกดยาอยู่หรือไม่ (currentItemUse ไม่เป็น null)
        if (Player_Inventory.Instance != null && Player_Inventory.Instance.currentItemUse != null)
        {
            Consumable_Item item = Player_Inventory.Instance.currentItemUse;

            // 1. เปิด Panel ป๊อปอัปขึ้นมาโชว์
            if (consumablePopupPanel != null && !consumablePopupPanel.activeSelf)
            {
                consumablePopupPanel.SetActive(true);
            }

            // 2. อัปเดตข้อมูลยาลงไปใน UI
            if (consumableName != null) consumableName.text = item.itemName;
            if (consumableAmount != null) consumableAmount.text = "x" + item.CurrentAmount.ToString();

            if (consumableIcon != null && item.itemIcon != null)
            {
                consumableIcon.sprite = item.itemIcon;
                consumableIcon.enabled = true; // เปิดการแสดงผลรูปภาพ
            }
        }
        else
        {
            // ปิด Panel ป๊อปอัปทิ้งไปเลย เมื่อไม่ได้กดยา (ฮีลเสร็จ หรือกดยกเลิก)
            if (consumablePopupPanel != null && consumablePopupPanel.activeSelf)
            {
                consumablePopupPanel.SetActive(false);
            }
        }
    }


    void CheckTakedownPopup()
    {
        if (Player_Script != null && takedownPopup != null)
        {
            bool isGrabbing = (Player_Script.currentState == Player.PlayerState.GrabbingEnemy);
            if (takedownPopup.activeSelf != isGrabbing)
            {
                takedownPopup.SetActive(isGrabbing);
            }
        }
    }
}