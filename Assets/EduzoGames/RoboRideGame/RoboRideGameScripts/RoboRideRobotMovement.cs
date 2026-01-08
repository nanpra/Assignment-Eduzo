using DG.Tweening;
using UnityEngine;

namespace Eduzo.Games.RoboRide.Core
{
    public class RoboRideRobotMovement : MonoBehaviour
    {
        public RectTransform robotRT;
        public GameObject thinkingVFX;
        public GameObject bigStarVFX;

        public Tween CrossPlatforms(float targetX, float duration)
        {
            robotRT.DOKill();

            return robotRT
                .DOAnchorPosX(targetX, duration)
                .SetEase(Ease.OutQuart);
        }

        public void CollectStar(RectTransform star)
        {
            float xValue = star.GetComponent<RectTransform>().anchoredPosition.x;
            star.DOJumpAnchorPos(new Vector2(xValue, 0), 200, 1, 0.4f);
            star.DOScale(0f, 0.4f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    bigStarVFX.SetActive(true);
                });
        }

        public Tween FallDown(float fallY, float duration)
        {
            robotRT.DOKill();

            return robotRT
                .DOAnchorPosY(fallY, duration)
                .SetEase(Ease.InQuad);
        }

        public void StartThinkingVFX() => thinkingVFX.SetActive(true);
        public void StopThinkingVFX() => thinkingVFX.SetActive(false);
            
    }
}