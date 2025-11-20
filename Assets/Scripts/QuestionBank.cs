using UnityEngine;

[CreateAssetMenu(fileName = "QuestionBank", menuName = "ScriptableObjects/Question Bank")]
public class QuestionBank : ScriptableObject
{
    public QuestionPattern[] questions;

    public QuestionPattern GetQuestion(int level)
    {
        if (level < 0 || level >= questions.Length)
            return null;

        return questions[level];
    }
}