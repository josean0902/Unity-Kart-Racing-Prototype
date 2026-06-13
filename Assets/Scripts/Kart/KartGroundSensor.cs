using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartGroundSensor : MonoBehaviour
{


    [SerializeField] private LayerMask trackLayer;

    public RaycastHit GroundHit => groundHit;

    private RaycastHit groundHit;

    private void Update()
    {
        RaycastGround();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        // Raycast draw
        float distance = 2.0f;
        Vector3 raycastStart = transform.position + Vector3.up;
        Vector3 raycastEnd = raycastStart + Vector3.down * distance;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(raycastStart, raycastEnd);

        // Ground normal draw
        Vector3 normalStart = groundHit.point;
        Vector3 normalEnd = groundHit.point + groundHit.normal;


        Gizmos.color = Color.blue;
        Gizmos.DrawLine(normalStart, normalEnd);
    }

    private void RaycastGround()
    {
        Vector3 origin = transform.position + Vector3.up;
        Vector3 direction = Vector3.down;
        float maxDistance = 50.0f;

        Physics.Raycast(origin, direction, out groundHit, maxDistance, trackLayer);
    }

}
