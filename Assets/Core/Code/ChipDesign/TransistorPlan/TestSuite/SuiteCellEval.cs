using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.ChipDesign
{
    public class SuiteCellEval : MonoBehaviour
    {
        public Image Img;
        public Sprite CorrectImg;
        public Sprite IncorrectImg;

        public void SetCorrect()
        {
            Img.sprite = CorrectImg;
        }

        public void SetIncorrect()
        {
            Img.sprite = IncorrectImg;
        }
    }
}