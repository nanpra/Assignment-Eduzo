using DG.Tweening;
using UnityEngine;
using System;


namespace Eduzo.Games.Archery.Core
{
    public class ArcheryArrowController : MonoBehaviour
    {
        public RectTransform arrow;
        public RectTransform bowOrigin;
        public float shootDuration = 0.35f;

        public void ShootTo(RectTransform target, Action onHit)
        {
            arrow.DOMove(target.position, shootDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    arrow.gameObject.SetActive(false);
                    target.GetComponent<ArcheryTargetUI>().PlayHitVFX();
                    onHit?.Invoke();
                    ResetArrow();
                });
        }

        private void ResetArrow()
        {
            arrow.gameObject.SetActive(true);
            arrow.position = bowOrigin.position;
        }
    }
}