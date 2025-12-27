using UnityEngine;
using System.Collections.Generic;

public sealed class SpawnPointCycler : MonoBehaviour
{
    [SerializeField] private List<ArrayWrapper> arrayCollection;
    private int _currentIndex = 0;

    public SpawnPoint[] GetNextArray()
    {
        if (arrayCollection == null || arrayCollection.Count == 0)
        {
            return System.Array.Empty<SpawnPoint>();
        }

        SpawnPoint[] result = arrayCollection[_currentIndex].Values;
        _currentIndex = (_currentIndex + 1) % arrayCollection.Count;

        return result;
    }
}

[System.Serializable]
public sealed class ArrayWrapper
{
    [SerializeField] private SpawnPoint[] _values;

    public SpawnPoint[] Values => _values;
}
