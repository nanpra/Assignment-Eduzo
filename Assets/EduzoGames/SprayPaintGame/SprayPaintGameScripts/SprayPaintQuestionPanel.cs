using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum SprayPaintQuestionType
{
    Number,
    Alphabets
}

[Serializable]
public class SprayPaintQuestionData
{
    public string question;
    public SprayPaintQuestionType answerType;
    public string selectedAnswer;
}

namespace Eduzo.Games.SprayPaint.UI
{
    public class SprayPaintQuestionPanel : MonoBehaviour
    {
        [Header("Question")]
        public TMP_Dropdown questionTypeDropdown;
        public TMP_Dropdown answerDropdown;

        private void Awake()
        {
            SetupQuestionTypeDropdown();
            questionTypeDropdown.onValueChanged.AddListener(OnQuestionTypeChanged);
        }

        private void Start()
        {
            OnQuestionTypeChanged(questionTypeDropdown.value);
        }

        #region Dropdown Setup

        private void SetupQuestionTypeDropdown()
        {
            questionTypeDropdown.ClearOptions();
            questionTypeDropdown.AddOptions(new List<string>
            {
                "Number",
                "Alphabets"
            });
        }

        private void OnQuestionTypeChanged(int index)
        {
            SprayPaintQuestionType type = (SprayPaintQuestionType)index;
            PopulateAnswerDropdown(type);
        }

        private void PopulateAnswerDropdown(SprayPaintQuestionType type)
        {
            answerDropdown.ClearOptions();
            List<string> options = new();

            if (type == SprayPaintQuestionType.Number)
            {
                for (int i = 0; i <= 9; i++)
                    options.Add(i.ToString());
            }
            else // Alphabets
            {
                for (char c = 'A'; c <= 'Z'; c++)
                    options.Add(c.ToString());
            }

            answerDropdown.AddOptions(options);
            answerDropdown.value = 0;
            answerDropdown.RefreshShownValue();

            // Small pop animation for UX polish
            answerDropdown.transform.localScale = Vector3.zero;
            answerDropdown.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        }

        #endregion

        #region Validation

        public bool Validate(out string error)
        {
            error = "";

            if (string.IsNullOrWhiteSpace(questionTypeDropdown.itemText.text))
            {
                error = "Question cannot be empty";
                return false;
            }

            return true;
        }

        #endregion

        #region Data

        public SprayPaintQuestionData GetData()
        {
            return new SprayPaintQuestionData
            {
                answerType = (SprayPaintQuestionType)questionTypeDropdown.value,
                selectedAnswer = answerDropdown.options[answerDropdown.value].text
            };
        }

        #endregion
    }
}