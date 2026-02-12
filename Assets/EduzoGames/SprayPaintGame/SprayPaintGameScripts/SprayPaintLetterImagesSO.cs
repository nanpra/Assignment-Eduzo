using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(
    fileName = "LetterImages",
    menuName = "Eduzo/Games/Spray Paint/Letter Images"
)]
public class SprayPaintLetterImagesSO : ScriptableObject
{
    [Header("Number Sprites (0–9)")]
    public List<Sprite> numberSprites; // index = number

    [Header("Number Sprites (0–9)")]
    public List<Sprite> filledNumberSprites; // index = number

    [Header("Alphabet Sprites (A–Z)")]
    public List<Sprite> alphabetSprites; // index = A=0, B=1 ...

    [Header("Alphabet Sprites (A–Z)")]
    public List<Sprite> filledAlphabetSprites; // index = A=0, B=1 ...

    public Sprite GetSprite(SprayPaintQuestionType type, string value)
    {
        if (type == SprayPaintQuestionType.Number)
        {
            if (int.TryParse(value, out int number) &&
                number >= 0 && number < numberSprites.Count)
            {
                return numberSprites[number];
            }
        }
        else
        {
            char c = value.ToUpper()[0];
            int index = c - 'A';

            if (index >= 0 && index < alphabetSprites.Count)
            {
                return alphabetSprites[index];
            }
        }

        Debug.LogError($"[SprayPaint] Sprite not found for {type} : {value}");
        return null;
    }

    public Sprite GetFilledSprite(SprayPaintQuestionType type, string value)
    {
        if (type == SprayPaintQuestionType.Number)
        {
            if (int.TryParse(value, out int number) &&
                number >= 0 && number < filledNumberSprites.Count)
            {
                return filledNumberSprites[number];
            }
        }
        else
        {
            char c = value.ToUpper()[0];
            int index = c - 'A';

            if (index >= 0 && index < filledAlphabetSprites.Count)
            {
                return filledAlphabetSprites[index];
            }
        }

        Debug.LogError($"[SprayPaint] Filled sprite not found for {type} : {value}");
        return null;
    }
}