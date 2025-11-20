using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject winVFX;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private int correctAnswers = 0;

    // Reset tracked score for a fresh play
    public void ResetGame() => correctAnswers = 0;
    public void OnCorrectAnswer() => correctAnswers++; // track score

    private void Start() => AudioManager.Instance.PlayBgMusic("BgMusic");

    public void EndGameWin()
    {
        UIManager.Instance.ShowGameOverPanel();
        AudioManager.Instance.PlaySFX("GameWin");
        winVFX.SetActive(true);
        GameOverScreen.Instance.ShowWin(
            score: 100  // full correct
        );
        ResetParents();
    }

    public void EndGameLose()
    {
        int total = UIPatternLoader.currentQuestion.missingIndices.Length;
        int score = Mathf.RoundToInt((float)correctAnswers / total * 100);

        UIManager.Instance.ShowGameOverPanel();
        AudioManager.Instance.PlaySFX("GameLose");
        GameOverScreen.Instance.ShowLose(score);
        ResetParents();
    }

    private void ResetParents()
    {
        UIPatternLoader.Instance.questionParent.gameObject.SetActive(false);
        UIPatternLoader.Instance.optionsParent.gameObject.SetActive(false);
    }
}