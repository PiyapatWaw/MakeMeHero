using UnityEngine;

namespace MakeMeHero.Game
{
    /// <summary>Keeps the complete reference composition visible at any Game-view aspect ratio.</summary>
    [ExecuteAlways, RequireComponent(typeof(Camera))]
    public sealed class ReferenceCameraFit : MonoBehaviour
    {
        [SerializeField] private Vector2 compositionSize = new Vector2(16.72f, 9.41f);
        private void LateUpdate()
        {
            var camera = GetComponent<Camera>();
            if (!camera.orthographic || camera.targetTexture != null) return;
            camera.orthographicSize = Mathf.Max(compositionSize.y * .5f, compositionSize.x * .5f / Mathf.Max(.01f, camera.aspect));
        }
    }
}
