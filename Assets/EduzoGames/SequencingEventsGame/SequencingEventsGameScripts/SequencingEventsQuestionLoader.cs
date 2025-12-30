using DG.Tweening;
using Eduzo.Games.SequencingEvents.Audio;
using Eduzo.Games.SequencingEvents.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Eduzo.Games.SequencingEvents.Core
{
    public class SequencingEventsQuestionsLoader : MonoBehaviour
    {
        public static SequencingEventsQuestionsLoader Instance;
        public Transform optionsContainer;
        public Transform answerContainer;
        public GameObject draggablePrefab;
        public GameObject winVFX;
        public GameObject loseVFX;
        public Image blackOverlay;
        public SequencingEventsHammerAnimator hammer;
        public List<Sprite> allFrames = new();
        public List<Image> allNumbers = new();
        public float dragLockedDuration = 1f;

        public bool IsInputLocked { get; private set; }
        private bool isResolving;


        private void Awake()
        {
            Instance = this;
            ResetNumbers();
        }

        public void LoadRuntimeQuestion(SequencingEventsRuntimeQuestion question)
        {
            ClearContainers();

            float delayStep = 0.15f;
            float animDuration = 0.25f;
            float rotationAmount = 30f;

            for (int i = 0; i < question.shuffledOrder.Length; i++)
            {
                Sprite optionSprite = question.shuffledOrder[i];
                int correctIndex = System.Array.IndexOf(question.correctOrder, optionSprite);

                GameObject go = Instantiate(draggablePrefab, optionsContainer);
                SequencingEventsDraggableItem item = go.GetComponent<SequencingEventsDraggableItem>();
                item.Init(optionSprite, correctIndex, i);

                // ASSIGN FRAME SPRITE (first child)
                AssignFrameSprite(go, i);
                RectTransform rt = go.GetComponent<RectTransform>();

                if (!go.TryGetComponent(out CanvasGroup cg))
                    cg = go.AddComponent<CanvasGroup>();

                rt.localScale = Vector3.zero;
                rt.localRotation = Quaternion.Euler(0, 0, -rotationAmount);
                cg.alpha = 0f;

                Sequence seq = DOTween.Sequence();
                seq.SetDelay(i * delayStep);
                seq.Append(rt.DOScale(1f, animDuration).SetEase(Ease.OutBack));
                seq.Join(rt.DORotate(Vector3.zero, animDuration * 0.8f, RotateMode.FastBeyond360));
                seq.Join(cg.DOFade(1f, animDuration));
            }

            ClearUIFocus();
        }

        private void AssignFrameSprite(GameObject optionGO, int index)
        {
            if (allFrames == null || allFrames.Count == 0)
            {
                Debug.LogError("Frames list is empty!");
                return;
            }

            int frameIndex = index % allFrames.Count;
            Transform frameChild = optionGO.transform.GetChild(0);
            
            if (!frameChild.TryGetComponent<Image>(out var frameImage))
            {
                Debug.LogError("Frame child does not have an Image component!");
                return;
            }

            frameImage.sprite = allFrames[frameIndex];
        }

        public bool AreAllSlotsFilled()
        {
            if(optionsContainer.childCount > 0)
                return false;

            return true;
        }

        public void OnSlotFilled()
        {
            if (isResolving) return;

            if (AreAllSlotsFilled())
                StartCoroutine(ResolveAnswer());
        }

        private IEnumerator ResolveAnswer()
        {
            isResolving = true;
            bool isCorrect = IsOrderCorrect();
            yield return new WaitForSeconds(1.5f);     // Wait for hammer animation before starting resolution flow
            List<string> playerOrder = CapturePlayerOrder();
            SequencingEventsGameManager.Instance.SetPlayerOrder(playerOrder);

            if (isCorrect)
                yield return CorrectOrderFlow();
            else
                yield return WrongOrderFlow();

            isResolving = false;
        }

        private IEnumerator CorrectOrderFlow()
        {
            float stagger = 0.5f;

            for (int i = 0; i < answerContainer.childCount; i++)
            {
                if (!answerContainer.GetChild(i).TryGetComponent<SequencingEventsDropSlot>(out var slot)) continue;

                // Slot feedback
                slot.PlayCorrectFeedback();

                // Number pop
                if (i < allNumbers.Count && allNumbers[i] != null)
                {
                    Image num = allNumbers[i];
                    num.gameObject.SetActive(true);
                    num.transform.localScale = Vector3.zero;

                    num.transform
                        .DOScale(1f, 0.25f)
                        .SetEase(Ease.OutBack);

                    num.transform
                        .DOPunchScale(Vector3.one * 0.2f, 0.2f);
                }

                yield return new WaitForSeconds(stagger);
            }

            yield return new WaitForSeconds(0.5f);
            SequencingEventsAudioManager.Instance.PlaySFX("CorrectAnswer");
            winVFX.SetActive(true);
            yield return BlackOverlaySequence();

            SequencingEventsGameManager.Instance.OnAnswerSubmitted(true);
        }

        private IEnumerator WrongOrderFlow()
        {
            float stagger = 0.5f;

            foreach (Transform slotT in answerContainer)
            {
                if (!slotT.TryGetComponent<SequencingEventsDropSlot>(out var slot)) continue;

                var item = slot.GetPlacedItem();
                if (item == null) continue;

                if (item.correctSlotIndex == slot.slotIndex)
                {
                    // Correct frame feedback
                    slot.PlayCorrectFeedback();
                }
                else
                {
                    // Wrong frame falls
                    slot.PlayWrongFeedback();
                }

                yield return new WaitForSeconds(stagger);
            }

            yield return new WaitForSeconds(0.5f);
            SequencingEventsAudioManager.Instance.PlaySFX("WrongAnswer");
            loseVFX.SetActive(true);
            yield return BlackOverlaySequence();

            SequencingEventsGameManager.Instance.OnAnswerSubmitted(false);
        }

        private SequencingEventsDraggableItem GetItemInSlot(Transform slot)
        {
            for (int i = 0; i < slot.childCount; i++)
            {
                if (slot.GetChild(i).TryGetComponent(out SequencingEventsDraggableItem item))
                    return item;
            }

            return null;
        }

        public List<string> CapturePlayerOrder()
        {
            List<string> order = new();

            foreach (Transform slot in answerContainer)
            {
                for (int i = 0; i < slot.childCount; i++)
                {
                    if (slot.GetChild(i).TryGetComponent<SequencingEventsDraggableItem>(out var item))
                    {
                        order.Add(item.image.sprite.name);
                        break;
                    }
                }
            }

            return order;
        }

        private IEnumerator BlackOverlaySequence()
        {
            yield return blackOverlay.DOFade(1, 0.6f).WaitForCompletion();
            ResetSlotsAndHammer();
            ResetNumbers();
            yield return blackOverlay.DOFade(0, 0.6f).WaitForCompletion();
        }

        private void ResetSlotsAndHammer()
        {
            foreach (Transform slot in answerContainer)
            {
                foreach (Transform child in slot)
                {
                    if (child.TryGetComponent<SequencingEventsDraggableItem>(out _))
                        Destroy(child.gameObject);
                }

                var ds = slot.GetComponent<SequencingEventsDropSlot>();
                ds.nail.gameObject.SetActive(false);

                if (ds.correctGlowVFX != null)
                    ds.correctGlowVFX.SetActive(false);
            }

            hammer.gameObject.SetActive(false);
        }

        private bool IsOrderCorrect()
        {
            foreach (Transform slot in answerContainer)
            {
                if (!slot.TryGetComponent<SequencingEventsDropSlot>(out var slotComp))
                    return false;

                var item = GetItemInSlot(slot);
                if (item == null)
                    return false;

                if (item.correctSlotIndex != slotComp.slotIndex)
                    return false;
            }
            return true;
        }

        public void LockInput(float duration)
        {
            if (IsInputLocked) return;
            StartCoroutine(LockRoutine(duration));
        }

        private IEnumerator LockRoutine(float duration)
        {
            IsInputLocked = true;
            SetOptionsDimmed(true);

            yield return new WaitForSeconds(duration);

            SetOptionsDimmed(false);
            IsInputLocked = false;
        }

        private void SetOptionsDimmed(bool dim)
        {
            foreach (Transform child in optionsContainer)
            {
                if (child.TryGetComponent<CanvasGroup>(out var cg))
                    cg.alpha = dim ? 0.6f : 1f;
            }
        }

        private void ClearContainers()
        {
            // Clear options
            foreach (Transform c in optionsContainer)
                Destroy(c.gameObject);

            // Clear answer slots
            foreach (Transform slot in answerContainer)
            {
                Transform content = slot.Find("OptionImagePrefab(Clone)");
                if (content != null && content.childCount > 0)
                    Destroy(content.gameObject);
            }
        }

        private void ResetNumbers()
        {
            foreach (var img in allNumbers)
            {
                if (img == null) continue;

                img.gameObject.SetActive(false);
                img.transform.localScale = Vector3.zero;
                img.DOKill();
            }
        }

        private void ClearUIFocus()
        { 
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }
}