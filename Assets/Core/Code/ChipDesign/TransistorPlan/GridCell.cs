using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    #region Enums & Structs

    public enum EdgeState
    {
        Disconnected,
        Connected
    }

    public enum EdgeDirs
    {
        NORTH,
        EAST,
        SOUTH,
        WEST,
        ASCEND,
        DESCEND
    }

    public enum TransferType
    {
        NONE,
        Via,
        Gate
    }

    public enum CellType
    {
        Input,
        Metal,
        NTransistor,
        PTransistor,
        Output
    }

    #endregion // Enums & Structs

    public class GridCell : MonoBehaviour
    {
        public CellType CellType;
        public EdgeState[] Edges = new EdgeState[6]; // one for each edge dir
        public TransferType TransferType; // informs how data is transferred between layers when either ASCEND or DESCEND edges are connected
    }
}