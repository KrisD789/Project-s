using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float Enemy_Health = 100;
    enemy_stage enemy_Stage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TryGetComponent<enemy_stage>(out enemy_Stage);
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
            enemy_Stage.currentState = enemy_stage.EnemyState.dead;
            Die();
        }
    }

    private void Die()
    {
        // แจ้ง Manager ว่ามีศัตรูร่วงไป 1 ตัวแล้วนะ!
        MissionManager.Instance.OnEnemyEliminated();
    }
}
