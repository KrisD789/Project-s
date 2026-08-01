using System;
using Unity.VisualScripting;
using UnityEngine;

public class Door : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public enum DoorState {Open, Closed, Locked, Tutorial }
    public DoorState currentState = DoorState.Closed;

    [Header("Settings")]
    public float openAngle = 90f;
    public float smoothSpeed = 5f;
    public float checkRadius = 3f;

    private Quaternion closedRot;
    private Quaternion openRot;
    private LayerMask EnemyLayer;

    public bool openByAi;

    void Awake()
    {
        // เก็บค่าเริ่มต้นไว้เป็นท่าปิด
        closedRot = transform.localRotation;
        openRot = Quaternion.Euler(transform.localEulerAngles + new Vector3(0, openAngle, 0));
        EnemyLayer = LayerMask.GetMask("enemyInteraction");
    }

    void Update()
    {
        // ใช้ Enum ในการเช็คว่าจะหมุนไปท่าไหน
        Quaternion targetRot = (currentState == DoorState.Open) ? openRot : closedRot;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * smoothSpeed);

        
    }

    public void ToggleDoor(bool isAi, DoorState state)
    {
        // สร้างทรงกลมรัศมี 2-3 เมตรรอบประตู เพื่อเช็ก Layer "enemy"
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, checkRadius, EnemyLayer);
        Debug.Log("hitCollider Lenght ; " + hitColliders.Length);

        // 1. เช็กก่อนว่าถ้าปัจจุบันล็อกอยู่ จะเปลี่ยนสถานะไม่ได้ (ยกเว้นสั่งมาเพื่อปลดล็อก)
        if (currentState == DoorState.Locked ) return;

        if (state == DoorState.Closed )
        {
            if (hitColliders.Length > 1)
            {
                return;
            }
        }

        // 2. อัปเดตสถานะและบันทึกว่าใครเป็นคนสั่ง
        currentState = state;
        openByAi = isAi;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }

    public void Automatic_Door_for_Tutorial_Map()
    {
        currentState = DoorState.Open;
    }
}
