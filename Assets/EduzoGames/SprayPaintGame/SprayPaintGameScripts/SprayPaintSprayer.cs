using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using static Eduzo.Games.SprayPaint.Core.SprayPaintColorManager;


namespace Eduzo.Games.SprayPaint.Core
{
    public class SprayPaintSprayer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public static SprayPaintSprayer Instance;
        public RectTransform canvasRect;
        public Camera uiCamera;

        [Header("Spray Tip")]
        public RectTransform sprayTip;

        [Header("Spray VFX")]
        public ParticleSystem redSprayVFX;
        public ParticleSystem blueSprayVFX;
        public ParticleSystem greenSprayVFX;

        private RectTransform rectTransform;
        private bool isLocked;
        private Vector2 originalPosition;


        private void Awake()
        {
            Instance = this;
            rectTransform = GetComponent<RectTransform>();
            originalPosition = rectTransform.anchoredPosition;
            StopAllVFX();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isLocked)
                return;

            PlayCurrentColorVFX();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isLocked)
                return;

            MoveToPointer(eventData);
            SprayPaintTracingManager.Instance.CheckCollision(sprayTip);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            StopAllVFX();
        }

        private void MoveToPointer(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                eventData.position,
                uiCamera,
                out Vector2 localPos
            );

            rectTransform.anchoredPosition = localPos;
        }

        public void OnTracingFailed()
        {
            isLocked = true;

            StopAllVFX();

            Sequence failSequence = DOTween.Sequence();

            // Shake first
            failSequence.Append(
                rectTransform.DOShakeAnchorPos(
                    0.8f,
                    new Vector2(30f, 30f),
                    25,
                    90,
                    false,
                    true
                )
            );

            // Then move back smoothly
            failSequence.Append(
                rectTransform
                    .DOAnchorPos(originalPosition, 0.6f)
                    .SetEase(Ease.OutBack)
            );

            failSequence.OnComplete(() =>
            {
                StartCoroutine(UnlockAfterDelay());
            });
        }

        public void ResetPosition()
        {
            rectTransform.anchoredPosition = originalPosition;
        }

        private IEnumerator UnlockAfterDelay()
        {
            yield return new WaitForSeconds(1f);
            isLocked = false;
        }

        public void ForceUnlock()
        {
            isLocked = false;
        }

        public void OnTracingSuccess()
        {
            isLocked = true;

            StopAllVFX();

            Sequence successSequence = DOTween.Sequence();

            // Small bounce scale
            successSequence.Append(
                rectTransform
                    .DOScale(1.15f, 0.2f)
                    .SetEase(Ease.OutBack)
            );

            successSequence.Append(
                rectTransform
                    .DOScale(1f, 0.2f)
                    .SetEase(Ease.InOutSine)
            );

            // Smooth return to original position
            successSequence.Append(
                rectTransform
                    .DOAnchorPos(originalPosition, 0.6f)
                    .SetEase(Ease.OutBack)
            );

            successSequence.OnComplete(() =>
            {
                StartCoroutine(UnlockAfterDelay());
            });
        }

        private void PlayCurrentColorVFX()
        {
            StopAllVFX();

            switch (SprayPaintColorManager.Instance.CurrentColor)
            {
                case SprayPaintColor.Red:
                    redSprayVFX.Play();
                    break;

                case SprayPaintColor.Blue:
                    blueSprayVFX.Play();
                    break;

                case SprayPaintColor.Green:
                    greenSprayVFX.Play();
                    break;
            }
        }

        private void StopAllVFX()
        {
            redSprayVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            blueSprayVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            greenSprayVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}