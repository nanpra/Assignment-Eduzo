using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Eduzo.Games.Patterns.Audio;
using Eduzo.Games.Patterns.Core;
using Eduzo.Games.Patterns.Data;

namespace Eduzo.Games.Patterns.UI
{
    public class PatternsUIPatternLoader : MonoBehaviour
    {
        public static PatternsUIPatternLoader Instance;

        [Header("Parents")]
        public Transform questionParentTransform;
        public Transform optionsParentTransform;

        [Header("Prefabs")]
        public GameObject questionImagePrefab;
        public GameObject optionButtonPrefab;

        [HideInInspector] public static PatternsQuestionPattern currentQuestion;
        public int correctAnswersCount = 0;

        // click lock
        private bool canClick = true;
        private readonly float clickLockSeconds = 1f;
        private List<char> displayedOptionLetters = new();
        private bool[] usedCorrectAnswerSlots;  // to track filled slots and avoid giving same correct answer again


        private void Awake()
        {
            Instance = this;
        }

        public void LoadQuestion(PatternsQuestionPattern question)
        {
            currentQuestion = question;
            usedCorrectAnswerSlots = new bool[currentQuestion.patternMissingIndices.Length];
            correctAnswersCount = 0;

            questionParentTransform.gameObject.SetActive(true);
            optionsParentTransform.gameObject.SetActive(true);

            ResetClickLock();
            LoadDisplayPattern();
            LoadOptions();
            TrackData();
        }

        private void TrackData()
        {
            var summary = new PatternsQuestionResult
            {
                QuestionNumber = PatternsGameManager.Instance.currentQuestionIndex,
                AnsweredCorrectly = false     // default
            };

            summary.patternLetters.AddRange(currentQuestion.letterKeys);
            summary.missingIndices.AddRange(currentQuestion.patternMissingIndices);
            summary.optionsPresented.AddRange(displayedOptionLetters);
            summary.correctAnswers.AddRange(currentQuestion.GetCorrectAnswerLetters());

            PatternsGameManager.Instance.gameDataSummary.Add(summary);
        }

        public void ClearAll()
        {
            foreach (Transform t in questionParentTransform) Destroy(t.gameObject);
            foreach (Transform t in optionsParentTransform) Destroy(t.gameObject);
            currentQuestion = null;
            correctAnswersCount = 0;
        }

        private void LoadDisplayPattern()
        {
            // Clear previous UI
            foreach (Transform child in questionParentTransform)
                Destroy(child.gameObject);

            Sprite[] displayPattern = currentQuestion.GetDisplayPattern();
            int count = displayPattern.Length;

            // Get optimal item size
            Vector2 itemSize = GetItemSize(count);

            // Spawn slots
            for (int i = 0; i < count; i++)
            {
                GameObject slot = Instantiate(questionImagePrefab, questionParentTransform);

                RectTransform rt = slot.GetComponent<RectTransform>();
                rt.sizeDelta = itemSize;

                Image img = rt.GetComponent<Image>();
                img.sprite = displayPattern[i];

                // Animation
                slot.transform.localScale = Vector3.zero;
                slot.transform
                    .DOScale(1f, 0.38f)
                    .SetDelay(i * 0.06f)
                    .SetEase(Ease.OutBack);
            }
        }

        private Vector2 GetItemSize(int count)
        {
            return count switch
            {
                <= 3 => new Vector2(700, 700),
                <= 6 => new Vector2(650, 650),
                <= 8 => new Vector2(600, 600),
                _ => new Vector2(500, 500) // 9–10
            };
        }

        private void LoadOptions()
        {
            foreach (Transform child in optionsParentTransform)
                Destroy(child.gameObject);

            if (currentQuestion == null) return;

            HashSet<Sprite> usedSprites = new(currentQuestion.patternIcons);
            usedSprites.Remove(null);

            List<Sprite> optionList = new(usedSprites);

            List<Sprite> wrongPool = new();
            var theme = PatternsPatternFormUI.Instance.themeDatabase.GetThemeByName(PatternsPatternFormUI.Instance.currentThemeName);

            if (theme != null && theme.icons != null)
            {
                foreach (var icon in theme.icons)
                {
                    if (icon == null) continue;
                    if (!usedSprites.Contains(icon))
                        wrongPool.Add(icon);
                }
            }

            // Add 1 random wrong option
            if (wrongPool.Count > 0)
            {
                Sprite randomWrong = wrongPool[Random.Range(0, wrongPool.Count)];
                optionList.Add(randomWrong);
            }

            ShuffleList(optionList);

            // CLEAR previous
            displayedOptionLetters.Clear();

            // Build option buttons
            for (int i = 0; i < optionList.Count; i++)
            {
                Sprite icon = optionList[i];

                // ADD REAL LETTER TO SUMMARY
                displayedOptionLetters.Add(GetLetterFromSprite(icon));

                GameObject opt = Instantiate(optionButtonPrefab, optionsParentTransform);
                Image img = opt.GetComponent<Image>();
                img.sprite = icon;

                opt.transform.localScale = Vector3.zero;
                opt.transform.DOScale(1f, 0.34f)
                    .SetDelay(0.06f * i)
                    .SetEase(Ease.OutBack);

                Button btn = opt.GetComponent<Button>();
                Sprite captured = icon;
                btn.onClick.AddListener(() => OnOptionSelected(captured));
            }
        }

        private static void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            for (int i = 0; i < n - 1; i++)
            {
                int r = Random.Range(i, n);
                (list[r], list[i]) = (list[i], list[r]);
            }
        }

