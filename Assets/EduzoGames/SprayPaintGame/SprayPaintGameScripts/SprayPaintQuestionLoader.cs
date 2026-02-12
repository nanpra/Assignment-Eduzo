using DG.Tweening;
using Eduzo.Games.SprayPaint.Audio;
using Eduzo.Games.SprayPaint.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Eduzo.Games.SprayPaint.Core
{
    public class SprayPaintQuestionsLoader : MonoBehaviour
    {
        public static SprayPaintQuestionsLoader Instance;

        [Header("UI References")]
        public RectTransform questionsBg;
        public CanvasGroup blackOverlay;

        [Header("Question Image")]
        public Image questionImage;
        public SprayPaintLetterImagesSO letterImagesSO;

        [Header("VFX References")]
        public GameObject correctAnswerVFX;
        public GameObject wrongAnswerVFX;

        [Header("Tracing")]
        public List<SprayPaintTracingSO> tracingData;

        private SprayPaintQuestionData currentQuestion;
        private readonly List<string> sequence = new();
        private readonly HashSet<int> usedTargetIndices = new();


        private void Awake()
        {
            Instance = this;
        }

        #region Question Setup

        public void LoadQuestion(SprayPaintQuestionData questionData)
        {
            currentQuestion = questionData;
            sequence.Clear();
            usedTargetIndices.Clear();

            LoadQuestionImage();

            HideQuestion();
            questionsBg.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        }

        public void HideQuestion()
        {
            questionsBg.localScale = Vector3.zero;
        }

        private void LoadQuestionImage()
        {
            Sprite sprite = letterImagesSO.GetSprite(
                currentQuestion.answerType,
                currentQuestion.selectedAnswer
            );

            questionImage.sprite = sprite;

            var tracingSO = tracingData.Find(t =>
                t.letter.Equals(
                    currentQuestion.selectedAnswer,
                    System.StringComparison.OrdinalIgnoreCase
                )
            );
            Debug.Log($"[SprayPaint] Question Letter: '{currentQuestion.selectedAnswer}'");
            SprayPaintTracingManager.Instance.StartTracing(tracingSO);
        }

        public void SetQuestionBgSize(SprayPaintGameMode gameMode)
        {
            if(gameMode == SprayPaintGameMode.Test)
                questionsBg.sizeDelta = new Vector2(1350, 1350);
            else
                questionsBg.sizeDelta = new Vector2(1700, 1700);
        }

        #endregion

        #region Result Flow

        public IEnumerator ShowFilledLetterAndContinue()
        {
            if (currentQuestion == null)
                yield break;

            // Get filled sprite
            Sprite filledSprite = letterImagesSO.GetFilledSprite(
                currentQuestion.answerType,
                currentQuestion.selectedAnswer
            );

            if (filledSprite != null)
            {
                questionImage.sprite = filledSprite;

                // APPLY SPRAY COLOR TINT
                questionImage.color =
                    SprayPaintColorManager.Instance.GetSprayCanSelectedColor();
            }

            yield return new WaitForSeconds(0.5f);
            PlayCorrect();

            // Let the player see the result
            yield return new WaitForSeconds(1.6f);

            // Reset color for next question
            questionImage.color = Color.white;
        }

        private void PlayCorrect()
        {
            correctAnswerVFX.SetActive(true);
            SprayPaintAudioManager.Instance.PlaySFX("CorrectAnswer");
            SprayPaintGameManager.Instance.totalCorrect++;
            StartCoroutine(ProceedAfterResult(true));
        }

        public void PlayWrong()
        {
            SprayPaintLifeManager.Instance.LoseLife();
            wrongAnswerVFX.SetActive(true);
            SprayPaintAudioManager.Instance.PlaySFX("WrongAnswer");
            SprayPaintGameManager.Instance.totalWrong++;
            StartCoroutine(ProceedAfterResult(false));
        }

        private IEnumerator ProceedAfterResult(bool wasCorrect)
        {
            yield return new WaitForSeconds(0.6f);

            correctAnswerVFX.SetActive(false);
            wrongAnswerVFX.SetActive(false);

            SprayPaintGameManager.Instance.ResolveAfterAnswer(wasCorrect);
        }

        #endregion
    }
}