using DG.Tweening;
using Eduzo.Games.Archery.Audio;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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
        private string lastSelectedValue;

        private void Awake()
        {
            Instance = this;
        }

        #region Question Setup

        public void LoadQuestion(ArcheryQuestionData questionData)
        {
            currentQuestion = questionData;
            sequence.Clear();
            lastSelectedValue = string.Empty;

            if (sequenceProgressText != null)
                sequenceProgressText.text = "";

            // Question text
            questionText.text = currentQuestion.question;

            // Animate BG
            questionsBg.localScale = Vector3.zero;
            questionsBg.DOScale(1f, 0.25f).SetEase(Ease.OutBack);

            LoadTargets(currentQuestion);
            ClearUIFocus();
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

        public void OnTargetSelected(string value)
        {
            if (currentQuestion == null)
                return;

            lastSelectedValue = value;

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
                PlayWrong();
            }
        }

        private void HandleArrangeInSequence(string value)
        {
            int expectedIndex = sequence.Count;

            if (expectedIndex >= currentQuestion.correctAnswers.Count ||
                value != currentQuestion.correctAnswers[expectedIndex])
            {
                PlayWrong();
                return;
            }

            // Correct step
            sequence.Add(value);
            ArcheryGameManager.Instance.totalCorrect++;

            if (sequenceProgressText != null)
                sequenceProgressText.text = string.Join(" ", sequence);

            // Completed sequence
            if (sequence.Count == currentQuestion.correctAnswers.Count)
                PlayCorrect();
        }

        #endregion

        #region Result Flow

        private void PlayCorrect()
        {
            ArcheryAudioManager.Instance.PlaySFX("CorrectAnswer");
            correctAnswerVFX.SetActive(true);
            StartCoroutine(ProceedAfterResult(true));
        }

        private void PlayWrong()
        {
            ArcheryLifeManager.Instance.LoseLife();
            ArcheryAudioManager.Instance.PlaySFX("WrongAnswer");
            wrongAnswerVFX.SetActive(true);

            ArcheryGameManager.Instance.totalWrong++;

            if (currentQuestion.answerType == ArcheryAnswerType.One)
                ArcheryGameManager.Instance.AddWrongAttempt(lastSelectedValue);
            else
                ArcheryGameManager.Instance.AddWrongAttempt(string.Join(",", sequence));

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

        private void ClearUIFocus()
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        public void ResetTargetVFX()
        {
            if (aimController != null)
                aimController.ResetTargetVFX();

            foreach (var text in targetTexts)
                text.text = string.Empty;
        }

        #endregion
    }
}