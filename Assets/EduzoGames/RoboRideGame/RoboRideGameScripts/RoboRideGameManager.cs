using DG.Tweening;
using Eduzo.Games.RoboRide.Audio;
using Eduzo.Games.RoboRide.Data;
using Eduzo.Games.RoboRide.UI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Eduzo.Games.RoboRide.Core
{
    public class RoboRideGameManager : MonoBehaviour
    {
        public static RoboRideGameManager Instance;

        [Header("Win Effects")]
        public GameObject starsVFX;

        [Header("Robot References")]
        public RoboRideRobotMovement robot;

        private List<RoboRideQuestionData> runtimeQuestions = new();
        public int currentQuestionIndex;

        // Tracking
        public int totalCorrect;
        public int totalWrong;
        private float questionStartTime;

        private readonly List<RoboRideQuestionResult> gameDataSummary = new();
        private RoboRideQuestionResult currentQuestionResult;
        private int fileID;

        private void Awake()
        {
            Instance = this;

            fileID = PlayerPrefs.GetInt("RoboRide_FileID", 1);
        }

        #region Question Flow

        public void LoadGeneratedQuestions(List<RoboRideQuestionData> questions)
        {
            runtimeQuestions = new List<RoboRideQuestionData>(questions);
            currentQuestionIndex = 0;
        }

        public void StartGameplay()
        {
            Debug.Log("[RoboRide] StartGameplay called");
            totalCorrect = 0;
            totalWrong = 0;
            gameDataSummary.Clear();
            currentQuestionIndex = 0;

            if (runtimeQuestions == null || runtimeQuestions.Count == 0)
            {
                Debug.LogError("[RoboRide] No questions loaded!");
                return;
            }

            RoboRideGameplayIntroController intro = FindAnyObjectByType<RoboRideGameplayIntroController>();
            intro.PlayIntro();
            LoadQuestion();
        }

        private void LoadQuestion()
        {
            questionStartTime = Time.time;

            RoboRideQuestionData q = runtimeQuestions[currentQuestionIndex];
            RoboRideQuestionsLoader.Instance.LoadQuestion(q);

            Invoke(nameof(ResetStar) , 1);

            // INIT QUESTION TRACKING
            currentQuestionResult = new RoboRideQuestionResult
            {
                QuestionIndex = currentQuestionIndex,
                EnteredQuestion = q.question,
                EnteredSentence = q.sentence,
                CorrectAnswerWord = q.correctWord,
                WrongAttemptsByUser = new List<string>(),
                AnsweredCorrectly = false
            };
        }

        private void ResetStar()
        {
            RectTransform starRt = RoboRideQuestionsLoader.Instance.star;
            starRt.localPosition = new Vector2(1600, -90);
            starRt.localScale = Vector2.zero;
            starRt.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetDelay(0.3f);
        }

        public void CompleteCurrentQuestion(bool answeredCorrectly , string selectedWrongWord = null)
        {
            //disable thinking vfx for robot
            robot.StopThinkingVFX();

            if (!answeredCorrectly)
            {
                HandleWrongAnswer(selectedWrongWord);
                return;
            }

            StartCoroutine(RoboRideQuestionsLoader.Instance.CorrectAnswerSequence());
        }

        public void HandleWrongAnswer(string submittedWord)
        {
            currentQuestionResult.WrongAttemptsByUser.Add(submittedWord);
            RoboRideLifeManager.Instance.LoseLife();
            StartCoroutine(RoboRideQuestionsLoader.Instance.WrongAnswerSequence(CheckLifeOver));
        }

        private void CheckLifeOver()
        {
            if (RoboRideLifeManager.Instance.CurrentLives <= 0)
            {
                FinalizeCurrentQuestion(false);
                HandleLose();
            }
        }

        private void FinalizeCurrentQuestion(bool answeredCorrectly)
        {
            currentQuestionResult.AnsweredCorrectly = answeredCorrectly;
            currentQuestionResult.ActiveTime = Time.time - questionStartTime;
            gameDataSummary.Add(currentQuestionResult);
        }

        public void ResolveAfterAnswer(bool wasCorrect)
        {
            bool isLastQuestion = currentQuestionIndex >= runtimeQuestions.Count - 1;
            FinalizeCurrentQuestion(wasCorrect);

            if (wasCorrect)
            {
                if (isLastQuestion)
                    HandleAllQuestionsCompleted();
                else
                {
                    currentQuestionIndex++;
                    LoadQuestion();
                }
            }
            else
            {
                if (!isLastQuestion)
                {
                    currentQuestionIndex++;
                    LoadQuestion();
                }
                else
                    LoadQuestion();
            }
        }

        #endregion

        #region End States

        public void OnTimerExpired()
        {
            if (RoboRideUIManager.Instance.CurrentMode == RoboRideGameMode.Test)
                HandleLose();
        }

        public void HandleAllQuestionsCompleted()
        {
            RoboRideAudioManager.Instance.PlaySFX("GameWin");
            starsVFX.SetActive(true);

            RoboRideUIManager.Instance.ShowGameOverPanel();
            RoboRideGameOverScreen.Instance.ShowPatternsWin(CalculateFinalScore());

            RoboRideCountdownTimer.Instance.StopTimer();
            SaveSessionToJson();
        }

        public void HandleLose()
        {
            RoboRideAudioManager.Instance.PlaySFX("GameLose");

            RoboRideUIManager.Instance.ShowGameOverPanel();
            RoboRideGameOverScreen.Instance.ShowPatternsLose(CalculateFinalScore());

            RoboRideCountdownTimer.Instance.StopTimer();
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
            var dto = new RoboRideGameSessionDTO
            {
                TotalFinalScore = CalculateFinalScore(),
                TotalActiveTime = GetTotalActiveTime(),
                NumberOfCorrectAnswersGiven = totalCorrect,
                NumberOfWrongAnswersGiven = totalWrong,
                TotalResponses = totalCorrect + totalWrong
            };

            foreach (var q in gameDataSummary)
            {
                dto.questions.Add(new RoboRideQuestionResultDTO
                {
                    QuestionNumber = q.QuestionIndex + 1,
                    EnteredQuestion = q.EnteredQuestion,
                    EnteredSentence = q.EnteredSentence,
                    CorrectAnswerWord = q.CorrectAnswerWord,
                    WrongAttemptsByUser = q.WrongAttemptsByUser,
                    AnsweredCorrectly = q.AnsweredCorrectly,
                    ActiveTime = q.ActiveTime
                });
            }

            string folder = Path.Combine(Application.persistentDataPath, "EduzoGames_RoboRide_GameSummaryReports");
            string fileName = $"EduzoGames_RoboRide_GameSummaryReport_{DateTime.Now:dd-MM-yyyy}_{fileID:00000}.json";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            File.WriteAllText(Path.Combine(folder, fileName), JsonUtility.ToJson(dto, true));

            fileID++;
            PlayerPrefs.SetInt("RoboRide_FileID", fileID);
            PlayerPrefs.Save();
        }

        #endregion
    }

    #region DTOs

    [Serializable]
    public class RoboRideGameSessionDTO
    {
        public int TotalFinalScore;
        public float TotalActiveTime;
        public int NumberOfCorrectAnswersGiven;
        public int NumberOfWrongAnswersGiven;
        public int TotalResponses;
        public List<RoboRideQuestionResultDTO> questions = new();
    }

    [Serializable]
    public class RoboRideQuestionResultDTO
    {
        public int QuestionNumber;
        public string EnteredQuestion;
        public string EnteredSentence;
        public string CorrectAnswerWord;

        public List<string> WrongAttemptsByUser;
        public bool AnsweredCorrectly;
        public float ActiveTime;
    }

    #endregion
}