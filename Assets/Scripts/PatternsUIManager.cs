using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Eduzo.Games.Patterns.Core;
using Eduzo.Games.Patterns.Audio;


public enum PatternsGameMode
{
    Practice,
    Test
}

namespace Eduzo.Games.Patterns.UI
{
    public class PatternsUIManager : MonoBehaviour
    {
        public static PatternsUIManager Instance;
        public PatternsGameMode CurrentMode { get; private set; }

        [Header("UI Panels")]
        public GameObject mainMenuPanel;
        public GameObject formPanel;       // PatternsPatternFormUI root GameObject
        public GameObject gameplayPanel;
        public GameObject gameOverPanel;

        [Header("Mode Buttons")]
        public Button practiceModeButton;
        public Button testModeButton;
        public Button homeButton;

        [Header("Animation")]
        public float animationFadeDuration = 0.6f;
        public Sprite questionMarkIcon;

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
        }

        private void OnPracticeClicked()
        {
            CurrentMode = PatternsGameMode.Practice;
            PatternsAudioManager.Instance.PlaySFX("ButtonClick");
            OpenFormScreen();
        }

        private void OnTestClicked()
        {
            CurrentMode = PatternsGameMode.Test;
            PatternsAudioManager.Instance.PlaySFX("ButtonClick");
            OpenFormScreen();
        }

        private void OpenFormScreen()
        {
            // Transition from main menu to form panel
            mainMenuCanvas.alpha = 0;
            mainMenuCanvas.DOFade(0, animationFadeDuration);
            mainMenuPanel.SetActive(false);

            formPanel.SetActive(true);
            formCanvas.alpha = 0;
            formCanvas.DOFade(1, animationFadeDuration);
        }

        public void StartGameplayFromForm()
        {
            // After form confirms and runtime questions are loaded
            gameplayPanel.SetActive(true);
            gameplayCanvas.alpha = 0;
            gameplayCanvas.DOFade(1, animationFadeDuration);

            // Setup mode-specific systems
            if (CurrentMode == PatternsGameMode.Test)
            {
                PatternsLifeManager.Instance.ResetLives();
                PatternsCountdownTimer.Instance.StartTimer();
            }
            else // Practice
            {
                if (PatternsLifeManager.Instance != null)
                    PatternsLifeManager.Instance.DisableLifes(); // hide/disable UI
                PatternsCountdownTimer.Instance.StopTimer();
                PatternsCountdownTimer.Instance.timerText.text = "∞∞ / ∞∞";  // infinite time display
            }

            // Tell GameManager to start playing
            PatternsGameManager.Instance.StartGameplay();
        }

        public void ShowGameOverPanel()
        {
            gameOverPanel.SetActive(true);
            gameOverCanvas.alpha = 0;
            gameOverCanvas.DOFade(1, animationFadeDuration);
        }

        public void OnHomeClicked()
        {
            // reset everything and return to main menu
            PatternsAudioManager.Instance.PlaySFX("ButtonClick");
            PatternsPatternFormUI.Instance.ResetFormUI();
            PatternsGameManager.Instance.ResetAllRuntimeData();

            formCanvas.DOFade(0, animationFadeDuration).OnComplete(() => formPanel.SetActive(false));
            gameplayPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            mainMenuCanvas.alpha = 0;
            mainMenuCanvas.DOFade(1, animationFadeDuration);
        }
    }
}