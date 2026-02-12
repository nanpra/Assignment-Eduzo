using UnityEngine;

namespace Eduzo.Games.SprayPaint.Core
{
    public class SprayPaintColorManager : MonoBehaviour
    {
        public static SprayPaintColorManager Instance;

        public enum SprayPaintColor
        {
            Red,
            Blue,
            Green
        }

        public SprayPaintColor CurrentColor { get; private set; }

        private void Awake()
        {
            Instance = this;
            SetRed(); // default
        }

        //assigned in the inspector -> color selection buttons
        public void SetRed()
        {
            CurrentColor = SprayPaintColor.Red;
        }

        public void SetBlue()
        {
            CurrentColor = SprayPaintColor.Blue;
        }

        public void SetGreen()
        {
            CurrentColor = SprayPaintColor.Green;
        }

        public Color GetSprayCanSelectedColor()
        {
            return CurrentColor switch
            {
                SprayPaintColor.Red => new Color(0.94f, 0.61f, 0.3f),
                SprayPaintColor.Blue => new Color(0.38f, 1f, 1f),
                SprayPaintColor.Green => new Color(0.33f, 1f, 0.3f),
                _ => Color.white
            };
        }
    }
}