using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FinishLineTrigger : MonoBehaviour
{
    [SerializeField] private GameObject victoryPanel;

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
            Time.timeScale = 0f;
        }
    }
}