using FieldDay;
using SpaceFab.ChipFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaferVersionMgr : MonoBehaviour
{
    public static WaferVersionMgr Instance;

    public ClickBox UndoButton;

    [HideInInspector] public List<WaferData> WaferHistory = new List<WaferData>();

    private void Awake()
    {
        Instance = this;

        Game.Events.Register(GameEvents.NewWaferCreated, HandleNewWaferCreated);
        Game.Events.Register(GameEvents.WaferStateUpdated, HandleWaferStateUpdated);
        UndoButton.OnMouseDown.AddListener(Undo);
    }

    public void ResetHistory()
    {
        WaferHistory.Clear();
    }

    public void Undo()
    {
        if (WaferHistory.Count > 0)
        {
            var index = WaferHistory.Count - 1;
            if (index > 0) {
                WaferHistory.RemoveAt(WaferHistory.Count - 1);
            }
            DragMgr.WaferInstance.Data = WaferHistory[WaferHistory.Count - 1];
            // DragMgr.WaferInstance.LatestMaskRenderer.enabled = false;
            Game.Events.Dispatch(GameEvents.WaferStateUndone);
        }
    }

    private void HandleNewWaferCreated()
    {
        ResetHistory();
        WaferHistory.Add(DragMgr.WaferInstance.Data);
    }

    private void HandleWaferStateUpdated()
    {
        if (DragMgr.WaferInstance == null) { return; }

        WaferHistory.Add(DragMgr.WaferInstance.Data);
    }
}
