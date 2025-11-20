using UnityEngine;

[CreateAssetMenu(fileName = "QuestionPattern", menuName = "ScriptableObjects/Question Pattern")]
public class QuestionPattern : ScriptableObject
{
    [Header("Pattern Setup")]
    [Tooltip("Assign the correct orderwise pattern icons")]
    public Sprite[] patternIcons;                // Full correct pattern

    [Tooltip("Assign the index for which the icon should be missing")]
    public int[] missingIndices;

    public Sprite questionMarkIcon;

    // Returns the pattern but replaces missing indices with question mark icons
    public Sprite[] GetDisplayPattern()
    {
        Sprite[] display = new Sprite[patternIcons.Length];
        patternIcons.CopyTo(display, 0);

        foreach (int index in missingIndices)
        {
            if (index >= 0 && index < display.Length)
                display[index] = questionMarkIcon;
        }

        return display;
    }

    // Returns correct answers for all missing slots.
    public Sprite[] GetCorrectAnswers()
    {
        Sprite[] answers = new Sprite[missingIndices.Length];

        for (int i = 0; i < missingIndices.Length; i++)
            answers[i] = patternIcons[missingIndices[i]];

        return answers;
    }
}