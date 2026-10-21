using UnityEngine;

namespace TremMasmorra.Game
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform Target;
        public float Distance = 5f;
        public float Height = 1.6f;
        public float Yaw;
        public float Pitch = 18f;

        // Limites do vagão, para a camera nunca sair da sala.
        public Vector3 MinBounds = new Vector3(-6.5f, 0.5f, 0.3f);
        public Vector3 MaxBounds = new Vector3(6.5f, 4.5f, 39.7f);

        void LateUpdate()
        {
            if (Target == null) return;
            if (!PlayerController.InputBlocked)
            {
                Yaw += Input.GetAxis("Mouse X") * 3f;
                Pitch = Mathf.Clamp(Pitch - Input.GetAxis("Mouse Y") * 2f, 5f, 55f);
            }
            var pivot = Target.position + Vector3.up * Height;
            var pos = pivot + Quaternion.Euler(Pitch, Yaw, 0f) * new Vector3(0f, 0f, -Distance);
            pos = new Vector3(
                Mathf.Clamp(pos.x, MinBounds.x, MaxBounds.x),
                Mathf.Clamp(pos.y, MinBounds.y, MaxBounds.y),
                Mathf.Clamp(pos.z, MinBounds.z, MaxBounds.z));
            transform.position = pos;
            transform.LookAt(pivot);
        }
    }
}
