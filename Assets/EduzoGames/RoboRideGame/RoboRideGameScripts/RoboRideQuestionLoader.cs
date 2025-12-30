using DG.Tweening;
using Eduzo.Games.RoboRide.Audio;
using Eduzo.Games.RoboRide.UI;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Eduzo.Games.RoboRide.Core
{
    public class RoboRideQuestionsLoader : MonoBehaviour
    {
        public static RoboRideQuestionsLoader Instance;

        [Header("UI References")]
        public RectTransform questionsBg;
        public TextMeshProUGUI questionText;
        public TextMeshProUGUI sentenceText;
        public Button submitButton;
        public CanvasGroup blackOverlay;
        public RectTransform star;

        [Header("Highlight Settings")]
        public float highlightInterval = 1f;

        [Header("VFX References")]
        public GameObject winVFX;
        public GameObject loseVFX;

        private RoboRideQuestionData currentQuestion;
        private string[] words;
        private int currentWordIndex;
        private bool canSelect;
        private Coroutine highlightRoutine;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            submitButton.onClick.AddListener(OnSubmitClicked);
        }

        private void OnSubmitClicked()
        {
            if (canSelect || words == null || words.Length == 0)
                return;
            HandleSubmit();
        }

        private void HandleSubmit()
        {
            RoboRideCountdownTimer.Instance.StopTimer();

            string highlightedWord = words[currentWordIndex];
            bool isCorrect = highlightedWord == currentQuestion.correctWord;

            StopHighlighting();

            if (isCorrect)
            {
                RoboRideAudioManager.Instance.PlaySFX("CorrectAnswer");
                RoboRideGameManager.Instance.CompleteCurrentQuestion(true);
                RoboRideGameManager.Instance.totalCorrect++;
            }
            else
            {
                RoboRideAudioManager.Instance.PlaySFX("WrongAnswer");
                RoboRideGameManager.Instance.CompleteCurrentQuestion(false , highlightedWord);
                RoboRideGameManager.Instance.totalWrong++;

                Invoke(nameof(StartHighlighting), 0.25f);
            }
        }

        public void LoadQuestion(RoboRideQuestionData questionData)
        {
            currentQuestion = questionData;

            questionText.text = currentQuestion.question;
            sentenceText.text = currentQuestion.sentence;

            PrepareSentence();
            ClearUIFocus();
        }

        private void ClearUIFocus()
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        private void PrepareSentence()
        {
            words = currentQuestion.sentence.Split(' ');
            currentWordIndex = 0;
        }

        public void StartHighlighting()
        {
            StopHighlighting();
            currentWordIndex = 0;
            highlightRoutine = StartCoroutine(HighlightRoutine());
        }

        private void StopHighlighting()
        {
            if (highlightRoutine != null)
                StopCoroutine(highlightRoutine);

            canSelect = true;
        }

        private IEnumerator HighlightRoutine()
        {
            while (true)
            {
                HighlightWord(currentWordIndex);
                canSelect = false;

                yield return new WaitForSeconds(highlightInterval);

                canSelect = true;
                currentWordIndex = (currentWordIndex + 1) % words.Length;
            }
        }

        private void HighlightWord(int index)
        {
            System.Text.StringBuilder sb = new();

            for (int i = 0; i < words.Length; i++)
            {
                if (i == index)
                    sb.Append($"<color=#FFD700><b>{words[i]}</b></color> ");
                else
                    sb.Append(words[i]).Append(' ');
            }

            sentenceText.text = sb.ToString().TrimEnd();
        }

        public IEnumerator WrongAnswerSequence(System.Action onComplete)
        {
            var platforms = RoboRidePlatformController.Instance;
            var robot = RoboRideGameManager.Instance.robot;

            loseVFX.SetActive(true);

            Transform qText = questionText.transform;
            Transform sText = sentenceText.transform;

            StopHighlighting();
            KillCommonTweens(qText, sText);

            Sequence seq = DOTween.Sequence();

            // Pop OUT question
            PopOutQuestion(seq, qText, sText);

            // Platforms settle DOWN first
            seq.AppendCallback(() =>
            {
                platforms.MovePlatformsDownForRobotCross(null);
            });

            // Wait for platforms to settle
            seq.AppendInterval(0.7f);

            // Robot starts crossing
            int xValue = Random.Range(300, 700);
            seq.Append(robot.CrossPlatforms(xValue ,1.2f));

            // MIDWAY FAIL — robot + platforms fall
            seq.InsertCallback(
                seq.Duration() - 0.7f,
                () => platforms.FallPlatforms());

            // Robot falls slightly AFTER platforms
            DOVirtual.DelayedCall(1.5f, () =>
            {
                robot.FallDown(-3500f, 0.6f);
                robot.robotRT.DOLocalRotate(new Vector3(0, 0, 30), 0.3f).SetEase(Ease.InSine);
            });

            // Wait AFTER fall
            seq.AppendInterval(1f);

            // Overlay fade IN
            FadeOverlayIn(seq);

            // Small hold
            seq.AppendInterval(0.25f);

            // Reset behind overlay
            seq.AppendCallback(() =>
            {
                ResetRobotBehindOverlay();
                platforms.ResetPlatforms();
                RoboRideGameManager.Instance.ResolveAfterAnswer(false);
            });
            onComplete?.Invoke();
            // Overlay fade OUT
            FadeOverlayOut(seq);

            // Pop IN question again
            PopInQuestion(seq, qText, sText);

            // Resume gameplay
            seq.AppendCallback(() =>
            {
                RoboRideCountdownTimer.Instance.StartTimer();
                StartHighlighting();
            });

            yield return seq.WaitForCompletion();
            loseVFX.SetActive(false);
        }

        public IEnumerator CorrectAnswerSequence()
        {
            var platforms = RoboRidePlatformController.Instance;
            var robot = RoboRideGameManager.Instance.robot;
            winVFX.SetActive(true);

            Transform qText = questionText.transform;
            Transform sText = sentenceText.transform;

            StopHighlighting();
            KillCommonTweens(qText, sText);

            Sequence seq = DOTween.Sequence();

            // Pop OUT
            PopOutQuestion(seq, qText, sText);

            // Platforms FALL
            seq.AppendCallback(() => platforms.MovePlatformsDownForRobotCross(null));
            seq.AppendInterval(0.8f);

            // Robot crosses
            seq.Append(robot.CrossPlatforms(2500f, 2f));

            // Collect star early
            seq.InsertCallback(
                seq.Duration() - 1f,
                () => robot.CollectStar(star));

            seq.AppendInterval(0.5f);
            FadeOverlayIn(seq);

            // Reset behind overlay
            seq.AppendCallback(() =>
            {
                ResetRobotBehindOverlay();
                platforms.ResetPlatforms();
                RoboRideGameManager.Instance.ResolveAfterAnswer(true);
            });

            seq.AppendInterval(0.25f);

            // Overlay out
            FadeOverlayOut(seq);

            // Pop IN
            PopInQuestion(seq, qText, sText);

            // Resume
            seq.AppendCallback(() =>
            {
                RoboRideCountdownTimer.Instance.StartTimer();
                StartHighlighting();
            });

            yield return seq.WaitForCompletion();
            robot.bigStarVFX.SetActive(false);
            winVFX.SetActive(false);
        }

        private void KillCommonTweens(Transform qText, Transform sText)
        {
            qText.DOKill();
            sText.DOKill();
            blackOverlay.DOKill();
        }

        private void PopOutQuestion(Sequence seq, Transform qText, Transform sText)
        {
            seq.Append(qText.DOScale(0f, 0.3f).SetEase(Ease.InBack));
            seq.Join(sText.DOScale(0f, 0.3f).SetEase(Ease.InBack));
            seq.Join(questionsBg.DOScale(0f, 0.3f).SetEase(Ease.InBack));
        }

        private void PopInQuestion(Sequence seq, Transform qText, Transform sText)
        {
            seq.Append(qText.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
            seq.Join(sText.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
            seq.Join(questionsBg.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
        }

        private void FadeOverlayIn(Sequence seq) => seq.Append(blackOverlay.DOFade(1f, 1f));
        private void FadeOverlayOut(Sequence seq) => seq.Append(blackOverlay.DOFade(0f, 1f));

        private void ResetRobotBehindOverlay()
        {
            RectTransform rt = RoboRideGameManager.Instance.robot.robotRT;
            rt.DOKill();
            rt.DOLocalRotate(Vector3.zero, 0.1f);
            rt.anchoredPosition = new Vector2(-2900f, -500f);
        }
    }
}