using UnityEngine;

public class Enemy_Hitbox : MonoBehaviour
{
    [Header("การเชื่อมต่อ")]
    public Enemy mainEnemyScript; // ลากสคริปต์ Enemy ตัวแม่มาใส่ช่องนี้
    public enemy_stage EnemyStateScript;

    [Header("ตั้งค่าความเสียหาย")]
    public float damageMultiplier = 1.0f; // ตัวคูณดาเมจ (หัว = 2 หรือ 3, ตัว = 1)
    public float stealthMultiplier = 10.0f; // ตัวคูณพิเศษตอนศัตรูเผลอ (เช่น x10)
    public bool isHeadshot = false;       // ไว้เช็กColliderที่สคริปตัวนี้ติดอยู่เป็นส่วนหัวรึป่าวถ้าใช่ก็กดติ็กถูกในหน้า Inspector  หรือใช้ เพื่อแสดง UI หรือเสียงพิเศษ

    private void Start()
    {
        // 1. ดึงสคริปต์ Enemy จากตัวแม่
        if (mainEnemyScript == null)
        {
            mainEnemyScript = GetComponentInParent<Enemy>();
        }

        // 2. ดึงสคริปต์สถานะ (enemy_stage) จากตัวแม่มาใช้อัตโนมัติ
        if (EnemyStateScript == null)
        {
            EnemyStateScript = GetComponentInParent<enemy_stage>();
        }
    }

    // ฟังก์ชันนี้จะถูกเรียกจากปืนของผู้เล่นตอนยิงโดน
    public void OnHit(float baseWeaponDamage)
    {
        // 1. ตั้งต้นตัวคูณที่ 1 ไว้ก่อน
        float currentMultiplier = 1.0f;

        // 2. เช็กสถานะ Stealth ก่อนเป็นอันดับแรก (โดนตรงไหนก็คูณ ถ้าศัตรูยังไม่รู้ตัว)
        if (EnemyStateScript != null && EnemyStateScript.baseState == enemy_stage.EnemyState.Patrol)
        {
            currentMultiplier *= stealthMultiplier;
            Debug.Log("STEALTH HIT! โจมตีตอนศัตรูเผลอ x" + stealthMultiplier);
        }

        // 3. เช็กว่าเป็นจุดอ่อน (Headshot) หรือไม่ 
        if (isHeadshot)
        {
            currentMultiplier *= damageMultiplier; // คูณทบเข้าไปอีกชั้น
            Debug.Log("HEADSHOT! ดาเมจจุดอ่อน x" + damageMultiplier);
        }

        // 4. เอาดาเมจปืนมาคูณตัวคูณสุทธิ 
        // (ถ้ายิงตัวตอนตื่น = x1, ยิงตัวตอนเผลอ = x10, ยิงหัวตอนตื่น = x2, ยิงหัวตอนเผลอ = x20)
        float finalDamage = baseWeaponDamage * currentMultiplier;

        // 5. ส่งดาเมจไปหักเลือด
        if (mainEnemyScript != null)
        {
            mainEnemyScript.TakeDamage(finalDamage);
        }
    }
}

