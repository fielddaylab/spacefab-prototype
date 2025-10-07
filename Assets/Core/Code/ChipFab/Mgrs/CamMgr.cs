using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    [Serializable]
    public struct CamPos
    {
        public Transform Pos;
        public float ViewSize;
    }

    public class CamMgr : MonoBehaviour
    {
        public static CamMgr Instance;

        public Camera Camera;
        public CamPositioner DefaultPos;
        public bool AlwaysZoomed;

        private CamPos CurrPos;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SetCamPos(DefaultPos.Pos);
        }

        public void ToggleAlwaysZoomed()
        {
            AlwaysZoomed = !AlwaysZoomed;
        }

        public void LoadCamPos(CamPos pos)
        {
            SetCamPos(pos);
        }

        public void UnloadCamPos(CamPos pos)
        {
            if (CurrPos.Pos == pos.Pos && CurrPos.ViewSize == pos.ViewSize)
            {
                SetCamPos(DefaultPos.Pos);
            }
        }

        private void SetCamPos(CamPos pos)
        {
            Camera.transform.position = pos.Pos.position;
            Camera.orthographicSize = pos.ViewSize;
            CurrPos = pos;
        }
    }
}