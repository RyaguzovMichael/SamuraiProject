#nullable enable
using UnityEngine;

namespace SamuraiProject.Player
{
    internal sealed class DummyAimStrategy : IAimStrategy
    {
        public Vector2 GetCharacterViewDirection()
        {
            return Vector2.zero;
        }
    }
}
