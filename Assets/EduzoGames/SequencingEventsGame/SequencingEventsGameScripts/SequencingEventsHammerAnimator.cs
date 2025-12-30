using DG.Tweening;
using UnityEngine;
using System.Collections;

namespace Eduzo.Games.SequencingEvents.UI
{
    public class SequencingEventsHammerAnimator : MonoBehaviour
    {
        public Transform hammer;

        [Header("Rotation")]
        private Vector3 baseRotation = new(0f, -90f, 90f);
        public float hitRotationZ = -35f;

        [Header("Timing")]
        public float hitDuration = 0.12f;

        private Quaternion baseQuat;

        private void Awake()
        {
            baseQuat = Quaternion.Euler(baseRotation);
        }

        public IEnumerator Hit(Transform nail)
        {
            hammer.gameObject.SetActive(true);

            // Position hammer above nail
            hammer.position = nail.position;

            // Reset to base rotation
            hammer.localRotation = baseQuat;

            // HIT (rotate only Z relative to base)
            yield return hammer
                .DOLocalRotate(
                    new Vector3(baseRotation.x, baseRotation.y, baseRotation.z + hitRotationZ),
                    hitDuration
                )
                .SetEase(Ease.InQuad)
                .WaitForCompletion();

            // RETURN
            yield return hammer
                .DOLocalRotate(baseRotation, hitDuration)
                .SetEase(Ease.OutQuad)
                .WaitForCompletion();
        }

        public void ResetNailAndHammer(Transform nail)
        {
            hammer.gameObject.SetActive(false);
            nail.gameObject.SetActive(false);
        }
    }
}