using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    public Dropdown levelDropdown;

    private void Start()
    {
        // Listen for option change
        levelDropdown.onValueChanged.AddListener(OnLevelSelected);
    }

    private void OnLevelSelected(int index)
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        UIPatternLoader.Instance.levelIndex = levelDropdown.value;
    }
}