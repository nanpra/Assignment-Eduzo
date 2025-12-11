using Eduzo.Games.Patterns.Audio;
using Eduzo.Games.Patterns.Data;
using Eduzo.Games.Patterns.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Eduzo.Games.Patterns.Core
{
    public class PatternsGameManager : MonoBehaviour
    {
        public static PatternsGameManager Instance;

        [Header("Win Effects")]
        public GameObject patternsWinVFX;

        private List<PatternsQuestionPattern> runtimeQuestions = new();
        public int currentQuestionIndex = 0;
        private int correctAnswersTotal = 0;

        //to track question results data
        public List<PatternsQuestionResult> gameDataSummary = new();
        public int totalCorrect = 0;
        public int totalWrong = 0;
        public float questionStartTime;
        private int fileID = 00001;

        private void Awake()
        {
            Instance = this;

            // Load last used file ID (default = 1)
            fileID = PlayerPrefs.GetInt("Patterns_FileID", 00001);
        }

        public void LoadGeneratedQuestions(List<PatternsQuestionPattern> q)
        {
            runtimeQuestions = q;
            currentQuestionIndex = 0;
        }

        public void StartGameplay()
        {
            // Reset tracking data
            correctAnswersTotal = 0;
            currentQuestionIndex = 0;
            totalWrong = 0;
            totalCorrect = 0;
            gameDataSummary.Clear();

            if (runtimeQuestions == null || runtimeQuestions.Count == 0)
            {
                Debug.LogWarning("No runtime questions loaded.");
                HandleAllQuestionsCompleted();
                return;
            }

            LoadCurrentQuestionToLoader();
        }

        private void LoadCurrentQuestionToLoader()
        {
            questionStartTime = Time.time;
            var q = runtimeQuestions[currentQuestionIndex];
            PatternsUIPatternLoader.Instance.LoadQuestion(q);
        }

        public void CompleteCurrentQuestion()
        {
            currentQuestionIndex++;

            if (currentQuestionIndex >= runtimeQuestions.Count)
                HandleAllQuestionsCompleted();
            else
                LoadCurrentQuestionToLoader();
        }

        private void HandleAllQuestionsCompleted()
        {
            PatternsAudioManager.Instance.PlaySFX("GameWin");
            patternsWinVFX.SetActive(true);
            PatternsUIManager.Instance.ShowGameOverPanel();

            int score = CalculateFinalScore();
            PatternsGameOverScreen.Instance.ShowPatternsWin(score);

            PatternsCountdownTimer.Instance.StopTimer();
            SaveSessionToJson();
        }

        public void HandleLose()
        {
            PatternsAudioManager.Instance.PlaySFX("GameLose");
            PatternsUIManager.Instance.ShowGameOverPanel();

            int score = CalculateFinalScore();
            PatternsGameOverScreen.Instance.ShowPatternsLose(score);

            PatternsCountdownTimer.Instance.StopTimer();
            SaveSessionToJson();
        }

        public void OnPlayerCorrectAnswerForSlot() => correctAnswersTotal++;

        public void ResetAllRuntimeData()
        {
            runtimeQuestions?.Clear();
            currentQuestionIndex = 0;
            correctAnswersTotal = 0;
            patternsWinVFX.SetActive(false);
            PatternsUIPatternLoader.Instance.ClearAll();
            PatternsCountdownTimer.Instance.StopTimer();
            if (PatternsLifeManager.Instance != null) PatternsLifeManager.Instance.ResetLives();
        }

        public void OnLivesDepleted()
        {
            if (PatternsUIManager.Instance.CurrentMode == PatternsGameMode.Test)
                HandleLose();
        }

        public void OnTimerExpired()
        {
            if (PatternsUIManager.Instance.CurrentMode == PatternsGameMode.Test)
                HandleLose();
        }

        public int CalculateFinalScore()
        {
            int totalResponses = totalCorrect + totalWrong;

            if (totalResponses == 0)
                return 0; // no gameplay happened

            float score = ((float)totalCorrect / totalResponses) * 100f;
            return Mathf.RoundToInt(score);
        }

        public float GetTotalActiveTime()
        {
            float total = 0f;

            foreach (var q in gameDataSummary)
                total += q.activeTime;

            return total;
        }

        public void SaveSessionToJson()
        {
            PatternsGameSessionDTO dto = new()
            {
                SelectedTheme = PatternsPatternFormUI.Instance.currentThemeName,
                TotalFinalScore = CalculateFinalScore(),
                TotalActiveTime = GetTotalActiveTime(),
                NumberOfCorrectAnswersGiven = totalCorrect,
                NumberOfWrongAnswersGiven = totalWrong,
                TotalResponses = totalCorrect + totalWrong
            };

            foreach (var q in gameDataSummary)
            {
                var qdto = new PatternsQuestionResultDTO
                {
                    QuestionNumber = q.QuestionNumber + 1
                };

                foreach (char c in q.patternLetters)
                    qdto.QuestionPatternFormed.Add(c.ToString());

                foreach (int idx in q.missingIndices)
                    qdto.MissingIndices.Add(idx);

                foreach (char c in q.optionsPresented)
                    qdto.GivenOptions.Add(c.ToString());

                foreach (char c in q.correctAnswers)
                    qdto.CorrectAnswers.Add(c.ToString());

                foreach (char c in q.wrongAnswers)
                    qdto.GivenWrongAnswers.Add(c.ToString());

                qdto.activeTime = q.activeTime;
                qdto.AnsweredCorrectly = q.AnsweredCorrectly;

                dto.questions.Add(qdto);
            }

            string folder = Application.persistentDataPath + "/EduzoGames_Patterns_GameSummaryReports/";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string dateStr = DateTime.Now.ToString("dd-MM-yyyy");
            string fileName = $"EduzoGames_Patterns_GameSummaryReport_{dateStr}_{fileID:00000}.json";
            string fullPath = folder + fileName;

            string json = JsonUtility.ToJson(dto, true);
            json = JsonFormatter.CompressArrays(json);

            // Save summary JSON file
            File.WriteAllText(fullPath, json);
            Debug.Log("Saved pattern summary report to: " + fullPath);

            // increment AFTER saving
            fileID++;

            // persist new ID
            PlayerPrefs.SetInt("Patterns_FileID", fileID);
            PlayerPrefs.Save();
        }
    }

    #region Helper DTO Classes for JSON Serialization

    [Serializable]
    public class PatternsGameSessionDTO
    {
        public string SelectedTheme;
        public int TotalFinalScore;
        public float TotalActiveTime;
        public int NumberOfCorrectAnswersGiven;
        public int NumberOfWrongAnswersGiven;
        public int TotalResponses;

        public List<PatternsQuestionResultDTO> questions = new();
    }

    [Serializable]
    public class PatternsQuestionResultDTO
    {
        public int QuestionNumber;
        public float activeTime;
        public List<string> QuestionPatternFormed = new();
        public List<string> GivenOptions = new();
        public List<int> MissingIndices = new();
        public List<string> GivenWrongAnswers = new();
        public List<string> CorrectAnswers = new();
        public bool AnsweredCorrectly;
    }

    public static class JsonFormatter
    {
        public static string CompressArrays(string json)
        {
            // Matches any array block including numbers or strings
            // Example:
            // [ "A", "B" ]
            // [ 1, 2, 3 ]
            return Regex.Replace(
                json,
                @"\[\s*(?:\r?\n\s*(?:(?:""[^""]*"")|\d+),?)*\s*\]",
                match =>
                {
                    // Remove newlines + extra whitespace
                    string compact = match.Value
                        .Replace("\r", "")
                        .Replace("\n", "")
                        .Replace("\t", "");

                    // Collapse spaces
                    while (compact.Contains("  "))
                        compact = compact.Replace("  ", " ");

                    // Trim spaces between brackets
                    compact = compact.Replace("[ ", "[")
                                     .Replace(" ]", "]");

                    return compact;
                },
                RegexOptions.Multiline
            );
        }
    }
    #endregion
}