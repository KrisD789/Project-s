using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl : MonoBehaviour
{
    [Header("Cameras")]
    public GameObject aimCamera;        // ลากกล้องเล็งมาใส่
    public InputActionReference aimAction; // ปุ่มคลิกขวาเล็ง (จาก Input Action ใหม่)

    private CinemachinePanTilt aimPanTilt;

    // ต้องใส่ฟังก์ชันนี้ เพื่อเปิดการรับค่าเมาส์ตอนสคริปต์ทำงาน
    private void OnEnable()
    {
        if (aimAction != null)
        {
            aimAction.action.Enable();
        }
    }

    // ต้องใส่ฟังก์ชันนี้ เพื่อปิดการรับค่าตอนสคริปต์หยุดทำงาน (กันบั๊กค้าง)
    private void OnDisable()
    {
        if (aimAction != null)
        {
            aimAction.action.Disable();
        }
    }

    void Start()
    {
        if (aimCamera != null)
        {
            // ดึง Component Pan Tilt ของกล้องเล็งมารอไว้
            aimPanTilt = aimCamera.GetComponent<CinemachinePanTilt>();
            aimCamera.SetActive(false);
        }
    }

    void Update()
    {
       
        // 1. จังหวะที่ "เริ่มกดคลิกขวาเล็ง"
        if (aimAction.action.WasPressedThisFrame())
        {
            // สั่งให้กล้องเล็งก๊อปปี้องศาจากกล้องหลักทันที
            SyncCameraAngles();
            // แล้วค่อยเปิดใช้งานกล้องเล็ง
            aimCamera.SetActive(true);
        }

        // 2. จังหวะที่ "ปล่อยคลิกขวา"
        else if (aimAction.action.WasReleasedThisFrame())
        {
            aimCamera.SetActive(false);
        }
    }

    void SyncCameraAngles()
    {
        if (aimPanTilt == null) return;

        // ดึงค่าองศาปัจจุบัน (Euler Angles) ที่ตาของผู้เล่น (Main Camera) กำลังมองอยู่จริงในฉาก
        Vector3 mainCamRotation = Camera.main.transform.eulerAngles;

        float targetPan = mainCamRotation.y;  // มุมหัน ซ้าย-ขวา
        float targetTilt = mainCamRotation.x; // มุมก้ม เงย

        // ดักบั๊กของ Unity: ระบบมุมของ Unity จะนับเป็น 0-360 องศา 
        // แต่ระบบ Pan Tilt ของ Cinemachine จะนับเป็น -180 ถึง 180 องศา
        // ถ้ามุมเงยเกิน 180 (เช่น มุมเงยขึ้นฟ้าเป็น 340) ต้องลบออก 360 เพื่อให้กลายเป็น -20 องศา กล้องจะได้ไม่เอ๋อครับ
        if (targetTilt > 180)
        {
            targetTilt -= 360;
        }

        // ยัดค่าองศาของกล้องหลัก ใส่เข้าไปในสมองของกล้องเล็งโดยตรง!
        aimPanTilt.PanAxis.Value = targetPan;
        aimPanTilt.TiltAxis.Value = targetTilt;
    }

    
}
