#nullable enable
using System;
using UnityEngine;

namespace SamuraiProject.AI.Learning
{
    public sealed class ArenaInfo : MonoBehaviour
    {
        [SerializeField] private SpawnPoint[] _spawnPoints = Array.Empty<SpawnPoint>();

        public Transform SceneCenter => transform;

        public ReadOnlySpan<SpawnPoint> GetSpawnPoints(string teamTag)
        {
            if (_spawnPoints.Length == 0) return ReadOnlySpan<SpawnPoint>.Empty;

            int startIndex = -1;
            int count = 0;

            for (var i = 0; i < _spawnPoints.Length; i++)
            {
                bool isMatch = string.Equals(_spawnPoints[i].TeamTag, teamTag, StringComparison.Ordinal);

                if (isMatch)
                {
                    if (startIndex == -1)
                    {
                        startIndex = i;
                    }
                    count++;
                }
                else
                {
                    if (startIndex != -1)
                    {
                        break;
                    }
                }
            }

            if (startIndex == -1) return ReadOnlySpan<SpawnPoint>.Empty;

            return new ReadOnlySpan<SpawnPoint>(_spawnPoints, startIndex, count);
        }

        private void OnValidate()
        {
            _spawnPoints = GetComponentsInChildren<SpawnPoint>(true);
        }

        private void Awake()
        {
            SortSpawnPoints();
        }

        private void SortSpawnPoints()
        {
            if (_spawnPoints.Length <= 1) return;

            Array.Sort(_spawnPoints, (a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return 1;
                if (b == null) return -1;

                return a.TeamTag.CompareTo(b.TeamTag);
            });
        }
    }
}

