using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Eduzo.Games.Archery.Core
{
    public class ArcheryTargetUI : MonoBehaviour
    {
        public TextMeshProUGUI letterText;
        public Outline highlightRing;
        public GameObject hitVFX;

        public string Value => letterText.text;

        public void SetHighlighted(bool active)
        {
            highlightRing.enabled = active;
            transform.localScale = active ? Vector3.one * 1.1f : Vector3.one;
        }

        public void PlayHitVFX()
        {
            if (hitVFX != null)
            {
                hitVFX.SetActive(false);
                hitVFX.SetActive(true);
            }
        }

        public void ResetVFX()
        {
            if (hitVFX != null)
                hitVFX.SetActive(false);
        }
    }
}