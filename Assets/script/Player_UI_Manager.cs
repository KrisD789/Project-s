using TMPro;
using UnityEngine;

public class Player_UI_Manager : MonoBehaviour
{
    public TextMeshProUGUI weaponName;
    public TextMeshProUGUI Ammo;

    public Weapon_system weapon_system;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        weapon_system = GameObject.FindAnyObjectByType<Weapon_system>();
    }

    // Update is called once per frame
    void Update()
    {
        weaponName.text = weapon_system.currentWeapon.itemName;
        Ammo.text = weapon_system.currentWeapon.Current_Ammo.ToString() + " / " + weapon_system.currentWeapon.Max_Ammo;
    }
}
