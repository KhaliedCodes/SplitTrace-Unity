using UnityEngine;

public class ScreamAreaEffect : MonoBehaviour
{
    public float radius ;
    public float stunDuration;

    private void Start()
    {
        GetComponent<SphereCollider>().radius = radius;
    }
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            var controller = other.GetComponent<CustomThridPersonController>();
            if (controller != null)
            {
                controller.Stun(stunDuration); 
            }
        }
    }


}
