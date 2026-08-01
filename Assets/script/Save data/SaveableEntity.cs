using UnityEngine;

public class SaveableEntity : MonoBehaviour
{
    [Tooltip("รหัสห้ามซ้ำกันเด็ดขาด (ระบบจะสุ่มให้เอง)")]
    public string uniqueID = "";

    // คลิกขวาที่สคริปต์นี้ใน Unity แล้วกด 'Generate New ID' เพื่อสุ่มรหัส
    [ContextMenu("Generate New ID")]
    private void GenerateID()
    {
        uniqueID = System.Guid.NewGuid().ToString();
        Debug.Log("สร้างรหัสใหม่ให้ " + gameObject.name + " เรียบร้อย!");
    }
}
