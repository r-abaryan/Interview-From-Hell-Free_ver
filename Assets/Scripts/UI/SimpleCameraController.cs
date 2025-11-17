using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple camera controller for orbiting around a target
/// Works with mouse and touch using NEW Input System
/// </summary>
public class SimpleCameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // The teen character to look at
    public Vector3 targetOffset = new Vector3(0, 1.5f, 0); // Look at teen's head height
    
    [Header("Orbit Settings")]
    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 10f;
    
    [Header("Rotation")]
    public float rotationSpeed = 100f;
    public float minVerticalAngle = -20f;
    public float maxVerticalAngle = 80f;
    
    [Header("Zoom")]
    public float zoomSpeed = 2f;
    
    [Header("Smooth Movement")]
    public float smoothTime = 0.1f;
    
    private float currentX = 0f;
    private float currentY = 20f;
    private float currentDistance;
    private Vector3 currentVelocity = Vector3.zero;
    
    private Vector2 lastTouchPosition;
    private bool isDragging = false;
    
    void Start()
    {
        currentDistance = distance;
        
        // Find teen if not assigned
        if (target == null)
        {
            GameObject teenObj = GameObject.Find("TeenAgent");
            if (teenObj != null)
            {
                target = teenObj.transform;
            }
            else
            {
                // Create a dummy target at origin
                GameObject dummy = new GameObject("CameraTarget");
                target = dummy.transform;
                target.position = Vector3.zero;
            }
        }
        
        // Initialize camera position
        UpdateCameraPosition();
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        HandleInput();
        UpdateCameraPosition();
    }
    
    void HandleInput()
    {
        // NEW INPUT SYSTEM - Mouse
        var mouse = Mouse.current;
        if (mouse != null)
        {
            // Right mouse button to rotate
            if (mouse.rightButton.isPressed)
            {
                Vector2 delta = mouse.delta.ReadValue();
                currentX += delta.x * rotationSpeed * 0.01f;
                currentY -= delta.y * rotationSpeed * 0.01f;
                currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
            }
            
            // Mouse scroll to zoom
            float scrollValue = mouse.scroll.ReadValue().y;
            if (scrollValue != 0f)
            {
                currentDistance -= scrollValue * zoomSpeed * 0.01f;
                currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            }
            
            // Middle mouse button to zoom
            if (mouse.middleButton.isPressed)
            {
                Vector2 delta = mouse.delta.ReadValue();
                currentDistance += delta.y * zoomSpeed * 0.01f;
                currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            }
        }
        
        // NEW INPUT SYSTEM - Touch
        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            var touches = touchscreen.touches;
            
            // Single touch - rotate
            if (touches.Count == 1 && touches[0].isInProgress)
            {
                var touch = touches[0];
                
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    lastTouchPosition = touch.position.ReadValue();
                    isDragging = true;
                }
                else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved && isDragging)
                {
                    Vector2 currentPos = touch.position.ReadValue();
                    Vector2 delta = currentPos - lastTouchPosition;
                    lastTouchPosition = currentPos;
                    
                    currentX += delta.x * rotationSpeed * 0.002f;
                    currentY -= delta.y * rotationSpeed * 0.002f;
                    currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
                }
                else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended ||
                         touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    isDragging = false;
                }
            }
            
            // Two finger pinch - zoom
            if (touches.Count == 2 && touches[0].isInProgress && touches[1].isInProgress)
            {
                var touch0 = touches[0];
                var touch1 = touches[1];
                
                Vector2 touch0Pos = touch0.position.ReadValue();
                Vector2 touch1Pos = touch1.position.ReadValue();
                Vector2 touch0Delta = touch0.delta.ReadValue();
                Vector2 touch1Delta = touch1.delta.ReadValue();
                
                Vector2 touch0PrevPos = touch0Pos - touch0Delta;
                Vector2 touch1PrevPos = touch1Pos - touch1Delta;
                
                float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
                float currentMagnitude = (touch0Pos - touch1Pos).magnitude;
                
                float difference = prevMagnitude - currentMagnitude;
                
                currentDistance += difference * zoomSpeed * 0.01f;
                currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
                
                isDragging = false; // Disable single touch while pinching
            }
        }
    }
    
    /// <summary>
    /// Rotate camera (call from UI buttons or other scripts)
    /// </summary>
    public void RotateCamera(float deltaX, float deltaY)
    {
        currentX += deltaX;
        currentY += deltaY;
        currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
    }
    
    /// <summary>
    /// Zoom camera (call from UI buttons)
    /// </summary>
    public void ZoomCamera(float delta)
    {
        currentDistance += delta;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }
    
    void UpdateCameraPosition()
    {
        // Calculate desired position
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = rotation * Vector3.back;
        
        Vector3 targetPosition = target.position + targetOffset;
        Vector3 desiredPosition = targetPosition + direction * currentDistance;
        
        // Smooth movement
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        
        // Always look at target
        transform.LookAt(targetPosition);
    }
    
    /// <summary>
    /// Reset camera to default position
    /// </summary>
    public void ResetCamera()
    {
        currentX = 0f;
        currentY = 20f;
        currentDistance = distance;
    }
    
    /// <summary>
    /// Set new target to orbit around
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}

