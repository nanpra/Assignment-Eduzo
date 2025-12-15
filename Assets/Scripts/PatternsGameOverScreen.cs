using DG.Tweening;
using Eduzo.Games.Patterns.Audio;
using Eduzo.Games.Patterns.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Eduzo.Games.Patterns.UI
{
    public class PatternsGameOverScreen : MonoBehaviour
    {
        public static PatternsGameOverScreen Instance;

        [Header("UI")]
        public Image titleIcon;
        public Sprite gameWinIcon;
        public Sprite gameLoseIcon;
        public Image[] starImages;
        public TextMeshProUGUI finalScoreText;
        public Button retryButton;
        public Button homeButton;


        private void Awake() => Instance = this;

        private void Start()
        {
            retryButton.onClick.AddListener(OnRetryClicked);
            homeButton.onClick.AddListener(OnHomeClicked);
        }

        public void ShowPatternsWin(int score)
        {
            finalScoreText.text = "Score:\n" + score;
            finalScoreText.transform.localScale = Vector3.zero;
            finalScoreText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

            titleIcon.sprite = gameWinIcon;
            titleIcon.transform.localScale = Vector3.zero;
            titleIcon.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);

            SetGameOverScreenButtons();

            float delay = 0.25f;

            for (int i = 0; i < starImages.Length; i++)
            {
                Image star = starImages[i];

                star.transform.localScale = Vector3.zero;
                star.DOFade(0, 0);
                star.transform.DOScale(1f, 0.35f).SetDelay(delay).SetEase(Ease.OutBack);
                star.DOFade(1, 0.35f).SetDelay(delay);

                // Pulse ONLY the first star
                if (i == 0)
                {
                    star.transform
                        .DOScale(1.2f, 0.6f)
                        .SetDelay(delay + 0.35f) // wait until it appears
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                }

                delay += 0.15f;
            }
        }

        public void ShowPatternsLose(int score)
        {
            finalScoreText.text = "Score: " + score;
            finalScoreText.transform.localScale = Vector3.zero;
            finalScoreText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            SetGameOverScreenButtons();

            foreach (var s in starImages) s.enabled = false;

            titleIcon.sprite = gameLoseIcon;
            titleIcon.transform.localScale = Vector3.zero;
            titleIcon.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);
        }

        private void SetGameOverScreenButtons()
        {
            bool isTestMode = PatternsUIManager.Instance.CurrentMode == PatternsGameMode.Test;
            retryButton.transform.parent.GetComponent<Transform>().gameObject.SetActive(!isTestMode); //disable retry button for test mode

            RectTransform parentRT = homeButton.transform.parent.GetComponent<RectTransform>();
            if (isTestMode)
                parentRT.anchoredPosition = new Vector2(-3500, -2500);
            else
                parentRT.anchoredPosition = new Vector2(-3800, -2500);
        }

        private void OnRetryClicked()
        {
            PatternsAudioManager.Instance.PlaySFX("ButtonClick");

            // Hide win effects
            PatternsGameManager.Instance.patternsWinVFX.SetActive(false);
            PatternsGameManager.Instance.starsVFX.SetActive(false);

            // Fade out Game Over Panel
            CanvasGroup cg = PatternsUIManager.Instance.gameOverPanel.GetComponent<CanvasGroup>();
            cg.DOFade(0, 0.5f).OnComplete(() =>
            {
                PatternsUIManager.Instance.gameOverPanel.SetActive(false);

                // Restart based on mode
                if (PatternsUIManager.Instance.CurrentMode == PatternsGameMode.Test)
                {
                    // TEST MODE: Lives + Timer
                    PatternsLifeManager.Instance.ResetLives();
                    PatternsCountdownTimer.Instance.ResetTimer();
                    PatternsCountdownTimer.Instance.StartTimer();
                }
                else
                {
                    // PRACTICE MODE: No timer, No lives
                    if (PatternsLifeManager.Instance != null)
                        PatternsLifeManager.Instance.DisableLifes();

                    if(PatternsCountdownTimer.Instance != null)
                        PatternsCountdownTimer.Instance.DisableTimer();
                }

                // Now restart gameplay with the SAME loaded questions
                PatternsGameManager.Instance.StartGameplay();

                // Show gameplay panel again
                var gameplayCanvas = PatternsUIManager.Instance.gameplayPanel.GetComponent<CanvasGroup>();
                PatternsUIManager.Instance.gameplayPanel.SetActive(true);
                gameplayCanvas.alpha = 0;
                gameplayCanvas.DOFade(1, 0.5f);
            });
        }

        private void OnHomeClicked() => PatternsUIManager.Instance.OnHomeClicked();
    }
}