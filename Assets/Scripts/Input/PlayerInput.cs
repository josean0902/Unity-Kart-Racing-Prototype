using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{

    public event Action OnHoldPressed;
    public event Action OnHoldReleased;

    private bool wasHolding;

    private void Update()
    {
        bool isHolding = Input.GetMouseButton(0);

        if (!wasHolding && isHolding)
        {
            OnHoldPressed?.Invoke();
        }

        if(wasHolding && !isHolding)
        {
            OnHoldReleased?.Invoke();
        }

        wasHolding = isHolding;
    }

}
