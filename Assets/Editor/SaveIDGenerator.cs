#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SaveIDGenerator
{
    // สร้างเมนูใหม่ชื่อ Tools ไว้ด้านบนสุดของหน้าต่าง Unity
    [MenuItem("Tools/Generate All Save IDs")]
    public static void GenerateIDs()
    {
        // 1. กวาดหา GameObject ทุกชิ้นในฉากที่มีสคริปต์ SaveableEntity แปะอยู่ (ใช้คำสั่งใหม่ที่เร็วกว่าเดิม)
        SaveableEntity[] allEntities = Object.FindObjectsByType<SaveableEntity>(FindObjectsSortMode.None);

        // เอาไว้จดจำว่ารหัสไหนถูกใช้ไปแล้วบ้าง
        HashSet<string> usedIDs = new HashSet<string>();
        int updatedCount = 0;

        foreach (SaveableEntity entity in allEntities)
        {
            // 2. เช็คว่าถ้า ID ว่างเปล่า หรือ ID ดันไปซ้ำกับของคนอื่นที่ลงทะเบียนไปแล้ว
            if (string.IsNullOrEmpty(entity.uniqueID) || usedIDs.Contains(entity.uniqueID))
            {
                // สร้างรหัสใหม่ให้มันซะ!
                entity.uniqueID = System.Guid.NewGuid().ToString();

                // สั่งให้ Unity รับรู้ว่ามีการเปลี่ยนแปลงข้อมูล จะได้เซฟลง Scene ได้
                EditorUtility.SetDirty(entity);
                updatedCount++;
            }

            // 3. เอา ID ที่ไม่ซ้ำแล้ว จดลงสมุดบัญชีไว้เช็คกับตัวถัดไป
            usedIDs.Add(entity.uniqueID);
        }

        // แจ้งเตือนเมื่อทำงานเสร็จ
        Debug.Log($"<color=green><b>ตรวจสอบและสร้าง ID ใหม่ให้วัตถุจำนวน {updatedCount} ชิ้น สำเร็จ!</b></color>");
    }
}
#endif