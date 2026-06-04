using UnityEngine;

namespace Genese
{
    /// <summary>
    /// Câmera FPS fluída:
    ///  • Botão direito (segurar) → olhar com mouse suavizado
    ///  • WASD → mover; Q/E → descer/subir
    ///  • Scroll → zoom na direção do olhar
    ///  • Shift → velocidade 3×
    ///  • F → resetar ao ponto inicial
    /// </summary>
    public class CameraOrbit : MonoBehaviour
    {
        [Header("Posição inicial")]
        public Vector3 startPosition = new Vector3(0f, 55f, -90f);
        public float   startPitch    = 28f;
        public float   startYaw      = 0f;

        [Header("Controles")]
        public float moveSpeed       = 22f;
        public float fastMultiplier  = 3.5f;
        public float lookSensitivity = 2.8f;   // mais responsivo
        public float smoothTime      = 0.04f;  // suavização do mouse (FPS feel)
        public float scrollSpeed     = 28f;

        float _yaw, _pitch;
        float _targetYaw, _targetPitch;
        float _yawVel, _pitchVel;

        // Suavização do scroll
        float _scrollVel;

        void Start()
        {
            transform.position = startPosition;
            _yaw = _targetYaw = startYaw;
            _pitch = _targetPitch = startPitch;
            Apply();
        }

        void Update()
        {
            bool overUI = Nucleo.CoreHud.IsMouseOverPanel;

            // ── Mouse look (botão direito) ─────────────────────────────────
            if (Input.GetMouseButtonDown(1) && !overUI)
                Cursor.lockState = CursorLockMode.Locked;
            if (Input.GetMouseButtonUp(1))
                Cursor.lockState = CursorLockMode.None;

            if (Input.GetMouseButton(1) && !overUI)
            {
                // Raw input para responsividade máxima, sem dead-zone do Unity
                float dx = Input.GetAxisRaw("Mouse X");
                float dy = Input.GetAxisRaw("Mouse Y");
                _targetYaw   += dx * lookSensitivity;
                _targetPitch -= dy * lookSensitivity;
                _targetPitch  = Mathf.Clamp(_targetPitch, -15f, 88f);
            }

            // Suavização SmoothDamp: dá o "peso" de câmera FPS
            _yaw   = Mathf.SmoothDampAngle(_yaw,   _targetYaw,   ref _yawVel,   smoothTime);
            _pitch = Mathf.SmoothDampAngle(_pitch, _targetPitch, ref _pitchVel, smoothTime);
            Apply();

            // ── Teclado ────────────────────────────────────────────────────
            bool fast = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float spd = moveSpeed * (fast ? fastMultiplier : 1f) * Time.deltaTime;

            Vector3 move = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) move += transform.forward;
            if (Input.GetKey(KeyCode.S)) move -= transform.forward;
            if (Input.GetKey(KeyCode.A)) move -= transform.right;
            if (Input.GetKey(KeyCode.D)) move += transform.right;
            if (Input.GetKey(KeyCode.E)) move += Vector3.up;
            if (Input.GetKey(KeyCode.Q)) move += Vector3.down;

            if (move.sqrMagnitude > 0.001f)
                transform.position += move.normalized * spd;

            // ── Scroll → zoom suave ────────────────────────────────────────
            if (!overUI)
            {
                float scroll = Input.mouseScrollDelta.y * scrollSpeed;
                // Suaviza o scroll para evitar saltos bruscos
                float smoothScroll = Mathf.SmoothDamp(0f, scroll, ref _scrollVel, 0.06f);
                if (Mathf.Abs(scroll) > 0.001f)
                    transform.position += transform.forward * scroll * Time.deltaTime * 55f;
            }

            // ── F → resetar câmera ─────────────────────────────────────────
            if (Input.GetKeyDown(KeyCode.F))
            {
                transform.position = startPosition;
                _yaw = _targetYaw = startYaw;
                _pitch = _targetPitch = startPitch;
                _yawVel = _pitchVel = 0f;
                Apply();
            }
        }

        void Apply() => transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }
}
