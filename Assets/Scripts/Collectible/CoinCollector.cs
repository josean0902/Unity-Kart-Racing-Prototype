using UnityEngine;

public class CoinCollector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CoinManager coinManager;

    [Header("Layer")]
    [SerializeField] private LayerMask coinLayer;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & coinLayer.value) == 0)
            return;

        if (coinManager != null)
        {
            coinManager.CollectCoin(other.transform.position);
            Destroy(other.gameObject);
        }
    }
}
