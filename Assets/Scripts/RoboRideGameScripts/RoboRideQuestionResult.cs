using System;
using System.Collections.Generic;

namespace Eduzo.Games.RoboRide.Data
{
    [Serializable]
    public class RoboRideQuestionResult
    {
        public int QuestionIndex;
        public string EnteredQuestion;
        public string EnteredSentence;
        public string CorrectAnswerWord;

        public List<string> WrongAttemptsByUser = new();
        public bool AnsweredCorrectly;
        public float ActiveTime;
    }
}