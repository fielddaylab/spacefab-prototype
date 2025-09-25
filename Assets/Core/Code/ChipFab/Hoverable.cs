using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class Hoverable : MonoBehaviour
    {
        public GameObject HoverGroup;

        public void BeginHover()
        {
            HoverGroup.SetActive(true);
        }

        public void EndHover()
        {
            HoverGroup.SetActive(false);
        }
    }
}