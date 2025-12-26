using BeauRoutine;
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
            SetCamPosImmediate(DefaultPos.Pos);
        }

        public void ToggleAlwaysZoomed()
        {
            AlwaysZoomed = !AlwaysZoomed;
        }

        public void LoadCamPosImmediate(CamPos pos)
        {
            SetCamPosImmediate(pos);
        }

        public IEnumerator LoadCamPosRoutine(CamPos pos, float time)
        {
            yield return SetCamPosRoutine(pos, time);
        }

        public void UnloadCamPosImmediate(CamPos pos)
        {
            if (CurrPos.Pos == pos.Pos && CurrPos.ViewSize == pos.ViewSize)
            {
                SetCamPosImmediate(DefaultPos.Pos);
            }
        }

        public IEnumerator UnloadCamPosRoutine(CamPos pos, float time)
        {
            if (CurrPos.Pos == pos.Pos && CurrPos.ViewSize == pos.ViewSize)
            {
                yield return SetCamPosRoutine(DefaultPos.Pos, time);
            }
        }

        private IEnumerator SetCamPosRoutine(CamPos pos, float time)
        {
            yield return Routine.Combine(
                Camera.transform.MoveTo(pos.Pos.position, time)
                );

            Camera.orthographicSize = pos.ViewSize;
            CurrPos = pos;

            yield return null;
        }

        private void SetCamPosImmediate(CamPos pos)
        {
            Camera.transform.position = pos.Pos.position;
            Camera.orthographicSize = pos.ViewSize;
            CurrPos = pos;
        }
    }
}