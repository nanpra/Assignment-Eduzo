using Eduzo.Games.RoboRide.UI;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Eduzo.Games.RoboRide.Core
{
    public class RoboRideCountdownTimer : MonoBehaviour
    {
        public static RoboRideCountdownTimer Instance;

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
            if (RoboRideUIManager.Instance.CurrentMode == RoboRideGameMode.Practice) return;
            if (timerPanel != null) timerPanel.SetActive(true);

            currentTime = startTimeValue;
            runningRoutine = StartCoroutine(TimerRoutine());
            RoboRideGameManager.Instance.robot.StartThinkingVFX();
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
            RoboRideGameManager.Instance.OnTimerExpired();
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