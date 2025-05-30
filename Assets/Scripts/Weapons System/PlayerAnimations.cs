using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

    }

    public void SetAnimation(string anim)
    {
        if (animator) animator.SetTrigger(anim);
        
    }


}
