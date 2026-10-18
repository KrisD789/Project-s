using UnityEngine;

public class EnemyAudioController : MonoBehaviour
{
    private AudioSource enemySource;

    [Header("ไฟล์เสียงของศัตรู (ลากมาใส่ได้เลย)")]
    public AudioClip shootClip;      // เสียงปืนศัตรู
    public AudioClip suspiciousClip; // เสียงร้อง "หึ?", "ใครน่ะ?" (สงสัย)
    public AudioClip alertClip;      // เสียงร้อง "เจอตัวแล้ว!", "ยิงมัน!" (ตื่นตัว)
    public AudioClip hurtClip;       // เสียงร้องตอนโดนยิง
    public AudioClip deathClip;      // เสียงตอนตาย

    void Start()
    {
        // ให้สคริปต์วิ่งหาลำโพงที่ติดอยู่ที่ตัวศัตรูเอง
        enemySource = GetComponent<AudioSource>();
    }

    // ฟังก์ชันสั่งเล่นเสียงปืน (สุ่มความสูงต่ำของเสียงนิดนึงให้เป็นธรรมชาติ)
    public void PlayShootSound()
    {
        if (shootClip != null && enemySource != null)
        {
            enemySource.pitch = Random.Range(0.9f, 1.1f);
            enemySource.PlayOneShot(shootClip);
        }
    }

    // ฟังก์ชันสั่งเล่นเสียงร้องตกใจ/สงสัย
    public void PlaySuspiciousSound()
    {
        if (suspiciousClip != null && enemySource != null)
        {
            enemySource.pitch = 1f;
            enemySource.PlayOneShot(suspiciousClip);
        }
    }

    // เพิ่มฟังก์ชันเล่นเสียงร้องอื่นๆ ตามต้องการได้เลยครับ
    public void PlayAlertSound()
    {
        if (alertClip != null && enemySource != null)
        {
            enemySource.pitch = 1f;
            enemySource.PlayOneShot(alertClip);
        }
    }
}