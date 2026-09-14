using UnityEngine;
using UnityEngine.UI;

namespace MakeMeHero.Game
{
    /// <summary>Reusable UI health bar for a City, Hero, or Monster world-space canvas.</summary>
    public sealed class HealthBarView : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private GameObject barRoot;
        [SerializeField] private bool hideWhenFull;

        public void SetHealth(decimal currentHp, decimal maximumHp)
        {
            var normalized = maximumHp <= 0m ? 0f : (float)(currentHp / maximumHp);
            SetNormalized(normalized);
        }

        public void SetNormalized(float normalizedHealth)
        {
            var value = Mathf.Clamp01(normalizedHealth);
            if (fillImage != null) fillImage.fillAmount = value;
            if (barRoot != null && hideWhenFull) barRoot.SetActive(value < 1f);
        }
    }
}
