using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour ,IDamagable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Image healthBarFill;

    public float Health { get; set; }
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }

    private void Start()
    {
        Health = MaxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        Health = Mathf.Max(0, Health - damage);
        UpdateHealthUI();
        Debug.Log("Player took damage: " + damage + ", Current Health: " + Health);
        if (Health <= 0)
        {
            Die();
        }
    }

    public float UpdateHealth(float health, float damage)
    {
        Health = Mathf.Clamp(health - damage, 0, MaxHealth);
        UpdateHealthUI();
        return Health;
    }

    private void UpdateHealthUI()
    {
        if (healthBarFill != null)
            healthBarFill.transform.localScale = new Vector3(Health / MaxHealth, 1, 1); 
    }

    private void Die()
    {
        Debug.Log("Player died.");
        // TODO: Handle death animation, game over screen, etc.
    }
}
