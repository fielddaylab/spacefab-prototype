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
        GateAbove,
        GateBelow,
        Implicit // Input/Output to Metal
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

        public FlowState FlowState;

        #region Loading

        public void LoadCellConfig(GridCellConfig config)
        {
            CellType = config.CellType;
            SubtypeLabel = EvalUtility.GetSubtypeByPlacableID(config.SubtypeLabel);
            Edges = config.Edges;
            if (config.Edges.Length == 0)
            {
                Edges = new EdgeState[6];
            }
            else if (config.Edges.Length != 6) { Debug.LogError("[CellConfig] config does not have 6 edges!"); }
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
            TransferType = TransferType.NONE;

            for (int i = 0; i < Edges.Length; i++)
            {
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

            if (dir == EdgeDir.ASCEND || dir == EdgeDir.DESCEND)
            {
                TransferType = TransferType.NONE;
            }
        }
    }

    public static class EdgeUtility
    {
        /// <summary>
        /// Condenses 6 edge states to the 4 cardinal directions
        /// </summary>
        /// <param name="toCondense"></param>
        /// <returns></returns>
        public static EdgeState[] CondenseEdges(EdgeState[] toCondense)
        {
            if (toCondense.Length != 6) { 
                Debug.LogError("[EdgeUtility] unable to convert edges of length other than 6!");
                return null;
            }

            EdgeState[] condensed = new EdgeState[4];
            condensed[0] = toCondense[0]; // North
            condensed[1] = toCondense[1]; // East
            condensed[2] = toCondense[3]; // South
            condensed[3] = toCondense[4]; // West

            return condensed;
        }

        public static int NumConnections(EdgeState[] edges)
        {
            int num = 0;

            for (int i = 0; i < edges.Length; i++)
            {
                if (edges[i] == EdgeState.Connected)
                {
                    num++;
                }
            }

            return num;
        }
    }
}