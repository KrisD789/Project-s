using UnityEditor;
using UnityEngine;

public class LightModeChanger : EditorWindow
{
    [MenuItem("Tools/Change All Mixed to Baked")]
    public static void ChangeMixedToBaked()
    {
        // ใช้คำสั่งใหม่ตามที่ Unity แนะนำ และตั้งค่า None เพื่อให้สแกนหาไฟได้ไวที่สุด
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        int count = 0;

        foreach (Light light in allLights)
        {
            if (light.lightmapBakeType == LightmapBakeType.Mixed)
            {
                Undo.RecordObject(light, "Change Light Mode");
                light.lightmapBakeType = LightmapBakeType.Baked;
                count++;
            }
        }

        Debug.Log($"เปลี่ยนไฟจาก Mixed เป็น Baked สำเร็จจำนวน {count} ดวง!");
    }
}