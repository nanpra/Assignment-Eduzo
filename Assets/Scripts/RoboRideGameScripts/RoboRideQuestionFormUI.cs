using DG.Tweening;
using Eduzo.Games.RoboRide.Audio;
using Eduzo.Games.RoboRide.Core;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Eduzo.Games.RoboRide.UI
{
    public class RoboRideQuestionFormUI : MonoBehaviour
    {
        public static RoboRideQuestionFormUI Instance;

        [Header("Question Setup")]
        public TMP_InputField questionCountInput;
        public GameObject questionPanelPrefab;
        public Transform questionPanelsRoot;

        [Header("Buttons")]
        public Button confirmButton;
        public Button leftQuestionButton;
        public Button rightQuestionButton;
        public Button addQuestionButton;
        public Button removeQuestionButton;

        [Header("UI")]
        public TextMeshProUGUI questionCountText;
        public TextMeshProUGUI errorText;

        public float slideDistance = 5000f;

        private List<RoboRideQuestionPanel> panels = new();
        private int currentIndex;
        private CanvasGroup leftQuestionCG;
        private CanvasGroup rightQuestionCG;
        private CanvasGroup countQuestionCG;
        private CanvasGroup addButtonCG;
        private CanvasGroup removeButtonCG;
        private CanvasGroup confirmButtonCG;

        private void Awake()
        {
            Instance = this;
            CacheGroups();
        }

        private void Start()
        {
            questionCountInput.onEndEdit.AddListener(OnQuestionCountEntered);
            confirmButton.onClick.AddListener(OnConfirm);
            leftQuestionButton.onClick.AddListener(() => SlideTo(currentIndex - 1, false));
            rightQuestionButton.onClick.AddListener(() => SlideTo(currentIndex + 1, true));
            addQuestionButton.onClick.AddListener(OnAdd);
            removeQuestionButton.onClick.AddListener(OnRemove);
        }

        private void CacheGroups()
        {
            leftQuestionCG = leftQuestionButton.GetComponent<CanvasGroup>();
            rightQuestionCG = rightQuestionButton.GetComponent<CanvasGroup>();
            countQuestionCG = questionCountText.GetComponent<CanvasGroup>();
            addButtonCG = addQuestionButton.GetComponent<CanvasGroup>();
            removeButtonCG = removeQuestionButton.GetComponent<CanvasGroup>();
            confirmButtonCG = confirmButton.GetComponent<CanvasGroup>();
        }

        private void HideAllDynamic()
        {
            CanvasGroup[] groups =
            {
                leftQuestionCG, rightQuestionCG, countQuestionCG,
                addButtonCG, removeButtonCG, confirmButtonCG
            };

            foreach (var g in groups)
            {
                g.alpha = 0;
                g.gameObject.SetActive(false);
            }
        }

        private void OnQuestionCountEntered(string value)
        {
            if (!int.TryParse(value, out int count) || count <= 0)
            {
                ShowError("Enter a valid question count.");
                return;
            }

            ResetPanels();

            for (int i = 0; i < count; i++)
                AddPanel();

            currentIndex = 0;
            UpdatePositions();
            ShowButtons();
            UpdateNavigation();
        }

        private void AddPanel()
        {
            GameObject go = Instantiate(questionPanelPrefab, questionPanelsRoot);
            var panel = go.GetComponent<RoboRideQuestionPanel>();
            panels.Add(panel);

            RectTransform rt = go.GetComponent<RectTransform>();
            int index = panels.Count - 1;

            if (index == 0)
            {
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.zero;
                rt.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            }
            else
            {
                rt.anchoredPosition = new Vector2(slideDistance, 0);
            }
        }

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
            bool wasVisible = removedIndex == currentIndex;

            Destroy(panels[removedIndex].gameObject);
            panels.RemoveAt(removedIndex);

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

        private void UpdatePositions()
        {
            for (int i = 0; i < panels.Count; i++)
            {
                RectTransform rt = panels[i].GetComponent<RectTransform>();
                rt.anchoredPosition = i == 0 ? Vector2.zero : new Vector2(slideDistance, 0);
            }
        }

        private void SlideIn(int index)
        {
            if (index < 0 || index >= panels.Count)
                return;

            RectTransform rt = panels[index].GetComponent<RectTransform>();

            // Start off-screen (left)
            rt.anchoredPosition = new Vector2(-slideDistance, 0);

            // Slide into view
            rt.DOAnchorPos(Vector2.zero, 0.45f)
              .SetEase(Ease.OutCubic);
        }

        private void SlideTo(int newIndex, bool fromRight)
        {
            if (newIndex < 0 || newIndex >= panels.Count) return;

            RoboRideAudioManager.Instance.PlaySFX("Slide");

            RectTransform oldPanel = panels[currentIndex].GetComponent<RectTransform>();
            RectTransform newPanel = panels[newIndex].GetComponent<RectTransform>();

            newPanel.anchoredPosition = new Vector2(fromRight ? slideDistance : -slideDistance, 0);

            oldPanel.DOAnchorPos(new Vector2(fromRight ? -slideDistance : slideDistance, 0), 0.4f);
            newPanel.DOAnchorPos(Vector2.zero, 0.4f);

            currentIndex = newIndex;
            UpdateNavigation();
        }

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

            leftQuestionButton.interactable = currentIndex != 0;
            rightQuestionButton.interactable = currentIndex != panels.Count - 1;
        }

        private void OnConfirm()
        {
            Debug.Log("[RoboRide] Confirm pressed");

            List<RoboRideQuestionData> finalQuestions = new();

            foreach (var panel in panels)
            {
                if (!panel.Validate(out string error))
                {
                    ShowError(error);
                    return;
                }

                finalQuestions.Add(panel.GetData());
            }

            RoboRideGameManager.Instance.LoadGeneratedQuestions(finalQuestions);
            RoboRideUIManager.Instance.StartGameplayFromForm();

            gameObject.SetActive(false);
        }

        private void ResetPanels()
        {
            foreach (var p in panels)
                Destroy(p.gameObject);

            panels.Clear();
        }

        private void ShowError(string msg)
        {
            errorText.text = msg;
            errorText.alpha = 1;
            CancelInvoke(nameof(HideError));
            Invoke(nameof(HideError), 2f);
        }

        private void ShowButtons()
        {
            addButtonCG.gameObject.SetActive(true);
            removeButtonCG.gameObject.SetActive(true);
            confirmButtonCG.gameObject.SetActive(true);

            addButtonCG.DOFade(1f, 0.25f);
            removeButtonCG.DOFade(1f, 0.25f);
            confirmButtonCG.DOFade(1f, 0.25f);
        }

        private void HideError()
        {
            errorText.alpha = 0;
        }

        public void ResetFormUI()
        {
            foreach (var p in panels)
                if (p != null)
                    Destroy(p.gameObject);

            panels.Clear();
            currentIndex = 0;

            questionCountInput.text = "";
            HideAllDynamic();

            errorText.text = "";
            errorText.alpha = 0;
        }
    }
}