using System.Collections.Generic;
using UnityEngine;

namespace Eduzo.Games.SequencingEvents.Data
{
    [System.Serializable]
    public class SequencingEventsQuestionResult
    {
        public int QuestionNumber;
        public SequencingEventsScenario SelectedScenario;
        public float ActiveTime;
        public bool AnsweredCorrectly;

        // Order entered by player (sprite names or IDs)
        public List<string> PlayerOrder = new();
    }
}