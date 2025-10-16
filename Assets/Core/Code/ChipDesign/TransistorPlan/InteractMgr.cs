using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    public class InteractMgr : MonoBehaviour
    {
        // Inputs in grid start at 0, 0 in the bottom left. Row indices increase from bottom to top.

        private void Update()
        {
            ProcessInputs();
        }

        private void ProcessInputs()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // get mouse position in world space
                var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var gridPos = new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));

                Debug.Log("Mouse Coords: (" + gridPos.x + " , " + gridPos.y + ")");
            }
        }
    }
}