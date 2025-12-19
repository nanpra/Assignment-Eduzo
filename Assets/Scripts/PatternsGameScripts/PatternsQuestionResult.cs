using System.Collections.Generic;

namespace Eduzo.Games.Patterns.Data
{
    [System.Serializable]
    public class PatternsQuestionResult
    {
        public int QuestionNumber;

        // Full pattern letters in order
        public List<char> patternLetters = new();

        // Missing slot indices (ex: [2,5])
        public List<int> missingIndices = new();

        // Options shown to player
        public List<char> optionsPresented = new();

        // Correct answers for missing positions
        public List<char> correctAnswers = new();

        // Wrong answers selected by the player
        public List<char> wrongAnswers = new();

        // Time taken to answer this question
        public float activeTime;

        // Whether the player answered this question correctly
        public bool AnsweredCorrectly;
    }
}