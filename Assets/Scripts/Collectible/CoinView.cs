using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinView : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 45.0f;

    private void Update()
    {
        HandleRotation();
    }

    private void HandleRotation()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
