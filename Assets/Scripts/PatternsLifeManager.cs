using DG.Tweening;
using Eduzo.Games.Patterns.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Eduzo.Games.Patterns.Core
{
    public class PatternsLifeManager : MonoBehaviour
    {
        public static PatternsLifeManager Instance;

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
            if (PatternsUIManager.Instance.CurrentMode == PatternsGameMode.Practice)
                return; // no life lost in practice

            int index = lifeCurrentIndex;
            if (index >= totalLives) return;

            lifeCurrentIndex++;

            Image filled = lifeFilledImages[index];
            Image empty = lifeEmptyImages[index];

            DOTween.Kill(filled.transform);
            filled.raycastTarget = false;

            Vector3 originalPos = filled.transform.localPosition;

            Sequence seq = DOTween.Sequence();
            seq.Append(filled.transform.DOLocalMoveY(originalPos.y + 80f, 0.25f).SetEase(Ease.OutQuad));
            seq.Append(filled.transform.DOLocalMoveY(originalPos.y - 30f, 0.30f).SetEase(Ease.InQuad));
            seq.OnComplete(() =>
            {
                filled.enabled = false;
                filled.transform.localPosition = originalPos;
                empty.DOFade(1, 0.35f);
                if (lifeCurrentIndex >= totalLives)
                {
                    // notify game manager
                    PatternsGameManager.Instance.OnLivesDepleted();
                }
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
    }
}