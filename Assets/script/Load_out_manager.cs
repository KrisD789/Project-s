using UnityEngine;

public class Load_out_manager : MonoBehaviour
{
    public static Load_out_manager Instance; // ใช้ Singleton เพื่อให้เรียกใช้ง่ายๆ

    [Header("อุปกรณ์ที่เลือกไว้สำหรับภารกิจ")]
    public item selectedPrimaryWeapon;
    //public item selectedSecondaryWeapon;
    //public item selectedGadget;
    //public item selectedSuit; // เช่น ชุดพรางตัวที่ลดการสะท้อนแสง

    void Awake()
    {
        // ป้องกันไม่ให้มี Manager ซ้ำซ้อนเวลาโหลดด่านใหม่
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // <--- คำสั่งศักดิ์สิทธิ์ที่ทำให้ข้อมูลไม่หาย
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        print(selectedPrimaryWeapon.itemName);
    }
}
