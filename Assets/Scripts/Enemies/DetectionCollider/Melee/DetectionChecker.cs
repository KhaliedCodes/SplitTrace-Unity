using UnityEngine;

public class DetectionChecker : MonoBehaviour
{
   [SerializeField] private Enemy _enemy;

    public void Initialize(float range, Enemy enemy)
    {
        _enemy = enemy;
        GetComponent<SphereCollider>().radius = range;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _enemy.SetPlayerInDetectionRange(true) ;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _enemy.SetPlayerInDetectionRange(false);
        }
    }

}
