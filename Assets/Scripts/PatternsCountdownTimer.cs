using System.Collections;
using TMPro;
using UnityEngine;

namespace Eduzo.Games.Patterns.Core
{
    public class PatternsCountdownTimer : MonoBehaviour
    {
        public static PatternsCountdownTimer Instance;

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
            StopTimer();
            currentTime = startTimeValue;
            runningRoutine = StartCoroutine(TimerRoutine());
        }

        public void StopTimer()
        {
            if (runningRoutine != null)
                StopCoroutine(runningRoutine);

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
            PatternsGameManager.Instance.OnTimerExpired();
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
            if (timerText != null)
                timerText.text = "∞∞ / ∞∞";  // infinity time
        }
    }
}