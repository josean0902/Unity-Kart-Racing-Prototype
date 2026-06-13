using UnityEngine;

public class KartTrackFollower : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private TrackBuilder track;

    [Header("Track Follow")]
    [SerializeField] private float baseLookAhead = 3f;
    [SerializeField] private float speedFactor = 0.1f;

    public int CurrentTrackPoint {  get; private set; }
    public int TrackPointsCount => track.TrackPoints.Count;

    private void Start()
    {
        CurrentTrackPoint = GetClosestTrackPointIndex();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, track.GetTrackPoint(CurrentTrackPoint));
    }

    public Vector3 GetLookAheadPoint(float speed)
    {
        float lookAheadDistance = baseLookAhead + speed * speedFactor;

        int index = CurrentTrackPoint;
        float accumulatedDistance = 0f;

        int count = track.TrackPoints.Count;
        if (count == 0)
            return Vector3.zero;

        while (accumulatedDistance < lookAheadDistance)
        {
            int nextIndex = (index + 1) % count;

            Vector3 currentPoint = track.GetTrackPoint(index);
            Vector3 nextPoint = track.GetTrackPoint(nextIndex);

            accumulatedDistance += Vector3.Distance(currentPoint, nextPoint);

            index = nextIndex;
        }

        return track.GetTrackPoint(index);
    }

    public void UpdateTrackPointIndex()
    {
        int nextIndex = (CurrentTrackPoint + 1) % track.TrackPoints.Count;

        Vector3 currentPoint = track.GetTrackPoint(CurrentTrackPoint);
        Vector3 nextPoint = track.GetTrackPoint(nextIndex);

        Vector3 segmentDir = (nextPoint - currentPoint);
        Vector3 toPlayer = (transform.position - currentPoint);

        if (Vector3.Dot(toPlayer, segmentDir) < 0)
            return;

        CurrentTrackPoint = nextIndex;
    }

    public float GetTrackProgress()
    {
        int next =
            (CurrentTrackPoint + 1)
            % track.TrackPoints.Count;

        Vector3 a =
            track.GetTrackPoint(CurrentTrackPoint);

        Vector3 b =
            track.GetTrackPoint(next);

        Vector3 ab = b - a;
        Vector3 ap = transform.position - a;

        float projection =
            Vector3.Dot(ap, ab.normalized);

        float segmentLength = ab.magnitude;

        float segmentProgress =
            Mathf.Clamp01(projection / segmentLength);

        return CurrentTrackPoint + segmentProgress;
    }


    private int GetClosestTrackPointIndex()
    {
        int trackPointsCount = track.TrackPoints.Count;
        int closestTrackPointIndex = -1;

        if (trackPointsCount == 0)
            return closestTrackPointIndex;

        float minDistance = Mathf.Infinity;
        for (int i = 0; i < trackPointsCount; i++)
        {
            Vector3 trackPointVector = track.GetTrackPoint(i) - transform.position;

            if (Vector3.Dot(transform.forward, trackPointVector.normalized) < 0)
                continue;

            float distance = Vector3.Magnitude(trackPointVector);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTrackPointIndex = i;
            }
        }

        return closestTrackPointIndex;
    }
}