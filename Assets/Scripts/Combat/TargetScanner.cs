using System.Collections.Generic;
using UnityEngine;

namespace SamuraiProject.Combat
{
    public sealed class TargetScanner : MonoBehaviour
    {
        private readonly Collider2D[] _resultsBuffer = new Collider2D[250];

        private readonly List<ITargetable> _targets = new();

        private LayerMask _targetLayer;

        public void SetTargetMask(LayerMask mask)
        {
            _targetLayer = mask;
        }

        public void ScanEnvironment(float radius)
        {
            _targets.Clear();

            ContactFilter2D filter = new();
            filter.SetLayerMask(_targetLayer);
            filter.useLayerMask = true;

            int count = Physics2D.OverlapCircle(transform.position, radius, filter, _resultsBuffer);

            for (int i = 0; i < count; i++)
            {
                var col = _resultsBuffer[i];

                // Игнорируем себя
                if (col.gameObject == gameObject) continue;

                if (col.TryGetComponent<ITargetable>(out var target))
                {
                    _targets.Add(target);
                }
            }
        }

        public ITargetable GetNearestTarget()
        {
            ITargetable nearest = null;
            float minDistanceSqr = float.MaxValue;
            Vector3 currentPos = transform.position;

            // Обычный перебор найденного списка
            for (int i = _targets.Count - 1; i >= 0; i--)
            {
                var target = _targets[i];
                if (target == null || target.Transform == null)
                {
                    _targets.RemoveAt(i);
                    continue;
                }

                float distSqr = (target.Transform.position - currentPos).sqrMagnitude;
                if (distSqr < minDistanceSqr)
                {
                    minDistanceSqr = distSqr;
                    nearest = target;
                }
            }

            return nearest;
        }
    }
}

