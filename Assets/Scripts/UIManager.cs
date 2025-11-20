using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Refrences")]
    public Button startBtn;
    public Button homeBtn;
    public GameObject mainMenuPanel;
    public GameObject gameplayPanel;
    public GameObject gameOverPanel;
    public float fadeDuration = 1f;

    private CanvasGroup mainMenuCanvasGroup;
    [HideInInspector] public CanvasGroup gameplayCanvasGroup;
    [HideInInspector] public CanvasGroup gameOverCanvasGroup;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        mainMenuCanvasGroup = mainMenuPanel.GetComponent<CanvasGroup>();
        gameplayCanvasGroup = gameplayPanel.GetComponent<CanvasGroup>();
        gameOverCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();

        // Initialize panels state
        gameplayPanel.SetActive(false);
        gameplayCanvasGroup.alpha = 0;
        gameOverCanvasGroup.alpha = 0;

        // Assign the listener to the buttons
        startBtn.onClick.AddListener(OnStartButtonClicked);
        homeBtn.onClick.AddListener(OnHomeButtonClicked);
    }

    private void OnStartButtonClicked()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        
        // Reset gameplay state
        GameManager.Instance.ResetGame();
        UIPatternLoader.Instance.filledCount = 0;
        if (LifeManager.Instance != null)
            LifeManager.Instance.ResetLives();

        // Fade out the main menu panel
        mainMenuCanvasGroup.DOFade(0, fadeDuration).OnComplete(() =>
        {
            mainMenuPanel.SetActive(false);
            gameplayPanel.SetActive(true);
            gameplayCanvasGroup.DOFade(1, fadeDuration);
            
            UIPatternLoader.Instance.LoadQuestion(UIPatternLoader.Instance.levelIndex);
            CountdownTimer.Instance.StartTimer();
        });
    }

    public void OnHomeButtonClicked()
    {
        GameManager.Instance.winVFX.SetActive(false);
        AudioManager.Instance.PlaySFX("ButtonClick");
        // Reset gameplay state
        UIPatternLoader.Instance.filledCount = 0;
        GameManager.Instance.ResetGame();
        if (LifeManager.Instance != null)
            LifeManager.Instance.ResetLives();
        CountdownTimer.Instance.StopTimer();

        // Fade out the gameplay panel
        gameOverCanvasGroup.DOFade(0, fadeDuration).OnComplete(() => gameOverPanel.SetActive(false));
        gameplayCanvasGroup.DOFade(0, fadeDuration).OnComplete(() =>
        {
            gameplayPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            mainMenuCanvasGroup.DOFade(1, fadeDuration);
        });
    }

    public void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
        gameOverCanvasGroup.DOFade(1, fadeDuration);
    }

    public void OnRetryButtonClicked()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        ResetData();
    }

    public void OnNextLevelButtonClicked()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");     //using for very few buttons so directly calling button click sound
        UIPatternLoader.Instance.levelIndex++;
        if(UIPatternLoader.Instance.levelIndex == 4)
            UIPatternLoader.Instance.levelIndex = 0;

        ResetData();
    }

    private void ResetData()
    {
        GameManager.Instance.winVFX.SetActive(false);
        UIPatternLoader.Instance.filledCount = 0;
        UIPatternLoader.Instance.LoadQuestion(UIPatternLoader.Instance.levelIndex);
        GameManager.Instance.ResetGame();
        CountdownTimer.Instance.currentTime = CountdownTimer.Instance.startTime;
        CountdownTimer.Instance.StopTimer();
        gameOverCanvasGroup.DOFade(0, fadeDuration).OnComplete(() =>
        {
            gameOverPanel.SetActive(false);
            gameplayCanvasGroup.DOFade(1, fadeDuration);
            CountdownTimer.Instance.StartTimer();
            if (LifeManager.Instance != null)
                LifeManager.Instance.ResetLives();
        });
    }
}