using DG.Tweening;
using Eduzo.Games.Archery.Audio;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace Eduzo.Games.Archery.Core
{
    public class ArcheryQuestionsLoader : MonoBehaviour
    {
        public static ArcheryQuestionsLoader Instance;

        [Header("UI References")]
        public RectTransform questionsBg;
        public TextMeshProUGUI questionText;
        public CanvasGroup blackOverlay;
        public ArcheryAimController aimController;

        [Header("Target Options")]
        public List<TextMeshProUGUI> targetTexts;

        [Header("Sequence UI")]
        public TextMeshProUGUI sequenceProgressText;

        [Header("VFX References")]
        public GameObject correctAnswerVFX;
        public GameObject wrongAnswerVFX;

        private ArcheryQuestionData currentQuestion;
        private readonly List<string> sequence = new();
        private readonly HashSet<int> usedTargetIndices = new();


        private void Awake()
        {
            Instance = this;
        }

        #region Question Setup

        public void LoadQuestion(ArcheryQuestionData questionData)
        {
            currentQuestion = questionData;
            sequence.Clear();
            usedTargetIndices.Clear();

            if (sequenceProgressText != null)
                sequenceProgressText.text = "";

            questionText.text = currentQuestion.question;

            questionsBg.localScale = Vector3.zero;
            questionsBg.DOScale(1f, 0.25f).SetEase(Ease.OutBack);

            ArcheryArrowPool.Instance.ResetPool();
            aimController.ResetTargetVFX();

            LoadTargets(currentQuestion);
        }

        private void LoadTargets(ArcheryQuestionData questionData)
        {
            List<string> options = new();

            options.AddRange(questionData.correctAnswers);

            if (questionData.wrongAnswers != null)
                options.AddRange(questionData.wrongAnswers);

            while (options.Count < 5)
                options.Add("");

            // Shuffle
            for (int i = options.Count - 1; i > 0; i--)
            {
                int rnd = Random.Range(0, i + 1);
                (options[i], options[rnd]) = (options[rnd], options[i]);
            }

            for (int i = 0; i < targetTexts.Count; i++)
            {
                targetTexts[i].text = options[i];
                targetTexts[i].transform.localScale = Vector3.zero;
                targetTexts[i].transform
                    .DOScale(1f, 0.25f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(i * 0.1f);
            }

            // Register options ONCE per question
            ArcheryGameManager.Instance.RegisterOptionsForCurrentQuestion(options);
        }

        #endregion

        #region Answer Handling

        public bool IsTargetUsed(int index)
        {
            return usedTargetIndices.Contains(index);
        }

        public void OnTargetSelected(string value, int targetIndex)
        {
            if (currentQuestion == null)
                return;

            // Mark target as used
            usedTargetIndices.Add(targetIndex);

            if (currentQuestion.answerType == ArcheryAnswerType.One)
                HandleOneCorrect(value);
            else
                HandleArrangeInSequence(value);
        }

        private void HandleOneCorrect(string value)
        {
            bool isCorrect = value == currentQuestion.correctAnswers[0];

            if (isCorrect)
            {
                ArcheryGameManager.Instance.totalCorrect++;
                PlayCorrect();
            }
            else
            {
                ArcheryGameManager.Instance.AddWrongAttempt(value);
                PlayWrong();
            }
        }

        private void HandleArrangeInSequence(string value)
        {
            int expectedIndex = sequence.Count;

            // WRONG STEP
            if (expectedIndex >= currentQuestion.correctAnswers.Count ||
                value != currentQuestion.correctAnswers[expectedIndex])
            {
                // Build FULL attempt including wrong value
                List<string> failedAttempt = new(sequence)
                {
                    value
                };

                ArcheryGameManager.Instance.AddWrongAttempt(
                    string.Join(",", failedAttempt)
                );

                PlayWrong();
                return;
            }

            // CORRECT STEP
            sequence.Add(value);
            ArcheryGameManager.Instance.totalCorrect++;

            if (sequenceProgressText != null)
                sequenceProgressText.text = string.Join(" ", sequence);

            // SEQUENCE COMPLETED
            if (sequence.Count == currentQuestion.correctAnswers.Count)
                PlayCorrect();
        }

        #endregion

        #region Result Flow

        private void PlayCorrect()
        {
            correctAnswerVFX.SetActive(true);
            ArcheryAudioManager.Instance.PlaySFX("CorrectAnswer");
            StartCoroutine(ProceedAfterResult(true));
        }

        private void PlayWrong()
        {
            ArcheryLifeManager.Instance.LoseLife();
            wrongAnswerVFX.SetActive(true);
            ArcheryAudioManager.Instance.PlaySFX("WrongAnswer");

            ArcheryGameManager.Instance.totalWrong++;

            StartCoroutine(ProceedAfterResult(false));
        }

        private IEnumerator ProceedAfterResult(bool wasCorrect)
        {
            yield return new WaitForSeconds(0.6f);

            correctAnswerVFX.SetActive(false);
            wrongAnswerVFX.SetActive(false);

            ArcheryGameManager.Instance.ResolveAfterAnswer(wasCorrect);
        }

        #endregion

        #region Utility

        public void ResetTargetTexts()
        {
            foreach (var text in targetTexts)
                text.text = string.Empty;
        }

        #endregion
    }
}