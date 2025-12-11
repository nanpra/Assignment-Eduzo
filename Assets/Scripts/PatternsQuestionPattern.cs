using UnityEngine;

namespace Eduzo.Games.Patterns.Data
{
    [CreateAssetMenu(
        fileName = "Patterns Question Pattern",
        menuName = "Eduzo/Games/Patterns/Question Pattern"
    )]
    public class PatternsQuestionPattern : ScriptableObject
    {
        // FULL SPRITE PATTERN
        public Sprite[] patternIcons;

        // LETTER VERSION OF PATTERN (A, B, C, ...)
        public char[] letterKeys;

        // INDICES OF MISSING VALUES
        public int[] patternMissingIndices;

        // UNIVERSAL QUESTION MARK ICON
        public Sprite patternQuestionMarkIcon;


        // DISPLAY PATTERN (replacing missing indices with ? sprite)
        public Sprite[] GetDisplayPattern()
        {
            Sprite[] display = new Sprite[patternIcons.Length];
            patternIcons.CopyTo(display, 0);

            foreach (int idx in patternMissingIndices)
            {
                if (idx >= 0 && idx < display.Length)
                    display[idx] = patternQuestionMarkIcon;
            }

            return display;
        }

        // SPRITE ANSWERS
        public Sprite[] GetCorrectAnswers()
        {
            Sprite[] answers = new Sprite[patternMissingIndices.Length];

            for (int i = 0; i < patternMissingIndices.Length; i++)
                answers[i] = patternIcons[patternMissingIndices[i]];

            return answers;
        }

        // CORRECT LETTER ANSWERS
        public char[] GetCorrectAnswerLetters()
        {
            char[] outArr = new char[patternMissingIndices.Length];

            for (int i = 0; i < patternMissingIndices.Length; i++)
                outArr[i] = letterKeys[patternMissingIndices[i]];

            return outArr;
        }
    }
}