using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float smoothSpeed = 0.125f;
    private Vector3 offset;

    private void Start()
    {
        if (playerTransform != null)
        {
            offset = transform.position - playerTransform.position;
        }
    }

    private void LateUpdate()
    {
        if (playerTransform == null)
            return;
        Vector3 desiredPosition = playerTransform.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed
        );
        transform.position = smoothedPosition;
    }



}
