using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedEffectController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FullScreenPassRendererFeature fullscreenSpeed;
    [SerializeField] private KartController playerKartController;

    private void Start()
    {
        DisableFullScreenSpeed();

        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnSubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if(playerKartController != null)
        {
            playerKartController.OnBoostStarted += EnableFullScreenSpeed;
            playerKartController.OnBoostEnded += DisableFullScreenSpeed;
        }
    }

    private void UnSubscribeFromEvents()
    {
        if (playerKartController != null)
        {
            playerKartController.OnBoostStarted -= EnableFullScreenSpeed;
            playerKartController.OnBoostEnded -= DisableFullScreenSpeed;
        }
    }

    private void EnableFullScreenSpeed() => fullscreenSpeed?.SetActive(true);
    private void DisableFullScreenSpeed() => fullscreenSpeed?.SetActive(false);
}
