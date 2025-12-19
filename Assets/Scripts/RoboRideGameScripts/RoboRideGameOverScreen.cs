using DG.Tweening;
using Eduzo.Games.RoboRide.Audio;
using Eduzo.Games.RoboRide.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Eduzo.Games.RoboRide.UI
{
    public class RoboRideGameOverScreen : MonoBehaviour
    {
        public static RoboRideGameOverScreen Instance;

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
            bool isTestMode = RoboRideUIManager.Instance.CurrentMode == RoboRideGameMode.Test;
            retryButton.transform.parent.GetComponent<Transform>().gameObject.SetActive(!isTestMode); //disable retry button for test mode

            RectTransform parentRT = homeButton.transform.parent.GetComponent<RectTransform>();
            if (isTestMode)
                parentRT.anchoredPosition = new Vector2(-3500, -2500);
            else
                parentRT.anchoredPosition = new Vector2(-3800, -2500);
        }

        private void OnRetryClicked()
        {
            RoboRideAudioManager.Instance.PlaySFX("ButtonClick");

            // Hide win effects
            RoboRideGameManager.Instance.patternsWinVFX.SetActive(false);
            RoboRideGameManager.Instance.starsVFX.SetActive(false);

            // Fade out Game Over Panel
            CanvasGroup cg = RoboRideUIManager.Instance.gameOverPanel.GetComponent<CanvasGroup>();
            cg.DOFade(0, 0.5f).OnComplete(() =>
            {
                RoboRideUIManager.Instance.gameOverPanel.SetActive(false);

                // Restart based on mode
                if (RoboRideUIManager.Instance.CurrentMode == RoboRideGameMode.Test)
                {
                    // TEST MODE: Lives
                    RoboRideLifeManager.Instance.ResetLives();
                    RoboRideCountdownTimer.Instance.ResetTimer();
                }
                else
                {
                    // PRACTICE MODE: No timer, No lives
                    if (RoboRideLifeManager.Instance != null)
                        RoboRideLifeManager.Instance.DisableLifes();

                    if (RoboRideCountdownTimer.Instance != null)
                        RoboRideCountdownTimer.Instance.DisableTimer();
                }

                // reset the objects to initial animation positions
                var intro = FindAnyObjectByType<RoboRideGameplayIntroController>();
                if (intro != null)
                    intro.ResetIntroState();

                // Now restart gameplay with the SAME loaded questions
                RoboRideGameManager.Instance.StartGameplay();

                // Show gameplay panel again
                var gameplayCanvas = RoboRideUIManager.Instance.gameplayPanel.GetComponent<CanvasGroup>();
                RoboRideUIManager.Instance.gameplayPanel.SetActive(true);
                gameplayCanvas.alpha = 0;
                gameplayCanvas.DOFade(1, 0.5f);
            });
        }

        private void OnHomeClicked() => RoboRideUIManager.Instance.OnHomeClicked();
    }
}