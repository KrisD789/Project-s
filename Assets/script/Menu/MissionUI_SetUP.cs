using TMPro;
using UnityEngine;

public class MissionUI_SetUP : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI missionNameText; // ช่องแสดงชื่อเควสต์
    public TextMeshProUGUI statusText;      // ช่องแสดงสถานะ (เช่น กำลังทำ / สำเร็จ)

    // ฟังก์ชันนี้จะถูกเรียกใช้โดยลูป เพื่อโยนข้อมูลภารกิจมาให้
    public void Setup(MissionData data)
    {
        missionNameText.text = data.missionName;

        if (data.isCompleted)
        {
            statusText.text = "<color=green>Complete</color>";
        }
        else
        {
            // ตรวจสอบประเภทเควสต์ ถ้าเป็นเควสต์กำจัดศัตรู ให้โชว์ตัวเลขด้วย
            if (data.type == MissionType.EliminateEnemies)
            {
                statusText.text = $"<color=yellow> InProgress ({data.currentAmount}/{data.targetAmount})</color>";
            }
            else
            {
                statusText.text = "<color=yellow> InProgress </color>";
            }
        }
    }
}
