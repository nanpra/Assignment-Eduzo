using UnityEngine;
using DG.Tweening;
using System.Collections;

public class StartScreenUIAnimations : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform startButton;          // Slide from TOP
    public RectTransform levelDropdown;        // Slide from BOTTOM

    public float animDuration = 0.6f;
    public float offset = 300f;                // How far it should slide from off-screen

    private Vector2 startOriginalPos;
    private Vector2 dropdownOriginalPos;

    private void Start()
    {
        // Save original positions
        startOriginalPos = startButton.anchoredPosition;
        dropdownOriginalPos = levelDropdown.anchoredPosition;

        AnimateMainMenu();
    }

    public void AnimateMainMenu()
    {
        // Move Start Button ABOVE screen
        startButton.anchoredPosition = new Vector2(
            startOriginalPos.x,
            startOriginalPos.y + offset
        );

        // Move Dropdown BELOW screen
        levelDropdown.anchoredPosition = new Vector2(
            dropdownOriginalPos.x,
            dropdownOriginalPos.y - offset
        );

        startButton.gameObject.SetActive(true);
        levelDropdown.gameObject.SetActive(true);

        StartCoroutine(DelayAnimation());
    }

    private IEnumerator DelayAnimation()
    {
        //wait 2 frames as slideSFX is called too early
        yield return null;
        yield return null;

        Sequence seq = DOTween.Sequence();

        // Start Button Animation
        seq.Append(
            startButton.DOAnchorPos(startOriginalPos, animDuration)
                .SetEase(Ease.OutBack)
                .OnStart(() => AudioManager.Instance.PlaySFX("Slide"))
        );

        // Dropdown Animation
        seq.Append(
            levelDropdown.DOAnchorPos(dropdownOriginalPos, animDuration)
                .SetEase(Ease.OutBack)
                .OnStart(() => AudioManager.Instance.PlaySFX("Slide"))
        );
    }
}