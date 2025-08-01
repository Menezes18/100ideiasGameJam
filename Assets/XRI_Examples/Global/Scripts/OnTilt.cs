using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Content.Interaction
{
    /// <summary>
    /// Dispara eventos quando este objeto entra/sai do feixe de uma luz UV (Spot Light),
    /// e controla a própria luz pelo gatilho do XRController.
    /// </summary>
    public class OnUVDetect : MonoBehaviour
    {

        public void Teste1()
        {
            Debug.LogError("on");
        }

        public void Teste()
        {
            Debug.LogError("off");
        }
    }
}
