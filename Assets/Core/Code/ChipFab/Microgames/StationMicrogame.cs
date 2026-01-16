using BeauRoutine;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.ChipFab
{
    public enum StationId
    {
        Furnace,
        Photolithograph,
        Resist,
        Sputter,
        Etch,
        Wash
    }

    public abstract class StationMicrogame : MonoBehaviour, IStationMicrogame
    {
        public GameObject Container;
        public ClickBox ActivateButton;
        public GameObject ActivateGroup;
        public CamPositioner CamPos;

        public bool IsCurrentSessionAutomated;

        protected Routine m_AutomationRoutine;

        protected virtual void Start()
        {
            if (Container)
            {
                Container.SetActive(false);
            }

            if (ActivateGroup)
            {
                ActivateGroup.SetActive(false);
                ActivateButton.OnMouseDown.AddListener(HandleActivateClicked);
            }
        }

        protected void OnDestroy()
        {
            if (Game.IsShuttingDown) { return; }

            ActivateButton.OnMouseDown.RemoveListener(HandleActivateClicked);
        }

        public virtual void Activate(WaferState waferState, bool isAutomated)
        {
            // CamMgr.Instance.LoadCamPosImmediate(CamPos.Pos);
            IsCurrentSessionAutomated = isAutomated;

            if (Container && !isAutomated)
            {
                Container.SetActive(true);
            }

            if (ActivateButton)
            {
                ActivateButton.gameObject.SetActive(false);
            }
        }

        public virtual void Deactivate()
        {
            if (Container)
            {
                Container.SetActive(false);
            }

            if (AutomationMgr.Instance.CurrInstruction.Valid)
            {
                Game.Events.Dispatch(GameEvents.AutomationCompleted);
            }

            if (ActivateButton)
            {
                ActivateButton.gameObject.SetActive(true);
            }
        }

        public abstract bool TryCancel();



        #region Handlers

        private void HandleActivateClicked()
        {
            if (TimeMgr.Instance.IsRunning())
            {
                if (ControlsMgr.Instance && ControlsMgr.Instance.BotInstance)
                {
                    ControlsMgr.Instance.BotInstance.TryActivateCurrStation(false);
                }
            }
        }

        #endregion Handlers
    }
}
