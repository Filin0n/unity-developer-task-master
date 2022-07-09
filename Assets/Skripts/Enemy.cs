using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Vector2 _spawnRange;

    [HideInInspector] public bool isDead = false;

    public void Hit()
    {
        isDead = true;
        Invoke("Die", 0.8f);
        Invoke("SpawnNewPosition", 5f);
    }

    private void Die()
    {
        _animator.enabled = false;
    }

    private void SpawnNewPosition()
    {
        _animator.enabled = true;
        isDead = false;
        Vector3 newPosition = new Vector3(Random.Range(-_spawnRange.x, _spawnRange.x),transform.position.y, Random.Range(-_spawnRange.y, _spawnRange.y));
        transform.position = newPosition;
    }
}
