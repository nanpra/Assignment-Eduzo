using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class LifeManager : MonoBehaviour
{
    public static LifeManager Instance;

    [Header("Life Setup")]
    public int totalLives = 3;
    public Image[] lifeFilledImages;
    public Image[] lifeEmptyImages;

    private int currentIndex = 0;

    // store original local positions to restore on reset
    private Vector3[] initialPositions;

    private void Awake()
    {
        Instance = this;

        // cache initial positions
        initialPositions = new Vector3[lifeFilledImages.Length];
        for (int i = 0; i < lifeFilledImages.Length; i++)
            initialPositions[i] = lifeFilledImages[i].transform.localPosition;

        // Hide empty images initially
        foreach (var img in lifeEmptyImages)
        {
            var c = img.color;
            img.color = new Color(c.r, c.g, c.b, 0);
        }
    }

    public void LoseLife()
    {
        // capture the index to operate on so callbacks reference the correct slot
        int index = currentIndex;

        if (index >= totalLives)
            return;

        // move to next life index immediately to reflect that one life is being consumed
        currentIndex++;

        Image filled = lifeFilledImages[index];
        Image empty = lifeEmptyImages[index];

        // Ensure any existing tweens on this transform are killed before starting new ones
        DOTween.Kill(filled.transform);

        // Disable interaction during animation
        filled.raycastTarget = false;

        Vector3 originalPos = filled.transform.localPosition;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            filled.transform.DOLocalMoveY(originalPos.y + 100, 0.25f)
            .SetEase(Ease.OutQuad)
        );

        seq.Append(
            filled.transform.DOLocalMoveY(originalPos.y - 30, 0.30f)
            .SetEase(Ease.InQuad)
        );

        seq.OnComplete(() =>
        {
            // Turn off filled image component
            filled.enabled = false;

            // Reset transform to original position so reuse shows correctly
            filled.transform.localPosition = originalPos;

            // Fade in empty image
            empty.DOFade(1, 0.4f).SetEase(Ease.OutSine);

            // No lives left -> trigger game over
            if (currentIndex >= totalLives)
            {
                Debug.Log("NO LIVES LEFT! Game Over");
                GameManager.Instance.EndGameLose();
            }
        });
    }

    // Reset lives for restart / new level
    public void ResetLives()
    {
        currentIndex = 0;
        totalLives = lifeFilledImages.Length;

        for (int i = 0; i < totalLives; i++)
        {
            // Kill any running tweens to avoid leftover animations
            DOTween.Kill(lifeFilledImages[i].transform);
            DOTween.Kill(lifeEmptyImages[i].transform);

            // Restore original position
            if (initialPositions != null && i < initialPositions.Length)
                lifeFilledImages[i].transform.localPosition = initialPositions[i];

            // Filled hearts ON
            lifeFilledImages[i].gameObject.SetActive(true);
            lifeFilledImages[i].enabled = true;
            lifeFilledImages[i].raycastTarget = true;
            var f = lifeFilledImages[i].color;
            lifeFilledImages[i].color = new Color(f.r, f.g, f.b, 1);

            // Empty hearts OFF (transparent)
            var e = lifeEmptyImages[i].color;
            lifeEmptyImages[i].color = new Color(e.r, e.g, e.b, 0);
        }
    }
}