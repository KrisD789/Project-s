using UnityEngine;

public class Light_Prob_count : MonoBehaviour
{
    void Start()
    {
        // ต้องทำการ Bake แสง (Generate Lighting) ไปแล้ว ค่าถึงจะไม่เป็น null
        LightProbes bakedProbes = LightmapSettings.lightProbes;

        if (bakedProbes != null)
        {
            int totalCount = bakedProbes.count;
            Debug.Log("จำนวน Light Probe ที่ Bake แล้วทั้งฉาก: " + totalCount);
        }
        else
        {
            Debug.LogWarning("ยังไม่ได้ Bake แสง หรือไม่มี Light Probe ในฉากเลย");
        }
    }
}
