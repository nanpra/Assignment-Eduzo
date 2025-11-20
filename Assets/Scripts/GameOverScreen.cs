using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    public static GameOverScreen Instance;

    [Header("Game Over UI")]
    public Image titleIcon;
    public Sprite winIcon;
    public Sprite loseIcon;
    public Image[] stars;

    [Header("Score")]
    public TextMeshProUGUI scoreText;

    [Header("Buttons")]
    public Button retryButton;
    public Button homeButton;
    public Button nextLevelButton;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        retryButton.onClick.AddListener(() => UIManager.Instance.OnRetryButtonClicked());
        homeButton.onClick.AddListener(() => UIManager.Instance.OnHomeButtonClicked());
        nextLevelButton.onClick.AddListener(() => UIManager.Instance.OnNextLevelButtonClicked());
    }

    public void ShowWin(int score)
    {
        scoreText.text = "Score:\n" + score;
        scoreText.transform.localScale = Vector3.zero;
        scoreText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

        // Adjust buttons
        nextLevelButton.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(false);

        // WIN icon pop
        titleIcon.sprite = winIcon;
        titleIcon.transform.localScale = Vector3.zero;
        titleIcon.transform
            .DOScale(1, 0.5f)
            .SetEase(Ease.OutBack);

        // Stars appear one by one
        float delay = 0.3f;
        foreach (var star in stars)
        {
            star.transform.localScale = Vector3.zero;
            star.DOFade(0, 0);

            star.transform.DOScale(1, 0.4f)
                .SetDelay(delay)
                .SetEase(Ease.OutBack);

            star.DOFade(1, 0.4f).SetDelay(delay);
            delay += 0.2f;
        }
    }

    public void ShowLose(int score)
    {
        scoreText.text = "Score: " + score;

        // Adjust buttons
        nextLevelButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(true);

        // Disable stars
        foreach (var star in stars)
            star.enabled = false;

        titleIcon.sprite = loseIcon;
        titleIcon.transform.localScale = Vector3.zero;
        titleIcon.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);
    }
}