using UnityEngine;

namespace Eduzo.Games.SequencingEvents.Data
{
    [CreateAssetMenu(
        fileName = "New Sequencing Scenario",
        menuName = "Eduzo/Games/Sequencing Events/Scenario")]
    public class SequencingEventsScenario : ScriptableObject
    {
        public string scenarioId;
        public string displayName;

        [Header("Sequence Data (ORDER MATTERS)")]
        public Sprite[] sequenceSprites; // size = 4 (ordered)
    }
}