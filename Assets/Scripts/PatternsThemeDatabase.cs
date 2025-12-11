using UnityEngine;

namespace Eduzo.Games.Patterns.Data
{
    [CreateAssetMenu(
        fileName = "Patterns Theme Database",
        menuName = "Eduzo/Games/Patterns/Theme Database"
    )]
    public class PatternsThemeDatabase : ScriptableObject
    {
        public PatternsThemeSO shapesTheme;
        public PatternsThemeSO alphabetsTheme;
        public PatternsThemeSO animalsTheme;

        public PatternsThemeSO GetThemeByName(string name)
        {
            switch (name)
            {
                case "ShapesTheme": return shapesTheme;
                case "AlphabetsTheme": return alphabetsTheme;
                case "AnimalsTheme": return animalsTheme;
                default:
                    Debug.LogError("No theme found: " + name);
                    return null;
            }
        }
    }
}