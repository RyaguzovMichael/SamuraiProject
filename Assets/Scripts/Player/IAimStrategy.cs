#nullable enable
using UnityEngine;

namespace SamuraiProject.Player
{
    internal interface IAimStrategy
    {
        Vector2 GetCharacterViewDirection();
    }
}
