using Eduzo.Games.SprayPaint.Audio;
using Eduzo.Games.SprayPaint.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Eduzo.Games.SprayPaint.Core
{
    public class SprayPaintGameManager : MonoBehaviour
    {
        public static SprayPaintGameManager Instance;

        [Header("Win Effects")]
        public GameObject starsVFX;
        public GameObject confettiVFX;

        private List<SprayPaintQuestionData> runtimeQuestions = new();
        public int currentQuestionIndex;

        // Tracking
        public int totalCorrect;
        public int totalWrong;
        private float questionStartTime;

        private readonly List<SprayPaintQuestionResult> gameDataSummary = new();
        private SprayPaintQuestionResult currentQuestionResult;
        private int fileID;

        private void Awake()
        {
            Instance = this;
            fileID = PlayerPrefs.GetInt("SprayPaint_FileID", 1);
        }

        #region Question Flow

        public void LoadGeneratedQuestions_Backend(List<SprayPaintQuestionData> questions)
        {
            runtimeQuestions = new List<SprayPaintQuestionData>(questions);
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

            SprayPaintCountdownTimer.Instance.StartTimer();
            StartCoroutine(LoadQuestion_FrontEnd());
        }

        private IEnumerator LoadQuestion_FrontEnd()
        {
            questionStartTime = Time.time;
            SprayPaintQuestionData q = runtimeQuestions[currentQuestionIndex];

            EnsureQuestionResult(q);

            yield return new WaitForSeconds(1);
            SprayPaintQuestionsLoader.Instance.LoadQuestion(q);
        }

        private void EnsureQuestionResult(SprayPaintQuestionData q)
        {
            // If already created, DO NOTHING
            if (currentQuestionResult != null &&
                currentQuestionResult.QuestionIndex == currentQuestionIndex)
                return;

            currentQuestionResult = new SprayPaintQuestionResult
            {
                QuestionIndex = currentQuestionIndex,
                QuestionType = q.answerType,
                EnteredQuestion = q.selectedAnswer,
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
            bool isTestMode = SprayPaintUIManager.Instance.CurrentMode == SprayPaintGameMode.Test;
            bool isLastQuestion = currentQuestionIndex >= runtimeQuestions.Count - 1;

            // TEST MODE (ONE ATTEMPT PER QUESTION)
            if (isTestMode)
            {
                FinalizeCurrentQuestion(wasCorrect);

                // NO LIVES LEFT → END GAME
                if (!SprayPaintLifeManager.Instance.HasLivesLeft())
                {
                    HandleLose();
                    return;
                }

                // LAST QUESTION → END GAME
                if (isLastQuestion)
                {
                    if (wasCorrect)
                        HandleAllQuestionsCompleted();
                    else
                        HandleLose();

                    return;
                }

                // NOT LAST QUESTION → MOVE FORWARD
                currentQuestionIndex++;
                StartCoroutine(LoadQuestion_FrontEnd());
                return;
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
            if (SprayPaintUIManager.Instance.CurrentMode == SprayPaintGameMode.Test)
                HandleLose();
        }

        public void HandleAllQuestionsCompleted()
        {
            SprayPaintAudioManager.Instance.PlaySFX("GameWin");
            starsVFX.SetActive(true);
            confettiVFX.SetActive(true);

            SprayPaintUIManager.Instance.ShowGameOverPanel();
            SprayPaintGameOverScreen.Instance.HandleWin(CalculateFinalScore());

            SprayPaintCountdownTimer.Instance.StopTimer();
            SaveSessionToJson();
        }

        public void HandleLose()
        {
            SprayPaintAudioManager.Instance.PlaySFX("GameLose");

            SprayPaintUIManager.Instance.ShowGameOverPanel();
            SprayPaintGameOverScreen.Instance.HandleLose(CalculateFinalScore());

            SprayPaintCountdownTimer.Instance.StopTimer();
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
            var dto = new SprayPaintGameSessionDTO
            {
                TotalFinalScore = CalculateFinalScore(),
                TotalActiveTime = GetTotalActiveTime(),
                NumberOfCorrectAnswersGiven = totalCorrect,
                NumberOfWrongAnswersGiven = totalWrong,
                TotalResponses = totalCorrect + totalWrong
            };

            foreach (var q in gameDataSummary)
            {
                dto.questions.Add(new SprayPaintQuestionResultDTO
                {
                    QuestionNumber = q.QuestionIndex + 1,
                    QuestionType = q.QuestionType.ToString(),
                    EnteredQuestion = q.EnteredQuestion,
                    AnsweredCorrectly = q.AnsweredCorrectly,
                    ActiveTime = q.ActiveTime
                });
            }

            string folder = Path.Combine(Application.persistentDataPath, "EduzoGames_SprayPaint_GameSummaryReports");
            string fileName = $"EduzoGames_SprayPaint_GameSummaryReport_{DateTime.Now:dd-MM-yyyy}_{fileID:00000}.json";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            File.WriteAllText(Path.Combine(folder, fileName), JsonUtility.ToJson(dto, true));

            fileID++;
            PlayerPrefs.SetInt("SprayPaint_FileID", fileID);
            PlayerPrefs.Save();
        }

        #endregion
    }

    #region DTOs

    [Serializable]
    public class SprayPaintGameSessionDTO
    {
        public int TotalFinalScore;
        public float TotalActiveTime;
        public int NumberOfCorrectAnswersGiven;
        public int NumberOfWrongAnswersGiven;
        public int TotalResponses;
        public List<SprayPaintQuestionResultDTO> questions = new();
    }

    [Serializable]
    public class SprayPaintQuestionResultDTO
    {
        public int QuestionNumber;
        public string QuestionType;
        public string EnteredQuestion;
        public bool AnsweredCorrectly;
        public float ActiveTime;
    }

    #endregion
}