using SpaceFab.ChipDesign;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    [Serializable]
    public struct GridCellConfig
    {
        public int LayerIndex;
        public int RowIndex;
        public int ColumnIndex;

        public CellType CellType;
        public Placeable SubtypeLabel;
        public EdgeStateData[] Edges;
        public TransferType TransferType; // informs how data is transferred between layers when either ASCEND or DESCEND edges are connected
    }

    [CreateAssetMenu(menuName = "Chip Design/Grid Stack Config")]
    public class GridStackConfig : ScriptableObject
    {
        public Dimensions LayerDims;
        public GridCellConfig[] Cells;
    }
}