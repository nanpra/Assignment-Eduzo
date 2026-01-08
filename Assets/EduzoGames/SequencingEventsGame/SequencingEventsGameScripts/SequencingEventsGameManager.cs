using Eduzo.Games.SequencingEvents.Audio;
using Eduzo.Games.SequencingEvents.Data;
using Eduzo.Games.SequencingEvents.UI;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Eduzo.Games.SequencingEvents.Core
{
    public class SequencingEventsGameManager : MonoBehaviour
    {
        public static SequencingEventsGameManager Instance;

        [Header("Win Effects")]
        public GameObject winVFX;
        public GameObject starsVFX;

        private readonly List<SequencingEventsRuntimeQuestion> runtimeQuestions = new();
        public int currentQuestionIndex;

        // Tracking
        public int totalCorrect;
        public int totalWrong;
        private float questionStartTime;
        private SequencingEventsQuestionResult currentQuestionResult;
        private List<string> cachedPlayerOrder;

        private readonly List<SequencingEventsQuestionResult> gameDataSummary = new();
        private int fileID;

        private void Awake()
        {
            Instance = this;
            fileID = PlayerPrefs.GetInt("RoboRide_FileID", 1);
        }

        #region Question Flow

        public void StartGameWithScenarios(List<SequencingEventsScenario> scenarios)
        {
            runtimeQuestions.Clear();

            foreach (var scenario in scenarios)
            {
                SequencingEventsRuntimeQuestion q = new()
                {
                    scenario = scenario,
                    correctOrder = scenario.sequenceSprites,
                    shuffledOrder = ShuffleSprites(scenario.sequenceSprites)
                };

                runtimeQuestions.Add(q);
            }

            currentQuestionIndex = 0;
            SequencingEventsUIManager.Instance.StartGameplayFromForm();
        }

        private Sprite[] ShuffleSprites(Sprite[] original)
        {
            Sprite[] arr = (Sprite[])original.Clone();

            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }

            return arr;
        }

        public void StartGameplay()
        {
            Debug.Log("[RoboRide] StartGameplay called");
            totalCorrect = 0;
            totalWrong = 0;
            gameDataSummary.Clear();
            currentQuestionIndex = 0;
            SequencingEventsQuestionsLoader.Instance.winVFX.SetActive(false);
            SequencingEventsQuestionsLoader.Instance.loseVFX.SetActive(false);

            if (runtimeQuestions == null || runtimeQuestions.Count == 0)
            {
                Debug.LogError("[RoboRide] No questions loaded!");
                return;
            }

            SequencingEventsCountdownTimer.Instance.StartTimer();
            Invoke(nameof(LoadQuestion) , 1f);   //wait for the fade animation of black overlay
        }

        private void LoadQuestion()
        {
            var q = runtimeQuestions[currentQuestionIndex];

            // Start time
            questionStartTime = Time.time;

            // Create new result entry
            currentQuestionResult = new SequencingEventsQuestionResult
            {
                QuestionNumber = currentQuestionIndex + 1,
                SelectedScenario = q.scenario
            };

            SequencingEventsQuestionsLoader.Instance.LoadRuntimeQuestion(q);
        }

        private void TrackData(bool wasCorrect)
        {
            currentQuestionResult.AnsweredCorrectly = wasCorrect;
            currentQuestionResult.ActiveTime = Time.time - questionStartTime;
            currentQuestionResult.PlayerOrder = cachedPlayerOrder;

            gameDataSummary.Add(currentQuestionResult);
        }

        public void SetPlayerOrder(List<string> order)
        {
            cachedPlayerOrder = new List<string>(order);
        }

        public void OnAnswerSubmitted(bool wasCorrect)
        {
            TrackData(wasCorrect);

            bool isLastQuestion = currentQuestionIndex >= runtimeQuestions.Count - 1;
            bool isTestMode = SequencingEventsUIManager.Instance.CurrentMode == SequencingEventsGameMode.Test;

            if (wasCorrect)
            {
                if (isLastQuestion)
                {
                    HandleAllQuestionsCompleted();
                    return;
                }

                currentQuestionIndex++;
                LoadQuestion();
                return;
            }

            // WRONG ANSWER
            if (!isTestMode)
            {
                // PRACTICE MODE → retry same question
                LoadQuestion();
                return;
            }

            // TEST MODE → lose life
            SequencingEventsLifeManager.Instance.LoseLife();

            if (SequencingEventsLifeManager.Instance.CurrentLives <= 0)
            {
                HandleLose();
                return;
            }

            // Move to next question in TEST mode
            if (!isLastQuestion)
                currentQuestionIndex++;

            LoadQuestion();
        }

        #endregion

        #region End States

        public void OnTimerExpired()
        {
            if (SequencingEventsUIManager.Instance.CurrentMode == SequencingEventsGameMode.Test)
                HandleLose();
        }

        public void HandleAllQuestionsCompleted()
        {
            SequencingEventsAudioManager.Instance.PlaySFX("GameWin");

            winVFX.SetActive(true);
            starsVFX.SetActive(true);

            SequencingEventsUIManager.Instance.ShowGameOverPanel();
            SequencingEventsGameOverScreen.Instance.ShowPatternsWin(CalculateFinalScore());

            SequencingEventsCountdownTimer.Instance.StopTimer();
            SaveSessionToJson();
        }

        public void HandleLose()
        {
            SequencingEventsAudioManager.Instance.PlaySFX("GameLose");

            SequencingEventsUIManager.Instance.ShowGameOverPanel();
            SequencingEventsGameOverScreen.Instance.ShowPatternsLose(CalculateFinalScore());

            SequencingEventsCountdownTimer.Instance.StopTimer();
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
            var dto = new SequencingEventsGameSessionDTO
            {
                TotalFinalScore = CalculateFinalScore(),
                TotalActiveTime = GetTotalActiveTime(),
                NumberOfCorrectAnswersGiven = totalCorrect,
                NumberOfWrongAnswersGiven = totalWrong,
                TotalResponses = totalCorrect + totalWrong
            };

            foreach (var q in gameDataSummary)
            {
                dto.questions.Add(new SequencingEventsQuestionResultDTO
                {
                    QuestionNumber = q.QuestionNumber,
                    ScenarioName = q.SelectedScenario.displayName,
                    ActiveTime = q.ActiveTime,
                    AnsweredCorrectly = q.AnsweredCorrectly,
                    PlayerOrder = q.PlayerOrder
                });
            }

            string folder = Path.Combine(Application.persistentDataPath, "EduzoGames_SequencingEvents_GameSummaryReports");
            string fileName = $"EduzoGames_SequencingEvents_GameSummaryReport_{System.DateTime.Now:dd-MM-yyyy}_{fileID:00000}.json";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            File.WriteAllText(Path.Combine(folder, fileName), JsonUtility.ToJson(dto, true));

            fileID++;
            PlayerPrefs.SetInt("SequencingEvents_FileID", fileID);
            PlayerPrefs.Save();
        }

        #endregion
    }

    #region DTOs

    [System.Serializable]
    public class SequencingEventsGameSessionDTO
    {
        public int TotalFinalScore;
        public float TotalActiveTime;
        public int NumberOfCorrectAnswersGiven;
        public int NumberOfWrongAnswersGiven;
        public int TotalResponses;
        public List<SequencingEventsQuestionResultDTO> questions = new();
    }

    [System.Serializable]
    public class SequencingEventsQuestionResultDTO
    {
        public int QuestionNumber;
        public string ScenarioName;
        public float ActiveTime;
        public bool AnsweredCorrectly;
        public List<string> PlayerOrder;
    }

    #endregion

    [System.Serializable]
    public class SequencingEventsRuntimeQuestion
    {
        public SequencingEventsScenario scenario;
        public Sprite[] correctOrder;   // index = slot index
        public Sprite[] shuffledOrder;
    }
}