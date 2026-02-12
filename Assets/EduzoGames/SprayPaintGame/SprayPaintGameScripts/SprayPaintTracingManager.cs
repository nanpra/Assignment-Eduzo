using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eduzo.Games.SprayPaint.Core
{
    public class SprayPaintTracingManager : MonoBehaviour
    {
        public static SprayPaintTracingManager Instance;

        [Header("Tracing")]
        public GameObject tracingPointPrefab;
        public RectTransform tracingRoot;

        private SprayPaintTracingSO currentTracing;
        private int currentStrokeIndex;
        private int currentPointIndex;
        private Coroutine demoRoutine;
        private bool isDemoRunning;
        private bool isLocked;

        private readonly List<SprayPaintTracingPoint> spawnedPoints = new();

        private void Awake()
        {
            Instance = this;
        }

        #region Public API

        public void StartTracing(SprayPaintTracingSO tracingSO)
        {
            Clear();

            if (tracingSO == null || tracingSO.strokes == null || tracingSO.strokes.Count == 0)
            {
                Debug.LogError("[SprayPaint] Invalid tracing SO");
                return;
            }

            currentTracing = tracingSO;
            currentStrokeIndex = 0;

            SpawnStroke();
        }

        #endregion

        #region Core Logic

        private void SpawnStroke()
        {
            Clear();

            if (currentStrokeIndex >= currentTracing.strokes.Count)
            {
                CompleteTracing();
                return;
            }

            StrokeData stroke = currentTracing.strokes[currentStrokeIndex];
            currentPointIndex = 0;

            foreach (Vector2 pointPos in stroke.points)
            {
                GameObject go = Instantiate(tracingPointPrefab, tracingRoot);

                RectTransform rt = go.GetComponent<RectTransform>();
                rt.anchoredPosition = pointPos;

                var point = go.GetComponent<SprayPaintTracingPoint>();
                point.pointIndex = spawnedPoints.Count;
                point.SetIdle(pointPos);

                spawnedPoints.Add(point);
            }

            StartDemoLoop(); // start visual guidance loop
        }

        private void StartDemoLoop()
        {
            StopDemoLoop();
            demoRoutine = StartCoroutine(DemoSequenceLoop());
        }

        private void StopDemoLoop()
        {
            if (demoRoutine != null)
            {
                StopCoroutine(demoRoutine);
                demoRoutine = null;
            }

            foreach (var p in spawnedPoints)
                p.StopHint(true);

            isDemoRunning = false;
        }

        private IEnumerator DemoSequenceLoop()
        {
            isDemoRunning = true;

            while (isDemoRunning)
            {
                for (int i = 0; i < spawnedPoints.Count; i++)
                {
                    // Stop if player started painting
                    if (!isDemoRunning)
                        yield break;

                    // Skip completed points (in case of resume)
                    if (spawnedPoints[i].isCompleted)
                        continue;

                    // Animate this point
                    spawnedPoints[i].SetGuide();

                    yield return new WaitForSeconds(0.7f);

                    spawnedPoints[i].StopHint(true);
                }

                // Small pause before looping again
                yield return new WaitForSeconds(0.3f);
            }
        }

        public void CheckCollision(RectTransform sprayTip)
        {
            //prevent multiple collisions in the same frame
            if (isLocked)
                return;

            if (spawnedPoints.Count == 0)
                return;

            // Wrong point → fail
            for (int i = 0; i < spawnedPoints.Count; i++)
            {
                if (i == currentPointIndex) continue;
                if (spawnedPoints[i].isCompleted) continue;

                if (IsOverlapping(sprayTip, spawnedPoints[i].RectTransform))
                {
                    StopDemoLoop();
                    FailTracing();
                    return;
                }
            }

            // Correct point
            SprayPaintTracingPoint expected = spawnedPoints[currentPointIndex];

            if (!expected.isCompleted &&
                IsOverlapping(sprayTip, expected.RectTransform))
            {
                expected.Complete();
                currentPointIndex++;

                if (currentPointIndex >= spawnedPoints.Count)
                {
                    currentStrokeIndex++;
                    SpawnStroke();
                }
            }
        }

        private bool IsOverlapping(RectTransform a, RectTransform b)
        {
            Rect rectA = GetWorldRect(a);
            Rect rectB = GetWorldRect(b);

            return rectA.Overlaps(rectB);
        }

        private Rect GetWorldRect(RectTransform rt)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            return new Rect(
                corners[0],
                corners[2] - corners[0]
            );
        }

        #endregion

        #region End States

        private void CompleteTracing()
        {
            Debug.Log("[SprayPaint] Tracing Complete");

            StopDemoLoop();

            // Ask UI to show filled letter, then continue
            SprayPaintSprayer.Instance.OnTracingSuccess();
            StartCoroutine(SprayPaintQuestionsLoader.Instance.ShowFilledLetterAndContinue());
        }

        private void FailTracing()
        {
            if (isLocked)
                return;

            isLocked = true;

            Debug.Log("[SprayPaint] Tracing Failed");

            StopDemoLoop();

            //tracing points fall animation
            AnimatePointsFall();

            // Tell spray can to react
            SprayPaintSprayer.Instance.OnTracingFailed();

            // Wait 2 seconds before continuing
            StartCoroutine(FailDelayRoutine());
        }

        private IEnumerator FailDelayRoutine()
        {
            yield return new WaitForSeconds(1f);
            SprayPaintQuestionsLoader.Instance.PlayWrong();
            isLocked = false;
        }

        private void AnimatePointsFall()
        {
            foreach (var point in spawnedPoints)
            {
                if (point == null) continue;

                RectTransform rt = point.RectTransform;

                // Kill guide animation
                point.StopHint(true);

                // Random slight rotation
                float randomRotate = Random.Range(-30f, 30f);

                Sequence fallSeq = DOTween.Sequence();

                fallSeq.Append(
                    rt.DOAnchorPosY(rt.anchoredPosition.y - 400f, 0.8f)
                      .SetEase(Ease.InBack)
                );

                fallSeq.Join(
                    rt.DORotate(new Vector3(0, 0, randomRotate), 0.8f)
                );

                if (!point.TryGetComponent<CanvasGroup>(out var cg))
                    cg = point.gameObject.AddComponent<CanvasGroup>();

                fallSeq.Join(cg.DOFade(0, 2f));
            }
        }

        #endregion

        #region Cleanup

        private void Clear()
        {
            foreach (Transform child in tracingRoot)
                Destroy(child.gameObject);

            spawnedPoints.Clear();
        }

        #endregion
    }
}