using System.Linq;
using MakeMeHero.Core;
using UnityEngine;

namespace MakeMeHero.Presentation
{
    /// <summary>Temporary inspector/debug bridge. Replace with HUD and board presenters later.</summary>
    public sealed class RunDebugView : MonoBehaviour
    {
        [SerializeField] private GameManager controller;

        private void OnEnable()
        {
            if (controller != null) controller.RunChanged += Render;
        }

        private void OnDisable()
        {
            if (controller != null) controller.RunChanged -= Render;
        }

        private void Start()
        {
            if (controller != null && controller.CurrentRun != null) Render(controller.CurrentRun);
        }

        private void Render(Run run)
        {
            var fieldHeroes = run.Heroes.Count(hero => !hero.IsDead && hero.Position.HasValue);
            Debug.Log(string.Format("Run day {0} | {1} | City {2} | Gold {3} | Field heroes {4}", run.Day, run.Phase, run.CityHp, run.Gold, fieldHeroes), this);
        }
    }
}
