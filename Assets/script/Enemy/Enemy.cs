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
        if(gameObject.CompareTag("ScamCommander")) return;

        Enemy_Health -= damageAmount;
        Debug.Log("ศัตรูโดนยิง! เลือดเหลือ: " + Enemy_Health);

        if (Enemy_Health <= 0)
        {
            Debug.Log("ศัตรูตายแล้ว!");
            //enemy_Stage.currentState = enemy_stage.EnemyState.dead;
            Die();
        }

        else
        {
            // 1. บังคับเข้าโหมด Alert
            if (enemy_Stage.currentState != enemy_stage.EnemyState.Alert)
            {
                enemy_Stage.currentState = enemy_stage.EnemyState.Alert;
            }

            if (TryGetComponent<Enemy_Alert>(out Enemy_Alert enemyAlert))
            {
                if (Player.Instance != null)
                {
                    Vector3 playerPos = Player.Instance.transform.position;
                    enemyAlert.HandleNoiseAlert(playerPos);
                    enemyAlert.PointBlankShoot(playerPos);

                    // เพิ่มบรรทัดนี้: โดนยิงปุ๊บ ตะโกนบอกพิกัดให้เพื่อนทั้งแคมป์รู้ทันที!
                    enemyAlert.Start_TriggerGroupAlert();
                }
            }
        }
    }

    private void Die()
    {
        // แจ้ง Manager ว่ามีศัตรูร่วงไป 1 ตัวแล้วนะ!
        //MissionManager.Instance.OnEnemyEliminated();

        enemy_Stage.ChangeState(enemy_stage.EnemyState.dead);
    }
}
