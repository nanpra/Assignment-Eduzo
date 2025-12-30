using TMPro;
using UnityEngine;
using System.Collections.Generic;
using Eduzo.Games.SequencingEvents.Data;

namespace Eduzo.Games.SequencingEvents.UI
{
    public class SequencingEventsQuestionPanel : MonoBehaviour
    {
        [Header("UI")]
        public TMP_Dropdown scenarioDropdown;

        // Populate dropdown using scenario ScriptableObjects
        public void Initialize(List<SequencingEventsScenario> scenarios)
        {
            if (scenarioDropdown == null)
            {
                Debug.LogError("[SequencingEventsQuestionPanel] Scenario Dropdown is missing.");
                return;
            }

            scenarioDropdown.ClearOptions();

            var options = new List<string> { "Select Scenario" };

            if (scenarios != null)
                foreach (var scenario in scenarios)
                    if (scenario != null)
                        options.Add(scenario.displayName);

            scenarioDropdown.AddOptions(options);
            scenarioDropdown.value = 0;
        }

        // Validate selection and return selected scenario
        public bool IsValid(out SequencingEventsScenario selectedScenario, List<SequencingEventsScenario> allScenarios)
        {
            selectedScenario = null;

            if (scenarioDropdown == null || allScenarios == null)
                return false;

            if (scenarioDropdown.value <= 0)
                return false;

            int index = scenarioDropdown.value - 1;

            if (index < 0 || index >= allScenarios.Count)
                return false;

            selectedScenario = allScenarios[index];
            return selectedScenario != null;
        }
    }
}