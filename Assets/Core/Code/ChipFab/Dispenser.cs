using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class Dispenser : MonoBehaviour
    {
        public GameObject ToDispense;
        public ClickBox ClickBox;

        private void Start()
        {
            ClickBox.OnMouseDown.AddListener(HandleMouseDown);
        }

        private void HandleMouseDown()
        {
            var newObj = Instantiate(ToDispense);
            DragMgr.Instance.SetCurrDrag(newObj.transform);
        }
    }
}