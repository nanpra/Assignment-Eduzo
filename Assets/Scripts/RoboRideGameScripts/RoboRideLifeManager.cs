using DG.Tweening;
using Eduzo.Games.RoboRide.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Eduzo.Games.RoboRide.Core
{
    public class RoboRideLifeManager : MonoBehaviour
    {
        public static RoboRideLifeManager Instance;

        [Header("Life UI")]
        public Image[] lifeFilledImages;
        public Image[] lifeEmptyImages;

        public int totalLives = 3;
        private int lifeCurrentIndex = 0;
        private Vector3[] initialPositions;

        private void Awake()
        {
            Instance = this;

            initialPositions = new Vector3[lifeFilledImages.Length];
            for (int i = 0; i < lifeFilledImages.Length; i++)
                initialPositions[i] = lifeFilledImages[i].transform.localPosition;

            // hide empty heart alphas
            foreach (var e in lifeEmptyImages)
            {
                if (e != null)
                {
                    var c = e.color;
                    e.color = new Color(c.r, c.g, c.b, 0);
                }
            }
        }

        public void LoseLife()
        {
            if (RoboRideUIManager.Instance.CurrentMode == RoboRideGameMode.Practice)
                return;

            if (lifeCurrentIndex >= totalLives)
                return;

            int index = lifeCurrentIndex;
            lifeCurrentIndex++;

            Image filled = lifeFilledImages[index];
            Image empty = lifeEmptyImages[index];

            filled.raycastTarget = false;
            LoseLifeAnimation(filled, empty);
        }

        private void LoseLifeAnimation(Image filled, Image empty)
        {
            RectTransform rt = filled.rectTransform;
            Vector2 startPos = rt.anchoredPosition;
            Vector3 startScale = rt.localScale;

            DOTween.Kill(rt);

            // Disable layout while animating
            LayoutGroup layout = filled.GetComponentInParent<LayoutGroup>();
            if (layout != null) layout.enabled = false;

            Sequence seq = DOTween.Sequence();

            // Jump UP
            seq.Append(
                rt.DOAnchorPosY(startPos.y + 150f, 0.3f)
                  .SetEase(Ease.OutQuad)
            );

            // Fall DOWN
            seq.Append(
                rt.DOAnchorPosY(startPos.y, 0.2f)
                  .SetEase(Ease.InQuad)
            );

            // Impact hit
            seq.AppendCallback(() =>
            {
                rt.DOPunchScale(Vector3.one * 0.25f, 0.25f, 10);
                rt.DOPunchAnchorPos(Vector2.down * 20f, 0.25f, 10);
            });

            // Swap heart
            seq.AppendInterval(0.05f);
            seq.AppendCallback(() =>
            {
                filled.enabled = false;
                rt.localScale = startScale;
                rt.anchoredPosition = startPos;

                empty.DOFade(1f, 0.25f);
            });

            seq.OnComplete(() =>
            {
                // Re-enable layout
                if (layout != null) layout.enabled = true;

                //if (lifeCurrentIndex >= totalLives)
                    //RoboRideGameManager.Instance.OnLivesDepleted();
            });
        }

        public void ResetLives()
        {
            lifeCurrentIndex = 0;
            totalLives = lifeFilledImages.Length;

            for (int i = 0; i < totalLives; i++)
            {
                DOTween.Kill(lifeFilledImages[i].transform);
                DOTween.Kill(lifeEmptyImages[i].transform);

                if (initialPositions != null && i < initialPositions.Length)
                    lifeFilledImages[i].transform.localPosition = initialPositions[i];

                lifeFilledImages[i].gameObject.SetActive(true);
                lifeFilledImages[i].enabled = true;
                lifeFilledImages[i].raycastTarget = true;
                var f = lifeFilledImages[i].color;
                lifeFilledImages[i].color = new Color(f.r, f.g, f.b, 1);

                lifeEmptyImages[i].gameObject.SetActive(true);
                var e = lifeEmptyImages[i].color;
                lifeEmptyImages[i].color = new Color(e.r, e.g, e.b, 0);
            }
        }

        // hide life UI or disable interactions for Practice mode
        public void DisableLifes()
        {
            foreach (var f in lifeFilledImages)
                f.gameObject.SetActive(false);
            foreach (var e in lifeEmptyImages)
                e.gameObject.SetActive(false);
        }

        public int CurrentLives => totalLives - lifeCurrentIndex;
    }
}