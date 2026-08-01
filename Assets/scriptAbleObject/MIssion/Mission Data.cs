using UnityEngine;

[CreateAssetMenu(fileName = "MissionData", menuName = "Scriptable Objects/MissionData")]
public class MissionData : ScriptableObject
{
    // ไม่ต้องมี Mission ID แล้ว! เพราะตัวไฟล์นี้แหละคือ ID ของมันเอง
    public string missionName;
    public string missionDescrip;
    public MissionType type;
    public bool isCompleted = false;
    public bool isRequiredForExit = true; // ตัวแปลไว้บอกว่าเควสนั้นๆ บังคับทำหรือไม่ ถ้าทำเควสที่บังคับยังไม่เสร็จ จะไม่สามารถออกจากด่านได้

    [Header("สำหรับเควสกำจัดศัตรู (EliminateEnemies)")]
    public int targetAmount = 0;   // จำนวนเป้าหมายที่ต้องกำจัด
    public int currentAmount = 0;  // จำนวนที่จัดการไปแล้ว

}
