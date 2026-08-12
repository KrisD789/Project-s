using TMPro;
using UnityEngine;

public class Player_UI_Manager : MonoBehaviour
{
    public TextMeshProUGUI weaponName;
    public TextMeshProUGUI Ammo;
    public TextMeshProUGUI Player_Health;
    public TextMeshProUGUI Player_Light_Meter;
    public TextMeshProUGUI Player_armor;

    public Weapon_system weapon_system;
    public Player Player_Script;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        weapon_system = Weapon_system.Instance;
        Player_Script = Player.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (weapon_system.Player_Primary_weapon != null)
        {
            weaponName.text = weapon_system.currentWeapon.itemName;
            Ammo.text = weapon_system.CurrentAmmo.ToString() + " / " + weapon_system.CurrentReserveAmmo;
            Player_Health.text = "Health: " + Player_Script.currentHP.ToString() + " / " + Player_Script.MaxHP;
            Player_Light_Meter.text = "Light Meter: " + LightDetect.Instance.light_meter.ToString();
            Player_armor.text = Player_Script.currentArmorDurability.ToString();
        }

        else
        {
            weaponName.text = "Not Found weapon inLoadout";
            Ammo.text = "Not Found weapon inLoadout";
        }
    }
}
