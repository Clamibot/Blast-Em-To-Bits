using UnityEngine;

/// <summary>
/// Adds a slight lag to camera rotation to make the third person camera a little more interesting.
/// Requires that it starts parented to something in order to follow it correctly.
/// </summary>
[RequireComponent(typeof(Camera))]
public class ShipCamera : MonoBehaviour
{
    [Tooltip("Speed at which the camera rotates. (Camera uses Slerp for rotation.)")]
    public float rotateSpeed = 90.0f;

    [Tooltip("If the parented object is using FixedUpdate for movement, check this box for smoother movement.")]
    public bool usedFixedUpdate = true;

    public Transform target;
    private Vector3 startOffset;

    private void Start()
    {
        if (target == null)
            Debug.LogWarning(name + ": Lag Camera will not function correctly without a target.");
        if (transform.parent == null)
            Debug.LogWarning(name + ": Lag Camera will not function correctly without a parent to derive the initial offset from.");

        startOffset = transform.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!usedFixedUpdate)
            UpdateCamera();
    }

    private void FixedUpdate()
    {
        if (usedFixedUpdate)
            UpdateCamera();
    }

    private void UpdateCamera()
    {
        if (target != null)
        {
            transform.position = target.TransformPoint(startOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, rotateSpeed * Time.deltaTime);
        }
    }
}