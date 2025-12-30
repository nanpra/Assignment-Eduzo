using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Eduzo.Games.Patterns.UI
{
    public class PatternsQuestionPanel : MonoBehaviour
    {
        [Header("Inputs")]
        public TMP_InputField itemsCountField;

        [Header("Containers")]
        public Transform letterSlotContainer;

        [Header("Prefabs")]
        public GameObject letterPrefab;

        public List<TMP_Dropdown> letterDropdowns = new();
        public List<bool> missingFlags = new();

        public int MIN_ITEMS = 3;
        public int MAX_ITEMS = 10;
        public int MAX_MISSINGINDEXES = 3;

        public void GenerateLetterDropdowns(int count)
        {
            var form = PatternsPatternFormUI.Instance;

            if (string.IsNullOrEmpty(form.currentThemeName))
            {
                Debug.LogError("GenerateLetterDropdowns called with no theme selected.");
                PatternsPatternFormUI.Instance.ShowError("Select a theme first.");
                return;
            }

            var theme = form.themeDatabase.GetThemeByName(form.currentThemeName);

            if (theme == null)
            {
                Debug.LogError("Theme not found: " + form.currentThemeName);
                PatternsPatternFormUI.Instance.ShowError("Invalid theme selected.");
                return;
            }

            if (theme.icons == null || theme.icons.Count == 0)
            {
                PatternsPatternFormUI.Instance.ShowError("Theme has no icons assigned.");
                return;
            }

            // SAFE TO CONTINUE

            // Anything previously created?
            foreach (Transform t in letterSlotContainer)
                Destroy(t.gameObject);

            letterDropdowns.Clear();
            missingFlags.Clear();

            count = Mathf.Clamp(count, MIN_ITEMS, MAX_ITEMS);

            for (int i = 0; i < count; i++)
            {
                GameObject slot = Instantiate(letterPrefab, letterSlotContainer);

                slot.transform.localScale = Vector3.zero;
                slot.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetDelay(i * 0.05f);

                TMP_Dropdown dd = slot.GetComponentInChildren<TMP_Dropdown>();
                Toggle toggleBtn = slot.GetComponentInChildren<Toggle>();

                if (slot.transform.Find("QuestionMarkImage").TryGetComponent<Image>(out var questionMarkImage))
                    questionMarkImage.color = new Color(1f, 1f, 1f, 0f);

                // Populate dropdown dynamically
                dd.options.Clear();
                dd.options.Add(new TMP_Dropdown.OptionData("_"));

                for (int j = 0; j < theme.icons.Count; j++)
                {
                    char letter = (char)('A' + j);
                    dd.options.Add(new TMP_Dropdown.OptionData(letter.ToString()));
                }

                dd.value = 0;

                // store references
                letterDropdowns.Add(dd);
                missingFlags.Add(false);

                int index = i;

                toggleBtn.onValueChanged.AddListener(isOn =>
                {
                    HandleMissingToggle(index, questionMarkImage, toggleBtn);
                });
            }
        }

        private void HandleMissingToggle(int index, Image questionMarkImage, Toggle toggle)
        {
            int currentlySelected = 0;
            foreach (bool f in missingFlags)
                if (f) currentlySelected++;

            bool enabling = toggle.isOn;

            // Block more than 3
            if (enabling && currentlySelected >= MAX_MISSINGINDEXES)
            {
                toggle.SetIsOnWithoutNotify(false);
                PatternsPatternFormUI.Instance.ShowError("Only 3 missing slots allowed.");
                return;
            }

            missingFlags[index] = toggle.isOn;

            if (questionMarkImage != null)
                questionMarkImage.DOFade(toggle.isOn ? 1f : 0f, 0.25f);
        }

        public bool ValidateItemsCount(out string error)
        {
            error = "";
            if (!int.TryParse(itemsCountField.text, out int count))
            {
                error = "Enter valid number of items.";
                return false;
            }
            if (count < MIN_ITEMS || count > MAX_ITEMS)
            {
                error = $"Items must be between {MIN_ITEMS} and {MAX_ITEMS}.";
                return false;
            }
            return true;
        }

        public bool AllLettersAssigned(out string error)
        {
            error = "";
            foreach (var dd in letterDropdowns)
            {
                if (dd.value == 0)
                {
                    error = "Assign all letters.";
                    return false;
                }
            }
            return true;
        }

        public bool HasAtLeastTwoDifferentLetters()
        {
            HashSet<string> letters = new();
            foreach (var dd in letterDropdowns)
            {
                string key = dd.options[dd.value].text;
                if (key != "_")
                    letters.Add(key);
            }
            return letters.Count >= 2;
        }

        public bool HasMissingSelected(out string error)
        {
            error = "";
            foreach (bool f in missingFlags)
                if (f) return true;

            error = "Mark at least one missing.";
            return false;
        }

        public List<char> GetLetterList()
        {
            List<char> output = new();

            foreach (var dd in letterDropdowns)
            {
                string s = dd.options[dd.value].text;

                if (s != "_")
                    output.Add(s[0]);
            }
            return output;
        }

        public List<int> GetMissingList()
        {
            List<int> list = new();
            for (int i = 0; i < missingFlags.Count; i++)
                if (missingFlags[i])
                    list.Add(i);
            return list;
        }
    }
}