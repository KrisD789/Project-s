using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WeaponInfo_prefab : MonoBehaviour
{
    public Image iconImage;
    private item myData;
    private LoadOut_UI_manager uiManager; // จำตัวผู้จัดการไว้ เพื่อตะโกนเรียกเวลากดปุ่ม

    // ฟังก์ชันนี้ทำงานตอน Manager สั่งเสกปุ่ม
    public void Setup(item data, LoadOut_UI_manager manager)
    {
        myData = data;
        uiManager = manager;
        iconImage.sprite = data.itemIcon;
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
