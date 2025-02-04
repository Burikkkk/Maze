using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  // Ссылка на игрока
    public Vector3 offset;  // Смещение камеры относительно игрока

    void Start()
    {
        // Устанавливаем начальное смещение камеры, если оно нужно
        offset = transform.position - player.position;
    }

    void Update()
    {
        if (player != null)
        {
            // Камера следует за игроком с учётом смещения
            transform.position = player.position + offset;
        }
    }
}
