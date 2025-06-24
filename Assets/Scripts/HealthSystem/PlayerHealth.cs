using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

    bool isDead = false;
    private void Start()
    {
        Health = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        Health = Mathf.Max(0, Health - damage);
        UpdateHealthUI();
        if (Health <= 0 && !isDead)
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
        isDead = true;
        Debug.Log("Player died.");
        GetComponent<PlayerInput>().enabled = false;
        
        WeaponManager.Instance.DropCurrentWeapon();

        WeaponManager.Instance.animator.SetTrigger("Die");
        StartCoroutine(LoadManinMenu());
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
   
    IEnumerator LoadManinMenu()
    {
        yield return new WaitForSeconds(5f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
