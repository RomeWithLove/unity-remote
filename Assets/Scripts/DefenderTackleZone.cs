using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DefenderTackleZone : MonoBehaviour
{
    private DefenderController mainController;

    private void Awake()
    {
        mainController = GetComponentInParent<DefenderController>();
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && mainController != null)
        {
            mainController.OnPlayerDetected(other.transform);
        }
    }
}