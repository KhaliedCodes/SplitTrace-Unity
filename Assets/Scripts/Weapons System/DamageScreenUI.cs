using UnityEngine;
using UnityEngine.UI;

public class DamageScreenUI : MonoBehaviour
{
    public Image damageEffect;
    public float fadeDuration = 0.3f;
    public float damageFadeSpeed = 3f;
    public static DamageScreenUI Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if(damageEffect.color.a != 0)
        {
            damageEffect.color = new Color(damageEffect.color.r, damageEffect.color.g, damageEffect.color.b, Mathf.MoveTowards(damageEffect.color.a, 0, Time.deltaTime * damageFadeSpeed));
        }
    }
    public void ShowDamageEffect()
    {
        damageEffect.color = new Color(damageEffect.color.r, damageEffect.color.g, damageEffect.color.b, 0.3f);
    }

}
