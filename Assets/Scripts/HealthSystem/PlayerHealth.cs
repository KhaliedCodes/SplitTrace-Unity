using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamagable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Image healthBarFill;
    [SerializeField] int HealthItemsAmount = 0;
    [SerializeField] float HealAmount = 10f;
    [SerializeField] TextMeshProUGUI healthCounterText;

    public float Health { get; set; }
    public int _HealthItemsAmount { get => HealthItemsAmount;}
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }

    private void Start()
    {
        Health = 50f;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        Health = Mathf.Max(0, Health - damage);
        UpdateHealthUI();
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
            healthBarFill.fillAmount = Health / MaxHealth;
    }

    private void Die()
    {
        Debug.Log("Player died.");
        // TODO: Handle death animation, game over screen, etc.
    }

    public void UpdateNumberOfHealthItem(int i)
    {
        //Update health number Who player have
        HealthItemsAmount += i;
        UpdateHealthCounterUI();

    }

    public  void IncreaseHealth()
    {
        if (Health < 100f)
        {
            if (HealthItemsAmount > 0)
            {
                Health += HealAmount;
                HealthItemsAmount -= 1;
                UpdateHealthUI();
                UpdateHealthCounterUI();
                Debug.Log("You Heal yourself");

            }
            Debug.Log("you don`t have Health Item");
        }
        else {
            Debug.Log("Health is Full");
        }

    }
    public void UpdateHealthCounterUI() {
        healthCounterText.text = HealthItemsAmount.ToString();
    }
   
}
