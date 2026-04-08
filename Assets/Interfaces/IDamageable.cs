
using UnityEngine;
public interface IDamageable
{
    void TakeDamage(float damage, float knockback = 1f){}
    void Die(){}

}