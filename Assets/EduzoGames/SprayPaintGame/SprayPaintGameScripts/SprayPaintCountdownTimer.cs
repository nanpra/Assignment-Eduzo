using Eduzo.Games.SprayPaint.UI;
using System.Collections;
using TMPro;
using UnityEngine;


namespace Eduzo.Games.SprayPaint.Core
{
    public class SprayPaintCountdownTimer : MonoBehaviour
    {
        public static SprayPaintCountdownTimer Instance;

        public GameObject timerPanel;
        public TextMeshProUGUI timerText;
        public int startTimeValue = 60; // Test Mode default time

        [HideInInspector] public int currentTime;

        private Coroutine runningRoutine;

        private void Awake()
        {
            Instance = this;
        }

        public void StartTimer()
        {
            if (SprayPaintUIManager.Instance.CurrentMode == SprayPaintGameMode.Practice) return;
            if (timerPanel != null) timerPanel.SetActive(true);

            currentTime = startTimeValue;
            runningRoutine = StartCoroutine(TimerRoutine());
        }

        public void StopTimer()
        {
            if (runningRoutine != null) StopCoroutine(runningRoutine);
            if (timerPanel != null) timerPanel.SetActive(false);

            runningRoutine = null;
        }

        public void ResetTimer()
        {
            StopTimer();
            currentTime = startTimeValue;
            UpdateTimerText();
        }

        private IEnumerator TimerRoutine()
        {
            while (currentTime > 0)
            {
                UpdateTimerText();
                yield return new WaitForSeconds(1f);
                currentTime--;
            }

            UpdateTimerText();

            // time expired -> notify game manager
            SprayPaintGameManager.Instance.OnTimerExpired();
        }

        //DISPLAY UPDATE
        private void UpdateTimerText()
        {
            if (timerText == null) return;

            int minutes = currentTime / 60;
            int seconds = currentTime % 60;
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        // PRACTICE MODE TIMER DISABLE
        public void DisableTimer()
        {
            StopTimer();
            if (timerPanel != null)
                timerPanel.SetActive(false);
        }
    }
}