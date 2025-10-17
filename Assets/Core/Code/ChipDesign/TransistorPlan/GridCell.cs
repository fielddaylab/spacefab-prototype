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

    public enum EdgeDir
    {
        NORTH,
        EAST,
        ASCEND,
        SOUTH,
        WEST,
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
        NONE,
        Input,
        Metal,
        NTransistor,
        PTransistor,
        Output
    }

    #endregion // Enums & Structs

    public class GridCell
    {
        public CellType CellType;
        public string SubtypeLabel;
        public EdgeState[] Edges = new EdgeState[6]; // one for each edge dir
        public TransferType TransferType; // informs how data is transferred between layers when either ASCEND or DESCEND edges are connected

        #region Loading

        public void LoadCellConfig(GridCellConfig config)
        {
            CellType = config.CellType;
            Edges = config.Edges;
            if (config.Edges.Length != 6) { Debug.LogError("[CellConfig] config does not have 6 edges!"); }
            TransferType = config.TransferType;
        }

        #endregion // Loading

        /// <summary>
        /// Returns a list of danglingEdges
        /// </summary>
        /// <returns></returns>
        public void Erase(out List<EdgeDir> danglingEdges)
        {
            danglingEdges = new List<EdgeDir>();

            CellType = CellType.NONE;
            SubtypeLabel = default;

            for (int i = 0; i < Edges.Length; i++)
            {
                // TODO: remove reciprocal edges
                if (Edges[i] == EdgeState.Connected)
                {
                    danglingEdges.Add((EdgeDir)i);
                }

                Edges[i] = EdgeState.Disconnected;
            }
            TransferType = TransferType.NONE;
        }

        public void EraseEdge(EdgeDir dir)
        {
            Edges[(int)dir] = EdgeState.Disconnected;
        }
    }
}