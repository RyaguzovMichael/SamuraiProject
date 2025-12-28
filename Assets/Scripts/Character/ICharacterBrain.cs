using System;

namespace SamuraiProject.Character
{
    public interface ICharacterBrain
    {
        ThinkResult Think(float time);

        public event Action OnLightAttack;
        public event Action OnHeavyAttack;
        public event Action OnParry;
        public event Action OnHoldBlock;
        public event Action OnReleaseBlock;
    }
}
