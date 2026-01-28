using DG.Tweening;
using UnityEngine;
using System;

namespace Eduzo.Games.Archery.Core
{
    public class ArcheryArrowController : MonoBehaviour
    {
        private RectTransform bowOrigin;
        public float shootDuration = 0.35f;

        public void Init(RectTransform origin)
        {
            bowOrigin = origin;
            ResetArrow();
        }

        public void ShootTo(RectTransform target, int targetIndex, Action onImpact)
        {
            transform.SetParent(transform.root);
            transform.SetPositionAndRotation(bowOrigin.position, Quaternion.identity);
            transform.localScale = Vector3.one;

            Vector3 finalRotation = GetRotationForIndex(targetIndex);
            Vector3 finalScale = Vector3.one * 0.5f;

            Sequence seq = DOTween.Sequence();

            // Move to target
            seq.Append(
                transform.DOMove(target.position, shootDuration)
                    .SetEase(Ease.OutQuad)
            );

            // ROTATE + SCALE DURING FLIGHT
            seq.Join(
                transform.DORotate(finalRotation, shootDuration)
                    .SetEase(Ease.OutQuad)
            );

            seq.Join(
                transform.DOScale(finalScale, shootDuration)
                    .SetEase(Ease.OutQuad).OnComplete(() =>
                    {
                        transform.DOLocalRotate(finalRotation + new Vector3(2f, 0, 0), 0.05f).
                            SetLoops(2, LoopType.Yoyo);
                    })
            );

            // Stick at the end
            seq.AppendCallback(() =>
            {
                transform.SetParent(target);
                transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(finalRotation));
                transform.localScale = finalScale;

                onImpact?.Invoke();
            });
        }

        private Vector3 GetRotationForIndex(int index)
        {
            return index switch
            {
                0 => new Vector3(-30f, -30f, 0f),
                1 => new Vector3(-30f, -30f, 0f),
                2 => new Vector3(-30f, 0f, 0f),
                3 => new Vector3(-30f, 30f, 0f),
                4 => new Vector3(-30f, 30f, 0f),
                _ => new Vector3(-30f, 0f, 0f)
            };
        }

        public void ResetArrow()
        {
            transform.DOKill();
            transform.SetParent(bowOrigin);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            transform.localScale = Vector3.one;
            gameObject.SetActive(false);
        }
    }
}