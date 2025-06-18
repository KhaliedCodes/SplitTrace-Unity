using UnityEngine;

public class ScreamAreaEffect : MonoBehaviour
{
    public float radius ;
    public float stunDuration;
    public CustomThridPersonController controller;
    private void Start()
    {
        GetComponent<SphereCollider>().radius = radius;

    }
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
                controller = other.GetComponent<CustomThridPersonController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            controller = null;
          
        }
    }


}
