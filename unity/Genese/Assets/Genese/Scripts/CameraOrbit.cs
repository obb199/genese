using UnityEngine;

namespace Genese
{
    /// <summary>Câmera de diorama: órbita automática + arraste do mouse + zoom na roda.</summary>
    public class CameraOrbit : MonoBehaviour
    {
        public float radius = 32f, azimuth = 0.6f, elevation = 0.62f, autoSpeed = 0.05f;
        public Vector3 target = new Vector3(0, 1f, 0);
        float userAz = 0f;
        Vector3 lastMouse; bool dragging;

        void Update()
        {
            if (Input.GetMouseButtonDown(0)) { dragging = true; lastMouse = Input.mousePosition; }
            if (Input.GetMouseButtonUp(0)) dragging = false;
            if (dragging)
            {
                Vector3 d = Input.mousePosition - lastMouse;
                userAz -= d.x * 0.005f;
                elevation = Mathf.Clamp(elevation + d.y * 0.004f, 0.12f, 1.5f); // até quase "de cima"
                lastMouse = Input.mousePosition;
            }

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.001f) radius = Mathf.Clamp(radius - scroll * 1.8f, 10f, 55f);

            azimuth += Time.deltaTime * autoSpeed;
            float az = azimuth + userAz;
            transform.position = new Vector3(
                Mathf.Cos(az) * radius * Mathf.Cos(elevation),
                Mathf.Sin(elevation) * radius + 1.5f,
                Mathf.Sin(az) * radius * Mathf.Cos(elevation));
            transform.LookAt(target);
        }
    }
}
