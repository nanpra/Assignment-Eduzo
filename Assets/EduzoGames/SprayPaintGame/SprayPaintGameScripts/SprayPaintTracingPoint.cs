using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Eduzo.Games.SprayPaint.Core
{
    public class SprayPaintTracingPoint : MonoBehaviour
    {
        public int pointIndex;
        public bool isCompleted;

        public RectTransform RectTransform { get; private set; }

        private Image img;
        private Vector2 baseAnchoredPos;
        private Tween hintTween;

        [Header("Colors")]
        public Color idleColor = Color.white;
        public Color guideColor = new(1f, 0.5f, 0f); // orange
        public Color completedColor = Color.green;

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            img = GetComponent<Image>();
        }

        public void SetIdle(Vector2 anchoredPos)
        {
            baseAnchoredPos = anchoredPos;
            isCompleted = false;

            img.color = idleColor;
            RectTransform.anchoredPosition = baseAnchoredPos;

            StopHint(false);
        }

        public void SetGuide()
        {
            if (isCompleted) return;

            StopHint(false);
            img.color = guideColor;

            hintTween = RectTransform
                .DOScale(1.3f, 0.4f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }

        public void StopHint(bool resetColor)
        {
            hintTween?.Kill();
            RectTransform.localScale = Vector3.one;

            if (resetColor && !isCompleted)
                img.color = idleColor;
        }

        public void Complete()
        {
            if (isCompleted) return;

            isCompleted = true;
            hintTween?.Kill();
            RectTransform.localScale = Vector3.one;
            img.color = completedColor;
        }
    }
}