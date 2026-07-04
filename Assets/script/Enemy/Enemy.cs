using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float Enemy_Health = 100;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float damageAmount)
    {
        Enemy_Health -= damageAmount;
        Debug.Log("ศัตรูโดนยิง! เลือดเหลือ: " + Enemy_Health);

        if (Enemy_Health <= 0)
        {
            Debug.Log("ศัตรูตายแล้ว!");
            // ใส่โค้ดทำลายตัวเอง หรือเล่นแอนิเมชันตายตรงนี้
            // Destroy(gameObject); 
        }
    }
}
