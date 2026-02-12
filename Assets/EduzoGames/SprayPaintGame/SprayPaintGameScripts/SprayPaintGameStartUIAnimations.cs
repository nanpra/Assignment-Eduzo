using UnityEngine;
using DG.Tweening;
using System.Collections;
using Eduzo.Games.SprayPaint.Audio;


namespace Eduzo.Games.SprayPaint.UI
{
    public class SprayPaintGameStartUIAnimations : MonoBehaviour
    {
        [Header("Patterns UI References")]
        public RectTransform practiceModeButton;
        public RectTransform testModeButton;

        public float patternsAnimDuration = 0.6f;
        public float patternsOffset = 300f;

        private Vector2 practiceModeButtonOriginalPos;
        private Vector2 testModeButtonOriginalPos;

        private void Start()
        {
            practiceModeButtonOriginalPos = practiceModeButton.anchoredPosition;
            testModeButtonOriginalPos = testModeButton.anchoredPosition;

            AnimatePatternsMainMenu();
        }

        public void AnimatePatternsMainMenu()
        {
            practiceModeButton.anchoredPosition = new Vector2(
                practiceModeButtonOriginalPos.x,
                practiceModeButtonOriginalPos.y + patternsOffset
            );

            testModeButton.anchoredPosition = new Vector2(
                testModeButtonOriginalPos.x,
                testModeButtonOriginalPos.y - patternsOffset
            );

            practiceModeButton.gameObject.SetActive(true);
            testModeButton.gameObject.SetActive(true);

            StartCoroutine(PatternsDelayAnimation());
        }

        private IEnumerator PatternsDelayAnimation()
        {
            yield return null;
            yield return null;

            Sequence seq = DOTween.Sequence();

            seq.Append(
                practiceModeButton.DOAnchorPos(practiceModeButtonOriginalPos, patternsAnimDuration)
                    .SetEase(Ease.OutBack)
                    .OnStart(() => SprayPaintAudioManager.Instance.PlaySFX("Slide"))
            );

            seq.Append(
                testModeButton.DOAnchorPos(testModeButtonOriginalPos, patternsAnimDuration)
                    .SetEase(Ease.OutBack)
                    .OnStart(() => SprayPaintAudioManager.Instance.PlaySFX("Slide"))
            );

            seq.OnComplete(() => SprayPaintAudioManager.Instance.PlayBgMusic("BgMusic"));
        }
    }
}