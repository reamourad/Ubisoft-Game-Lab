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
        
        [Tooltip("Use NavMesh pathfinding for the entire rope path")]
        public bool useNavMeshPathfinding = true;

        [Tooltip("Area mask for NavMesh pathfinding")]
        public int navMeshAreaMask = NavMesh.AllAreas;
        
        // Path for NavMesh pathfinding
        private NavMeshPath navMeshPath;
        private List<Vector3> navMeshPathPoints = new List<Vector3>();
        private bool prevUseNavMeshPathfinding;

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
        private bool prevUseNavMeshCollision;
        private Collider ropeCollider;

        
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
                navMeshPath = new NavMeshPath(); // Add this line
                SetSplinePoint();
            }
            ropeCollider = GetComponent<Collider>();

        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                InitializeLineRenderer();
                if (AreEndPointsValid())
                {
                    navMeshPath = navMeshPath ?? new NavMeshPath(); // Add this line
                    RecalculateRope();
                    SimulatePhysics();
                }
                else
                {
                    lineRenderer.positionCount = 0;
                }
            }
        }
        public static Rope CreateRope(GameObject ropePrefab, Transform start, Transform end)
        {
            GameObject ropeObject = Instantiate(ropePrefab, Vector3.zero, Quaternion.identity);
            Rope rope = ropeObject.GetComponent<Rope>();

            // Ensure initialization before setting points
            rope.InitializeComponents();

            // Now set the points
            rope.SetStartPoint(start, true);
            rope.SetEndPoint(end, true);

            return rope;
        }
        
        public void InitializeComponents()
        {
            // Initialize all necessary components here
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
            navMeshPath = new NavMeshPath();
            // Other initialization
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
                prevUseNavMeshCollision = useNavMeshCollision;
                prevUseNavMeshPathfinding = useNavMeshPathfinding;
                if (ropeCollider != null)
                {
                    UpdateCollider();
                }
            }
        }

        private void UpdateCollider()
        {
            if (ropeCollider != null)
            {
                // For BoxCollider or CapsuleCollider
                if (ropeCollider is BoxCollider boxCollider)
                {
                    // Adjust box collider to match rope path
                    Vector3 startPos = StartPoint.position;
                    Vector3 endPos = EndPoint.position;
                    Vector3 center = (startPos + endPos) / 2f;

                    // Set the center of the collider relative to the GameObject
                    boxCollider.center = transform.InverseTransformPoint(center);

                    // Calculate direction and length
                    Vector3 direction = endPos - startPos;
                    float length = direction.magnitude;

                    // Set the size (adjust width/height as needed)
                    boxCollider.size = new Vector3(0.2f, 0.2f, length);

                    // Rotate the GameObject to align with the rope direction
                    transform.rotation = Quaternion.LookRotation(direction);
                }
                else if (ropeCollider is CapsuleCollider capsuleCollider)
                {
                    // Similar approach for capsule collider
                    Vector3 startPos = StartPoint.position;
                    Vector3 endPos = EndPoint.position;
                    Vector3 center = (startPos + endPos) / 2f;

                    capsuleCollider.center = transform.InverseTransformPoint(center);

                    Vector3 direction = endPos - startPos;
                    float length = direction.magnitude;

                    // Set capsule direction along rope (usually Z-axis for a capsule)
                    capsuleCollider.direction = 2; // 0 = X, 1 = Y, 2 = Z
                    capsuleCollider.height = length;
                    capsuleCollider.radius = 0.1f; // Adjust as needed

                    transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }

        private bool AreEndPointsValid()
        {
            return startPoint != null && endPoint != null;
        }

        private void SetSplinePoint()
        {
            if (!AreEndPointsValid()) return;

            Vector3 mid = GetMidPoint();
            targetValue = mid;
            mid = currentValue;

            if (midPoint != null)
            {
                midPoint.position = GetRationalBezierPoint(startPoint.position, mid, endPoint.position, midPointPosition, StartPointWeight, midPointWeight, EndPointWeight);
            }

            // Generate navmesh path if needed
            if (useNavMeshCollision && useNavMeshPathfinding)
            {
                CalculateNavMeshPath();
            }

            // Clear previous adjusted points
            navMeshAdjustedPoints.Clear();
    
            // If using NavMesh pathfinding and we have a valid path, use that to set the rope points
            if (useNavMeshCollision && useNavMeshPathfinding && navMeshPathPoints.Count > 1)
            {
                SetRopePointsFromPath();
            }
            else
            {
                // Use the bezier curve as normal
                SetRopePointsFromBezier(mid);
            }
        }
        
        private void CalculateNavMeshPath()
        {
            navMeshPathPoints.Clear();
            navMeshPath.ClearCorners();
            
            // Get NavMesh positions for start and end points
            NavMeshHit startHit, endHit;
            Vector3 startNavPos = startPoint.position;
            Vector3 endNavPos = endPoint.position;
            
            bool foundStartPos = NavMesh.SamplePosition(startPoint.position, out startHit, navMeshRaycastDistance, navMeshAreaMask);
            bool foundEndPos = NavMesh.SamplePosition(endPoint.position, out endHit, navMeshRaycastDistance, navMeshAreaMask);
            
            if (foundStartPos && foundEndPos)
            {
                startNavPos = startHit.position + Vector3.up * navMeshOffset;
                endNavPos = endHit.position + Vector3.up * navMeshOffset;
                
                // Calculate path between the two points
                if (NavMesh.CalculatePath(startNavPos, endNavPos, navMeshAreaMask, navMeshPath))
                {
                    // Convert corners to path points
                    navMeshPathPoints.AddRange(navMeshPath.corners);
                    
                    // If we need more points for a smoother path, interpolate between corners
                    if (linePoints + 1 > navMeshPathPoints.Count && navMeshPathPoints.Count > 1)
                    {
                        List<Vector3> interpolatedPoints = new List<Vector3>();
                        
                        // Calculate total path length
                        float totalLength = 0;
                        for (int i = 0; i < navMeshPathPoints.Count - 1; i++)
                        {
                            totalLength += Vector3.Distance(navMeshPathPoints[i], navMeshPathPoints[i + 1]);
                        }
                        
                        // Create evenly spaced points along the path
                        for (int i = 0; i <= linePoints; i++)
                        {
                            float t = i / (float)linePoints;
                            float targetDistance = t * totalLength;
                            float currentDistance = 0;
                            
                            if (i == 0)
                            {
                                interpolatedPoints.Add(navMeshPathPoints[0]);
                                continue;
                            }
                            
                            if (i == linePoints)
                            {
                                interpolatedPoints.Add(navMeshPathPoints[navMeshPathPoints.Count - 1]);
                                continue;
                            }
                            
                            for (int j = 0; j < navMeshPathPoints.Count - 1; j++)
                            {
                                float segmentLength = Vector3.Distance(navMeshPathPoints[j], navMeshPathPoints[j + 1]);
                                
                                if (currentDistance + segmentLength >= targetDistance)
                                {
                                    float segmentT = (targetDistance - currentDistance) / segmentLength;
                                    Vector3 point = Vector3.Lerp(navMeshPathPoints[j], navMeshPathPoints[j + 1], segmentT);
                                    interpolatedPoints.Add(point);
                                    break;
                                }
                                
                                currentDistance += segmentLength;
                            }
                        }
                        
                        navMeshPathPoints = interpolatedPoints;
                    }
                }
            }
        }

        private void SetRopePointsFromPath()
        {
            if (navMeshPathPoints.Count == 0)
            {
                return;
            }
            
            // Match the renderer position count to our path point count
            lineRenderer.positionCount = navMeshPathPoints.Count;
            
            // Set all the points
            for (int i = 0; i < navMeshPathPoints.Count; i++)
            {
                lineRenderer.SetPosition(i, navMeshPathPoints[i]);
                navMeshAdjustedPoints.Add(navMeshPathPoints[i]);
            }
        }

        private void SetRopePointsFromBezier(Vector3 midControlPoint)
        {
            if (lineRenderer.positionCount != linePoints + 1)
            {
                lineRenderer.positionCount = linePoints + 1;
            }
            
            for (int i = 0; i <= linePoints; i++)
            {
                float t = i / (float)linePoints;
                Vector3 point;
                
                if (i == linePoints)
                {
                    point = endPoint.position; // Make sure the last point is exactly the end point
                }
                else if (i == 0)
                {
                    point = startPoint.position; // Make sure the first point is exactly the start point
                }
                else
                {
                    point = GetRationalBezierPoint(startPoint.position, midControlPoint, endPoint.position, t, StartPointWeight, midPointWeight, EndPointWeight);
                }
                
                // Apply NavMesh collision if enabled without pathfinding
                if (useNavMeshCollision && !useNavMeshPathfinding)
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

            // If NavMesh pathfinding is enabled, interpolate between path points
            if (useNavMeshCollision && useNavMeshPathfinding && navMeshPathPoints.Count > 1)
            {
                int index = Mathf.FloorToInt(t * (navMeshPathPoints.Count - 1));
                float localT = (t * (navMeshPathPoints.Count - 1)) - index;
        
                if (index >= navMeshPathPoints.Count - 1)
                    return navMeshPathPoints[navMeshPathPoints.Count - 1];
            
                return Vector3.Lerp(navMeshPathPoints[index], navMeshPathPoints[index + 1], localT);
            }
    
            // Original bezier behavior
            Vector3 bezierPoint = GetRationalBezierPoint(startPoint.position, currentValue, endPoint.position, t, StartPointWeight, midPointWeight, EndPointWeight);
    
            // If NavMesh collision is enabled without pathfinding
            if (useNavMeshCollision && !useNavMeshPathfinding && navMeshAdjustedPoints.Count > 0)
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

        // Methods to control NavMesh collision via script
        public void EnableNavMeshCollision()
        {
            if (!useNavMeshCollision)
            {
                useNavMeshCollision = true;
                RecalculateRope();
            }
        }

        public void DisableNavMeshCollision()
        {
            if (useNavMeshCollision)
            {
                useNavMeshCollision = false;
                RecalculateRope();
            }
        }

        public void ToggleNavMeshCollision()
        {
            useNavMeshCollision = !useNavMeshCollision;
            RecalculateRope();
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
            var pathfindingChanged = prevUseNavMeshPathfinding != useNavMeshPathfinding;

            return lineQualityChanged
                   || ropeWidthChanged
                   || stiffnessChanged
                   || dampnessChanged
                   || ropeLengthChanged
                   || midPointPositionChanged
                   || midPointWeightChanged
                   || pathfindingChanged;
        }
        // Add these methods for toggling pathfinding
        public void EnableNavMeshPathfinding()
        {
            if (!useNavMeshPathfinding)
            {
                useNavMeshPathfinding = true;
                RecalculateRope();
            }
        }

        public void DisableNavMeshPathfinding()
        {
            if (useNavMeshPathfinding)
            {
                useNavMeshPathfinding = false;
                RecalculateRope();
            }
        }

        public void ToggleNavMeshPathfinding()
        {
            useNavMeshPathfinding = !useNavMeshPathfinding;
            RecalculateRope();
        }

        // Modify OnDrawGizmos to show the NavMesh path
        private void OnDrawGizmos()
        {
            if (!AreEndPointsValid())
                return;

            Vector3 midPos = GetMidPoint();
    
            // Draw NavMesh path points if pathfinding is enabled
            if (useNavMeshCollision && useNavMeshPathfinding && navMeshPathPoints.Count > 0)
            {
                Gizmos.color = Color.blue;
                foreach (Vector3 point in navMeshPathPoints)
                {
                    Gizmos.DrawSphere(point, 0.2f);
                }
        
                // Draw lines between path points
                Gizmos.color = Color.yellow;
                for (int i = 0; i < navMeshPathPoints.Count - 1; i++)
                {
                    Gizmos.DrawLine(navMeshPathPoints[i], navMeshPathPoints[i + 1]);
                }
            }
            // Draw individual adjusted points if using simple collision
            else if (useNavMeshCollision && navMeshAdjustedPoints.Count > 0)
            {
                Gizmos.color = Color.green;
                foreach (Vector3 point in navMeshAdjustedPoints)
                {
                    Gizmos.DrawSphere(point, 0.1f);
                }
            }
        }
    }
}