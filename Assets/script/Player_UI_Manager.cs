using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player_UI_Manager : MonoBehaviour
{
    [Header("Weapon UI")]
    public TextMeshProUGUI weaponName;
    public TextMeshProUGUI Ammo;
    public TextMeshProUGUI fireModeText;
    public Image weaponIcon;

    [Header("Consumable UI")]
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
        if (consumablePopupPanel != null) consumablePopupPanel.SetActive(false); // ปิด UI ยาไว้ก่อนตอนเริ่มเกม
    }

    void Update()
    {
        // --- 1. อัปเดตสถานะผู้เล่น ---
        if (Player_Script != null)
        {
            if (Player_Health != null)
                Player_Health.text = "Health: " + Player_Script.currentHP.ToString() + " / " + Player_Script.MaxHP;

            // [อุดรอยรั่ว] เช็กว่าผู้เล่นใส่เกราะอยู่ไหม ถ้าไม่ใส่ให้แสดงคำว่า "No Armor"
            if (Player_armor != null)
            {
                if (Player_Script.currentArmorProfile != null)
                {
                    Player_armor.text = Player_Script.currentArmorProfile.name + ": " + Player_Script.currentArmorDurability.ToString();
                }
                else
                {
                    Player_armor.text = "No Armor";
                }
            }
        }

        if (LightDetect.Instance != null && lightCircleUI != null)
        {
            if (Player_Light_Meter != null)
                Player_Light_Meter.text = "Light Meter: " + LightDetect.Instance.light_meter.ToString();

            float currentBrightness = LightDetect.Instance.currentBrightness;
            lightCircleUI.color = lightColorGradient.Evaluate(currentBrightness);
        }

        // --- 2. อัปเดตระบบอาวุธ ---
        if (weapon_system != null && weapon_system.currentWeapon != null)
        {
            if (weaponName != null) weaponName.text = weapon_system.currentWeapon.itemName;
            if (Ammo != null) Ammo.text = weapon_system.CurrentAmmo.ToString() + " / " + weapon_system.CurrentReserveAmmo;

            if (fireModeText != null)
            {
                fireModeText.text = weapon_system.current_Weapon_FireMode == Weapon_system.CurrentFireMode.Semi_Auto ? "SEMI" : "AUTO";
            }

            if (weaponIcon != null)
            {
                if (weapon_system.currentWeapon.itemIcon != null)
                {
                    weaponIcon.sprite = weapon_system.currentWeapon.itemIcon;
                    weaponIcon.enabled = true;
                }
                else
                {
                    weaponIcon.enabled = false;
                }
            }
        }
        else
        {
            if (weaponName != null) weaponName.text = "Unarmed";
            if (Ammo != null) Ammo.text = "-";
            if (fireModeText != null) fireModeText.text = "-";
            if (weaponIcon != null) weaponIcon.enabled = false;
        }

        // --- 3. อัปเดตระบบไอเทมกดใช้ (ยา/เกราะ) ---
        UpdateConsumableUI();

        // --- 4. อัปเดตระบบป๊อปอัป (ล็อคคอศัตรู) ---
        CheckTakedownPopup();
    }

    void UpdateConsumableUI()
    {
        // [อุดรอยรั่ว] เช็กให้ชัวร์ว่ามียาอยู่ และ "จำนวนยาต้องมากกว่า 0" UI ถึงจะทำงาน
        if (Player_Inventory.Instance != null &&
            Player_Inventory.Instance.currentItemUse != null &&
            Player_Inventory.Instance.currentItemUse.CurrentAmount > 0)
        {
            Consumable_Item item = Player_Inventory.Instance.currentItemUse;

            if (consumablePopupPanel != null && !consumablePopupPanel.activeSelf)
            {
                consumablePopupPanel.SetActive(true);
            }

            if (consumableName != null) consumableName.text = item.itemName;
            if (consumableAmount != null) consumableAmount.text = "x" + item.CurrentAmount.ToString();

            if (consumableIcon != null)
            {
                if (item.itemIcon != null)
                {
                    consumableIcon.sprite = item.itemIcon;
                    consumableIcon.enabled = true;
                }
                else
                {
                    consumableIcon.enabled = false;
                }
            }
        }
        else
        {
            // ถ้าไม่มียา หรือยาเหลือ 0 ชิ้น ให้ซ่อน Popup ทันที
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