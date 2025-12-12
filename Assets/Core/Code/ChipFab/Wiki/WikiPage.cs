using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public enum WikiPageCategoryIndex
    {
        Concept,
        Station,
    }

    public enum WikiPageSubCategoryIndex
    {
        Furnace,
        Resist,
        Photo,
        Etch,
        Spray
    }

    public class WikiPage : MonoBehaviour
    {
        public WikiPageCategoryIndex CategoryId;
        public WikiPageSubCategoryIndex SubCategoryId;
    }
}