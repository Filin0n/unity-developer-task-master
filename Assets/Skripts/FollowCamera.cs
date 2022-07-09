using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private float _cameraSpeed;

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, _player.position, _cameraSpeed * Time.deltaTime);
    }
}
