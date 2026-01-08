using DG.Tweening;
using Eduzo.Games.SequencingEvents.Audio;
using Eduzo.Games.SequencingEvents.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Eduzo.Games.SequencingEvents.Core
{
    public class SequencingEventsDropSlot : MonoBehaviour, IDropHandler
    {
        [Header("Correct Slot Index (0-based)")]
        public int slotIndex;
        public RectTransform nail;
        public RectTransform frame;
        public GameObject correctGlowVFX;

        private Image raycastImage;

        private void Awake()
        {
            // Ensure slot receives raycasts
            raycastImage = GetComponent<Image>();
            if (raycastImage == null)
            {
                raycastImage = gameObject.AddComponent<Image>();
                raycastImage.color = new Color(1, 1, 1, 0); // invisible
            }

            raycastImage.raycastTarget = true;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (SequencingEventsQuestionsLoader.Instance.IsInputLocked) return;
            if (eventData.pointerDrag == null) return;
            if (!eventData.pointerDrag.TryGetComponent(out SequencingEventsDraggableItem item)) return;

            // Return existing item
            var existing = GetPlacedItem();
            if (existing != null)
                existing.ReturnToOptions();

            // Place
            item.transform.SetParent(transform, false);
            item.SnapToCenter();

            // Hammer animation (always 2 hits)
            StartCoroutine(PlayHammerAnimation());

            SequencingEventsQuestionsLoader.Instance.OnSlotFilled();
        }

        private IEnumerator PlayHammerAnimation()
        {
            var hammer = SequencingEventsQuestionsLoader.Instance.hammer;
            SequencingEventsQuestionsLoader.Instance.LockInput(SequencingEventsQuestionsLoader.Instance.dragLockedDuration);

            yield return hammer.Hit(nail);
            yield return hammer.Hit(nail);

            nail.localScale = Vector3.zero;
            nail.gameObject.SetActive(true);
            nail.DOScale(10f, 0.2f).SetEase(Ease.OutBack).OnComplete(() => hammer.hammer.gameObject.SetActive(false));
        }

        public SequencingEventsDraggableItem GetPlacedItem()
        {
            for (int i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).TryGetComponent(out SequencingEventsDraggableItem item))
                    return item;

            return null;
        }

        public void PlayCorrectFeedback()
        {
            if (correctGlowVFX != null)
            {
                SequencingEventsAudioManager.Instance.PlaySFX("CorrectFrame");
                SequencingEventsGameManager.Instance.totalCorrect++;
                correctGlowVFX.SetActive(true);
                correctGlowVFX.transform.localScale = Vector3.zero;
                correctGlowVFX.transform
                    .DOScale(180f, 0.25f)
                    .SetEase(Ease.OutBack);
            }

            var item = GetPlacedItem();
            if (item != null)
            {
                item.transform
                    .DOPunchScale(Vector3.one * 0.3f, 0.3f, 1)
                    .SetEase(Ease.OutQuad);
            }
        }

        public void PlayWrongFeedback()
        {
            SequencingEventsAudioManager.Instance.PlaySFX("WrongFrame");
            SequencingEventsGameManager.Instance.totalWrong++;
            var item = GetPlacedItem();
            if (item == null) return;

            item.GetComponent<RectTransform>()
                .DOAnchorPosY(-2000f, 0.6f)
                .SetEase(Ease.InBack);
        }
    }
}