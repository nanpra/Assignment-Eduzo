using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "SprayPaintTracingData",
    menuName = "Eduzo/Games/Spray Paint/Tracing Data"
)]
public class SprayPaintTracingSO : ScriptableObject
{
    public string letter; // "A", "B", "5"
    public List<StrokeData> strokes;
}

[Serializable]
public class StrokeData
{
    public List<Vector2> points; // LOCAL positions over letter image
}
