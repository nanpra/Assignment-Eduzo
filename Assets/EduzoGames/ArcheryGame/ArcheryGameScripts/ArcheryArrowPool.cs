using System.Collections.Generic;
using UnityEngine;


namespace Eduzo.Games.Archery.Core
{
    public class ArcheryArrowPool : MonoBehaviour
    {
        public static ArcheryArrowPool Instance;

        [Header("Arrow Pool")]
        public ArcheryArrowController arrowPrefab;
        public RectTransform bowOrigin;
        public int poolSize = 5;

        private readonly List<ArcheryArrowController> pool = new();
        private int currentIndex;

        private void Awake()
        {
            Instance = this;
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                var arrow = Instantiate(arrowPrefab, transform);
                arrow.Init(bowOrigin);
                arrow.gameObject.SetActive(false);
                pool.Add(arrow);
            }
        }

        public ArcheryArrowController GetArrow()
        {
            if (currentIndex >= pool.Count)
                return null; // no arrows left this question

            var arrow = pool[currentIndex];
            currentIndex++;
            arrow.gameObject.SetActive(true);
            return arrow;
        }

        public void ResetPool()
        {
            currentIndex = 0;

            foreach (var arrow in pool)
                arrow.ResetArrow();
        }
    }
}