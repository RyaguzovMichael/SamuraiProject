#nullable enable
using System;
using UnityEngine;

namespace SamuraiProject.AI.Learning
{
    public sealed class ArenasManager : MonoBehaviour
    {
        [SerializeField] private ArenaInfo[] _arenaInfos = null!;

        private int _currentIndex = 0;

        public ArenaInfo CurrentArena
        {
            get
            {
                if (_arenaInfos == null || _arenaInfos.Length == 0) return null!;
                return _arenaInfos[_currentIndex];
            }
        }
        public event Action? SceneChanged;

        public void ChangeScene()
        {
            if (_arenaInfos == null || _arenaInfos.Length <= 0) return;
            _arenaInfos[_currentIndex].gameObject.SetActive(false);

            _currentIndex++;
            if (_currentIndex >= _arenaInfos.Length)
            {
                _currentIndex = 0;
            }

            _arenaInfos[_currentIndex].gameObject.SetActive(true);
            SceneChanged?.Invoke();
        }

        [ContextMenu("Refresh Scenes")]
        private void RefreshScenesInEditor() => RefreshScenes();
        private void Awake() => RefreshScenes();

        private void RefreshScenes()
        {
            _arenaInfos = GetComponentsInChildren<ArenaInfo>(true);
            _currentIndex = 0;
            bool isOneActive = false;
            foreach (var arenaInfo in _arenaInfos)
            {
                if (isOneActive)
                {
                    arenaInfo.gameObject.SetActive(false);
                }
                else
                {
                    arenaInfo.gameObject.SetActive(true);
                    isOneActive = true;
                }
            }
        }
    }
}

