using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Eduzo.Games.Archery.UI
{
    public class ArcherySingleLetterInput : MonoBehaviour
    {
        public TMP_InputField input;

        private static readonly Regex regex = new("[^a-zA-Z0-9]");

        private void Awake()
        {
            input.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(string value)
        {
            string filtered = regex.Replace(value, "").ToUpperInvariant();

            if (filtered.Length > 1)
                filtered = filtered[..1];

            if (input.text != filtered)
                input.SetTextWithoutNotify(filtered);
        }
    }
}