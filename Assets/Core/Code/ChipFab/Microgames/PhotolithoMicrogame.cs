using FieldDay;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class PhotolithoMicrogame : StationMicrogame, IStationMicrogame
    {
        public ClickBox MaskAButton;
        public ClickBox MaskBButton;
        public ClickBox MaskCButton;

        public ClickBox RotateCCButton;
        public ClickBox RotateCButton;

        public ClickBox DevelopButton;

        public TMP_Text RotText;

        public Transform PreviewPos;
        public GameObject PreviewPrefab;
        private GameObject m_currPreview;
        private SpriteRenderer m_currPreviewRenderer;

        private MaskId m_currSelectedMask = MaskId.NONE;
        private int m_currRotation = 0;

        private float m_rotateCooldown = 0.1f;
        private float m_cooldownTimer = 0;

        private bool m_autoRoutineStarted = false;


        public override void Activate(WaferState waferState)
        {
            base.Activate(waferState);

            MaskAButton.OnMouseDown.AddListener(HandleMaskADown);
            MaskBButton.OnMouseDown.AddListener(HandleMaskBDown);
            MaskCButton.OnMouseDown.AddListener(HandleMaskCDown);

            RotateCCButton.OnMouseDown.AddListener(HandleRotateCCDown);
            RotateCButton.OnMouseDown.AddListener(HandleRotateCDown);

            DevelopButton.OnMouseDown.AddListener(HandleDevelopDown);

            m_currSelectedMask = MaskId.NONE;
            m_currRotation = 0;

            DevelopButton.transform.parent.gameObject.SetActive(false);

            m_currPreview = Instantiate(PreviewPrefab, PreviewPos);
            m_currPreviewRenderer = m_currPreview.GetComponent<SpriteRenderer>();
            m_currPreviewRenderer.enabled = false;

            m_autoRoutineStarted = false;

            RotText.SetText("0°");

            // PREREQS: Resist FULL
            if (DragMgr.WaferInstance.Data.ResistLayer.State != ResistState.Full)
            {
                Debug.Log("Invalid Prereqs");
                Deactivate();
            }
        }

        public override void Deactivate()
        {
            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Photolithograph)
            {
                if (ControlsMgr.Instance.ConveyorEnabled)
                {
                    ConveyorMgr.Instance.TryReturnToConveyor();
                }
            }

            base.Deactivate();

            MaskAButton.OnMouseDown.RemoveAllListeners();
            MaskBButton.OnMouseDown.RemoveAllListeners();
            MaskCButton.OnMouseDown.RemoveAllListeners();

            RotateCCButton.OnMouseDown.RemoveAllListeners();
            RotateCButton.OnMouseDown.RemoveAllListeners();

            DevelopButton.OnMouseDown.RemoveAllListeners();
        }

        public override bool TryCancel()
        {
            Deactivate();
            return true;
        }

        #region Handlers

        private void Update()
        {
            if (m_cooldownTimer > 0)
            {
                m_cooldownTimer -= Time.deltaTime;
            }

            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Photolithograph)
            {
                if (!m_autoRoutineStarted)
                {
                    m_AutomationRoutine.Replace(AutomationRoutine());
                    m_autoRoutineStarted = true;
                }
            }
        }

        private IEnumerator AutomationRoutine()
        {
            yield return 0.5f;

            var instruction = AutomationMgr.Instance.CurrInstruction;
            switch (instruction.MaskToApply)
            {
                case MaskId.A:
                    HandleMaskADown();
                    break;
                case MaskId.B:
                    HandleMaskBDown();
                    break;
                case MaskId.C:
                    HandleMaskCDown();
                    break;
                default:
                    break;
            }

            yield return 0.5f;

            m_currRotation = -instruction.Rotation;
            if (m_currRotation < 0) { m_currRotation += 360; }
            else if (m_currRotation > 359) { m_currRotation -= 360; }
            SetRotation();

            yield return 0.5f;

            HandleDevelopDown();
        }

        private void HandleMaskADown()
        {
            m_currPreviewRenderer.enabled = true;
            m_currSelectedMask = MaskId.A;
            m_currPreviewRenderer.sprite = GameDB.Instance.MaskA;
            DevelopButton.transform.parent.gameObject.SetActive(true);
        }

        private void HandleMaskBDown()
        {
            m_currPreviewRenderer.enabled = true;
            m_currSelectedMask = MaskId.B;
            m_currPreviewRenderer.sprite = GameDB.Instance.MaskB;
            DevelopButton.transform.parent.gameObject.SetActive(true);
        }

        private void HandleMaskCDown()
        {
            m_currPreviewRenderer.enabled = true;
            m_currSelectedMask = MaskId.C;
            m_currPreviewRenderer.sprite = GameDB.Instance.MaskC;
            DevelopButton.transform.parent.gameObject.SetActive(true);
        }

        private void HandleRotateCCDown()
        {
            if (m_cooldownTimer > 0) { return; }

            m_cooldownTimer = m_rotateCooldown;

            m_currRotation += 90;

            if (m_currRotation == 360) { m_currRotation = 0; }

            SetRotation();
        }

        private void HandleRotateCDown()
        {
            if (m_cooldownTimer > 0) { return; }

            m_cooldownTimer = m_rotateCooldown;

            m_currRotation -= 90;

            if (m_currRotation == -360) { m_currRotation = 0; }

            SetRotation();
        }

        private void SetRotation()
        {
            var angles = DragMgr.WaferInstance.transform.localEulerAngles;
            angles.z = m_currRotation;
            DragMgr.WaferInstance.transform.localEulerAngles = angles;

            RotText.SetText(m_currRotation + "°");
        }

        private void HandleDevelopDown()
        {
            if (!m_currPreview) { return; }
            // pattern rotates inverse of wafer
            DragMgr.WaferInstance.SetPhotoState(m_currSelectedMask, -m_currRotation);
            m_currPreview.transform.SetParent(DragMgr.WaferInstance.transform, true);
            DragMgr.WaferInstance.LatestMaskRenderer = m_currPreviewRenderer;
            m_currPreviewRenderer.sortingOrder = m_currPreviewRenderer.sortingOrder + (++DragMgr.WaferInstance.NumLayers);
            m_currPreview.transform.localScale = Vector3.one;
            m_currPreview.transform.localPosition = Vector3.zero;
            m_currPreview = null;
            m_currPreviewRenderer = null;
            Deactivate();

            Game.Events.Dispatch(GameEvents.WaferStateUpdated);
        }

        #endregion // Handlers
    }
}