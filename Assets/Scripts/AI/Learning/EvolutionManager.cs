#nullable enable
using System;
using System.Text;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using SamuraiProject.AI.Configs;
using SamuraiProject.AI.Abstractions;
using UnityEngine.AI.Abstractions;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SamuraiProject.AI.Learning
{
    public sealed class EvolutionManager : MonoBehaviour
    {
        [Header("Controls")]
        [SerializeField] private bool _enableEvolution = true;
        [SerializeField] private bool _notLoadFromDisk = true;

        [SerializeField] private float _generationDuration = 50f;
        [SerializeField] private EvolutionTeamConfig[] _teamConfigs = Array.Empty<EvolutionTeamConfig>();

        [SerializeField] private ArenasManager _arenasManager = null!;

        private TeamManager[] _teamManagers = Array.Empty<TeamManager>();

        private float _timer = 0f;
        private int _generationCount = 1;
        private readonly StringBuilder _sb = new();

        private void OnValidate()
        {
            if (_arenasManager == null)
            {
                _arenasManager = FindFirstObjectByType<ArenasManager>();
            }
        }

        private void Awake()
        {
            if (_arenasManager == null)
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
            }
            _teamManagers = new TeamManager[_teamConfigs.Length];
            for (int i = 0; i < _teamConfigs.Length; i++)
            {
                var teamConfig = _teamConfigs[i];
                _teamManagers[i] = new TeamManager(
                    teamConfig,
                    _enableEvolution,
                    _notLoadFromDisk
                );
            }
        }

        private void Start()
        {
            StartNewGeneration();
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            var allDead = false;
            for (var i = 0; i < _teamManagers.Length; i++)
            {
                var teamManager = _teamManagers[i];
                if (teamManager.AllDead)
                {
                    allDead = true;
                    break;
                }
            }

            if (allDead || _timer >= _generationDuration)
            {
                EndGeneration();
                StartNewGeneration();
            }
        }

        private void OnDestroy()
        {
            foreach (var teamManager in _teamManagers)
            {
                teamManager?.Dispose();
            }
        }

        private void StartNewGeneration()
        {
            _timer = 0f;

            foreach (var teamManager in _teamManagers)
            {
                var currentArena = _arenasManager.CurrentArena;
                var spawnPoints = currentArena.GetSpawnPoints(teamManager.TeamTag);
                teamManager.SpawnBots(spawnPoints);
                teamManager.Activate();
            }
        }

        private void EndGeneration()
        {
            _arenasManager.ChangeScene();

            _sb.Clear();
            _sb.Append($"<color=#49FF49>[Gen {_generationCount}]</color> ");
            _generationCount++;

            for (int i = 0; i < _teamManagers.Length; i++)
            {
                var teamManager = _teamManagers[i];
                var alive = teamManager.Alive;
                teamManager.Deactivate();

                float bestFitness = teamManager.Reset();

                _sb.Append(teamManager.Name);
                _sb.Append(": ");
                _sb.Append(bestFitness.ToString("F2"));
                _sb.Append('/');
                _sb.Append(alive);

                if (i < _teamManagers.Length - 1)
                {
                    _sb.Append(" | ");
                }
            }

            Debug.Log(_sb.ToString());
        }

        private sealed class TeamManager : IDisposable
        {
            private readonly EvolutionTeamConfig _config;
            private readonly IBot[] _bots;
            private readonly bool _enableEvolution;
            private readonly string _defaultSavePath;

            private int _aliveCount;
            private float _currentBest;

            public TeamManager(
                EvolutionTeamConfig config,
                bool enableEvolution,
                bool notLoadFromDisk
            )
            {
                _config = config;
                _bots = new IBot[config.GenerationSize];
                _aliveCount = _bots.Length;
                _currentBest = float.MinValue;
                _enableEvolution = enableEvolution;

                var saveDirectory = Path.Combine(Application.dataPath, "Data");
                if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);
                _defaultSavePath = Path.Combine(saveDirectory, $"best_brain_team_{config.TeamName}.bytes");
                byte[]? botsBrain = null;
                if (config.InitialWeight == null)
                {
                    if (File.Exists(_defaultSavePath) && !notLoadFromDisk)
                    {
                        botsBrain = File.ReadAllBytes(_defaultSavePath);
                    }
                }
                else
                {
                    botsBrain = config.InitialWeight.bytes;
                }

                if (config.Prefab == null)
                {
                    throw new InvalidDataException("The prefab in config can't be null");
                }

                if (config.EvlutionConfig == null)
                {
                    throw new InvalidDataException("AIConfig can not be null");
                }

                for (int i = 0; i < _bots.Length; i++)
                {
                    var botObject = Instantiate(
                        config.Prefab,
                        Vector3.zero,
                        Quaternion.identity
                    );
                    IBot bot = botObject.GetComponent<IBot>();
                    _bots[i] = bot;
                    // TODO: handle null to create new brain
                    bot.Initialize(
                        botsBrain,
                        _config.WallsLayer
                    );
                    bot.Dead += BotDeadHandler;
                    IAIRewardable ai = botObject.GetComponent<IAIRewardable>();
                }
            }

            public bool AllDead => _aliveCount <= 0;
            public int Alive => _aliveCount;
            public string Name => _config.TeamName;
            public string TeamTag => _config.TeamTag;

            public void Activate()
            {
                foreach (var bot in _bots)
                {
                    bot.Activate();
                }
            }

            public void Deactivate()
            {
                foreach (var bot in _bots)
                {
                    bot.Deactivate();
                }
            }

            public float Reset()
            {
                _aliveCount = _bots.Length;

                if (!_enableEvolution || _bots.Length <= 0) return 0;

                Array.Sort(_bots, static (a, b) => b.Fitness.CompareTo(a.Fitness));
                var bestBot = _bots[0];
                var bestBotFitness = bestBot.Fitness;
                if (bestBotFitness > _currentBest)
                {
                    _currentBest = bestBotFitness;
                    Debug.Log($"<color=cyan>{_config.TeamName}: new best bot with fitness: {_currentBest}</color>");
                    bestBot.SaveBrainToFile(_defaultSavePath);
                }

                for (int i = 0; i < _bots.Length; i++)
                {
                    _bots[i].Reset();
                    if (i > _config.ElitismCount)
                    {
                        _bots[i].Evolve(bestBot, _config.MutationRate, _config.MutationStrength);
                    }
                }

                int n = _bots.Length;
                while (n > 1)
                {
                    n--;
                    int k = UnityEngine.Random.Range(0, n + 1);
                    (_bots[k], _bots[n]) = (_bots[n], _bots[k]);
                }

                return bestBotFitness;
            }

            public void SpawnBots(ReadOnlySpan<SpawnPoint> spawnPoints)
            {
                if (spawnPoints.Length == 0) return;
                if (spawnPoints.Length == 1)
                {
                    spawnPoints[0].SpawnBots(_bots);
                    return;
                }
                int botsPerSpawner = _bots.Length / spawnPoints.Length;
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    var startIndex = i * botsPerSpawner;
                    var length = i == spawnPoints.Length - 1 ? _bots.Length - startIndex : botsPerSpawner;
                    ReadOnlySpan<IBot> currentPart = _bots.AsSpan(startIndex, length);
                    spawnPoints[i].SpawnBots(currentPart);
                }
            }

            private void BotDeadHandler()
            {
                _aliveCount--;
            }

            public void Dispose()
            {
                foreach (var bot in _bots)
                {
                    bot.Dead -= BotDeadHandler;
                }
            }
        }
    }
}

