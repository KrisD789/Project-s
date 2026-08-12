using System.Collections.Generic;
using UnityEngine;

public class Enemy_combatManager : MonoBehaviour
{
    public static Enemy_combatManager Instance { get; private set; }

    [Header("Token Settings")]
    public int maxCloseCombatTokens = 2; // จำนวนศัตรูที่ยอมให้รุมผู้เล่นพร้อมกัน

    // ลิสต์จดชื่อศัตรูที่กำลังถือตั๋วเข้าตีอยู่
    public List<GameObject> currentAttackers = new List<GameObject>();
    

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 1. ฟังก์ชันให้ศัตรู "ขอตั๋ว" เข้าตี
    public bool RequestAttackToken(GameObject enemy)
    {
        // ถ้าศัตรูตัวนี้มีตั๋วอยู่แล้ว ก็ลุยต่อได้เลย
        if (currentAttackers.Contains(enemy)) return true;

        // ถ้าตั๋วยังเหลือ ให้แจกตั๋วแล้วบันทึกชื่อไว้
        if (currentAttackers.Count < maxCloseCombatTokens)
        {
            currentAttackers.Add(enemy);
            Debug.Log($"{enemy.name} ได้รับตั๋วเข้าปะทะ! (ตั๋วถูกใช้ {currentAttackers.Count}/{maxCloseCombatTokens})");
            return true;
        }

        // คิวเต็ม! แจ้งให้ศัตรูกลับไปยืนรอ
        return false;
    }

    // 2. ฟังก์ชัน "คืนตั๋ว" (เมื่อศัตรูตาย, โดนสตัน, หรือหลุดสายตา)
    public void ReleaseToken(GameObject enemy)
    {
        if (currentAttackers.Contains(enemy))
        {
            currentAttackers.Remove(enemy);
            Debug.Log($"{enemy.name} คืนตั๋วแล้ว (ตั๋วว่างเหลือ {maxCloseCombatTokens - currentAttackers.Count} ใบ)");
        }
    }

    //  1. ฟังก์ชันเช็คว่าศัตรูตัวนี้มีตั๋วอยู่ไหม (ไว้ใช้ตอน Save)
    public bool HasToken(GameObject enemy)
    {
        return currentAttackers.Contains(enemy);
    }

    //  2. ฟังก์ชันยัดตั๋วคืนเข้าลิสต์ (ไว้ใช้ตอน Load)
    public void RestoreToken(GameObject enemy)
    {
        if (!currentAttackers.Contains(enemy))
        {
            currentAttackers.Add(enemy);
            Debug.Log($"[โหลดเซฟ] คืนตั๋วเข้าปะทะให้ {enemy.name}");
        }
    }

    //  3. ล้างตั๋วทั้งหมด (ควรเรียกใช้ใน SaveManager ก่อนเริ่มโหลดด่านใหม่ ป้องกันตั๋วค้าง)
    public void ClearAllTokens()
    {
        currentAttackers.Clear();
    }
}
