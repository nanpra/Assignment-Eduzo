using DG.Tweening;
using UnityEngine;

namespace Eduzo.Games.RoboRide.Core
{
    public class RoboRidePlatformController : MonoBehaviour
    {
        public static RoboRidePlatformController Instance;

        public RectTransform[] platforms;

        [Header("Idle Motion")]
        public float baseFloatAmplitude = 25f;
        public float floatAmplitudeVariance = 10f;

        public float baseFloatDuration = 1.2f;
        public float floatDurationVariance = 0.4f;

        private Tween[] idleTweens;
        private Vector2[] basePositions;

        private void Awake()
        {
            if(Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

                idleTweens = new Tween[platforms.Length];
            basePositions = new Vector2[platforms.Length];

            for (int i = 0; i < platforms.Length; i++)
                basePositions[i] = platforms[i].anchoredPosition;
        }

        public void StartIdleMotion()
        {
            StopIdleMotion();

            for (int i = 0; i < platforms.Length; i++)
            {
                RectTransform rt = platforms[i];

                // Alternate direction (+ / -)
                float direction = (i % 2 == 0) ? 1f : -1f;

                // Slight variance per platform
                float amplitude = baseFloatAmplitude +
                                  Random.Range(-floatAmplitudeVariance, floatAmplitudeVariance);

                float duration = baseFloatDuration +
                                 Random.Range(-floatDurationVariance, floatDurationVariance);

                idleTweens[i] = rt
                    .DOAnchorPosY(
                        basePositions[i].y + (amplitude * direction),
                        duration
                    )
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        public void StopIdleMotion()
        {
            for (int i = 0; i < idleTweens.Length; i++)
            {
                if (idleTweens[i] != null)
                {
                    idleTweens[i].Kill();
                    idleTweens[i] = null;
                }
            }
        }

        public void MovePlatformsDownForRobotCross(System.Action onComplete)
        {
            StopIdleMotion();
            int completed = 0;

            for (int i = 0; i < platforms.Length; i++)
            {
                RectTransform rt = platforms[i];

                // Slight duration variance for settle motion
                float duration = 0.5f + (i * 0.1f);

                rt.DOAnchorPosY(-1150f, duration).SetEase(Ease.OutCubic).SetDelay(0.2f).OnComplete(() =>
                  {
                      completed++;
                      if (completed == platforms.Length)
                          onComplete?.Invoke();
                  });
            }
        }

        public void FallPlatforms()
        {
            StopIdleMotion();

            for (int i = 0; i < platforms.Length; i++)
            {
                RectTransform rt = platforms[i];
                float duration = 0.5f + i * 0.08f;
                rt.DOAnchorPosY(-2500f, duration).SetEase(Ease.InQuad);
            }
        }

        public void ResetPlatforms()
        {
            StopIdleMotion();
            int completed = 0;

            for (int i = 0; i < platforms.Length; i++)
            {
                RectTransform rt = platforms[i];

                // Slight stagger for natural feel
                float duration = 0.3f + (i * 0.1f);

                rt.DOAnchorPosY(basePositions[i].y, duration).SetEase(Ease.OutCubic).OnComplete(() =>
                  {
                      completed++;
                      if (completed == platforms.Length)
                          StartIdleMotion();
                  });
            }
        }
    }
}