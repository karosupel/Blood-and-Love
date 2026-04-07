using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Stats/Enemy")]
public class EnemyStats : ScriptableObject
{
    public float health;
    public float damage;

    public float moveSpeed;

    public float knockbackForce;

    public float attackRange;

    public float attackCooldown;

    public float retreatRange; //a distance at which the enemy will switch to retreating if the player is too close

    public float retreatDistance; //a distance from the player that the enemy will try to maintain when retreating
}
