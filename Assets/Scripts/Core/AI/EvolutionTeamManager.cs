#nullable enable
using System;
using System.IO;
using UnityEngine;

public sealed class EvolutionTeamManager : IDisposable
{
    private readonly EvolutionTeamConfig _config;
    private readonly IBot[] _bots;
    private readonly EvolutionRewardManager[] _evolutionRewardManagers;
    private readonly bool _enableEvolution;
    private readonly string _defaultSavePath;
    private readonly string _teamTag;

    private int _aliveCount;
    private SpawnPoint[] _spawnPoints;
    private float _currentBest;

    public EvolutionTeamManager(
        EvolutionTeamConfig config,
        SpawnPoint[] spawnPoints,
        bool enableEvolution,
        bool notLoadFromDisk,
        string teamTag
    )
    {
        _config = config;
        _spawnPoints = spawnPoints;
        _bots = new IBot[config.GenerationSize];
        _evolutionRewardManagers = new EvolutionRewardManager[config.GenerationSize];
        _aliveCount = _bots.Length;
        _currentBest = float.MinValue;
        _enableEvolution = enableEvolution;
        _teamTag = teamTag;

        var saveDirectory = Path.Combine(Application.dataPath, "Data");
        if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);
        _defaultSavePath = Path.Combine(saveDirectory, $"best_brain_team_{config.TeamName}.bytes");
        SimpleNeuralNetwork? botsBrain = null;
        if (config.InitialWeight == null)
        {
            if (File.Exists(_defaultSavePath) && !notLoadFromDisk)
            {
                botsBrain = SimpleNeuralNetwork.LoadBinary(_defaultSavePath);
            }
        }
        else
        {
            botsBrain = SimpleNeuralNetwork.LoadFromMemory(config.InitialWeight.bytes);
        }

        if (config.Prefab == null)
        {
            throw new InvalidDataException("The prefab in config can't be null");
        }

        if (config.AIConfig == null)
        {
            throw new InvalidDataException("AIConfig can not be null");
        }

        for (int i = 0; i < _bots.Length; i++)
        {
            var botObject = UnityEngine.Object.Instantiate(config.Prefab, Vector3.zero, Quaternion.identity);
            IBot bot = botObject.GetComponent<IBot>();
            _bots[i] = bot;
            bot.Initialize(
                botsBrain?.Clone(),
                _config.EnemyLayer,
                _config.WallsLayer
            );
            _evolutionRewardManagers[i] = new EvolutionRewardManager(
                bot.RewardEvents,
                _config.AIConfig!,
                _enableEvolution
            );
            bot.Dead += BotDeadHandler;
        }

        SpawnBots();
    }

    public bool HasAlive => _aliveCount > 0;
    public int Alive => _aliveCount;
    public string Name => _config.TeamName;
    public string TeamTag => _teamTag;

    public void Activate()
    {
        foreach (var bot in _bots)
        {
            bot.Activate();
        }
        SpawnBots();
    }

    public void Deactivate()
    {
        foreach (var bot in _bots)
        {
            bot.Deactivate();
        }
    }

    public float Reset(SpawnPoint[] newSpawnPoints)
    {
        _aliveCount = _bots.Length;
        _spawnPoints = newSpawnPoints;

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

    private void SpawnBots()
    {
        if (_spawnPoints.Length == 0) return;
        if (_spawnPoints.Length == 1)
        {
            _spawnPoints[0].SpawnBots(_bots);
            return;
        }
        int botsPerSpawner = _bots.Length / _spawnPoints.Length;
        for (int i = 0; i < _spawnPoints.Length; i++)
        {
            var startIndex = i * botsPerSpawner;
            var length = i == _spawnPoints.Length - 1 ? _bots.Length - startIndex : botsPerSpawner;
            ReadOnlySpan<IBot> currentPart = _bots.AsSpan(startIndex, length);
            _spawnPoints[i].SpawnBots(currentPart);
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
        foreach (var revardEventManager in _evolutionRewardManagers)
        {
            revardEventManager.Dispose();
        }
    }
}
