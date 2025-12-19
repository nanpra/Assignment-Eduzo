using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Patterns/Flexible Theme")]
public class PatternsThemeSO : ScriptableObject
{
    [Header("Icons in order (A, B, C, D, E, F...)")]
    public List<Sprite> icons = new();


    public Sprite GetSpriteForLetter(char letter)
    {
        letter = char.ToUpper(letter);

        int index = letter - 'A';  // A→0, B→1, C→2 ...

        if (index < 0 || index >= icons.Count)
            return null; // out of range — letter not defined in theme

        return icons[index];
    }
}