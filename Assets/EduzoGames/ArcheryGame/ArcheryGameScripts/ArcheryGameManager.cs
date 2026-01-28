using Eduzo.Games.Archery.Audio;
using Eduzo.Games.Archery.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Eduzo.Games.Archery.Core
{
    public class ArcheryGameManager : MonoBehaviour
    {
        public static ArcheryGameManager Instance;

        [Header("Win Effects")]
        public GameObject starsVFX;
        public GameObject confettiVFX;

        private List<ArcheryQuestionData> runtimeQuestions = new();
        public int currentQuestionIndex;

        // Tracking
        public int totalCorrect;
        public int totalWrong;
        private float questionStartTime;

        private readonly List<ArcheryQuestionResult> gameDataSummary = new();
        private ArcheryQuestionResult currentQuestionResult;
        private int fileID;

        private void Awake()
        {
            Instance = this;
            fileID = PlayerPrefs.GetInt("Archery_FileID", 1);
        }

        #region Question Flow

        private void CleanupGameplay()
        {
            ArcheryArrowPool.Instance.ResetPool();
        }

        public void LoadGeneratedQuestions_Backend(List<ArcheryQuestionData> questions)
        {
            runtimeQuestions = new List<ArcheryQuestionData>(questions);
            currentQuestionIndex = 0;
        }

        public void StartGameplay()
        {
            Debug.Log("[Archery] StartGameplay called");
            totalCorrect = 0;
            totalWrong = 0;
            gameDataSummary.Clear();
            currentQuestionIndex = 0;

            if (runtimeQuestions == null || runtimeQuestions.Count == 0)
            {
                Debug.LogError("[Archery] No questions loaded!");
                return;
            }

            ArcheryCountdownTimer.Instance.StartTimer();
            StartCoroutine(LoadQuestion_FrontEnd());
        }

        public void AddWrongAttempt(string attempt)
        {
            if (currentQuestionResult == null || string.IsNullOrEmpty(attempt))
                return;

            currentQuestionResult.WrongAttemptsByUser.Add(attempt);
        }

        public void RegisterOptionsForCurrentQuestion(List<string> options)
        {
            if (currentQuestionResult == null)
                return;

            // Only set once
            if (currentQuestionResult.OptionsProvided.Count == 0)
                currentQuestionResult.OptionsProvided.AddRange(options);
        }

        private IEnumerator LoadQuestion_FrontEnd()
        {
            questionStartTime = Time.time;
            ArcheryQuestionData q = runtimeQuestions[currentQuestionIndex];

            EnsureQuestionResult(q);

            yield return new WaitForSeconds(1);
            ArcheryQuestionsLoader.Instance.LoadQuestion(q);
        }

        private void EnsureQuestionResult(ArcheryQuestionData q)
        {
            // If already created, DO NOTHING
            if (currentQuestionResult != null &&
                currentQuestionResult.QuestionIndex == currentQuestionIndex)
                return;

            currentQuestionResult = new ArcheryQuestionResult
            {
                QuestionIndex = currentQuestionIndex,
                AnswerType = q.answerType,
                EnteredQuestion = q.question,
                CorrectSequence = string.Join(",", q.correctAnswers),
                OptionsProvided = new List<string>(),
                WrongAttemptsByUser = new List<string>(),
                AnsweredCorrectly = false
            };
        }

        private void FinalizeCurrentQuestion(bool answeredCorrectly)
        {
            if (currentQuestionResult == null || currentQuestionResult.IsFinalized)
                return;

            currentQuestionResult.AnsweredCorrectly = answeredCorrectly;
            currentQuestionResult.ActiveTime += Time.time - questionStartTime;
            currentQuestionResult.IsFinalized = true;

            gameDataSummary.Add(currentQuestionResult);
            currentQuestionResult = null;
        }

        public void ResolveAfterAnswer(bool wasCorrect)
        {
            bool isTestMode = ArcheryUIManager.Instance.CurrentMode == ArcheryGameMode.Test;
            bool isLastQuestion = currentQuestionIndex >= runtimeQuestions.Count - 1;

            ArcheryQuestionData currentQuestion = runtimeQuestions[currentQuestionIndex];
            bool isSequenceQuestion = currentQuestion.answerType == ArcheryAnswerType.ArrangeInSequence;

            // TEST MODE (ONE ATTEMPT PER QUESTION)
            if (isTestMode)
            {
                FinalizeCurrentQuestion(wasCorrect);

                // NO LIVES LEFT → END GAME
                if (!ArcheryLifeManager.Instance.HasLivesLeft())
                {
                    HandleLose();
                    return;
                }

                // LAST QUESTION → END GAME
                if (isLastQuestion && !isSequenceQuestion)
                {
                    if (wasCorrect)
                        HandleAllQuestionsCompleted();
                    else
                        HandleLose();

                    return;
                }

                if (isSequenceQuestion)
                {
                    // In sequence questions, even in test mode, allow retries until correct
                    if (wasCorrect)
                        currentQuestionIndex++;

                    StartCoroutine(LoadQuestion_FrontEnd());
                    return;
                }

                // NOT LAST QUESTION → MOVE FORWARD
                currentQuestionIndex++;
                StartCoroutine(LoadQuestion_FrontEnd());
            }

            // PRACTICE MODE (RETRIES ALLOWED)
            if (wasCorrect)
            {
                FinalizeCurrentQuestion(true);

                if (isLastQuestion)
                {
                    HandleAllQuestionsCompleted();
                    return;
                }

                currentQuestionIndex++;
                StartCoroutine(LoadQuestion_FrontEnd());
                return;
            }

            // Retry same question
            StartCoroutine(LoadQuestion_FrontEnd());
        }

        #endregion

        #region End States

        public void OnTimerExpired()
        {
            if (ArcheryUIManager.Instance.CurrentMode == ArcheryGameMode.Test)
                HandleLose();
        }

        public void HandleAllQuestionsCompleted()
        {
            CleanupGameplay();
            ArcheryAudioManager.Instance.PlaySFX("GameWin");
            starsVFX.SetActive(true);
            confettiVFX.SetActive(true);

            ArcheryUIManager.Instance.ShowGameOverPanel();
            ArcheryGameOverScreen.Instance.HandleWin(CalculateFinalScore());

            ArcheryCountdownTimer.Instance.StopTimer();
            SaveSessionToJson();
        }

        public void HandleLose()
        {
            CleanupGameplay();
            ArcheryAudioManager.Instance.PlaySFX("GameLose");

            ArcheryUIManager.Instance.ShowGameOverPanel();
            ArcheryGameOverScreen.Instance.HandleLose(CalculateFinalScore());

            ArcheryCountdownTimer.Instance.StopTimer();
            SaveSessionToJson();
        }

        #endregion

        #region Score & Time

        private int CalculateFinalScore()
        {
            int totalResponses = totalCorrect + totalWrong;
            if (totalResponses == 0) return 0;

            return Mathf.RoundToInt((float)totalCorrect / totalResponses * 100f);
        }

        private float GetTotalActiveTime()
        {
            float total = 0;
            foreach (var q in gameDataSummary)
                total += q.ActiveTime;
            return total;
        }

        #endregion

        #region Save JSON

        private void SaveSessionToJson()
        {
            var dto = new ArcheryGameSessionDTO
            {
                TotalFinalScore = CalculateFinalScore(),
                TotalActiveTime = GetTotalActiveTime(),
                NumberOfCorrectAnswersGiven = totalCorrect,
                NumberOfWrongAnswersGiven = totalWrong,
                TotalResponses = totalCorrect + totalWrong
            };

            foreach (var q in gameDataSummary)
            {
                dto.questions.Add(new ArcheryQuestionResultDTO
                {
                    QuestionNumber = q.QuestionIndex + 1,
                    AnswerType = q.AnswerType.ToString(),
                    EnteredQuestion = q.EnteredQuestion,
                    CorrectAnswer = q.CorrectSequence,
                    OptionsProvided = q.OptionsProvided,
                    WrongAttemptsByUser = q.WrongAttemptsByUser,
                    AnsweredCorrectly = q.AnsweredCorrectly,
                    ActiveTime = q.ActiveTime
                });
            }

            string folder = Path.Combine(Application.persistentDataPath, "EduzoGames_Archery_GameSummaryReports");
            string fileName = $"EduzoGames_Archery_GameSummaryReport_{DateTime.Now:dd-MM-yyyy}_{fileID:00000}.json";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            File.WriteAllText(Path.Combine(folder, fileName), JsonUtility.ToJson(dto, true));

            fileID++;
            PlayerPrefs.SetInt("Archery_FileID", fileID);
            PlayerPrefs.Save();
        }

        #endregion
    }

    #region DTOs

    [Serializable]
    public class ArcheryGameSessionDTO
    {
        public int TotalFinalScore;
        public float TotalActiveTime;
        public int NumberOfCorrectAnswersGiven;
        public int NumberOfWrongAnswersGiven;
        public int TotalResponses;
        public List<ArcheryQuestionResultDTO> questions = new();
    }

    [Serializable]
    public class ArcheryQuestionResultDTO
    {
        public int QuestionNumber;
        public string AnswerType;
        public string EnteredQuestion;
        public string CorrectAnswer;
        public List<string> OptionsProvided;
        public List<string> WrongAttemptsByUser;
        public bool AnsweredCorrectly;
        public float ActiveTime;
    }

    #endregion
}