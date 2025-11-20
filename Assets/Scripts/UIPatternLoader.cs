using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPatternLoader : MonoBehaviour
{
    public static UIPatternLoader Instance;

    public QuestionBank questionBank;
    public int levelIndex;

    [Header("Parents")]
    public Transform questionParent;
    public Transform optionsParent;

    [Header("Prefabs")]
    public GameObject questionSlotPrefab;
    public GameObject optionButtonPrefab;

    public static QuestionPattern currentQuestion;
    public int filledCount = 0;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void LoadQuestion(int level)
    {
        questionParent.gameObject.SetActive(true);
        optionsParent.gameObject.SetActive(true);

        currentQuestion = questionBank.GetQuestion(level);

        if (currentQuestion == null)
        {
            Debug.LogError("INVALID LEVEL!");
            return;
        }

        filledCount = 0;
        LoadPattern();
        LoadOptions();
    }

    private void LoadPattern()
    {
        // Clear old entries
        foreach (Transform child in questionParent)
            Destroy(child.gameObject);

        var displayPattern = currentQuestion.GetDisplayPattern();

        for (int i = 0; i < displayPattern.Length; i++)
        {
            GameObject slot = Instantiate(questionSlotPrefab, questionParent);
            Image img = slot.GetComponent<Image>();
            img.sprite = displayPattern[i];

            // Child animation
            slot.transform.localScale = Vector3.zero;
            slot.transform.DOScale(1f, 0.4f)
                .SetDelay(i * 0.1f) // slight stagger effect
                .SetEase(Ease.OutBack);
        }
    }

    private void LoadOptions()
    {
        // Clear old options
        foreach (Transform child in optionsParent)
            Destroy(child.gameObject);

        Sprite[] correctAnswers = currentQuestion.GetCorrectAnswers();

        // Instantiate ONLY the icons used in the pattern (unique)
        var usedIcons = new HashSet<Sprite>(currentQuestion.patternIcons);

        foreach (var icon in usedIcons)
        {
            GameObject opt = Instantiate(optionButtonPrefab, optionsParent);
            Image img = opt.GetComponent<Image>();
            img.sprite = icon;

            // Child animation
            opt.transform.localScale = Vector3.zero;
            opt.transform.DOScale(1f, 0.4f)
                .SetDelay(0.1f * optionsParent.childCount) // stagger each option
                .SetEase(Ease.OutBack);

            Button btn = opt.GetComponent<Button>();
            Sprite captured = icon;

            btn.onClick.AddListener(() => OnOptionSelected(captured));
        }
    }

    private void OnOptionSelected(Sprite chosenIcon)
    {
        Sprite[] answers = currentQuestion.GetCorrectAnswers();

        foreach (var correct in answers)
        {
            if (chosenIcon == correct)
            {
                OnCorrectAnswer(chosenIcon);
                return;
            }
        }

        OnWrongAnswer();
    }

    private void OnWrongAnswer()
    {
        Debug.Log("Wrong!");
        AudioManager.Instance.PlaySFX("WrongAnswer");
        // Reduce life
        LifeManager.Instance.LoseLife();

        foreach (Transform slot in questionParent)
        {
            Image img = slot.GetComponent<Image>();

            if (img.sprite == currentQuestion.questionMarkIcon)
            {
                // Shake animation
                slot.DOShakePosition(0.5f, 25, 20);
                slot.DOShakeRotation(0.5f, 25, 15);
            }
        }
    }

    private void OnCorrectAnswer(Sprite chosen)
    {
        Debug.Log("Correct!");
        AudioManager.Instance.PlaySFX("CorrectAnswer");
        Sprite[] answers = currentQuestion.GetCorrectAnswers();

        for (int i = 0; i < answers.Length; i++)
        {
            if (answers[i] == chosen)
            {
                int indexToFill = currentQuestion.missingIndices[i];
                Transform slot = questionParent.GetChild(indexToFill);
                Image img = slot.GetComponent<Image>();

                // Prevent double-filling same slot
                if (img.sprite != currentQuestion.questionMarkIcon)
                    return;

                filledCount++; // Increase now, but check later
                GameManager.Instance.OnCorrectAnswer();

                Sequence seq = DOTween.Sequence();

                // Start rotation
                seq.Append(img.transform.DORotate(new Vector3(0, 360, 0), 0.6f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine));

                // Scale up while rotating (mid pop)
                seq.Join(img.transform.DOScale(1.3f, 0.3f).SetEase(Ease.OutQuad));

                // At half rotation → change sprite
                seq.InsertCallback(0.3f, () => img.sprite = chosen);

                // Scale back to normal
                seq.Append(img.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));

                // Wait for animation end → then check win
                seq.OnComplete(() =>
                {
                    if (filledCount == currentQuestion.missingIndices.Length)
                        GameManager.Instance.EndGameWin();
                });

                return;
            }
        }
    }
}