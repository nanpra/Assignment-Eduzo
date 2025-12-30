using DG.Tweening;
using Eduzo.Games.SequencingEvents.Audio;
using Eduzo.Games.SequencingEvents.Core;
using Eduzo.Games.SequencingEvents.Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Eduzo.Games.SequencingEvents.UI
{
    public class SequencingEventsGameFormUI : MonoBehaviour
    {
        public static SequencingEventsGameFormUI Instance;

        #region Inspector
        public TMP_InputField questionCountInput;

        public GameObject questionPanelPrefab;
        public Transform questionPanelsRoot;

        public Button addQuestionButton;
        public Button removeQuestionButton;
        public Button leftQuestionButton;
        public Button rightQuestionButton;
        public Button confirmButton;

        public TextMeshProUGUI questionCountText;
        public TextMeshProUGUI errorText;

        public float slideDistance = 5000f;

        [Header("Scenario Data")]
        public List<SequencingEventsScenario> scenarios;
        #endregion

        #region Private
        private readonly List<SequencingEventsQuestionPanel> panels = new();
        private int currentIndex;

        private CanvasGroup addCG, removeCG, leftCG, rightCG, countCG, confirmCG;
        #endregion

        #region Init
        private void Awake()
        {
            Instance = this;
            CacheGroups();
            HideAllDynamic();
        }

        private void Start()
        {
            questionCountInput.onEndEdit.AddListener(OnQuestionCountEntered);

            addQuestionButton.onClick.AddListener(OnAdd);
            removeQuestionButton.onClick.AddListener(OnRemove);
            confirmButton.onClick.AddListener(OnConfirm);

            leftQuestionButton.onClick.AddListener(() => SlideTo(currentIndex - 1, false));
            rightQuestionButton.onClick.AddListener(() => SlideTo(currentIndex + 1, true));
        }
        #endregion

        #region Setup
        private void CacheGroups()
        {
            addCG = addQuestionButton.GetComponent<CanvasGroup>();
            removeCG = removeQuestionButton.GetComponent<CanvasGroup>();
            leftCG = leftQuestionButton.GetComponent<CanvasGroup>();
            rightCG = rightQuestionButton.GetComponent<CanvasGroup>();
            countCG = questionCountText.GetComponent<CanvasGroup>();
            confirmCG = confirmButton.GetComponent<CanvasGroup>();
        }

        private void HideAllDynamic()
        {
            CanvasGroup[] groups = { addCG, removeCG, leftCG, rightCG, countCG, confirmCG };
            foreach (var g in groups)
            {
                g.alpha = 0;
                g.gameObject.SetActive(false);
            }
        }
        #endregion

        #region Question Count
        private void OnQuestionCountEntered(string value)
        {
            if (!int.TryParse(value, out int count) || count <= 0)
            {
                ShowError("Enter a valid question count.");
                return;
            }

            while (panels.Count > count)
                RemovePanel(panels.Count - 1);

            while (panels.Count < count)
                AddPanel();

            currentIndex = 0;
            UpdatePositions();
            ShowButtons();
            UpdateNavigation();
        }
        #endregion

        #region Panels
        private void AddPanel()
        {
            GameObject go = Instantiate(questionPanelPrefab, questionPanelsRoot);
            var panel = go.GetComponent<SequencingEventsQuestionPanel>();
            panel.Initialize(scenarios);

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
            {
                rt.anchoredPosition = new Vector2(slideDistance, 0);
            }
        }

        private void RemovePanel(int index)
        {
            Destroy(panels[index].gameObject);
            panels.RemoveAt(index);
        }

        private void UpdatePositions()
        {
            for (int i = 0; i < panels.Count; i++)
            {
                RectTransform rt = panels[i].GetComponent<RectTransform>();
                rt.anchoredPosition = i == 0 ? Vector2.zero : new Vector2(slideDistance, 0);
            }
        }
        #endregion

        #region Navigation
        private void SlideTo(int newIndex, bool fromRight)
        {
            if (newIndex < 0 || newIndex >= panels.Count) return;

            SequencingEventsAudioManager.Instance.PlaySFX("Slide");

            RectTransform oldRT = panels[currentIndex].GetComponent<RectTransform>();
            RectTransform newRT = panels[newIndex].GetComponent<RectTransform>();

            newRT.anchoredPosition = new Vector2(fromRight ? slideDistance : -slideDistance, 0);

            oldRT.DOAnchorPos(new Vector2(fromRight ? -slideDistance : slideDistance, 0), 0.45f);
            newRT.DOAnchorPos(Vector2.zero, 0.45f);

            currentIndex = newIndex;
            UpdateNavigation();
        }

        private void UpdateNavigation()
        {
            if (panels.Count <= 1)
            {
                leftCG.gameObject.SetActive(false);
                rightCG.gameObject.SetActive(false);
                countCG.gameObject.SetActive(false);
                return;
            }

            leftCG.gameObject.SetActive(true);
            rightCG.gameObject.SetActive(true);
            countCG.gameObject.SetActive(true);

            questionCountText.text = $"Question {currentIndex + 1}/{panels.Count}";

            leftCG.DOFade(currentIndex == 0 ? 0.3f : 1f, 0.2f);
            rightCG.DOFade(currentIndex == panels.Count - 1 ? 0.3f : 1f, 0.2f);
        }

        private void ShowButtons()
        {
            addCG.gameObject.SetActive(true);
            removeCG.gameObject.SetActive(true);
            confirmCG.gameObject.SetActive(true);

            addCG.DOFade(1, 0.25f);
            removeCG.DOFade(1, 0.25f);
            confirmCG.DOFade(1, 0.25f);
        }
        #endregion

        #region Confirm
        private void OnConfirm()
        {
            if (panels.Count == 0)
            {
                ShowError("Create at least one question.");
                return;
            }

            List<SequencingEventsScenario> selected = new();

            foreach (var p in panels)
            {
                if (!p.IsValid(out var scenario, scenarios))
                {
                    ShowError("Please select all scenarios.");
                    return;
                }

                selected.Add(scenario);
            }

            SequencingEventsGameManager.Instance.StartGameWithScenarios(selected);
            gameObject.SetActive(false);
        }
        #endregion

        #region Error
        private void ShowError(string msg)
        {
            errorText.text = msg;
            errorText.alpha = 1;
            CancelInvoke(nameof(HideError));
            Invoke(nameof(HideError), 2f);
        }

        private void HideError()
        {
            errorText.alpha = 0;
        }
        #endregion

        #region Add / Remove
        private void OnAdd()
        {
            AddPanel();

            questionCountInput.text = panels.Count.ToString();
            UpdateNavigation();
        }

        private void OnRemove()
        {
            if (panels.Count == 0)
                return;

            int removedIndex = panels.Count - 1;
            bool wasVisible = removedIndex == currentIndex;

            RemovePanel(removedIndex);

            questionCountInput.text = panels.Count.ToString();

            if (panels.Count == 0)
            {
                HideAllDynamic();
                return;
            }

            if (wasVisible)
            {
                currentIndex = Mathf.Clamp(removedIndex - 1, 0, panels.Count - 1);
                SlideIn(currentIndex);
            }

            UpdateNavigation();
        }

        private void SlideIn(int index)
        {
            RectTransform rt = panels[index].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-slideDistance, 0);
            rt.DOAnchorPos(Vector2.zero, 0.45f).SetEase(Ease.OutCubic);
        }

        #endregion

        public void ResetForm()
        {
            // Destroy all question panels
            foreach (var panel in panels)
            {
                if (panel != null)
                    Destroy(panel.gameObject);
            }

            panels.Clear();
            currentIndex = 0;

            // Reset input field
            questionCountInput.text = string.Empty;

            // Reset navigation text
            questionCountText.text = string.Empty;

            // Hide dynamic buttons & navigation
            HideAllDynamic();

            // Reset error text
            errorText.text = string.Empty;
            errorText.alpha = 0;

            // Optional: reset focus
            if (UnityEngine.EventSystems.EventSystem.current != null)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }
}