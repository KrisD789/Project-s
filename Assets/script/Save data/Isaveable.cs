using UnityEngine;

public interface Isaveable
{
    // ใช้ส่งรหัสบัตรประชาชนของตัวเอง
    string GetSaveID();

    // ใช้แพ็คข้อมูลของตัวเองส่งมาให้ (ส่งมาเป็นข้อความ)
    string SaveState();

    // ใช้รับข้อมูลในอดีตไปตั้งค่าให้ตัวเอง (รับเป็นข้อความ)
    void LoadState(string stateData);
}
