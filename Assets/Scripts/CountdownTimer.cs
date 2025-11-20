using UnityEngine;
using System.Collections;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public static CountdownTimer Instance;

    public TextMeshProUGUI timerText;
    public int startTime = 120;

    [HideInInspector] public int currentTime;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void StartTimer()
    {
        currentTime = startTime;
        StopAllCoroutines();
        StartCoroutine(TimerRoutine());
    }

    public void StopTimer()
    {
        StopAllCoroutines();
    }

    private IEnumerator TimerRoutine()
    {
        while (currentTime > 0)
        {
            UpdateTimerText();
            yield return new WaitForSeconds(1f);
            currentTime--;
        }

        // Final update when reaching 0
        UpdateTimerText();
        GameManager.Instance.EndGameLose();
    }

    private void UpdateTimerText()
    {
        int minutes = currentTime / 60;
        int seconds = currentTime % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}