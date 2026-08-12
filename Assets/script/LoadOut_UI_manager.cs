using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadOut_UI_manager : MonoBehaviour
{
    [Header("ฐานข้อมูลอาวุธทั้งหมด")]
    public Base_Item[] all_item;

    [Header("โซนไอคอนฝั่งซ้าย")]
    public GameObject iconButtonPrefab;
    public Transform gridContentPanel;

    // --- เพิ่มตัวแปรนี้เข้ามาใหม่ ---
    [Header("หน้าต่างฝั่งขวา (เอาไว้สั่งเปิด/ปิด)")]
    public GameObject detailPanelObject; // ลากกล่อง Panel ฝั่งขวามาใส่ช่องนี้

    [Header("ข้อมูลตัวหนังสือฝั่งขวา")]
    //public Image weaponIcon;
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI WeaponDamage;
    public TextMeshProUGUI Ammo;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI noiseText;

    private Base_Item currentSelect;


    void Start()
    {
        // 1. สั่งปิดหน้าต่างฝั่งขวาทิ้งไปเลยตั้งแต่ตอนเริ่ม
        if (detailPanelObject != null)
        {
            detailPanelObject.SetActive(false);
        }

        // 2. วนลูปเสกแค่ปุ่มฝั่งซ้ายตามปกติ
        //foreach (item weapon in allWeapons)
        //{
            //GameObject newBtn = Instantiate(iconButtonPrefab, gridContentPanel, false);
            //newBtn.GetComponent<WeaponInfo_prefab>().Setup(weapon, this);
        //}

        // ลบโค้ดที่เคยสั่ง ShowDetail(allWeapons[0]) ออกไปเลยครับ
    }

    private void OnEnable()
    {
        //วนลูปเสกแค่ปุ่มฝั่งซ้ายตามปกติ
        foreach (Base_Item weapon in all_item)
        {
            GameObject newBtn = Instantiate(iconButtonPrefab, gridContentPanel, false);
            newBtn.GetComponent<WeaponInfo_prefab>().Setup(weapon, this);
        }

        // ลบโค้ดที่เคยสั่ง ShowDetail(allWeapons[0]) ออกไปเลยครับ
    }

    // ฟังก์ชันนี้จะทำงานก็ต่อเมื่อผู้เล่นคลิกปุ่มฝั่งซ้ายแล้วเท่านั้น
    public void ShowDetail(Base_Item itemData)
    {
        currentSelect = itemData;

        if (detailPanelObject != null)
            detailPanelObject.SetActive(true);

        // ดึงข้อมูลพื้นฐานที่มีในไอเทมทุกชิ้นมาโชว์ก่อน
        weaponNameText.text = itemData.itemName;
        descriptionText.text = itemData.description;

        // ตรวจสอบว่าไอเทมชิ้นนี้คือ "ปืน" (WeaponItem) ใช่หรือไม่?
        if (itemData is Weapon_Item weapon)
        {
            // ถ้าใช่ ตัวแปร weapon จะดึงค่ากระสุนและดาเมจมาใช้ได้ทันที
            WeaponDamage.text = "Damage: " + weapon.weaponDamage.ToString();
            Ammo.text = "Ammo: " + weapon.Max_Ammo.ToString();
            noiseText.text = "Noise: " + weapon.noiseLevel.ToString() + " dB";
        }
        // ตรวจสอบว่าไอเทมชิ้นนี้คือ "เกราะ" (ArmorItem) ใช่หรือไม่?
        else if (itemData is Armor_Item armor)
        {
            // ดึงค่าของเกราะมาโชว์แทน (เช่น แปลงค่าลดดาเมจเป็นเปอร์เซ็นต์)
            WeaponDamage.text = "Damage Resist: " + (armor.Max_Armor_Durability * 100).ToString() + "%";
            Ammo.text = "Speed: " + armor.Movement_Speed_Multiplier.ToString() + "x";
            noiseText.text = "Noise Multiplier: " + armor.Noise_Multiplier.ToString() + "x";
        }
        else
        {
            // ถ้าเป็นของทั่วไปอื่นๆ ก็ล้างตัวหนังสือทิ้งไปไม่ให้มันรก
            WeaponDamage.text = "";
            Ammo.text = "";
            noiseText.text = "";
        }
    }

    public void selectWeapon()
    {
        // 4. ก่อนจะบันทึก ต้องเช็คก่อนว่ามันคืออะไร จะได้ยัดใส่ช่องใน Load_out_manager ถูก
        if (currentSelect is Weapon_Item weapon)
        {
            if (Load_out_manager.Instance.selectedPrimaryWeapon == null)
            {
                Load_out_manager.Instance.selectedPrimaryWeapon = weapon;
                Debug.Log("สวมใส่: ปืนหลัก");
            }
            else
            {
                Load_out_manager.Instance.selectedSecondaryWeapon = weapon;
                Debug.Log("สวมใส่: ปืนรอง");
            }
        }
        else if (currentSelect is Armor_Item armor)
        {
            // สมมติว่าคุณเพิ่ม public ArmorItem selectedArmor; ใน Load_out_manager แล้ว
            // Load_out_manager.Instance.selectedArmor = armor; 
            Debug.Log("สวมใส่: ชุดเกราะ");
        }
    }

    public void ClosePanel()
    {
        if (detailPanelObject != null)
        {
            detailPanelObject.SetActive(false);
        }
    }
}
