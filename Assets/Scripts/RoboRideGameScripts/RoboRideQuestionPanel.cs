using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Eduzo.Games.RoboRide.UI
{
    [System.Serializable]
    public class RoboRideQuestionData
    {
        public string question;
        public string sentence;
        public string correctWord;
    }

    public class RoboRideQuestionPanel : MonoBehaviour
    {
        [Header("Question Inputs")]
        public TMP_InputField questionTextInput;
        public TMP_InputField sentenceInput;
        public TMP_InputField correctAnswerInput;

        // Only A–Z,a-z and SPACE 
        private static readonly Regex NonAlphabetAndSpaceRegex =new("[^a-zA-Z ]", RegexOptions.Compiled);


        private void Awake()
        {
            questionTextInput.onValueChanged.AddListener(OnGeneralTextChanged);
            sentenceInput.onValueChanged.AddListener(OnSentenceChanged);
            correctAnswerInput.onValueChanged.AddListener(OnCorrectAnswerChanged);
        }

        #region Live Input Filtering

        private void OnGeneralTextChanged(string value)
        {
            if (!TryGetActiveField(out TMP_InputField field)) return;

            string filtered = NonAlphabetAndSpaceRegex.Replace(value, "").ToUpperInvariant();

            if (value != filtered)
                field.SetTextWithoutNotify(filtered);
        }

        private void OnSentenceChanged(string value)
        {
            if (!TryGetActiveField(out TMP_InputField field)) return;

            string filtered = NonAlphabetAndSpaceRegex.Replace(value, "").ToUpperInvariant();

            if (value != filtered)
                field.SetTextWithoutNotify(filtered);

            // Sentence changed → re-validate correct answer
            ValidateCorrectAnswerAgainstSentence();
        }

        private void OnCorrectAnswerChanged(string value)
        {
            if (!TryGetActiveField(out TMP_InputField field)) return;

            string filtered = NonAlphabetAndSpaceRegex.Replace(value, "").ToUpperInvariant();

            if (value != filtered)
                field.SetTextWithoutNotify(filtered);

            ValidateCorrectAnswerAgainstSentence();
        }

        private void ValidateCorrectAnswerAgainstSentence()
        {
            if (string.IsNullOrWhiteSpace(sentenceInput.text))
            {
                correctAnswerInput.SetTextWithoutNotify("");
                return;
            }

            string[] sentenceWords = sentenceInput.text.Split(' ');
            string current = correctAnswerInput.text;    // Allow only prefixes of valid words
            bool validPrefix = false;

            foreach (string word in sentenceWords)
            {
                if (word.StartsWith(current))
                {
                    validPrefix = true;
                    break;
                }
            }

            if (!validPrefix)
                correctAnswerInput.SetTextWithoutNotify("");
        }

        private bool TryGetActiveField(out TMP_InputField field)
        {
            field = null;

            if (EventSystem.current == null ||
                EventSystem.current.currentSelectedGameObject == null)
                return false;

            return EventSystem.current.currentSelectedGameObject
                .TryGetComponent(out field);
        }

        #endregion

        #region Validation (Confirm Button)

        private enum ValidationResult
        {
            None,
            QuestionEmpty,
            SentenceEmpty,
            AnswerEmpty,
            AnswerNotInSentence
        }

        public bool Validate(out string error)
        {
            error = "";

            ValidationResult result = ValidateInternal();

            switch (result)
            {
                case ValidationResult.QuestionEmpty:
                    error = "Question text cannot be empty.";
                    return false;

                case ValidationResult.SentenceEmpty:
                    error = "Sentence cannot be empty.";
                    return false;

                case ValidationResult.AnswerEmpty:
                    error = "Correct answer cannot be empty.";
                    return false;

                case ValidationResult.AnswerNotInSentence:
                    error = "Correct answer must be a word from the sentence.";
                    return false;

                case ValidationResult.None:
                default:
                    return true;
            }
        }

        private ValidationResult ValidateInternal()
        {
            if (string.IsNullOrWhiteSpace(questionTextInput.text))
                return ValidationResult.QuestionEmpty;

            if (string.IsNullOrWhiteSpace(sentenceInput.text))
                return ValidationResult.SentenceEmpty;

            if (string.IsNullOrWhiteSpace(correctAnswerInput.text))
                return ValidationResult.AnswerEmpty;

            string[] sentenceWords = sentenceInput.text.Split(' ');

            foreach (string word in sentenceWords)
            {
                if (word == correctAnswerInput.text)
                    return ValidationResult.None;
            }

            return ValidationResult.AnswerNotInSentence;
        }

        #endregion

        public RoboRideQuestionData GetData()
        {
            return new RoboRideQuestionData
            {
                question = questionTextInput.text.Trim(),
                sentence = sentenceInput.text.Trim(),
                correctWord = correctAnswerInput.text.Trim()
            };
        }
    }
}