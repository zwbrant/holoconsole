using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.Events;

namespace Valorem.HoloConsole
{
    public class SimpleClickHandler : MonoBehaviour, IInputClickHandler
    {
        public UnityEvent Invokation;

        public void OnInputClicked(InputClickedEventData eventData)
        {
            Invokation.Invoke();
        }
    }
}
