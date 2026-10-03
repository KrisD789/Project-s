using UnityEngine;
using UnityEngine.AI;

public class Enemy_Anime_Controller : MonoBehaviour
{
    private Animator anim;
    private NavMeshAgent agent;
    private enemy_stage enemyScript;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        enemyScript = GetComponent<enemy_stage>();
    }

    void Update()
    {
        if (anim == null) return;

        if (agent != null && agent.isActiveAndEnabled)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }

        if (enemyScript != null)
        {
            anim.SetBool("isFainted", enemyScript.currentState == enemy_stage.EnemyState.faint);
            anim.SetBool("isDead", enemyScript.currentState == enemy_stage.EnemyState.dead);
        }
    }

    public void PlayGrabbed() { if (anim != null) anim.SetTrigger("OnGrabbed"); }
    public void PlayDie() { if (anim != null) anim.SetTrigger("Die"); }
}
