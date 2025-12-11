using DG.Tweening;
using Eduzo.Games.Patterns.Audio;
using Eduzo.Games.Patterns.Core;
using Eduzo.Games.Patterns.Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Eduzo.Games.Patterns.UI
{
    public class PatternsPatternFormUI : MonoBehaviour
    {
        public static PatternsPatternFormUI Instance;

        #region Inspector Fields
        public TMP_Dropdown themeDropdown;
        public PatternsThemeDatabase themeDatabase;

        public TMP_InputField questionCountInput;
        public GameObject questionPanelPrefab;
        public Transform questionPanelsRoot;

        public GameObject letterPrefab;

        public Button confirmButton;
        public Button addQuestionButton;
        public Button removeQuestionButton;

        public Button leftQuestionButton;
        public Button rightQuestionButton;

        public TextMeshProUGUI questionCountText;
        public TextMeshProUGUI errorText;

        public float slideDistance = 5000f;
        #endregion

        #region Private Fields
        private List<PatternsQuestionPanel> panels = new();
        private int currentIndex = 0;
        private CanvasGroup addButtonCG, removeButtonCG, confirmButtonCG, leftQuestionCG, rightQuestionCG, countQuestionCG;
        [HideInInspector] public string currentThemeName;
        #endregion

        #region Unity Initialization
        private void Awake()
        {
            Instance = this;
            CacheGroups();
            HideAllDynamic();
        }

        private void Start()
        {
            // Force first option to be "Select Theme"
            if (themeDropdown.options[0].text != "Select Theme")
            {
                themeDropdown.options.Insert(0, new TMP_Dropdown.OptionData("Select Theme"));
                themeDropdown.value = 0;
            }

            themeDropdown.onValueChanged.AddListener(OnThemeChanged);

            addQuestionButton.onClick.AddListener(OnAdd);
            removeQuestionButton.onClick.AddListener(OnRemove);
            confirmButton.onClick.AddListener(OnConfirm);

            leftQuestionButton.onClick.AddListener(() => SlideTo(currentIndex - 1, false));
            rightQuestionButton.onClick.AddListener(() => SlideTo(currentIndex + 1, true));

            questionCountInput.onEndEdit.AddListener(OnQuestionCountEntered);
        }
        #endregion

        #region UI Setup
        private void OnThemeChanged(int index)
        {
            // If user selects "Select Theme"
            if (index == 0)
            {
                currentThemeName = "";
                ResetQuestionsOnThemeChange();
                return;
            }

            // Set selected theme
            currentThemeName = themeDropdown.options[index].text;
            Debug.Log("Theme changed to: " + currentThemeName);

            // Also reset all panels when theme changes
            ResetQuestionsOnThemeChange();
        }

        private void CacheGroups()
        {
            addButtonCG = addQuestionButton.GetComponent<CanvasGroup>();
            removeButtonCG = removeQuestionButton.GetComponent<CanvasGroup>();
            confirmButtonCG = confirmButton.GetComponent<CanvasGroup>();

            leftQuestionCG = leftQuestionButton.GetComponent<CanvasGroup>();
            rightQuestionCG = rightQuestionButton.GetComponent<CanvasGroup>();
            countQuestionCG = questionCountText.GetComponent<CanvasGroup>();
        }

        private void HideAllDynamic()
        {
            CanvasGroup[] groups = {
                addButtonCG, removeButtonCG, confirmButtonCG,
                leftQuestionCG, rightQuestionCG, countQuestionCG
            };

            foreach (var g in groups)
            {
                g.alpha = 0;
                g.gameObject.SetActive(false);
            }
        }
        #endregion

        #region Question Count Handling
        private void OnQuestionCountEntered(string value)
        {
            if (!int.TryParse(value, out int count))
            {
                ShowError("Invalid number.");
                return;
            }

            count = Mathf.Max(1, count);

            // Remove extra panels
            while (panels.Count > count)
                RemovePanel(panels.Count - 1);

            // Add missing panels
            while (panels.Count < count)
                AddPanel();

            // Position only the first panel on screen
            for (int i = 0; i < panels.Count; i++)
            {
                RectTransform rt = panels[i].GetComponent<RectTransform>();
                rt.anchoredPosition = i == 0 ? Vector2.zero : new Vector2(slideDistance, 0);
            }

            currentIndex = 0;

            ShowButtons();
            UpdateNavigation();
        }

        private void ShowButtons()
        {
            addButtonCG.gameObject.SetActive(true);
            removeButtonCG.gameObject.SetActive(true);
            confirmButtonCG.gameObject.SetActive(true);

            addButtonCG.DOFade(1, 0.25f);
            removeButtonCG.DOFade(1, 0.25f);
            confirmButtonCG.DOFade(1, 0.25f);
        }
        #endregion

        #region Add / Remove Panels
        private void OnAdd()
        {
            AddPanel();

            questionCountInput.text = panels.Count.ToString();
            UpdateNavigation();
        }

        private void OnRemove()
        {
            if (panels.Count == 0) return;

            int removedIndex = panels.Count - 1;
            bool wasVisiblePanel = removedIndex == currentIndex;

            RemovePanel(removedIndex);

            questionCountInput.text = panels.Count.ToString();

            if (panels.Count == 0)
            {
                HideAllDynamic();
                return;
            }

            if (wasVisiblePanel)
            {
                currentIndex = Mathf.Clamp(removedIndex - 1, 0, panels.Count - 1);
                SlideIn(currentIndex);
            }

            UpdateNavigation();
        }
        #endregion

        #region Panel Creation
        private void AddPanel()
        {
            GameObject go = Instantiate(questionPanelPrefab, questionPanelsRoot);
            var panel = go.GetComponent<PatternsQuestionPanel>();
            panel.letterPrefab = letterPrefab;

            // Connect items count callback
            panel.itemsCountField.onEndEdit.AddListener(v => OnItemsCountEntered(panel, v));

            panels.Add(panel);

            RectTransform rt = go.GetComponent<RectTransform>();
            int index = panels.Count - 1;

            if (index == 0)
            {
                rt.anchoredPosition = Vector2.zero;
                go.transform.localScale = Vector3.zero;
                go.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            }
            else
                rt.anchoredPosition = new Vector2(slideDistance, 0);
        }

        private void RemovePanel(int index)
        {
            Destroy(panels[index].gameObject);
            panels.RemoveAt(index);
        }

        private void OnItemsCountEntered(PatternsQuestionPanel panel, string value)
        {
            if (string.IsNullOrEmpty(currentThemeName))
            {
                ShowError("Please select a theme first.");
                return;
            }

            if (!panel.ValidateItemsCount(out string error))
            {
                ShowError(error);
                return;
            }

            int count = int.Parse(value);
            panel.GenerateLetterDropdowns(count);
        }
        #endregion

        #region Sliding Navigation
        private void SlideTo(int newIndex, bool fromRight)
        {
            if (newIndex < 0 || newIndex >= panels.Count) return;

            PatternsAudioManager.Instance.PlaySFX("Slide");
            RectTransform oldPanel = panels[currentIndex].GetComponent<RectTransform>();
            RectTransform newPanel = panels[newIndex].GetComponent<RectTransform>();

            newPanel.anchoredPosition = new Vector2(fromRight ? slideDistance : -slideDistance, 0);

            oldPanel.DOAnchorPos(new Vector2(fromRight ? -slideDistance : slideDistance, 0), 0.45f);
            newPanel.DOAnchorPos(Vector2.zero, 0.45f);

            currentIndex = newIndex;
            UpdateNavigation();
        }

        private void SlideIn(int index)
        {
            RectTransform rt = panels[index].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-slideDistance, 0);
            rt.DOAnchorPos(Vector2.zero, 0.45f);
        }
        #endregion

        #region Navigation UI
        private void UpdateNavigation()
        {
            if (panels.Count <= 1)
            {
                leftQuestionCG.gameObject.SetActive(false);
                rightQuestionCG.gameObject.SetActive(false);
                countQuestionCG.gameObject.SetActive(false);
                return;
            }

            leftQuestionCG.gameObject.SetActive(true);
            rightQuestionCG.gameObject.SetActive(true);
            countQuestionCG.gameObject.SetActive(true);

            countQuestionCG.DOFade(1f, 0.2f);
            questionCountText.text = $"Question {currentIndex + 1}/{panels.Count}";

            leftQuestionCG.DOFade(currentIndex == 0 ? 0.3f : 1f, 0.2f);
            rightQuestionCG.DOFade(currentIndex == panels.Count - 1 ? 0.3f : 1f, 0.2f);
        }
        #endregion

        #region Confirm Logic
        private void OnConfirm()
        {
            if (panels.Count == 0) { ShowError("Create at least one question."); return; }

            currentThemeName = themeDropdown.options[themeDropdown.value].text;
            var theme = themeDatabase.GetThemeByName(currentThemeName);

            if (theme == null) { ShowError("Theme missing."); return; }

            List<PatternsQuestionPattern> final = new();

            foreach (var p in panels)
            {
                if (!p.ValidateItemsCount(out string e1)) { ShowError(e1); return; }
                if (!p.AllLettersAssigned(out string e2)) { ShowError(e2); return; }
                if (!p.HasAtLeastTwoDifferentLetters()) { ShowError("Use at least 2 different letters."); return; }
                if (!p.HasMissingSelected(out string e3)) { ShowError(e3); return; }

                List<char> letters = p.GetLetterList();
                List<int> missing = p.GetMissingList();

                Sprite[] icons = new Sprite[letters.Count];

                // Convert each letter → sprite using NEW flexible theme rules
                for (int i = 0; i < letters.Count; i++)
                {
                    char letter = letters[i];

                    Sprite sprite = theme.GetSpriteForLetter(letter);

                    if (sprite == null)
                    {
                        ShowError($"No sprite found for letter '{letter}' in theme '{PatternsPatternFormUI.Instance.currentThemeName}'");
                        return;
                    }

                    icons[i] = sprite;
                }

                PatternsQuestionPattern q = ScriptableObject.CreateInstance<PatternsQuestionPattern>();

                q.patternIcons = icons;
                q.patternMissingIndices = missing.ToArray();
                q.patternQuestionMarkIcon = PatternsUIManager.Instance.questionMarkIcon;

                // assign letter keys (A B C D E sequence)
                q.letterKeys = letters.ToArray();

                final.Add(q);
            }

            PatternsGameManager.Instance.LoadGeneratedQuestions(final);
            PatternsUIManager.Instance.StartGameplayFromForm();

            gameObject.SetActive(false);
        }
        #endregion

        #region Error + Reset
        public void ShowError(string msg)
        {
            errorText.text = msg;
            errorText.alpha = 0;
            errorText.DOFade(1, 0.25f);

            CancelInvoke(nameof(HideError));
            Invoke(nameof(HideError), 2f);
        }

        private void HideError()
        {
            errorText.DOFade(0, 0.25f);
        }

        public void ResetFormUI()
        {
            foreach (var p in panels)
                Destroy(p.gameObject);

            panels.Clear();
            currentIndex = 0;

            questionCountInput.text = "";
            HideAllDynamic();

            if (themeDropdown.options.Count > 0)
                themeDropdown.value = 0;

            errorText.text = "";
            errorText.alpha = 0;
        }

        private void ResetQuestionsOnThemeChange()
        {
            // Destroy all question panels
            foreach (var p in panels)
                if (p != null)
                    Destroy(p.gameObject);

            panels.Clear();
            currentIndex = 0;

            // Reset question count field
            questionCountInput.text = "";

            // Hide Add/Remove/Confirm & Navigation
            HideAllDynamic();

            // Hide navigation text as well
            questionCountText.text = "";

            Debug.Log("Questions reset due to theme change.");
        }
        #endregion
    }
}