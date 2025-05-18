using UnityEngine.UI;

public interface IDamagable 
{
    float _Damage { set; get; }
    float _Health { set; get; }
    float _MaxHealth { set; get; }
    Image _Image { set; get; }
    float UpdateHealth(float health,float damage);
    void UpdateHealthBar();
    void TakeDamage(float damage);
}
