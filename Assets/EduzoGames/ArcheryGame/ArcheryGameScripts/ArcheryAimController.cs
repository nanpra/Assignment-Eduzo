using DG.Tweening;
using Eduzo.Games.Archery.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Eduzo.Games.Archery.Core
{
    public class ArcheryAimController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Aim")]
        public RectTransform playerRotationTransform;
        public float rotationSensitivity = 0.15f;
        public float minAngle = -40f;
        public float maxAngle = 40f;

        [Header("Shooting")]
        public ArcheryArrowController arrowController;

        [Header("Targets")]
        public List<ArcheryTargetUI> targets;

        private float currentAngle;
        private int currentIndex;
        private Vector2 lastDragPosition;
        private bool inputLocked;


        private void Start()
        {
            currentAngle = NormalizeAngle(playerRotationTransform.localEulerAngles.z);
            UpdateSelection();
        }

        #region Drag Handlers (Sequencing Style)

        public void OnBeginDrag(PointerEventData eventData)
        {
            lastDragPosition = eventData.position;
            Debug.Log("Begin Drag");
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 currentPos = eventData.position;
            float deltaX = currentPos.x - lastDragPosition.x;

            RotatePlayer(deltaX);
            lastDragPosition = currentPos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            ShootCurrentTarget();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            lastDragPosition = eventData.position;
        }

        #endregion

        #region Rotation Logic

        private void RotatePlayer(float deltaX)
        {
            currentAngle += deltaX * rotationSensitivity;
            currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

            playerRotationTransform.localRotation =
                Quaternion.Euler(0, 0, -currentAngle);

            // Convert angle → index
            float t = Mathf.InverseLerp(minAngle, maxAngle, currentAngle);
            currentIndex = Mathf.RoundToInt(t * (targets.Count - 1));
            currentIndex = Mathf.Clamp(currentIndex, 0, targets.Count - 1);

            UpdateSelection();
        }

        private float NormalizeAngle(float angle)
        {
            if (angle > 180f)
                angle -= 360f;
            return angle;
        }

        #endregion

        #region Target Selection

        private void UpdateSelection()
        {
            if (targets == null || targets.Count == 0)
                return;

            int safety = 0;

            while (ArcheryQuestionsLoader.Instance.IsTargetUsed(currentIndex))
            {
                currentIndex++;
                if (currentIndex >= targets.Count)
                    currentIndex = 0;

                safety++;
                if (safety > targets.Count)
                    return; // all targets used
            }

            for (int i = 0; i < targets.Count; i++)
                targets[i].SetHighlighted(i == currentIndex);
        }

        private ArcheryTargetUI GetCurrentTarget()
        {
            return targets != null && targets.Count > 0
                ? targets[currentIndex]
                : null;
        }

        #endregion

        #region Shoot

        private void ShootCurrentTarget()
        {
            if (inputLocked) return;

            if (ArcheryQuestionsLoader.Instance.IsTargetUsed(currentIndex))
                return;

            var target = GetCurrentTarget();
            if (target == null || target.IsHit)
                return;

            var arrow = ArcheryArrowPool.Instance.GetArrow();
            if (arrow == null) return;

            inputLocked = true;
            ArcheryAudioManager.Instance.PlaySFX("ArrowShoot");

            arrow.ShootTo(
                target.GetComponent<RectTransform>(),
                currentIndex,
                () => OnArrowImpact(target, currentIndex)
            );
        }

        private void OnArrowImpact(ArcheryTargetUI target , int currentIndex)
        {
            target.MarkHit();
            target.PlayHitSFX();
            target.PlayHitVFX();

            StartCoroutine(DelayedResolve(target , currentIndex));
        }

        private IEnumerator DelayedResolve(ArcheryTargetUI target , int currentIndex)
        {
            yield return new WaitForSeconds(0.5f); // impact pause
            ArcheryQuestionsLoader.Instance.OnTargetSelected(target.Value , currentIndex);
            target.ResetTarget();
            yield return new WaitForSeconds(0.3f); // small delay before next input
            inputLocked = false;
        }

        public void ResetTargetVFX()
        {
            foreach (var t in targets)
                t.ResetTarget();

            inputLocked = false;
        }

        #endregion
    }
}