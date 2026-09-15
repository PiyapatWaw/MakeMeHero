using MakeMeHero.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MakeMeHero.Game
{
    /// <summary>Reusable HUD binding. Assign optional legacy UI Text fields in the scene or prefab.</summary>
    public sealed class RunHudView : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Text dayText;
        [SerializeField] private Text phaseText;
        [SerializeField] private Text goldText;
        [SerializeField] private Text cityHpText;
        [SerializeField] private HealthBarView cityHealthBar;

        private void OnEnable()
        {
            if (gameManager == null) gameManager = GameManager.Instance;
            if (gameManager != null) gameManager.RunChanged += Render;
        }
        private void Start() { if (gameManager != null) Render(gameManager.CurrentRun); }
        private void OnDisable() { if (gameManager != null) gameManager.RunChanged -= Render; }

        private void Render(Run run)
        {
            if (run == null) return;
            if (dayText != null) dayText.text = "Day " + run.Day;
            if (phaseText != null) phaseText.text = run.Phase.ToString();
            if (goldText != null) goldText.text = run.Gold.ToString();
            if (cityHpText != null) cityHpText.text = run.CityHp + "/25";
            if (cityHealthBar != null) cityHealthBar.SetHealth(run.CityHp, 25m);
        }
    }
}
