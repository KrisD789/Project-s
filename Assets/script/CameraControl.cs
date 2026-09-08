using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl : MonoBehaviour
{
    [Header("Cameras")]
    public GameObject mainCamera;
    public GameObject aimCamera;        // ลากกล้องเล็งมาใส่
    public InputActionReference aimAction; // ปุ่มคลิกขวาเล็ง (จาก Input Action ใหม่)

    private CinemachinePanTilt aimPanTilt;
    private CinemachineThirdPersonFollow aimFollow;
    //private CinemachinePanTilt Left_aimPanTilt;
    private CinemachineOrbitalFollow mainOrbitalFollow;

    public Transform aimCameraMount; // ลาก GameObject ที่กล้องเกาะอยู่มาใส่
    private bool isRightShoulder = true;

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
            aimFollow = aimCamera.GetComponent<CinemachineThirdPersonFollow>();
            aimCamera.SetActive(false);
        }

        if (mainCamera != null)
        {
            // ดึง Component ของกล้องหลัก (ถ้ากล้องหลักของคุณใช้ Component อื่นที่ไม่ใช่ PanTilt ให้เปลี่ยนชื่อตรงนี้)
            mainOrbitalFollow = mainCamera.GetComponent<CinemachineOrbitalFollow>();
        }
    }

    void Update()
    {
        
    }

    public void HandleAim(bool HoldPress)
    {
        if (HoldPress)
        {
            if (Player.Instance.currentState == Player.PlayerState.Idle)
            {
                Player.Instance.currentState = Player.PlayerState.Aim;
                // สั่งให้กล้องเล็งก๊อปปี้องศาจากกล้องหลักทันที
                SyncCameraAngles();
                // แล้วค่อยเปิดใช้งานกล้องเล็ง
                aimCamera.SetActive(true);
            }
        }
        else
        {
            // เช็คก่อนว่ากำลังอยู่ในสถานะเล็งจริงๆ ถึงจะทำการคืนค่า
            if (Player.Instance.currentState == Player.PlayerState.Aim)
            {
                SyncAimToMainCamera(); // ซิงค์ค่าจากกล้องเล็งกลับไปให้กล้องหลักก่อน!
                Player.Instance.currentState = Player.PlayerState.Idle;
                aimCamera.SetActive(false);
            }
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

    void SyncAimToMainCamera()
    {
        if (aimPanTilt == null || mainOrbitalFollow == null) return;

        // แกนแนวนอน (ซ้าย-ขวา)
        mainOrbitalFollow.HorizontalAxis.Value = aimPanTilt.PanAxis.Value;

        // แกนแนวตั้ง (ก้ม-เงย)
        mainOrbitalFollow.VerticalAxis.Value = aimPanTilt.TiltAxis.Value;
    }

    public void ApplyRecoil(float verticalRecoil, float horizontalRecoil)
    {
        // เช็กก่อนว่ากล้องเล็งเปิดอยู่ไหม
        if (aimPanTilt != null && aimCamera.activeSelf)
        {
            // ลบค่า Tilt (แกนก้มเงย) เพื่อให้กล้องเชิดหน้าขึ้น (เป้าดีดขึ้นฟ้า)
            aimPanTilt.TiltAxis.Value -= verticalRecoil;

            // สุ่มบวก/ลบค่า Pan (แกนซ้ายขวา) เพื่อให้ปืนส่ายออกข้างแบบสุ่ม
            aimPanTilt.PanAxis.Value += Random.Range(-horizontalRecoil, horizontalRecoil);
        }
    }

    public void SwapShoulder()
    {
        if (aimCamera != null && Player.Instance.currentState == Player.PlayerState.Aim)
        {
            isRightShoulder = !isRightShoulder; // สลับสถานะ

            // ดึงค่า Offset เดิมมา
            Vector3 currentOffset = aimFollow.ShoulderOffset;

            // ถ้าเป็นไหล่ขวา ให้ค่า X เป็นบวก, ถ้าซ้ายให้เป็นลบ (สมมติระยะห่างคือ 0.5f)
            currentOffset.x = isRightShoulder ? 1f : -1f;

            // ใส่ค่ากลับคืนไป
            aimFollow.ShoulderOffset = currentOffset;
        }
    }
}
