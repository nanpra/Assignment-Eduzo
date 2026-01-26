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
            currentAngle -= deltaX * rotationSensitivity;
            currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

            playerRotationTransform.localRotation =
                Quaternion.Euler(0, 0, currentAngle);

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

            float t = Mathf.InverseLerp(minAngle, maxAngle, currentAngle);
            int newIndex = Mathf.RoundToInt(t * (targets.Count - 1));
            newIndex = Mathf.Clamp(newIndex, 0, targets.Count - 1);

            if (newIndex == currentIndex)
                return;

            currentIndex = newIndex;

            for (int i = 0; i < targets.Count; i++)
                targets[i].SetHighlighted(i == 4 - currentIndex);
        }

        private ArcheryTargetUI GetCurrentTarget()
        {
            return targets != null && targets.Count > 0
                ? targets[4 - currentIndex]
                : null;
        }

        public void ResetTargetVFX()
        {
            foreach (var target in targets)
                target.ResetVFX();
        }

        #endregion

        #region Shoot

        private void ShootCurrentTarget()
        {
            var target = GetCurrentTarget();
            if (target == null || arrowController == null)
                return;

            arrowController.ShootTo(
                target.GetComponent<RectTransform>(),
                () => ArcheryQuestionsLoader.Instance
                    .OnTargetSelected(target.Value)
            );
        }

        #endregion
    }
}