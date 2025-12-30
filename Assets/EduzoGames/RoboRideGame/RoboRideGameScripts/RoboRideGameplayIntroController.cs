using DG.Tweening;
using UnityEngine;
using Eduzo.Games.RoboRide.Core;

namespace Eduzo.Games.RoboRide.UI
{
    public class RoboRideGameplayIntroController : MonoBehaviour
    {
        [Header("Canvases")]
        public CanvasGroup formCanvas;
        public CanvasGroup gameplayCanvas;

        [Header("UI Elements")]
        public RectTransform displayImages;
        public RectTransform questionBGPanel;
        public RectTransform collectibleStar;

        [Header("Text")]
        public Transform questionText;
        public Transform sentenceText;

        [Header("Robot")]
        public RoboRideRobotMovement robot;
        public Transform robotTransform;

        [Header("Robot Parts")]
        public Transform robotBody;
        public Transform robotHead;

        [Header("Timings")]
        public float canvasFadeDuration = 0.35f;
        public float uiReachDuration = 0.6f;
        public float robotMoveDuration = 0.35f;
        public float robotStopBounceOffset = 0.15f;

        private Vector2 displayImagesStartPos;
        private Vector2 questionBGStartPos;
        private Vector3 robotStartPos;
        private Vector3 robotBodyStartRot;
        private Vector3 robotHeadStartRot;
        private Vector3 robotHeadStartPos;


        private void Awake()
        {
            SetInitialState();
        }

        private void SetInitialState()
        {
            if (robotBody == null) robotBody = robotTransform;
            if (robotHead == null) robotHead = robotTransform;

            // Cache OFF-SCREEN starting positions
            displayImagesStartPos = displayImages.anchoredPosition;
            questionBGStartPos = questionBGPanel.anchoredPosition;
            robotStartPos = robotTransform.localPosition;

            robotBodyStartRot = robotBody.localRotation.eulerAngles;
            robotHeadStartRot = robotHead.localRotation.eulerAngles;
            robotHeadStartPos = robotHead.localPosition;
        }

        public void PlayIntro()
        {
            // Kill any previous animation safely
            DOTween.Kill(this);

            Sequence seq = DOTween.Sequence()
                .SetId(this)
                .SetUpdate(false);

            ResetInitialState();

            FadeOutForm(seq);
            FadeInGameplay(seq);
            AnimateIntroUI(seq);
            AnimateRobot(seq);
            AnimateTextsAndStar(seq);
            StartGameplaySystems(seq);
        }

        #region Sequence Parts

        private void ResetInitialState()
        {
            questionText.localScale = Vector3.zero;
            sentenceText.localScale = Vector3.zero;
            collectibleStar.localScale = Vector3.zero;
        }

        private void FadeOutForm(Sequence seq)
        {
            seq.Append(formCanvas
                    .DOFade(0, canvasFadeDuration))
               .AppendCallback(() => formCanvas.gameObject.SetActive(false));
        }

        private void FadeInGameplay(Sequence seq)
        {
            seq.AppendCallback(() =>
            {
                gameplayCanvas.gameObject.SetActive(true);
                gameplayCanvas.alpha = 0;
            });

            seq.Append(gameplayCanvas
                .DOFade(1, canvasFadeDuration));
        }

        private void AnimateIntroUI(Sequence seq)
        {
            seq.Append(displayImages
                .DOAnchorPosY(0, uiReachDuration)
                .SetEase(Ease.OutCubic));

            seq.Join(questionBGPanel
                .DOAnchorPosX(-2000, uiReachDuration)
                .SetEase(Ease.OutCubic));
        }

        private void AnimateRobot(Sequence seq)
        {
            Tween moveTween = robotTransform
                .DOLocalMoveX(-2900f, robotMoveDuration)
                .SetEase(Ease.OutQuart);

            seq.Append(moveTween);

            // Anticipated stop bounce
            seq.Insert(
                seq.Duration() - robotStopBounceOffset,
                CreateRobotStopBounceTween()
            );
        }

        private void AnimateTextsAndStar(Sequence seq)
        {
            seq.Append(collectibleStar
                .DOScale(1f, 0.25f)
                .SetEase(Ease.OutBack)
                .SetDelay(0.3f));

            seq.Join(questionText
                .DOScale(1f, 0.3f)
                .SetEase(Ease.OutBack)
                .SetDelay(0.5f));

            seq.Join(sentenceText
                .DOScale(1f, 0.3f)
                .SetEase(Ease.OutBack)
                .SetDelay(0.5f));
        }

        private void StartGameplaySystems(Sequence seq)
        {
            seq.AppendCallback(() =>
            {
                RoboRideQuestionsLoader.Instance.StartHighlighting();
                RoboRidePlatformController.Instance.StartIdleMotion();
                RoboRideCountdownTimer.Instance.StartTimer();
            });
        }

        #endregion

        #region Robot Stop Bounce

        private Tween CreateRobotStopBounceTween()
        {
            Sequence bounce = DOTween.Sequence();

            // Lift / inertia
            bounce.Append(robotBody
                .DOLocalRotate(new Vector3(0, 0, -20f), 0.12f)
                .SetEase(Ease.OutQuad));

            bounce.Join(robotHead
                .DOLocalRotate(new Vector3(0, 0, -25f), 0.12f)
                .SetEase(Ease.OutQuad));

            bounce.Join(robotHead
                .DOLocalMoveX(280f, 0.12f)
                .SetEase(Ease.OutQuad));

            // Snap back
            bounce.Append(robotBody
                .DOLocalRotate(Vector3.zero, 0.18f)
                .SetEase(Ease.OutBack));

            bounce.Join(robotHead
                .DOLocalRotate(Vector3.zero, 0.18f)
                .SetEase(Ease.OutBack));

            bounce.Join(robotHead
                .DOLocalMoveX(100f, 0.18f)
                .SetEase(Ease.OutBack));

            return bounce;
        }

        #endregion

        public void ResetIntroState()
        {
            // Kill any running tweens related to intro
            DOTween.Kill(this);
            DOTween.Kill(robotTransform);
            DOTween.Kill(robotBody);
            DOTween.Kill(robotHead);
            DOTween.Kill(displayImages);
            DOTween.Kill(questionBGPanel);
            DOTween.Kill(collectibleStar);
            DOTween.Kill(questionText);
            DOTween.Kill(sentenceText);

            // Reset UI positions
            displayImages.anchoredPosition = displayImagesStartPos;
            questionBGPanel.anchoredPosition = questionBGStartPos;

            // Reset text + star
            questionText.localScale = Vector3.zero;
            sentenceText.localScale = Vector3.zero;
            collectibleStar.localScale = Vector3.zero;

            // Reset robot
            robotTransform.localPosition = robotStartPos;
            robotBody.localRotation = Quaternion.Euler(robotBodyStartRot);
            robotHead.SetLocalPositionAndRotation(robotHeadStartPos, Quaternion.Euler(robotHeadStartRot));

            // Reset canvas
            gameplayCanvas.alpha = 0;
            gameplayCanvas.gameObject.SetActive(false);
        }
    }
}