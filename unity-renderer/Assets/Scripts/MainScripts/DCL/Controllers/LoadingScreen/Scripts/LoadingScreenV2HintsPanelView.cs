using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace DCL.LoadingScreen.V2
{
    public class LoadingScreenV2HintsPanelView : MonoBehaviour
    {
        [SerializeField] private Button previousHintButton;
        [SerializeField] private Button nextHintButton;
        [SerializeField] private RectTransform[] dotsList;
        private List<Image> dotsImages;
        private int currentDotIndex = 0;
        private int previousDotIndex = 0;
        private int dotsCount = 0;

        private Vector2 bigSize = new Vector2(12, 12);
        private Vector2 smallSize = new Vector2(8, 8);
        private Color activeColor = new Color(1, 0.1764706f, 0.3333333f);
        private Color inactiveColor = Color.white;

        public event Action OnPreviousClicked;
        public event Action OnNextClicked;

        public void Initialize(int hintsAmount)
        {
            if (dotsList == null)
                throw new WarningException("HintDotsView - DotsList is not assigned!");

            if (hintsAmount <= 0)
                throw new WarningException("HintDotsView - DotsCount is not valid!");

            dotsCount = hintsAmount;
            dotsImages = new List<Image>();
            var dotsListLength = dotsList.Length;

            for (int i = 0; i < dotsListLength && i < dotsCount; i++)
            {
                dotsList[i].gameObject.SetActive(true);
                dotsImages.Add(dotsList[i].GetComponent<Image>());
                dotsList[i].gameObject.SetActive(true);
            }

            currentDotIndex = 0;
            ToggleDot(currentDotIndex);
        }

        public void ToggleDot(int index)
        {
            if (index < 0 || index > dotsCount)
                return;

            previousDotIndex = currentDotIndex;
            currentDotIndex = index;

            // Set current dot to active state
            dotsList[index].sizeDelta = bigSize;
            dotsImages[index].color = activeColor;

            if (currentDotIndex != previousDotIndex)
            {
                // Set previous dot to inactive state
                dotsList[previousDotIndex].sizeDelta = smallSize;
                dotsImages[previousDotIndex].color = inactiveColor;
            }
        }

        public void OnLeftClicked()
        {
            Debug.Log("FD:: HintsPanelView - OnLeftClicked");
            OnPreviousClicked?.Invoke();
        }

        public void OnRightClicked()
        {
            Debug.Log("FD:: HintsPanelView - OnRightClicked");
            OnNextClicked?.Invoke();
        }

        public void CleanUp()
        {
            foreach (var dot in dotsList)
            {
                dot.gameObject.SetActive(false);
            }
            OnPreviousClicked = null;
            OnNextClicked = null;
        }
    }
}
