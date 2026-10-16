using TMPro;
using UnityEngine;
using UnityEngine.UI; // สำคัญมาก: ต้องมีเพื่อเรียกใช้งานคอมโพเนนต์ Image

public class LoadOut_UI_manager : MonoBehaviour
{
    [Header("ฐานข้อมูลไอเทมทั้งหมด")]
    public Base_Item[] all_item;

    [Header("โซนปุ่มเลือกไอเทม (ฝั่งซ้าย)")]
    public GameObject iconButtonPrefab;
    public Transform gridContentPanel;

    [Header("หน้าต่างรายละเอียด (ตรงกลาง)")]
    public GameObject detailPanelObject; // กล่อง Panel ที่จะเปิด/ปิด
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI WeaponDamage;
    public TextMeshProUGUI Ammo;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI noiseText;

    // --- ส่วนที่เพิ่มเข้ามาใหม่สำหรับโชว์รูปฝั่งขวา ---
    [Header("ช่องแสดงรูปไอเทมที่สวมใส่ (ฝั่งขวา)")]
    public Image equippedPrimaryIcon;
    public Image equippedSecondaryIcon;
    public Image equippedArmorIcon;
    public Image equippedConsumable1Icon;
    public Image equippedConsumable2Icon;

    private Base_Item currentSelect;

    void Start()
    {
        if (detailPanelObject != null)
        {
            detailPanelObject.SetActive(false); // ซ่อนหน้ารายละเอียดไว้ก่อนตอนเริ่ม
        }

        UpdateEquippedUI(); // อัปเดตรูปภาพฝั่งขวาทันทีที่เปิดหน้าต่าง
    }

    private void OnEnable()
    {
        // ล้างปุ่มเก่าทิ้งก่อน (กันปุ่มเบิ้ลเวลาเปิด/ปิดหน้าต่างหลายรอบ)
        foreach (Transform child in gridContentPanel)
        {
            Destroy(child.gameObject);
        }

        // สร้างปุ่มไอเทมฝั่งซ้าย
        foreach (Base_Item weapon in all_item)
        {
            GameObject newBtn = Instantiate(iconButtonPrefab, gridContentPanel, false);
            newBtn.GetComponent<WeaponInfo_prefab>().Setup(weapon, this);
        }

        UpdateEquippedUI();
    }

    // ฟังก์ชันสำหรับกดดูรายละเอียดไอเทม (โชว์ Stat ตรงกลาง)
    public void ShowDetail(Base_Item itemData)
    {
        currentSelect = itemData;

        if (detailPanelObject != null)
            detailPanelObject.SetActive(true);

        weaponNameText.text = itemData.itemName;
        descriptionText.text = itemData.description;

        if (itemData is Weapon_Item weapon)
        {
            WeaponDamage.text = "Damage: " + weapon.weaponDamage.ToString();
            Ammo.text = "Ammo: " + weapon.Max_Ammo.ToString();
            noiseText.text = "Noise: " + weapon.noiseLevel.ToString() + " dB";
        }
        else if (itemData is Armor_Item armor)
        {
            weaponNameText.text = armor.itemName;
            WeaponDamage.text = "Damage Resist: " + (armor.Armor_Block_Percentage * 100).ToString() + "%";
            descriptionText.text = "Armor Durability: " + armor.Max_Armor_Durability.ToString();
            Ammo.text = "Speed: " + armor.Movement_Speed_Multiplier.ToString() + "x";
            noiseText.text = "Noise Multiplier: " + armor.Noise_Multiplier.ToString() + "x";
        }
        else if (itemData is Consumable_Item ConSumeItem)
        {
            weaponNameText.text = ConSumeItem.itemName;
            WeaponDamage.text = "";
            descriptionText.text = $"Hp ++ : {ConSumeItem.healAmount} // MaxHp ++ : {ConSumeItem.MaxHP_Recover}";
            Ammo.text = "";
            noiseText.text = "";
        }
        else
        {
            WeaponDamage.text = "";
            Ammo.text = "";
            noiseText.text = "";
        }
    }

    // ฟังก์ชันสวมใส่ไอเทม (ผูกกับปุ่ม Select)
    public void selectItem()
    {
        if (currentSelect is Weapon_Item weapon)
        {
            if (weapon.type == Weapon_Item.WeaponType.Primary)
                Load_out_manager.Instance.selectedPrimaryWeapon = weapon;
            else
                Load_out_manager.Instance.selectedSecondaryWeapon = weapon;
        }
        else if (currentSelect is Armor_Item armor)
        {
            Load_out_manager.Instance.selectedArmor = armor;
        }
        else if (currentSelect is Consumable_Item SelectConsumeItem)
        {
            if (Load_out_manager.Instance.selectedConsumable_1 == null)
                Load_out_manager.Instance.selectedConsumable_1 = SelectConsumeItem;
            else
                Load_out_manager.Instance.selectedConsumable_2 = SelectConsumeItem;
        }

        // ใส่ของเสร็จปุ๊บ สั่งอัปเดตรูปภาพฝั่งขวาทันที!
        UpdateEquippedUI();
    }

    // ฟังก์ชันสำหรับดึงรูปภาพไอเทมมาโชว์บนช่อง UI ฝั่งขวา
    public void UpdateEquippedUI()
    {
        if (Load_out_manager.Instance == null) return;

        // 1. ปืนหลัก
        UpdateIcon(equippedPrimaryIcon, Load_out_manager.Instance.selectedPrimaryWeapon);

        // 2. ปืนรอง
        UpdateIcon(equippedSecondaryIcon, Load_out_manager.Instance.selectedSecondaryWeapon);

        // 3. ชุดเกราะ
        UpdateIcon(equippedArmorIcon, Load_out_manager.Instance.selectedArmor);

        // 4. ยาช่องที่ 1
        UpdateIcon(equippedConsumable1Icon, Load_out_manager.Instance.selectedConsumable_1);

        // 5. ยาช่องที่ 2
        UpdateIcon(equippedConsumable2Icon, Load_out_manager.Instance.selectedConsumable_2);
    }

    // ฟังก์ชันย่อยสำหรับจัดการเปลี่ยนรูปภาพและสีแบบอัตโนมัติ (เขียนแยกไว้จะได้ไม่เปลืองบรรทัด)
    private void UpdateIcon(Image slotIcon, Base_Item item)
    {
        if (slotIcon != null)
        {
            if (item != null && item.itemIcon != null)
            {
                slotIcon.sprite = item.itemIcon; // เอารูปจากไอเทมมาใส่
                slotIcon.color = Color.white;    // ปรับสีให้สว่างเต็มที่ 100%
            }
            else
            {
                slotIcon.sprite = null;          // ถ้าไม่มีของ ให้ล้างรูปทิ้ง
                slotIcon.color = new Color(1, 1, 1, 0.2f); // ปรับสีจางๆ (โปร่งใส 20%) ให้ดูรู้ว่าเป็นช่องว่าง
            }
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