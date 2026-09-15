using System.Linq;
using MakeMeHero.Core;
using UnityEngine;

namespace MakeMeHero.Game
{
    /// <summary>Optional attachable IMGUI panel for researcher-only manual JSON validation and apply.</summary>
    public sealed class EvolutionImportDebugPanel : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField, TextArea(8, 20)] private string decisionJson;
        private EvolutionDecisionResult _result;

        private void Awake() { if (gameManager == null) gameManager = GameManager.Instance; }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20f, 20f, 480f, 430f), "Evolution Import", GUI.skin.window);
            decisionJson = GUILayout.TextArea(decisionJson, GUILayout.Height(220f));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate") && gameManager != null) _result = gameManager.ValidateEvolutionJson(decisionJson);
            if (GUILayout.Button("Apply") && gameManager != null) _result = gameManager.ApplyEvolutionJson(decisionJson);
            GUILayout.EndHorizontal();
            if (_result != null) GUILayout.Label(_result.IsValid ? "VALID" : "INVALID");
            if (_result != null) foreach (var issue in _result.Errors.Concat(_result.Warnings)) GUILayout.Label(issue.Code + ": " + issue.Message);
            GUILayout.EndArea();
        }
    }
}
