using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Variables")]
    [SerializeField] int healthNumber=2;
    [SerializeField] float ammountPerHealth = 0.3f;
    [SerializeField] float maxHealth = 1;
    [SerializeField] float currentHealth;

    [Header("Health UI")]
    [SerializeField] Image healthBar;

    private void Start()
    {
        SetHealthValueUI();
    }
   public void UpdateNumberOfHealthItem() {
        //Update health number Who player have
        healthNumber += 1;
   }
    void SetHealthValueUI() {
        healthBar.fillAmount = currentHealth;
    }

    void IncreaseHealth() {
        if (currentHealth < 1.0f) {
            if (healthNumber > 0) {
                currentHealth += ammountPerHealth;
                SetHealthValueUI();
                healthNumber -= 1;
            }
        }
    
    }
}
