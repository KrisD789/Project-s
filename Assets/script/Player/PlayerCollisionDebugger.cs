using UnityEngine;

public class PlayerCollisionDebugger : MonoBehaviour
{
    // 1. ดักจับการชนแบบปกติ (Rigidbody ชนกับกล่อง/กำแพงทั่วไป)
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("<color=red> [ชนแบบปกติ]</color> ผู้เล่นเดินชนกับ: <b>" + collision.gameObject.name + "</b>");
    }

    // 2. ดักจับการชนแบบทะลุ (กล่องที่ติ๊ก Is Trigger ไว้)
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("<color=orange> [ชนแบบ Trigger]</color> ผู้เล่นเดินผ่าน: <b>" + other.gameObject.name + "</b>");
    }

    // 3. ดักจับการชนกรณีที่ผู้เล่นใช้ CharacterController (แทน Rigidbody)
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // ป้องกันไม่ให้มันรายงานชื่อ "พื้น" รัวๆ ตอนเดิน
        if (hit.gameObject.CompareTag("Untagged") || hit.gameObject.name == "Floor") return;

        Debug.Log("<color=yellow> [Character Controller]</color> ผู้เล่นเดินชนกับ: <b>" + hit.gameObject.name + "</b>");
    }
}