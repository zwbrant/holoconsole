using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Valorem.HoloConsole
{
    public class HoloConsoleKeywordHandler : MonoBehaviour, ISpeechHandler
    {
        public const string ToggleConsoleKeyword = "Toggle Console";
        public const string AttachConsoleKeyword = "Attach Console";
        public const string DetachConsoleKeyword = "Detach Console";
        public const string ClearConsoleLogKeyword = "Clear Log";
        public const string EnableConsoleKeyword = "Show Console";
        public const string DisableConsoleKeyword = "Hide Console";
        public const string OpenPerformanceViewKeyword = "Open Performance tab";
        public const string OpenConsoleViewKeyword = "Open Console tab";

        public void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
        {
            switch (eventData.RecognizedText)
            {
                case ToggleConsoleKeyword:
                    LogManager.Instance.ToggleEnabled();
                    break;
                case AttachConsoleKeyword:
                    LogManager.Instance.FollowCamera = true;
                    LogManager.Instance.gameObject.SetActive(true);
                    break;
                case DetachConsoleKeyword:
                    LogManager.Instance.FollowCamera = false;
                    break;
                case ClearConsoleLogKeyword:
                    LogManager.Instance.ClearLog();
                    break;
                case EnableConsoleKeyword:
                    LogManager.Instance.gameObject.SetActive(true);
                    break;
                case DisableConsoleKeyword:
                    LogManager.Instance.gameObject.SetActive(false);
                    break;
                case OpenPerformanceViewKeyword:
                    LogManager.Instance.TabSystem.SetSelectedTab(1);
                    break;
                case OpenConsoleViewKeyword:
                    LogManager.Instance.TabSystem.SetSelectedTab(0);
                    break;
                default:
                    break;
            }
        }
    }
}
