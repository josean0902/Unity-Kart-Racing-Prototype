using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public event Action<int> OnCoinCollected;

    [Header("VFX")]
    [SerializeField] private ParticleSystem pickupParticleSytem;

    [Header("Audio")]
    [SerializeField] private AudioSource pickupCoinAudioSource;
    [SerializeField] private Color pickupCoinColor = Color.yellow;
    [SerializeField] private float intensity = 2.5f;

    [Header("Pitch")]
    [SerializeField] private float pitchBase = 1.0f;
    [SerializeField] private float pitchIncrement = 0.1f;
    [SerializeField] private float timeBetweenPickupCoins = .5f;

    private float pitchIncrementTimer = 0.0f;

    private MaterialPropertyBlock materialPropertyBlock;

    private int collectedCoins = 0;

    private void Start()
    {
        SetupPickupCoinColor();
    }

    private void Update()
    {
        HandleCoinPickupPitch();
    }

    public void CollectCoin(Vector3 position)
    {
        collectedCoins++;

        if (pickupParticleSytem != null)
        {
            pickupParticleSytem.transform.position = position;
            pickupParticleSytem.Play();
        }

        if (pickupCoinAudioSource != null)
        {
            pickupCoinAudioSource.transform.position = position;
            pickupCoinAudioSource.Play();

            pitchIncrementTimer = timeBetweenPickupCoins;
            pickupCoinAudioSource.pitch += pitchIncrement;
        }

        OnCoinCollected?.Invoke(collectedCoins);

    }

    private void SetupPickupCoinColor()
    {
        materialPropertyBlock = new MaterialPropertyBlock();

        ParticleSystemRenderer pickupParticleSytemRenderer =
            pickupParticleSytem.GetComponent<ParticleSystemRenderer>();

        pickupParticleSytemRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetColor("_EmissionColor", pickupCoinColor * intensity);
        pickupParticleSytemRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private void HandleCoinPickupPitch()
    {
        pitchIncrementTimer -= Time.deltaTime;

        pitchIncrementTimer = Mathf.Max(pitchIncrementTimer, 0.0f);
        if (pitchIncrementTimer == 0.0f)
            pickupCoinAudioSource.pitch = pitchBase;
    }
}
