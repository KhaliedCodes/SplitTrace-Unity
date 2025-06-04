using UnityEngine;

public class StartSettingsMenuTransition : MonoBehaviour
{

    public Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
          animator = GetComponent<Animator>();  
    }

    public void GoSettings()
    {
        animator.SetTrigger("Settings");
    }
    
    public void GoBack()
    {
        animator.SetTrigger("Back");
    }




}
