using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GogoGaga.OptimizedRopesAndCables
{
    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    public class Rope : MonoBehaviour
    {
        public event Action OnPointsChanged;

        [Header("Rope Transforms")]
        [Tooltip("The rope will start at this point")]
        [SerializeField] private Transform startPoint;
        public Transform StartPoint => startPoint;

        [Tooltip("This will move at the center hanging from the rope, like a necklace, for example")]
        [SerializeField] private Transform midPoint;
        public Transform MidPoint => midPoint;

        [Tooltip("The rope will end at this point")]
        [SerializeField] private Transform endPoint;
        public Transform EndPoint => endPoint;

        [Header("Rope Settings")]
        [Tooltip("How many points should the rope have, 2 would be a triangle with straight lines, 100 would be a very flexible rope with many parts")]
        [Range(2, 100)] public int linePoints = 10;

        [Tooltip("Value highly dependent on use case, a metal cable would have high stiffness, a rubber rope would have a low one")]
        public float stiffness = 350f;

        [Tooltip("0 is no damping, 50 is a lot")]
        public float damping = 15f;

        [Tooltip("How long is the rope, it will hang more or less from starting point to end point depending on this value")]
        public float ropeLength = 15;

        [Tooltip("The Rope width set at start (changing this value during run time will produce no effect)")]
        public float ropeWidth = 0.1f;

        [Header("Rational Bezier Weight Control")]
        [Tooltip("Adjust the middle control point weight for the Rational Bezier curve")]
        [Range(1, 15)] public float midPointWeight = 1f;
        private const float StartPointWeight = 1f;
        private const float EndPointWeight = 1f;

        [Header("Midpoint Position")]
        [Tooltip("Position of the midpoint along the line between start and end points")]
        [Range(0.25f, 0.75f)] public float midPointPosition = 0.5f;

        [Header("NavMesh Collision")]
        [Tooltip("Should the rope collide with the NavMesh?")]
        public bool useNavMeshCollision = true;
        
        [Tooltip("How many segments to check for NavMesh collision")]
        [Range(1, 50)] public int navMeshSampleCount = 10;
        
        [Tooltip("How far to check downward for the NavMesh")]
        public float navMeshRaycastDistance = 20f;
        
        [Tooltip("Offset from the NavMesh surface")]
        public float navMeshOffset = 0.1f;

        private Vector3 currentValue;
        private Vector3 currentVelocity;
        private Vector3 targetValue;
        public Vector3 otherPhysicsFactors { get; set; }
        private const float valueThreshold = 0.01f;
        private const float velocityThreshold = 0.01f;

        private LineRenderer lineRenderer;
        private bool isFirstFrame = true;

        private Vector3 prevStartPointPosition;
        private Vector3 prevEndPointPosition;
        private float prevMidPointPosition;
        private float prevMidPointWeight;

        private float prevLineQuality;
        private float prevRopeWidth;
        private float prevstiffness;
        private float prevDampness;
        private float prevRopeLength;
        
        // Cache for NavMesh adjusted points
        private List<Vector3> navMeshAdjustedPoints = new List<Vector3>();
        
        public bool IsPrefab => gameObject.scene.rootCount == 0;
        
        private void Start()
        {
            InitializeLineRenderer();
            if (AreEndPointsValid())
            {
                currentValue = GetMidPoint();
                targetValue = currentValue;
                currentVelocity = Vector3.zero;
                SetSplinePoint(); // Ensure initial spline point is set correctly
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                InitializeLineRenderer();
                if (AreEndPointsValid())
                {
                    RecalculateRope();
                    SimulatePhysics();
                }
                else
                {
                    lineRenderer.positionCount = 0;
                }
            }
        }

        private void InitializeLineRenderer()
        {
            if (!lineRenderer)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }

            lineRenderer.startWidth = ropeWidth;
            lineRenderer.endWidth = ropeWidth;
        }

        private void Update()
        {
            if (IsPrefab)
            {
                return;
            }
            
            if (AreEndPointsValid())
            {
                SetSplinePoint();

                if (!Application.isPlaying && (IsPointsMoved() || IsRopeSettingsChanged()))
                {
                    SimulatePhysics();
                    NotifyPointsChanged();
                }

                prevStartPointPosition = startPoint.position;
                prevEndPointPosition = endPoint.position;
                prevMidPointPosition = midPointPosition;
                prevMidPointWeight = midPointWeight;

                prevLineQuality = linePoints;
                prevRopeWidth = ropeWidth;
                prevstiffness = stiffness;
                prevDampness = damping;
                prevRopeLength = ropeLength;
            }
        }

        private bool AreEndPointsValid()
        {
            return startPoint != null && endPoint != null;
        }

        private void SetSplinePoint()
        {
            if (lineRenderer.positionCount != linePoints + 1)
            {
                lineRenderer.positionCount = linePoints + 1;
            }

            Vector3 mid = GetMidPoint();
            targetValue = mid;
            mid = currentValue;

            if (midPoint != null)
            {
                midPoint.position = GetRationalBezierPoint(startPoint.position, mid, endPoint.position, midPointPosition, StartPointWeight, midPointWeight, EndPointWeight);
            }

            // Clear previous adjusted points
            navMeshAdjustedPoints.Clear();
            
            // Generate the base spline points
            for (int i = 0; i <= linePoints; i++)
            {
                float t = i / (float)linePoints;
                Vector3 point;
                
                if (i == linePoints)
                {
                    point = endPoint.position; // Make sure the last point is exactly the end point
                }
                else
                {
                    point = GetRationalBezierPoint(startPoint.position, mid, endPoint.position, t, StartPointWeight, midPointWeight, EndPointWeight);
                }
                
                // Apply NavMesh collision if enabled
                if (useNavMeshCollision && Application.isPlaying)
                {
                    point = AdjustPointToNavMesh(point);
                }
                
                navMeshAdjustedPoints.Add(point);
                lineRenderer.SetPosition(i, point);
            }
        }

        private Vector3 AdjustPointToNavMesh(Vector3 point)
        {
            // Cast ray down from the point to find NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(point, out hit, navMeshRaycastDistance, NavMesh.AllAreas))
            {
                // Calculate the projection of the original point onto the NavMesh
                Vector3 adjustedPoint = hit.position + new Vector3(0, navMeshOffset, 0);

                // Only adjust Y-coordinate to maintain the rope's horizontal path
                return new Vector3(point.x, adjustedPoint.y, point.z);
            }
            
            return point;
        }

        private float CalculateYFactorAdjustment(float weight)
        {
            float k = Mathf.Lerp(0.493f, 0.323f, Mathf.InverseLerp(1, 15, weight));
            float w = 1f + k * Mathf.Log(weight);
            return w;
        }

        private Vector3 GetMidPoint()
        {
            Vector3 startPointPosition = startPoint.position;
            Vector3 endPointPosition = endPoint.position;
            Vector3 midpos = Vector3.Lerp(startPointPosition, endPointPosition, midPointPosition);
            float yFactor = (ropeLength - Mathf.Min(Vector3.Distance(startPointPosition, endPointPosition), ropeLength)) / CalculateYFactorAdjustment(midPointWeight);
            midpos.y -= yFactor;
            return midpos;
        }

        private Vector3 GetRationalBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t, float w0, float w1, float w2)
        {
            Vector3 wp0 = w0 * p0;
            Vector3 wp1 = w1 * p1;
            Vector3 wp2 = w2 * p2;

            float denominator = w0 * Mathf.Pow(1 - t, 2) + 2 * w1 * (1 - t) * t + w2 * Mathf.Pow(t, 2);
            Vector3 point = (wp0 * Mathf.Pow(1 - t, 2) + wp1 * 2 * (1 - t) * t + wp2 * Mathf.Pow(t, 2)) / denominator;

            return point;
        }

        public Vector3 GetPointAt(float t)
        {
            if (!AreEndPointsValid())
            {
                Debug.LogError("StartPoint or EndPoint is not assigned.", gameObject);
                return Vector3.zero;
            }

            Vector3 bezierPoint = GetRationalBezierPoint(startPoint.position, currentValue, endPoint.position, t, StartPointWeight, midPointWeight, EndPointWeight);
            
            // If NavMesh collision is enabled and we have adjusted points, interpolate between them
            if (useNavMeshCollision && Application.isPlaying && navMeshAdjustedPoints.Count > 0)
            {
                int index = Mathf.FloorToInt(t * linePoints);
                float localT = (t * linePoints) - index;
                
                if (index >= navMeshAdjustedPoints.Count - 1)
                    return navMeshAdjustedPoints[navMeshAdjustedPoints.Count - 1];
                    
                return Vector3.Lerp(navMeshAdjustedPoints[index], navMeshAdjustedPoints[index + 1], localT);
            }
            
            return bezierPoint;
        }

        private void FixedUpdate()
        {
            if (IsPrefab)
            {
                return;
            }
            
            if (AreEndPointsValid())
            {
                if (!isFirstFrame)
                {
                    SimulatePhysics();
                }

                isFirstFrame = false;
            }
        }

        private void SimulatePhysics()
        {
            float dampingFactor = Mathf.Max(0, 1 - damping * Time.fixedDeltaTime);
            Vector3 acceleration = (targetValue - currentValue) * stiffness * Time.fixedDeltaTime;
            currentVelocity = currentVelocity * dampingFactor + acceleration + otherPhysicsFactors;
            currentValue += currentVelocity * Time.fixedDeltaTime;

            if (Vector3.Distance(currentValue, targetValue) < valueThreshold && currentVelocity.magnitude < velocityThreshold)
            {
                currentValue = targetValue;
                currentVelocity = Vector3.zero;
            }
        }

        private void OnDrawGizmos()
        {
            if (!AreEndPointsValid())
                return;

            Vector3 midPos = GetMidPoint();
            
            // Draw NavMesh sample points
            if (useNavMeshCollision && navMeshAdjustedPoints.Count > 0)
            {
                Gizmos.color = Color.green;
                foreach (Vector3 point in navMeshAdjustedPoints)
                {
                    Gizmos.DrawSphere(point, 0.1f);
                }
            }
        }

        // API methods for setting points
        public void SetStartPoint(Transform newStartPoint, bool instantAssign = false)
        {
            startPoint = newStartPoint;
            prevStartPointPosition = startPoint == null ? Vector3.zero : startPoint.position;

            if (instantAssign || newStartPoint == null)
            {
                RecalculateRope();
            }

            NotifyPointsChanged();
        }
        
        public void SetMidPoint(Transform newMidPoint, bool instantAssign = false)
        {
            midPoint = newMidPoint;
            prevMidPointPosition = midPoint == null ? 0.5f : midPointPosition;
            
            if (instantAssign || newMidPoint == null)
            {
                RecalculateRope();
            }
            NotifyPointsChanged();
        }
        
        public void SetEndPoint(Transform newEndPoint, bool instantAssign = false)
        {
            endPoint = newEndPoint;
            prevEndPointPosition = endPoint == null ? Vector3.zero : endPoint.position;

            if (instantAssign || newEndPoint == null)
            {
                RecalculateRope();
            }

            NotifyPointsChanged();
        }

        public void RecalculateRope()
        {
            if (!AreEndPointsValid())
            {
                lineRenderer.positionCount = 0;
                return;
            }

            currentValue = GetMidPoint();
            targetValue = currentValue;
            currentVelocity = Vector3.zero;
            SetSplinePoint();
        }

        private void NotifyPointsChanged()
        {
            OnPointsChanged?.Invoke();
        }

        private bool IsPointsMoved()
        {
            var startPointMoved = startPoint.position != prevStartPointPosition;
            var endPointMoved = endPoint.position != prevEndPointPosition;
            return startPointMoved || endPointMoved;
        }

        private bool IsRopeSettingsChanged()
        {
            var lineQualityChanged = !Mathf.Approximately(linePoints, prevLineQuality);
            var ropeWidthChanged = !Mathf.Approximately(ropeWidth, prevRopeWidth);
            var stiffnessChanged = !Mathf.Approximately(stiffness, prevstiffness);
            var dampnessChanged = !Mathf.Approximately(damping, prevDampness);
            var ropeLengthChanged = !Mathf.Approximately(ropeLength, prevRopeLength);
            var midPointPositionChanged = !Mathf.Approximately(midPointPosition, prevMidPointPosition);
            var midPointWeightChanged = !Mathf.Approximately(midPointWeight, prevMidPointWeight);

            return lineQualityChanged
                   || ropeWidthChanged
                   || stiffnessChanged
                   || dampnessChanged
                   || ropeLengthChanged
                   || midPointPositionChanged
                   || midPointWeightChanged;
        }
    }
}