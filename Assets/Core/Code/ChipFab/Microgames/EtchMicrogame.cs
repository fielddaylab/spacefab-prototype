using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public enum EtchMicrogameState
    {
        Activated,
        Ready,
        Etching,
        Finished,
        Deactivated
    }

    public class EtchMicrogame : StationMicrogame, IStationMicrogame
    {
        private static KeyCode FIRE_KEY = KeyCode.Space;

        public Blaster Blaster;

        public ClickBox FinishButton;

        public SideWaferDisplay WaferDisplay;

        private EtchMicrogameState m_state;

        public GameObject BlastableResistPrefab;
        public GameObject UnblastableResistPrefab;
        public GameObject BlastableOxidePrefab;
        public GameObject UnblastableOxidePrefab;

        public Transform ParentFrame;

        public Transform LUnblast, RUnblast, LBlast, RBlast;

        private List<GameObject> m_generatedLayerBlocks = new List<GameObject>();

        private int m_totalBlastables;

        #region IStationMicrogame

        public override void Activate(WaferState waferState)
        {
            base.Activate(waferState);

            FinishButton.OnMouseDown.RemoveAllListeners();

            FinishButton.transform.parent.gameObject.SetActive(false);
            FinishButton.OnMouseDown.AddListener(HandleFinishClicked);

            DragMgr.Instance.DragWaferEnabled = false;

            WaferDisplay.UpdateDisplay(DragMgr.WaferInstance.Data);

            // PREREQS Resist DEVELOPED
            if (DragMgr.WaferInstance.Data.ResistLayer.State != ResistState.Developed)
            {
                Debug.Log("Invalid prereqs");
                DragMgr.Instance.DragWaferEnabled = true;
                Deactivate();
                return;
            }

            m_totalBlastables = 0;

            GenerateEtchableLayers(DragMgr.WaferInstance.Data);

            TransitionToActivated();
        }

        public override void Deactivate()
        {
            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Etch)
            {
                if (ControlsMgr.Instance.ConveyorEnabled)
                {
                    ConveyorMgr.Instance.TryReturnToConveyor();
                }
            }

            base.Deactivate();

            m_state = EtchMicrogameState.Deactivated;

            FinishButton.OnMouseDown.RemoveListener(HandleFinishClicked);
        }

        public override bool TryCancel()
        {
            return m_state == EtchMicrogameState.Deactivated;
        }

        #endregion // IStationMicrogame


        private void Update()
        {
            if (!Container.activeInHierarchy) { return; }
            
            switch (m_state)
            {
                case EtchMicrogameState.Activated:
                    TransitionToReady();
                    break;
                case EtchMicrogameState.Ready:
                    TransitionToEtching();
                    break;
                case EtchMicrogameState.Etching:
                    ProcessMicrogame();
                    break;
                case EtchMicrogameState.Finished:
                    break;
                default:
                    break;
            }
        }

        private void ProcessMicrogame()
        {
            if (AutomationMgr.Instance.CurrInstruction.Valid && AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Etch)
            {
                if (!m_AutomationRoutine.Exists())
                {
                    m_AutomationRoutine.Replace(AutomationRoutine());
                }
            }
            else
            {
                ProcessManual();
            }
        }

        private void ProcessManual()
        {
            if (Input.GetKey(FIRE_KEY))
            {
                Blaster.Blast();
            }
        }

        private IEnumerator AutomationRoutine()
        {
            Blaster.transform.eulerAngles = new Vector3(0, 0, -40);

            yield return 0.5f;

            int steps = 50;
            float amt = 80;
            float stepAmt = amt / steps;
            for (int i = 0; i < steps; i++)
            {
                Blaster.Blast(true);
                Blaster.transform.Rotate(new Vector3(0, 0, 1) * stepAmt);
                yield return 0.02f;
            }

            yield return 0.5f;

            for (int i = 0; i < steps; i++)
            {
                Blaster.Blast(true);
                Blaster.transform.Rotate(new Vector3(0, 0, -1) * stepAmt);
                yield return 0.02f;
            }

            yield return 0.5f;

            HandleFinishClicked();
        }

        private void TransitionToActivated()
        {
            m_state = EtchMicrogameState.Activated;
            TransitionCommon();
        }

        private void TransitionToReady()
        {
            m_state = EtchMicrogameState.Ready;
            TransitionCommon();
        }

        private void TransitionToEtching()
        {
            m_state = EtchMicrogameState.Etching;
            FinishButton.transform.parent.gameObject.SetActive(true);
            TransitionCommon();
        }

        private void TransitionCommon()
        {

        }

        private float EvaluatePrecision()
        {
            int hitCount = 0;

            foreach (var obj in m_generatedLayerBlocks)
            {
                if (obj == null)
                {
                    hitCount++;
                }
            }

            return (float)hitCount / m_totalBlastables;
        }

        private void HandleFinishClicked()
        {
            var precision = EvaluatePrecision();
            DragMgr.Instance.DragWaferEnabled = true;
            if (DragMgr.WaferInstance.Data.MetallizationLayer.State == MetallizationState.Full)
            {
                DragMgr.WaferInstance.SetMetallizationStateEtch(precision);

            }
            else if (DragMgr.WaferInstance.Data.OxideLayer.State == OxideState.Full)
            {
                DragMgr.WaferInstance.SetOxideStateEtch(precision);
            }
            Deactivate();
            Game.Events.Dispatch(GameEvents.WaferStateUpdated);

            while (m_generatedLayerBlocks.Count > 0)
            {
                if (m_generatedLayerBlocks[0] != null)
                {
                    Destroy(m_generatedLayerBlocks[0]);
                }
                m_generatedLayerBlocks.RemoveAt(0);
            }
            m_generatedLayerBlocks.Clear();
        }

        private void GenerateEtchableLayers(WaferData data)
        {
            if (m_generatedLayerBlocks.Count > 0)
            {
                while (m_generatedLayerBlocks.Count > 0)
                {
                    if (m_generatedLayerBlocks[0] != null)
                    {
                        Destroy(m_generatedLayerBlocks[0]);
                    }
                    m_generatedLayerBlocks.RemoveAt(0);
                }
                m_generatedLayerBlocks.Clear();
            }

            // Resist Layer
            switch (data.ResistLayer.State)
            {
                case ResistState.Developed:
                    WaferDisplay.Resist.gameObject.SetActive(false);
                    // generate left unblastable
                    var newObj = Instantiate(UnblastableResistPrefab, ParentFrame);
                    var objPos = newObj.transform.position;
                    objPos.x = LUnblast.position.x;
                    objPos.y = WaferDisplay.Resist.transform.position.y;
                    newObj.transform.position = objPos;
                    m_generatedLayerBlocks.Add(newObj);

                    // generate right unblastable
                    newObj = Instantiate(UnblastableResistPrefab, ParentFrame);
                    objPos = newObj.transform.position;
                    objPos.x = RUnblast.position.x;
                    objPos.y = WaferDisplay.Resist.transform.position.y;
                    newObj.transform.position = objPos;
                    m_generatedLayerBlocks.Add(newObj);

                    // generate blastable
                    GenerateBlastables(BlastableResistPrefab, LBlast.position.x, RBlast.position.x, WaferDisplay.Resist.transform.position.y);
                    break;
                default:
                    break;
            }

            // Oxide Layer
            switch (data.OxideLayer.State)
            {
                case OxideState.Full:
                    WaferDisplay.Oxide.gameObject.SetActive(false);
                    // generate left unblastable
                    var newObj = Instantiate(UnblastableOxidePrefab, ParentFrame);
                    var objPos = newObj.transform.position;
                    objPos.x = LUnblast.position.x;
                    objPos.y = WaferDisplay.Oxide.transform.position.y;
                    newObj.transform.position = objPos;
                    m_generatedLayerBlocks.Add(newObj);

                    // generate right unblastable
                    newObj = Instantiate(UnblastableOxidePrefab, ParentFrame);
                    objPos = newObj.transform.position;
                    objPos.x = RUnblast.position.x;
                    objPos.y = WaferDisplay.Oxide.transform.position.y;
                    newObj.transform.position = objPos;
                    m_generatedLayerBlocks.Add(newObj);

                    // generate blastable
                    GenerateBlastables(BlastableOxidePrefab, LBlast.position.x, RBlast.position.x, WaferDisplay.Oxide.transform.position.y);
                    break;
                default:
                    break;
            }
        }

        private void GenerateBlastables(GameObject prefab, float leftX, float rightX, float y)
        {
            float step = 0.0441607297114818f / 2;
            float currX = leftX;
            int lastI = 0;
            for (int i = 0; leftX + i * step < rightX; i++)
            {
                currX = leftX + i * step;

                var newObj = Instantiate(prefab, ParentFrame);
                var objPos = newObj.transform.position;
                objPos.x = currX;
                objPos.y = y;
                newObj.transform.position = objPos;
                m_generatedLayerBlocks.Add(newObj);

                lastI = i + 1;
            }

            m_totalBlastables += lastI;
        }
    }
}