using UnityEngine;

namespace Assets.Authory.Scripts
{
    /// <summary>
    /// Sets the game resolution.
    /// </summary>
    class ResolutionManager : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1)) Screen.SetResolution(400, 400, false);
            if (Input.GetKeyDown(KeyCode.F2)) Screen.SetResolution(800, 800, false);
            if (Input.GetKeyDown(KeyCode.F3)) Screen.SetResolution(1200, 900, false);
            if (Input.GetKeyDown(KeyCode.F3)) Screen.SetResolution(1920, 1080, true);

        }
    }
}
