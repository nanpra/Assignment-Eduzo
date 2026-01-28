using System;
using System.Collections.Generic;

[Serializable]
public class ArcheryQuestionResult
{
    public int QuestionIndex;
    public ArcheryAnswerType AnswerType;

    public string EnteredQuestion;

    // For OneCorrect → single letter
    // For ArrangeInSequence → A,B,C,D,E
    public string CorrectSequence;

    // Shuffled options shown to user
    public List<string> OptionsProvided = new();

    // Each attempt is stored as a string
    // Examples:
    // "A"
    // "A,B,D"
    public List<string> WrongAttemptsByUser = new();

    public bool AnsweredCorrectly;
    public float ActiveTime;
    public bool IsFinalized;
}