using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WeaponInfo_prefab : MonoBehaviour
{
    public Image iconImage;
    private Base_Item myData;
    private Armor_Item armor;
    private LoadOut_UI_manager uiManager; // จำตัวผู้จัดการไว้ เพื่อตะโกนเรียกเวลากดปุ่ม

    // ฟังก์ชันนี้ทำงานตอน Manager สั่งเสกปุ่ม
    public void Setup(Base_Item data, LoadOut_UI_manager manager)
    {
        if (data is Weapon_Item weapon)
        {
            myData = weapon;
            uiManager = manager;
            iconImage.sprite = data.itemIcon;
        }

        if(data is Armor_Item armor)
        {
            myData = armor;
            uiManager = manager;
            iconImage.sprite = data.itemIcon;
        }
    }

    // ฟังก์ชันนี้เอาไปผูกกับปุ่ม OnClick() ใน Inspector
    public void OnButtonClicked()
    {
        // เวลากดปุ่ม จะตะโกนบอก Manager ว่า "เอ้ย เอารายละเอียดของปืนกระบอกนี้ไปโชว์หน่อย!"
        uiManager.ShowDetail(myData);
    }


    //public void OnClose()
    //{
        //uiManager.ClosePanel();
    //}

    
}
