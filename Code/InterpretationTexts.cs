using UnityEngine;

[System.Serializable]
public struct InterpretationTexts
{
    [Header("Text Content")]
    [TextArea(2, 4)] public string outcome;
    [TextArea(3, 10)] public string straightTalk;
    [TextArea(2, 4)] public string mantra;
}
