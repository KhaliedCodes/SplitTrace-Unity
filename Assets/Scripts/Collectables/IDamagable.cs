using UnityEngine.UI;

public interface IDamagable
{
    float Health { set; get; }
    float MaxHealth { set; get; }
    float UpdateHealth(float health, float damage);
    void TakeDamage(float damage);
}
