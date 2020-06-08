using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.UI;

namespace Valorem.HoloConsole
{
    public class ManipulationHandler : MonoBehaviour, IInputClickHandler, IFocusable, IManipulationHandler
    {
        public ScrollRect ScrollRect;

        public void OnFocusEnter()
        {
            //Debug.Log("focused");
        }

        public void OnFocusExit()
        {
            //Debug.Log("unfocused");
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            //Debug.Log("tap");
        }

        // Use this for initialization
        void Start()
        {
            //InteractionManager.SourcePressed += HandlePressed;
        }

        private void HandlePressed(UnityEngine.XR.WSA.Input.InteractionSourceState state)
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnManipulationStarted(ManipulationEventData eventData)
        {
            //throw new NotImplementedException();
        }

        public void OnManipulationUpdated(ManipulationEventData eventData)
        {
            //if (eventData.CumulativeDelta.y < 0f)
            //    LogManager.Instance.ScrollWindow.verticalNormalizedPosition += (float)Math.Pow(eventData.CumulativeDelta.y * 1f, 2f);
            //else
            //    LogManager.Instance.ScrollWindow.verticalNormalizedPosition -= (float)Math.Pow(eventData.CumulativeDelta.y * 1f, 2f);

        }

        public void OnManipulationCompleted(ManipulationEventData eventData)
        {
            //throw new NotImplementedException();
        }

        public void OnManipulationCanceled(ManipulationEventData eventData)
        {
            //throw new NotImplementedException();
        }
    }
}
