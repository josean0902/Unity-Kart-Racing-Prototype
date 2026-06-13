using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TrackBuilder : MonoBehaviour
{

    [SerializeField] private Transform waypointParent;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private int resolution = 4;

    public List<Vector3> TrackPoints { get; private set; }

    private List<Vector3> splinePoints = new List<Vector3>();
    private Transform[] points;    

    private Vector3[] lastPositions;

    private void OnEnable()
    {
        BuildTrack();
    }

    private void OnValidate()
    {
        BuildTrack();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            CheckForChanges();
        }
#endif
    }

    private void OnDrawGizmos()
    {
        if (points == null || splinePoints == null || TrackPoints == null)
            return;

        float sphereRadius = 0.5f;

        Gizmos.color = Color.red;

        foreach (Transform point in points)
        {
            Gizmos.DrawWireSphere(point.position, sphereRadius);
        }

        for (int i = 1; i < splinePoints.Count; i++)
        {
            // Original spline
            Gizmos.color = Color.white;
            Gizmos.DrawLine(splinePoints[i - 1], splinePoints[i]);
            Gizmos.DrawWireSphere(splinePoints[i], sphereRadius);

            // Snapped spline
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(TrackPoints[i - 1], TrackPoints[i]);
            Gizmos.DrawWireSphere(TrackPoints[i], sphereRadius);
        }

        // Close loop
        Gizmos.color = Color.white;
        Gizmos.DrawLine(splinePoints[^1], splinePoints[0]);
        Gizmos.DrawWireSphere(splinePoints[0], sphereRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(TrackPoints[^1], TrackPoints[0]);
        Gizmos.DrawWireSphere(TrackPoints[0], sphereRadius);
    }

    public Vector3 GetTrackPoint(int index)
    {
        if (TrackPoints == null || TrackPoints.Count == 0)
            return Vector3.zero;

        int size = TrackPoints.Count;

        return TrackPoints[(index + size) % size];
    }

    [ContextMenu("Build Track")]
    public void BuildTrack()
    {
        points = waypointParent.GetComponentsInChildren<Transform>();

        List<Transform> validPoints = new();

        foreach (var p in points)
        {
            if (p != waypointParent)
                validPoints.Add(p);
        }

        points = validPoints.ToArray();

        splinePoints = CatmullRomSpline.GenerateSpline(points, resolution);

        TrackPoints = SnapToTrack(splinePoints);

        CachePositions();
    }

    private List<Vector3> SnapToTrack(List<Vector3> splinePoints)
    {
        float maxDistance = 100.0f;
        float originVerticalOffset = 5.0f;

        List<Vector3> snappedPoints = new();

        foreach (var point in splinePoints)
        {
            Vector3 origin = point + Vector3.up * originVerticalOffset;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo, maxDistance, collisionMask))
            {
                snappedPoints.Add(hitInfo.point);
            }
        }

        return snappedPoints;
    }

    private void CachePositions()
    {
        lastPositions = new Vector3[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            lastPositions[i] = points[i].position;
        }
    }

    private void CheckForChanges()
    {
        if (points == null || points.Length == 0)
            return;

        bool changed = false;

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].position != lastPositions[i])
            {
                changed = true;
                break;
            }
        }

        if (changed)
        {
            BuildTrack();
        }
    }
}