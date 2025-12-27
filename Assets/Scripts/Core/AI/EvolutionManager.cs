#nullable enable
using System;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(SpawnPointCycler))]
public sealed class EvolutionManager : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private bool _enableEvolution = true;
    [SerializeField] private bool _notLoadFromDisk = true;

    [SerializeField] private float _generationDuration = 50f;
    [SerializeField] private EvolutionTeamConfig[] _teamConfigs = Array.Empty<EvolutionTeamConfig>();

    private EvolutionTeamManager[] _teamManagers = Array.Empty<EvolutionTeamManager>();
    private SpawnPoint[] _spawnPoints = Array.Empty<SpawnPoint>();

    private float _timer = 0f;
    private int _generationCount = 1;
    private SpawnPointCycler? _spawnPointCycler;

    private void Start()
    {
        _spawnPointCycler = GetComponent<SpawnPointCycler>();
        _spawnPoints = _spawnPointCycler.GetNextArray();
        if (_spawnPoints.Length == 0)
        {
            Debug.LogError("[EvolutionManager] No spawn points found!");
            return;
        }

        _teamManagers = new EvolutionTeamManager[_teamConfigs.Length];
        for (int i = 0; i < _teamManagers.Length; i++)
        {
            var teamConfig = _teamConfigs[i];
            var teamSpawnPoints = _spawnPoints
                .Where(sp => string.Equals(sp.TeamTag, teamConfig.TeamTag, StringComparison.Ordinal))
                .ToArray();
            _teamManagers[i] = new EvolutionTeamManager(
                teamConfig,
                teamSpawnPoints,
                _enableEvolution,
                _notLoadFromDisk,
                teamConfig.TeamTag
            );
        }
        StartNewGeneration();
    }

    [ContextMenu("Force Restart Round")]
    public void ForceRestart()
    {
        EndGeneration();
    }

    private void StartNewGeneration()
    {
        _timer = 0f;
        foreach (var teamManager in _teamManagers)
        {
            teamManager.Activate();
        }
    }

    private void EndGeneration()
    {
        _generationCount++;
        var newSpawnPoints = _spawnPointCycler!.GetNextArray();

        StringBuilder sb = new($"<color=#49FF49>[Gen {_generationCount - 1}]</color> ");

        for (int i = 0; i < _teamManagers.Length; i++)
        {
            var teamManager = _teamManagers[i];
            var spawnPoints = newSpawnPoints.Where(el => el.TeamTag == teamManager.TeamTag).ToArray();
            var alive = teamManager.Alive;
            teamManager.Deactivate();

            float bestFitness = teamManager.Reset(spawnPoints);

            sb.Append(teamManager.Name);
            sb.Append(": ");
            sb.Append(bestFitness.ToString("F2"));
            sb.Append('/');
            sb.Append(alive);

            if (i < _teamManagers.Length - 1)
            {
                sb.Append(" | ");
            }
        }

        Debug.Log(sb.ToString());
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        var allDead = false;
        for (var i = 0; i < _teamManagers.Length; i++)
        {
            var teamManager = _teamManagers[i];
            if (!teamManager.HasAlive)
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

    private void OnDisable()
    {
        foreach (var teamManager in _teamManagers)
        {
            teamManager.Dispose();
        }
    }
}
