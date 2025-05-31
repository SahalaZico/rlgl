using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Transform target yang akan diikuti oleh kamera
    public Transform target;

    // Kecepatan interpolasi lerp
    public float followSpeed = 5f;

    // Apakah kamera akan mengikuti target pada sumbu X, Y, dan Z
    public bool followX = true;
    public bool followY = true;
    public bool followZ = true;

    // Jarak offset antara kamera dan target
    public Vector3 offset;

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // Ambil posisi target yang akan diikuti
        Vector3 targetPosition = target.position + offset;

        // Hanya mengikuti sumbu yang diinginkan
        Vector3 newPosition = transform.position;
        if (followX) newPosition.x = Mathf.Lerp(transform.position.x, targetPosition.x, followSpeed * Time.deltaTime);
        if (followY) newPosition.y = Mathf.Lerp(transform.position.y, targetPosition.y, followSpeed * Time.deltaTime);
        if (followZ) newPosition.z = Mathf.Lerp(transform.position.z, targetPosition.z, followSpeed * Time.deltaTime);

        // Update posisi kamera
        transform.position = newPosition;
    }
}