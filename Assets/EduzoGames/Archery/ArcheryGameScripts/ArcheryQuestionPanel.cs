using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public enum ArcheryAnswerType
{
    One,
    ArrangeInSequence
}

[System.Serializable]
public class ArcheryQuestionData
{
    public string question;
    public ArcheryAnswerType answerType;
    public List<string> correctAnswers;
    public List<string> wrongAnswers;
}


namespace Eduzo.Games.Archery.UI
{
    public class ArcheryQuestionPanel : MonoBehaviour
    {
        [Header("Question")]
        public TMP_InputField questionInput;
        public TMP_Dropdown answerTypeDropdown;
        public GameObject wrongAnswerText;

        [Header("Prefabs")]
        public GameObject singleLetterInputPrefab;

        [Header("Containers")]
        public Transform correctAnswersRoot;
        public Transform wrongAnswersRoot;

        private readonly List<TMP_InputField> correctInputs = new();
        private readonly List<TMP_InputField> wrongInputs = new();

        private void Awake()
        {
            answerTypeDropdown.ClearOptions();
            answerTypeDropdown.AddOptions(new List<string> { "One Correct", "Arrange In Sequence" });
            answerTypeDropdown.onValueChanged.AddListener(OnAnswerTypeChanged);
        }

        private void Start()
        {
            OnAnswerTypeChanged(answerTypeDropdown.value);
        }

        #region Answer Type Logic

        private void OnAnswerTypeChanged(int index)
        {
            ClearAll();

            ArcheryAnswerType type = (ArcheryAnswerType)index;

            if (type == ArcheryAnswerType.One)
            {
                correctAnswersRoot.gameObject.SetActive(true);
                wrongAnswersRoot.gameObject.SetActive(true);
                wrongAnswerText.SetActive(true);

                SpawnCorrect(1);
                SpawnWrong(4);
            }
            else // ALL
            {
                correctAnswersRoot.gameObject.SetActive(true);
                wrongAnswersRoot.gameObject.SetActive(false);
                wrongAnswerText.SetActive(false);

                SpawnCorrect(5);
            }
        }

        #endregion

        #region Spawning

        private void SpawnCorrect(int count)
        {
            for (int i = 0; i < count; i++)
                correctInputs.Add(SpawnInput(correctAnswersRoot));
        }

        private void SpawnWrong(int count)
        {
            for (int i = 0; i < count; i++)
                wrongInputs.Add(SpawnInput(wrongAnswersRoot));
        }

        private TMP_InputField SpawnInput(Transform parent)
        {
            GameObject go = Instantiate(singleLetterInputPrefab, parent);
            go.transform.localScale = Vector3.zero;
            go.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);

            TMP_InputField input = go.GetComponent<TMP_InputField>();

            input.interactable = true;
            input.readOnly = false;
            input.enabled = true;

            return input;
        }

        private void ClearAll()
        {
            ClearList(correctInputs);
            ClearList(wrongInputs);
        }

        private void ClearList(List<TMP_InputField> list)
        {
            foreach (var i in list)
                if (i != null) Destroy(i.gameObject);
            list.Clear();
        }

        #endregion

        #region Validation

        public bool Validate(out string error)
        {
            error = "";

            if (string.IsNullOrWhiteSpace(questionInput.text))
            {
                error = "Question cannot be empty";
                return false;
            }

            if (correctInputs.Exists(i => string.IsNullOrEmpty(i.text)))
            {
                error = "Fill all correct answer fields";
                return false;
            }

            if ((ArcheryAnswerType)answerTypeDropdown.value == ArcheryAnswerType.One &&
                wrongInputs.Exists(i => string.IsNullOrEmpty(i.text)))
            {
                error = "Fill all wrong answer fields";
                return false;
            }

            return true;
        }

        #endregion

        #region Data

        public ArcheryQuestionData GetData()
        {
            return new ArcheryQuestionData
            {
                question = questionInput.text.Trim(),
                answerType = (ArcheryAnswerType)answerTypeDropdown.value,
                correctAnswers = Collect(correctInputs),
                wrongAnswers = Collect(wrongInputs)
            };
        }

        private List<string> Collect(List<TMP_InputField> inputs)
        {
            List<string> list = new();
            foreach (var i in inputs)
                list.Add(i.text.ToUpperInvariant());
            return list;
        }

        #endregion
    }
}