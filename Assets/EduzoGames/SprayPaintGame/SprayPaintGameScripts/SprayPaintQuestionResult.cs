using System;

[Serializable]
public class SprayPaintQuestionResult
{
    public int QuestionIndex;
    public SprayPaintQuestionType QuestionType;

    public string EnteredQuestion; // The selected letter/number

    public bool AnsweredCorrectly;
    public float ActiveTime;

    public bool IsFinalized;
}
