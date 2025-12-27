#nullable enable
using UnityEngine;

[CreateAssetMenu(fileName = "NewTeamConfig", menuName = "AI/Evolution Team Config")]
public sealed class EvolutionTeamConfig : ScriptableObject
{
    [Header("General")]
    public string TeamName = "BotTeam";
    public string TeamTag = "TeamTag";
    public GameObject? Prefab;

    [Header("Layers")]
    public LayerMask EnemyLayer;
    public LayerMask WallsLayer;

    [Header("AI & Weights")]
    public AIConfig? AIConfig;
    public TextAsset? InitialWeight;
    [Min(1)]
    public int GenerationSize = 1;

    [Header("Mutation Settings")]
    [Range(0.01f, 0.2f), Tooltip("Вероятность изменения каждого конкретного веса в геноме.")]
    public float MutationRate = 0.05f;
    [Range(0.05f, 0.3f), Tooltip("Сила изменения генов, которые выбраны для мутации")]
    public float MutationStrength = 0.1f;
    [Min(0), Tooltip("Количество лучших ботов, которые без изменений переходят в новое поколение")]
    public int ElitismCount = 20;

    private void OnValidate()
    {
        if (Prefab != null && Prefab.GetComponent<IBot>() == null)
        {
            Debug.Log($"Prefab: {Prefab.name} must contain a component with the IBot interface");
            Prefab = null;
        }

        if (ElitismCount > GenerationSize)
        {
            Debug.LogWarning($"ElitismCount in {TeamName} cannot be greater than GenerationSize. Clamping.");
            ElitismCount = GenerationSize;
        }
    }
}
