using System;
using UnityEngine;

public sealed class SpawnPoint : MonoBehaviour
{
    public string TeamTag;

    [SerializeField] private float _radius = 5f;
    // TODO: Если будут застревать добавить проверки
    // [SerializeField] private bool _validateSpawn = true;

    public void SpawnBots(ReadOnlySpan<IBot> bots)
    {
        foreach (var bot in bots)
        {
            var spawnPosition = GetValidSpawnPosition();
            bot.SetPosition(spawnPosition);
        }
    }

    private Vector3 GetValidSpawnPosition()
    {

        Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * _radius;
        Vector3 targetPosition = transform.position + (Vector3)randomPoint;

        // TODO: Если боты застревают, здесь можно добавить проверку:
        // if (_validateSpawn && Physics2D.OverlapCircle(targetPos, botRadius, obstacleLayer)) { ...try again... }

        return targetPosition;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        Gizmos.color = new Color(0, 1, 0, 0.3f); // Полупрозрачный зеленый
        Gizmos.DrawSphere(transform.position, 0.5f);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}
