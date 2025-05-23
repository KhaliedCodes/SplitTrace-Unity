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


   void UpdateHealthAmount() {
        //Update health of player and UI
        if (currentHealth < 1.0f) { 

            currentHealth += ammountPerHealth;
            healthBar.fillAmount += currentHealth;
        }
   }
   public void UpdateNumberOfHealthItem() {
        //Update health number Who player have
        healthNumber += 1;


    }
}
