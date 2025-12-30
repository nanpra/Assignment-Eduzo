using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Eduzo.Games.SequencingEvents.Audio;
using Eduzo.Games.SequencingEvents.Core;

public enum SequencingEventsGameMode
{
    Practice,
    Test
}

namespace Eduzo.Games.SequencingEvents.UI
{
    public class SequencingEventsUIManager : MonoBehaviour
    {
        public static SequencingEventsUIManager Instance;
        public SequencingEventsGameMode CurrentMode { get; private set; }

        [Header("UI Panels")]
        public GameObject mainMenuPanel;
        public GameObject formPanel;
        public GameObject gameplayPanel;
        public GameObject gameOverPanel;

        [Header("Mode Buttons")]
        public Button practiceModeButton;
        public Button testModeButton;
        public Button homeButton;

        [Header("Animation")]
        public float animationFadeDuration = 0.6f;

        private CanvasGroup mainMenuCanvas;
        private CanvasGroup formCanvas;
        private CanvasGroup gameplayCanvas;
        private CanvasGroup gameOverCanvas;

        private void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            mainMenuCanvas = mainMenuPanel.GetComponent<CanvasGroup>();
            formCanvas = formPanel.GetComponent<CanvasGroup>();
            gameplayCanvas = gameplayPanel.GetComponent<CanvasGroup>();
            gameOverCanvas = gameOverPanel.GetComponent<CanvasGroup>();

            gameplayPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            formPanel.SetActive(false);

            practiceModeButton.onClick.AddListener(OnPracticeClicked);
            testModeButton.onClick.AddListener(OnTestClicked);
            homeButton.onClick.AddListener(OnHomeClicked);

            SequencingEventsAudioManager.Instance.PlayBgMusic("BgMusic");
        }

        private void OnPracticeClicked()
        {
            CurrentMode = SequencingEventsGameMode.Practice;
            SequencingEventsAudioManager.Instance.PlaySFX("ButtonClick");
            OpenFormScreen();
        }

        private void OnTestClicked()
        {
            CurrentMode = SequencingEventsGameMode.Test;
            SequencingEventsAudioManager.Instance.PlaySFX("ButtonClick");
            OpenFormScreen();
        }

        private void OpenFormScreen()
        {
            // Transition from main menu to form panel
            mainMenuCanvas.alpha = 1;
            mainMenuCanvas.DOFade(0, animationFadeDuration).OnComplete(() =>
            {
                mainMenuPanel.SetActive(false);
                formCanvas.alpha = 0;
                formPanel.SetActive(true);
                formCanvas.DOFade(1, animationFadeDuration);
            });
        }

        public void StartGameplayFromForm()
        {
            formCanvas.DOFade(0, animationFadeDuration).OnComplete(() =>
            {
                formPanel.SetActive(false);
                // After form confirms and runtime questions are loaded
                gameplayCanvas.alpha = 0;
                gameplayPanel.SetActive(true);
                gameplayCanvas.DOFade(1, animationFadeDuration);
            });

            // Setup mode-specific systems
            if (CurrentMode == SequencingEventsGameMode.Test)
            {
                if (SequencingEventsLifeManager.Instance != null)
                    SequencingEventsLifeManager.Instance.ResetLives();
            }
            else // Practice
            {
                if (SequencingEventsLifeManager.Instance != null)
                    SequencingEventsLifeManager.Instance.DisableLifes();

                if (SequencingEventsCountdownTimer.Instance != null)
                    SequencingEventsCountdownTimer.Instance.DisableTimer();
            }

            // Tell GameManager to start playing
            SequencingEventsGameManager.Instance.StartGameplay();
        }

        public void ShowGameOverPanel()
        {
            gameplayPanel.SetActive(false);
            gameOverCanvas.alpha = 0;
            gameOverPanel.SetActive(true);
            gameOverCanvas.DOFade(1, animationFadeDuration);
        }

        public void OnHomeClicked()
        {
            // reset everything and return to main menu
            SequencingEventsAudioManager.Instance.PlaySFX("ButtonClick");
            SequencingEventsGameFormUI.Instance.ResetForm();

            if (gameplayPanel.activeSelf)
                gameplayCanvas.DOFade(0, animationFadeDuration).OnComplete(() => gameplayPanel.SetActive(false));
            if (gameOverPanel.activeSelf)
                gameOverCanvas.DOFade(0, animationFadeDuration).OnComplete(() => gameOverPanel.SetActive(false));
            mainMenuPanel.SetActive(true);
            mainMenuCanvas.alpha = 0;
            mainMenuCanvas.DOFade(1, animationFadeDuration);
        }
    }
}