using System;
using SamuraiProject.AI.Abstractions;
using UnityEngine;

namespace SamuraiProject.AI.Learning
{
    public sealed class SpawnPoint : MonoBehaviour
    {
        public string TeamTag;

        [SerializeField] private float _radius = 5f;

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

            return targetPosition;
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying) return;

            Gizmos.color = Color.darkRed;
            Gizmos.DrawSphere(transform.position, 0.5f);
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}

