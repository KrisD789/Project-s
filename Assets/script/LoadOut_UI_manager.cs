using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadOut_UI_manager : MonoBehaviour
{
    [Header("ฐานข้อมูลอาวุธทั้งหมด")]
    public item[] allWeapons;

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

    private item currentSelect;


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
        foreach (item weapon in allWeapons)
        {
            GameObject newBtn = Instantiate(iconButtonPrefab, gridContentPanel, false);
            newBtn.GetComponent<WeaponInfo_prefab>().Setup(weapon, this);
        }

        // ลบโค้ดที่เคยสั่ง ShowDetail(allWeapons[0]) ออกไปเลยครับ
    }

    // ฟังก์ชันนี้จะทำงานก็ต่อเมื่อผู้เล่นคลิกปุ่มฝั่งซ้ายแล้วเท่านั้น
    public void ShowDetail(item weaponData)
    {
        currentSelect = weaponData;

        // 1. ทันทีที่โดนคลิก สั่งเปิดหน้าต่างฝั่งขวาให้โชว์ขึ้นมา
        if (detailPanelObject != null)
        {
            detailPanelObject.SetActive(true);
        }

        // 2. อัปเดตตัวหนังสือตามข้อมูลปืนที่กด
        weaponNameText.text = weaponData.itemName;
        descriptionText.text = weaponData.description;
        WeaponDamage.text = "Damage: " + weaponData.weaponDamage.ToString();
        Ammo.text = "Ammo: " + weaponData.Max_Ammo.ToString();

        // เราสามารถนำตัวเลขมาแปลงเป็นข้อความด้วยคำสั่ง .ToString() 
        noiseText.text = "Noise: " + weaponData.noiseLevel.ToString() + " dB";

        // คำสั่ง .sprite ใช้สำหรับการเปลี่ยนรูปภาพในคอมโพเนนต์ Image
        //weaponIcon.sprite = weaponData.itemIcon;
    }

    public void selectWeapon()
    {
        if (Load_out_manager.Instance.selectedPrimaryWeapon == null)
        {
            Load_out_manager.Instance.selectedPrimaryWeapon = currentSelect;
        }

        else
        {
            Load_out_manager.Instance.selectedSecondaryWeapon = currentSelect;
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
