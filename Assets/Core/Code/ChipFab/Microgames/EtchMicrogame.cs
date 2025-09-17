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

            GenerateEtchableLayers(DragMgr.WaferInstance.Data);

            TransitionToActivated();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            m_state = EtchMicrogameState.Deactivated;

            FinishButton.OnMouseDown.RemoveListener(HandleFinishClicked);
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
            // TODO: finish implementation

            if (Input.GetKey(FIRE_KEY))
            {
                Blaster.Blast();
            }
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
                    break;
                default:
                    break;
            }
        }
    }
}