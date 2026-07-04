using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public float Health = 100f;

    public float Armor;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        //print("Player_Health: " + Health);
    }

    public void Player_TakeDamage(float E_Weapon_Damage)
    {
        if (Health > 0)
        {
            Health -= E_Weapon_Damage;
        }

        else Debug.Log("Player : กูตายแล้วยิงอะไรเยอะแยะ  ");
    }
}
