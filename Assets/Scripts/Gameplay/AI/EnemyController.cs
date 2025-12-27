using UnityEngine;

[RequireComponent(typeof(CombatController))]
public sealed class EnemyController : MonoBehaviour, ITargetable
{
    private CombatController _combatController;

    public Transform Transform => transform;

    public CombatController CombatController => _combatController;

    private void Awake()
    {
        _combatController = GetComponent<CombatController>();
    }

    private void OnEnable()
    {
        _combatController.OnDie += () => Destroy(gameObject);
    }

    private void OnDisable()
    {
        _combatController.OnDie -= () => Destroy(gameObject);
    }
}