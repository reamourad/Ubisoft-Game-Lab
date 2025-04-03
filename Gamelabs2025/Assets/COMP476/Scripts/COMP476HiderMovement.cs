using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class COMP476HiderMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float verticalMoveForce = 7f;

    // Input storage variables
    private float _horizontalInput;
    private float _verticalInput;
    private float _ascendInput;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        ConfigureRigidbody();
    }

    private void ConfigureRigidbody()
    {
        _rb.useGravity = false;
        _rb.linearDamping = 4f;
        _rb.angularDamping = 999f;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void FixedUpdate()
    {
        ApplyMovementForces();
        LimitVelocity();
    }

    // Public methods to set input values
    public void SetHorizontalInput(float input) => _horizontalInput = Mathf.Clamp(input, -1f, 1f);
    public void SetVerticalInput(float input) => _verticalInput = Mathf.Clamp(input, -1f, 1f);
    public void SetAscendInput(float input) => _ascendInput = Mathf.Clamp(input, -1f, 1f);

    public Vector3 GetCurrentVelocity() => _rb.linearVelocity;

    private void ApplyMovementForces()
    {
        // Horizontal movement (forward/back and rotation)
        Vector3 moveDirection = transform.forward * _verticalInput;
        _rb.AddForce(moveDirection * moveForce, ForceMode.Force);

        // Rotation
        if (_horizontalInput != 0)
        {
            Quaternion deltaRotation = Quaternion.Euler(Vector3.up * (_horizontalInput * rotationSpeed * Time.fixedDeltaTime));
            _rb.MoveRotation(_rb.rotation * deltaRotation);
        }

        // Vertical movement (up/down)
        if (_ascendInput != 0)
        {
            _rb.AddForce(Vector3.up * (_ascendInput * verticalMoveForce), ForceMode.Force);
        }
    }

    private void LimitVelocity()
    {
        Vector3 horizontalVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            Vector3 limitedVelocity = horizontalVelocity.normalized * maxSpeed;
            _rb.linearVelocity = new Vector3(limitedVelocity.x, _rb.linearVelocity.y, limitedVelocity.z);
        }
    }

    // Add these new methods:
    public void MoveToward(Vector3 worldPosition, float arrivalThreshold = 0.5f)
    {
        Vector3 direction = worldPosition - transform.position;

        // Convert to local space inputs
        Vector3 localDirection = transform.InverseTransformDirection(direction.normalized);

        SetVerticalInput(localDirection.z);
        SetHorizontalInput(localDirection.x);

        // Handle vertical movement
        float verticalDiff = direction.y;
        SetAscendInput(Mathf.Clamp(verticalDiff, -1f, 1f));

        // Auto-stop when close
        if (direction.magnitude < arrivalThreshold)
        {
            Stop();
        }
    }

    public void Stop()
    {
        SetVerticalInput(0f);
        SetHorizontalInput(0f);
        SetAscendInput(0f);
    }

    public bool HasArrived(Vector3 position, float threshold)
    {
        return Vector3.Distance(transform.position, position) < threshold;
    }

    // Optional: Add these for easy input debugging
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Horizontal Input: {_horizontalInput}");
        GUI.Label(new Rect(10, 30, 300, 20), $"Vertical Input: {_verticalInput}");
        GUI.Label(new Rect(10, 50, 300, 20), $"Ascend Input: {_ascendInput}");
    }
}