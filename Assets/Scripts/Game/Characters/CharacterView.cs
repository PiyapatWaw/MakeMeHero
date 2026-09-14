using MakeMeHero.Core;
using UnityEngine;

namespace MakeMeHero.Game
{
    /// <summary>Shared visual binding for a prefab representing a core Character.</summary>
    public abstract class CharacterView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private string characterId;

        protected Character BoundCharacter { get; private set; }
        public string CharacterId { get { return characterId; } }
        public SpriteRenderer SpriteRenderer { get { return spriteRenderer; } }
        public Animator Animator { get { return animator; } }

        public virtual void Bind(Character character)
        {
            BoundCharacter = character;
            characterId = character == null ? string.Empty : character.Id;
            RefreshFromCore();
        }

        public virtual void RefreshFromCore()
        {
            if (BoundCharacter == null) return;
            gameObject.SetActive(!BoundCharacter.IsDead);
        }

        public void FaceRight(bool facingRight)
        {
            if (spriteRenderer != null) spriteRenderer.flipX = !facingRight;
        }
    }
}
