using UnityEngine;

public class TUT : MonoBehaviour
{
    public TutorialData Weapontutorial;
    public TutorialData Enemytutorial;

    //private bool hasTriggeredEnemy = false;
    //private bool hasTriggeredWeapon = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (UiManager.Instance != null && UiManager.Instance.IsTutorialActive())
            return;
      

    }

    //public void OnTutorialStart(string objectEntered)
    //{
    //    if (objectEntered == "Enemy")
    //    {
    //        if (!hasTriggeredEnemy && !UiManager.Instance.HasShownTutorial(Enemytutorial.uniqueKey))
    //        {
    //            hasTriggeredEnemy = true;
    //            UiManager.Instance.ShowTutorial(Enemytutorial);
    //        }
    //    }

    //    if (objectEntered == "Weapon")
    //    {
    //        if (!hasTriggeredWeapon && !UiManager.Instance.HasShownTutorial(Weapontutorial.uniqueKey))
    //        {
    //            hasTriggeredWeapon = true;
    //            UiManager.Instance.ShowTutorial(Weapontutorial);
    //        }
    //    }
    //}

    public void OnTutorialStart(string objectEntered)
    {
        if (objectEntered == "Enemy" && !UiManager.Instance.HasShownTutorial(Enemytutorial.uniqueKey))
        {
            UiManager.Instance.ShowTutorial(Enemytutorial);
        }

        if (objectEntered == "Weapon" && !UiManager.Instance.HasShownTutorial(Weapontutorial.uniqueKey))
        {
            UiManager.Instance.ShowTutorial(Weapontutorial);
        }
    }

    public void OnTutorialEnd()
    {
        UiManager.Instance.HideTutorial();
        Destroy(gameObject,0.5f);
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag ("Enemy"))
        {
            OnTutorialStart("Enemy");
        } 
        
    }


}
