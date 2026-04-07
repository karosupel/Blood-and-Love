
public interface IDamageable
{
    float MaxHealth { get; }
    float CurrentHealth { get; }
    void TakeDamage(float damage);
    void Heal(float amount);
    void Die();
}