        private void OnOptionSelected(Sprite chosenIcon)
        {
            if (!canClick) return;
            LockClick();

            Sprite[] answers = currentQuestion.GetCorrectAnswers();

            foreach (var correct in answers)
            {
                if (chosenIcon == correct)
                {
                    HandleCorrectAnswer(chosenIcon);
                    return;
                }
            }

            HandleWrongAnswer(chosenIcon);
        }

        private void LockClick()
        {
            canClick = false;
            Invoke(nameof(ResetClickLock), clickLockSeconds);
        }

        private void ResetClickLock() => canClick = true;

        private void HandleWrongAnswer(Sprite chosenIcon)
        {
            PatternsAudioManager.Instance.PlaySFX("WrongAnswer");

            // update tracking data
            PatternsGameManager.Instance.totalWrong++;
            var summary = PatternsGameManager.Instance.gameDataSummary[^1];
            char wrongLetter = GetLetterFromSprite(chosenIcon);
            summary.wrongAnswers.Add(wrongLetter);

            if (PatternsUIManager.Instance.CurrentMode == PatternsGameMode.Test)
                if (PatternsLifeManager.Instance != null)
                    PatternsLifeManager.Instance.LoseLife();

            // shake missing slots
            foreach (Transform slot in questionParentTransform)
            {
                Image img = slot.GetComponent<Image>();
                if (img.sprite == currentQuestion.patternQuestionMarkIcon)
                {
                    slot.DOShakePosition(0.45f, 18, 12);
                    slot.DOShakeRotation(0.45f, 18, 10);
                }
            }
        }

        private void HandleCorrectAnswer(Sprite chosen)
        {
            PatternsAudioManager.Instance.PlaySFX("CorrectAnswer");

            char chosenLetter = GetLetterFromSprite(chosen);
            char[] correctLetters = currentQuestion.GetCorrectAnswerLetters();

            // Try to find a matching unused correct-answer slot
            int targetIndex = -1;

            for (int i = 0; i < correctLetters.Length; i++)
            {
                if (usedCorrectAnswerSlots[i])
                    continue; // this correct answer already used

                if (correctLetters[i] == chosenLetter)
                {
                    // verify the slot is still empty visually
                    int fillIndex = currentQuestion.patternMissingIndices[i];
                    Image img = questionParentTransform.GetChild(fillIndex).GetComponent<Image>();

                    if (img.sprite == currentQuestion.patternQuestionMarkIcon)
                    {
                        targetIndex = i;
                        break;
                    }
                }
            }

            // No matching correct letter left → this is a WRONG answer
            if (targetIndex == -1)
            {
                HandleWrongAnswer(chosen);
                return;
            }

            // Mark this answer as consumed
            usedCorrectAnswerSlots[targetIndex] = true;

            // Fill slot
            int slotIndex = currentQuestion.patternMissingIndices[targetIndex];
            Transform slot = questionParentTransform.GetChild(slotIndex);
            Image slotImg = slot.GetComponent<Image>();

            correctAnswersCount++;
            PatternsGameManager.Instance.totalCorrect++;
            PatternsGameManager.Instance.OnPlayerCorrectAnswerForSlot();
            AnimateCorrectImageIcon(slotImg , chosen);
        }

        private void AnimateCorrectImageIcon(Image slotImg , Sprite chosen)
        {
            // Animation
            Sequence seq = DOTween.Sequence();
            seq.Append(slotImg.transform.DORotate(new Vector3(0, 360, 0), 0.6f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine));
            seq.Join(slotImg.transform.DOScale(1.28f, 0.28f).SetEase(Ease.OutQuad));
            seq.InsertCallback(0.28f, () => slotImg.sprite = chosen);
            seq.Append(slotImg.transform.DOScale(1f, 0.28f).SetEase(Ease.OutBack));

            seq.OnComplete(() =>
            {
                if (correctAnswersCount >= currentQuestion.patternMissingIndices.Length)
                {
                    foreach (Transform t in questionParentTransform)
                        t.GetComponent<Image>().DOFade(0f, 0.35f);

                    foreach (Transform t in optionsParentTransform)
                        t.GetComponent<Image>().DOFade(0f, 0.28f);

                    Invoke(nameof(NotifyQuestionComplete), 0.42f);
                }
            });
        }

        private void NotifyQuestionComplete()
        {
            // Record elapsed time
            float elapsed = Time.time - PatternsGameManager.Instance.questionStartTime;
            PatternsQuestionResult result = PatternsGameManager.Instance.gameDataSummary[^1];
            result.activeTime = elapsed;

            result.AnsweredCorrectly = true;   // question completed correctly
            ClearAll();
            PatternsGameManager.Instance.CompleteCurrentQuestion();
        }

        private char GetLetterFromSprite(Sprite sprite)
        {
            // Check pattern icons first
            for (int i = 0; i < currentQuestion.patternIcons.Length; i++)
            {
                if (currentQuestion.patternIcons[i] == sprite)
                    return currentQuestion.letterKeys[i];
            }

            // Check theme icons → convert index → letter A,B,C,...
            var theme = PatternsPatternFormUI.Instance.themeDatabase.GetThemeByName(PatternsPatternFormUI.Instance.currentThemeName);
            if (theme != null)
            {
                for (int i = 0; i < theme.icons.Count; i++)
                {
                    if (theme.icons[i] == sprite)
                        return (char)('A' + i);  // letter based on theme order
                }
            }

            return '?';
        }
    }
}