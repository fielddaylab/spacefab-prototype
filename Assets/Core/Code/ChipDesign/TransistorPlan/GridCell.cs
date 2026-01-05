using System;
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

    [Serializable]
    public struct EdgeStateData
    {
        public EdgeState EdgeState;
        [HideInInspector] public bool Eraseable;

        public EdgeStateData(EdgeState state)
        {
            EdgeState = state;
            Eraseable = true;
        }

        public void Init()
        {
            EdgeState = EdgeState.Disconnected;
            Eraseable = true;
        }
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
        public EdgeStateData[] Edges = new EdgeStateData[6]; // one for each edge dir
        public TransferType TransferType; // informs how data is transferred between layers when either ASCEND or DESCEND edges are connected

        public FlowState FlowState;

        public CellType TempTransformation;

        public bool NodeEraseable = true;
        public bool TransferEraseable = true;

        #region Loading

        public void LoadCellConfig(GridCellConfig config)
        {
            CellType = config.CellType;
            SubtypeLabel = EvalUtility.GetSubtypeByPlacableID(config.SubtypeLabel);
            Edges = new EdgeStateData[6];
            if (config.Edges != null)
            {
                for (int e = 0; e < config.Edges.Length; e++)
                {
                    Edges[e] = config.Edges[e];
                }
            }

            if (CellType != CellType.NONE)
            {
                NodeEraseable = false; // pre-loaded nodes not erasable
            }
            if (config.TransferType != TransferType.NONE)
            {
                TransferEraseable = false; // pre-loaded nodes not erasable
            }
            TransferType = config.TransferType;

            if (config.Edges == null || config.Edges.Length == 0)
            {
                for (int i = 0; i < Edges.Length; i++)
                {
                    Edges[i].Init();
                }
            }
            else if (config.Edges.Length != 6) { Debug.LogError("[CellConfig] config does not have 6 edges!"); }
            else
            {
                for (int i = 0; i < Edges.Length; i++)
                {
                    // set connected edges to non-eraseable
                    Edges[i].Eraseable = Edges[i].EdgeState != EdgeState.Connected;
                }
            }
        }

        public void InitEdges()
        {
            for (int i = 0; i < Edges.Length; i++)
            {
                Edges[i].Eraseable = true;
            }
        }

        #endregion // Loading

        /// <summary>
        /// Returns a list of danglingEdges
        /// </summary>
        /// <returns></returns>
        public void Erase(out List<EdgeDir> danglingEdges)
        {
            danglingEdges = new List<EdgeDir>();

            for (int i = 0; i < Edges.Length; i++)
            {
                if (Edges[i].EdgeState == EdgeState.Connected)
                {
                    danglingEdges.Add((EdgeDir)i);
                }

                if (Edges[i].Eraseable) { 
                    Edges[i].EdgeState = EdgeState.Disconnected;
                }
            }

            if (NodeEraseable)
            {
                CellType = CellType.NONE;
                SubtypeLabel = default;
            }

            if (TransferEraseable)
            {
                TransferType = TransferType.NONE;
            }
        }

        public void EraseEdge(EdgeDir dir)
        {
            if (!Edges[(int)dir].Eraseable) { return; }

            Edges[(int)dir].EdgeState = EdgeState.Disconnected;

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
        public static EdgeState[] CondenseEdges(EdgeStateData[] toCondense)
        {
            if (toCondense.Length != 6) { 
                Debug.LogError("[EdgeUtility] unable to convert edges of length other than 6!");
                return null;
            }

            EdgeState[] condensed = new EdgeState[4];
            condensed[0] = toCondense[0].EdgeState; // North
            condensed[1] = toCondense[1].EdgeState; // East
            condensed[2] = toCondense[3].EdgeState; // South
            condensed[3] = toCondense[4].EdgeState; // West

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