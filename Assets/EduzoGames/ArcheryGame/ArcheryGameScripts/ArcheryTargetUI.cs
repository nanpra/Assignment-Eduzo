using Eduzo.Games.Archery.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ArcheryTargetUI : MonoBehaviour
{
    public TextMeshProUGUI letterText;
    public Outline highlightRing;
    public GameObject hitVFX;

    public bool IsHit { get; private set; }

    public string Value => letterText.text;

    private void Awake()
    {
        ResetTarget();
    }

    public void SetHighlighted(bool active)
    {
        if (IsHit) return; // no highlight after hit
        highlightRing.enabled = active;
        transform.localScale = active ? Vector3.one * 1.1f : Vector3.one;
    }

    public void MarkHit()
    {
        IsHit = true;
        highlightRing.enabled = false;
        transform.localScale = Vector3.one;
    }

    public void PlayHitVFX()
    {
        if (hitVFX != null)
        {
            hitVFX.SetActive(false);
            hitVFX.SetActive(true);
        }
    }

    public void PlayHitSFX()
    {
        ArcheryAudioManager.Instance.PlaySFX("ArrowHit");
    }

    public void ResetTarget()
    {
        IsHit = false;
        highlightRing.enabled = false;
        if (hitVFX != null)
            hitVFX.SetActive(false);
    }
}
