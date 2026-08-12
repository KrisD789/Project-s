using System.Collections.Generic; // อย่าลืมบรรทัดนี้ เพื่อให้ใช้ List ได้
using UnityEngine;

[System.Serializable]
public struct weapon_Data
{
    // เปลี่ยนจากเก็บคลาส เป็นการเก็บแค่ "ชื่อไฟล์" เพื่อให้ JSON แปลงข้อความได้
    public string Primary_Weapon_ID;
    public string Secondary_Weapon_ID;
    public string Current_Weapon_ID;

    // เก็บกระสุนแยกกระบอก
    public int Primary_CurrentAmmo;
    public int Primary_ReserveAmmo;
    public int Secondary_CurrentAmmo;
    public int Secondary_ReserveAmmo;
}

[System.Serializable]
public struct Player_DataBox
{
    public Vector3 position;
    public Vector3 rotation;

    // สถานะร่างกายและเกราะ
    public float currentHP;
    public float MaxHP;
    public bool isDead;
    public int PlayerState;

    public string Armor_ID; // เก็บชื่อไฟล์เกราะ
    public float currentArmorDurability;

    // กล่องอาวุธ
    public weapon_Data weaponStats;
}

public class Player_SaveData : MonoBehaviour, Isaveable
{
    SaveableEntity player_ID;
    Player Player_script;
    LightDetect LightDetect;
    Weapon_system weapon;

    [Header("Database คลังแสง (ลากไฟล์ทั้งหมดที่มีในเกมมาใส่ช่องนี้)")]
    public List<Weapon_Item> allWeaponsDatabase;
    public List<Armor_Item> allArmorsDatabase;

    private void Awake()
    {
        if (!TryGetComponent<SaveableEntity>(out player_ID))
            Debug.Log("!!!!! WARNING !!!!! --- Player_SaveData หา << Player_ID >> ไม่เจอ ---");

        if (!TryGetComponent<Player>(out Player_script))
            Debug.Log("!!!!! WARNING !!!!! --- Player_SaveData หา << Player_script >> ไม่เจอ ---");

        if (!TryGetComponent<LightDetect>(out LightDetect))
            Debug.Log("!!!!! WARNING !!!!! --- Player_SaveData หา << LightDetect >> ไม่เจอ ---");

        if (!TryGetComponent<Weapon_system>(out weapon))
            Debug.Log("!!!!! WARNING !!!!! --- Player_SaveData หา << weapon >> ไม่เจอ ---");
    }

    public string GetSaveID()
    {
        return player_ID.uniqueID;
    }

    public string SaveState()
    {
        Player_DataBox dataBox = new Player_DataBox();

        // 1. เก็บพิกัดผู้เล่น
        dataBox.position = transform.position;
        dataBox.rotation = transform.eulerAngles;

        // 2. เก็บข้อมูลจาก Player.cs
        if (Player_script != null)
        {
            dataBox.currentHP = Player_script.currentHP; 
            dataBox.MaxHP = Player_script.MaxHP; 
            dataBox.isDead = Player_script.isDead; 
            dataBox.PlayerState = (int)Player_script.currentState; 
            
            // เก็บความทนทานเกราะ
            dataBox.currentArmorDurability = Player_script.currentArmorDurability; 

            // เก็บชื่อไฟล์เกราะ (ถ้ามี)
            if (Player_script.currentArmorProfile != null) 
            {
                dataBox.Armor_ID = Player_script.currentArmorProfile.name; 
            }
        }

        // 3. เก็บข้อมูลจาก Weapon_system.cs
        if (weapon != null)
        {
            if (weapon.Player_Primary_weapon != null)
                dataBox.weaponStats.Primary_Weapon_ID = weapon.Player_Primary_weapon.name;

            if (weapon.Player_Secondary_weapon != null)
                dataBox.weaponStats.Secondary_Weapon_ID = weapon.Player_Secondary_weapon.name;

            if (weapon.currentWeapon != null)
                dataBox.weaponStats.Current_Weapon_ID = weapon.currentWeapon.name;

            dataBox.weaponStats.Primary_CurrentAmmo = weapon.primary_CurrentAmmo;
            dataBox.weaponStats.Primary_ReserveAmmo = weapon.primary_ReserveAmmo;
            dataBox.weaponStats.Secondary_CurrentAmmo = weapon.secondary_CurrentAmmo;
            dataBox.weaponStats.Secondary_ReserveAmmo = weapon.secondary_ReserveAmmo;
        }

        return JsonUtility.ToJson(dataBox);
    }

    public void LoadState(string stateData)
    {
        Player_DataBox dataBox = JsonUtility.FromJson<Player_DataBox>(stateData);

        // 1. คืนค่าพิกัด
        transform.position = dataBox.position;
        transform.eulerAngles = dataBox.rotation;

        // 2. คืนค่าผู้เล่นและเกราะ
        if (Player_script != null)
        {
            Player_script.currentHP = dataBox.currentHP; 
            Player_script.MaxHP = dataBox.MaxHP; 
            Player_script.isDead = dataBox.isDead; 
            Player_script.currentState = (Player.PlayerState)dataBox.PlayerState; 

            // ค้นหาเกราะจากคลังแสง
            if (!string.IsNullOrEmpty(dataBox.Armor_ID))
            {
                foreach (Armor_Item armor in allArmorsDatabase)
                {
                    if (armor.name == dataBox.Armor_ID)
                    {
                        Player_script.EquipArmor(armor); // สวมใส่เกราะ (ฟังก์ชันนี้จะเซ็ตความทนทานเป็นค่า Max เสมอ)
                        break;
                    }
                }
            }

            //  ต้องเอาความทนทานที่เซฟไว้ มาเขียนทับเป็นลำดับสุดท้าย ป้องกันเกราะกลับมาเต็ม 100
            Player_script.currentArmorDurability = dataBox.currentArmorDurability; 
        }

        // 3. คืนค่ากระสุนและอาวุธ
        if (weapon != null)
        {
            // ดึงปืนหลัก
            if (!string.IsNullOrEmpty(dataBox.weaponStats.Primary_Weapon_ID))
            {
                foreach (Weapon_Item w in allWeaponsDatabase)
                {
                    if (w.name == dataBox.weaponStats.Primary_Weapon_ID) { weapon.Player_Primary_weapon = w; break; }
                }
            }

            // ดึงปืนรอง
            if (!string.IsNullOrEmpty(dataBox.weaponStats.Secondary_Weapon_ID))
            {
                foreach (Weapon_Item w in allWeaponsDatabase)
                {
                    if (w.name == dataBox.weaponStats.Secondary_Weapon_ID) { weapon.Player_Secondary_weapon = w; break; }
                }
            }

            // คืนค่ากระสุน
            weapon.primary_CurrentAmmo = dataBox.weaponStats.Primary_CurrentAmmo;
            weapon.primary_ReserveAmmo = dataBox.weaponStats.Primary_ReserveAmmo;
            weapon.secondary_CurrentAmmo = dataBox.weaponStats.Secondary_CurrentAmmo;
            weapon.secondary_ReserveAmmo = dataBox.weaponStats.Secondary_ReserveAmmo;

            // สวมใส่ปืนที่กำลังถือล่าสุด
            if (dataBox.weaponStats.Current_Weapon_ID == dataBox.weaponStats.Primary_Weapon_ID)
                weapon.EquipPrimary();
            else if (dataBox.weaponStats.Current_Weapon_ID == dataBox.weaponStats.Secondary_Weapon_ID)
                weapon.EquipSecondary();
        }
    }
}