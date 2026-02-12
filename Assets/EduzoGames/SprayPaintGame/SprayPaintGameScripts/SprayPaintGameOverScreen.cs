using DG.Tweening;
using Eduzo.Games.SprayPaint.Audio;
using Eduzo.Games.SprayPaint.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Eduzo.Games.SprayPaint.UI
{
    public class SprayPaintGameOverScreen : MonoBehaviour
    {
        public static SprayPaintGameOverScreen Instance;

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

        public void HandleWin(int score)
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

        public void HandleLose(int score)
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
            bool isTestMode = SprayPaintUIManager.Instance.CurrentMode == SprayPaintGameMode.Test;
            retryButton.transform.parent.GetComponent<Transform>().gameObject.SetActive(!isTestMode); //disable retry button for test mode

            RectTransform parentRT = homeButton.transform.parent.GetComponent<RectTransform>();
            if (isTestMode)
                parentRT.anchoredPosition = new Vector2(0, -700);
            else
                parentRT.anchoredPosition = new Vector2(-200, -700);
        }

        private void OnRetryClicked()
        {
            SprayPaintAudioManager.Instance.PlaySFX("ButtonClick");

            // Hide win effects
            SprayPaintGameManager.Instance.starsVFX.SetActive(false);
            SprayPaintGameManager.Instance.confettiVFX.SetActive(false);
            SprayPaintSprayer.Instance.ForceUnlock();
            SprayPaintQuestionsLoader.Instance.HideQuestion();

            // Fade out Game Over Panel
            CanvasGroup cg = SprayPaintUIManager.Instance.gameOverPanel.GetComponent<CanvasGroup>();
            cg.DOFade(0, 0.5f).OnComplete(() =>
            {
                SprayPaintUIManager.Instance.gameOverPanel.SetActive(false);

                // Restart based on mode
                if (SprayPaintUIManager.Instance.CurrentMode == SprayPaintGameMode.Test)
                {
                    // TEST MODE: Lives
                    SprayPaintLifeManager.Instance.ResetLives();
                    SprayPaintCountdownTimer.Instance.ResetTimer();
                }
                else
                {
                    // PRACTICE MODE: No timer, No lives
                    if (SprayPaintLifeManager.Instance != null)
                        SprayPaintLifeManager.Instance.DisableLifes();

                    if (SprayPaintCountdownTimer.Instance != null)
                        SprayPaintCountdownTimer.Instance.DisableTimer();
                }

                // Now restart gameplay with the SAME loaded questions
                SprayPaintGameManager.Instance.StartGameplay();

                // Show gameplay panel again
                var gameplayCanvas = SprayPaintUIManager.Instance.gameplayPanel.GetComponent<CanvasGroup>();
                SprayPaintUIManager.Instance.gameplayPanel.SetActive(true);
                gameplayCanvas.alpha = 0;
                gameplayCanvas.DOFade(1, 0.5f);
            });
        }

        private void OnHomeClicked() => SprayPaintUIManager.Instance.OnHomeClicked();
    }
}