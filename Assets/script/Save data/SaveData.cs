using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SaveData
{
    [Header("ข้อมูลสำหรับโชว์หน้า UI")]
    public string saveTime;
    public string currentSceneName;

    [Header("ข้อมูลผู้เล่น (ถ้ามี)")]
    public Vector3 playerPosition;
    public Vector3 playerRotation;

    [Header("ข้อมูลสิ่งของในฉาก (ศัตรู, ประตู, สวิตช์)")]
    // เก็บ "รหัส" ของสิ่งของนั้นๆ
    public List<string> savedObjectIDs = new List<string>();

    // เก็บ "ข้อมูล" ของสิ่งของนั้นๆ (แปลงเป็นข้อความ JSON ไว้แล้ว)
    public List<string> savedObjectStates = new List<string>();
}
