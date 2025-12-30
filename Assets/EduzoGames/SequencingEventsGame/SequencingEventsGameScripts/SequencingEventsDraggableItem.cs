using Eduzo.Games.SequencingEvents.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Eduzo.Games.SequencingEvents.UI
{
    public class SequencingEventsDraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Image image;
        public int correctSlotIndex;
        public int originalOptionIndex;

        [HideInInspector] public Transform lastSlot;

        private Transform optionsContainer;
        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private LayoutElement layoutElement;
        private RectTransform rectTransform;
        private Vector2 dragOffset;

        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();

            layoutElement = GetComponent<LayoutElement>();
            if (layoutElement == null)
                layoutElement = gameObject.AddComponent<LayoutElement>();

            optionsContainer = SequencingEventsQuestionsLoader.Instance.optionsContainer;
        }

        public void Init(Sprite sprite, int correctIndex, int optionIndex)
        {
            image.sprite = sprite;
            correctSlotIndex = correctIndex;
            originalOptionIndex = optionIndex;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // BLOCK DRAGGING DURING HAMMER ANIMATION
            if (SequencingEventsQuestionsLoader.Instance.IsInputLocked)
                return;

            lastSlot = transform.parent != optionsContainer
                ? transform.parent
                : null;

            layoutElement.ignoreLayout = true;
            canvasGroup.blocksRaycasts = false;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                eventData.position,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : canvas.worldCamera,
                out dragOffset);

            transform.SetParent(canvas.transform, true);
            rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition3D =
                new Vector3(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y, 0f);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // BLOCK DRAGGING DURING HAMMER ANIMATION
            if (SequencingEventsQuestionsLoader.Instance.IsInputLocked)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : canvas.worldCamera,
                out Vector2 localPos);

            rectTransform.anchoredPosition =new Vector3(localPos.x - dragOffset.x, localPos.y - dragOffset.y, 0f);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // BLOCK DRAGGING DURING HAMMER ANIMATION
            if (SequencingEventsQuestionsLoader.Instance.IsInputLocked)
                return;

            canvasGroup.blocksRaycasts = true;
            layoutElement.ignoreLayout = false;

            // Not dropped on slot → return to options ALWAYS
            if (transform.parent == canvas.transform)
                ReturnToOptions();
        }

        public void ReturnToOptions()
        {
            transform.SetParent(optionsContainer, false);
            transform.SetSiblingIndex(originalOptionIndex);
            rectTransform.anchoredPosition3D = Vector3.zero;
        }

        public void SnapToCenter()
        {
            RectTransform rt = GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition3D = Vector3.zero;
        }
    }
}