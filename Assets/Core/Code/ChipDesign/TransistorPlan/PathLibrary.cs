using System;
using System.Collections.Generic;
using System.IO;
using BeauUtil;
using BeauUtil.Debugger;
using UnityEditor;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    [CreateAssetMenu(menuName = "Chip Design/Path Library")]
    public sealed class PathLibrary : ScriptableObject
    {
        [Serializable]
        private struct RotationEntry
        {
            public List<ushort> PathIndices;
            public List<ushort> PathRotations;
            // public ushort Rotation;
        }

        [Serializable]
        private struct PathData
        {
            public Sprite Sprite;
            public Quaternion Rotation;
            public float Scale;
            public bool FlipX;
        }

        public struct AssembledPathData
        {
            public Sprite Sprite;
            public Quaternion Rotation;
            public Vector3 Scale;
            public int Turns;
        }

        [SerializeField] private float m_RadiusReference = 0.43f;
        [SerializeField] private Sprite m_ErrorSprite = null;

        [Space]
        [SerializeField] private RotationEntry[] m_RotationEntries = new RotationEntry[16];
        [SerializeField] private PathData[] m_Paths = new PathData[16];

        static public readonly ActionEvent OnUpdated = new ActionEvent(16);

        public bool Lookup(EdgeState[] inEdges, out AssembledPathData pathData)
        {
            // TODO: convert inEdges from 6 to 4
            int lookup = 0;
            for (int i = 0; i < 4; i++)
            {
                if (inEdges[i] == EdgeState.Disconnected)
                {
                    lookup += 1 << i;
                }
            }

            RotationEntry rotData = m_RotationEntries[lookup];
            if (rotData.PathIndices.Count == 0)
            {
                Log.Warn("[PathLibrary] No path data available for edges");
                pathData = new AssembledPathData()
                {
                    Sprite = m_ErrorSprite,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    Turns = 0
                };
                return false;
            }

            // randomly select from available options
            int randIdx = UnityEngine.Random.Range(0, rotData.PathIndices.Count);
            PathData fData = m_Paths[rotData.PathIndices[randIdx]];

            pathData.Sprite = fData.Sprite;
            pathData.Rotation = Quaternion.identity;
            // filigreeData.Rotation = HexGrid.RotateQuaternion(tileData.Rotation, rotData.Rotation);
            pathData.Scale = new Vector3(fData.Scale, fData.Scale, fData.Scale);
            if (fData.FlipX)
            {
                pathData.Scale.x = -pathData.Scale.x;
            }
            pathData.Turns = rotData.PathRotations[randIdx];
            return true;
        }


#if UNITY_EDITOR

        [CustomEditor(typeof(PathLibrary))]
        private class Inspector : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                PathLibrary lib = target as PathLibrary;

                if (GUILayout.Button("Rebuild"))
                {
                    lib.Build();
                }

                if (GUILayout.Button("Refresh"))
                {
                    OnUpdated.Invoke();
                }
            }
        }

        [ContextMenu("Refresh")]
        private void Build()
        {
            Undo.RecordObject(this, "rebuilding path data");
            EditorUtility.SetDirty(this);

            PathPrefabData[] allPrefabs = FindAllFiligreePrefabs(Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)));
            Construct(allPrefabs, out m_RotationEntries, out m_Paths, m_RadiusReference);
        }

        static private PathPrefabData[] FindAllFiligreePrefabs(string directory)
        {
            string[] assetGuids = AssetDatabase.FindAssets("t:GameObject", new string[] { directory });
            List<PathPrefabData> prefabs = new List<PathPrefabData>(assetGuids.Length);
            foreach (var guid in assetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (obj && obj.TryGetComponent(out PathPrefabData component))
                {
                    prefabs.Add(component);
                }
            }
            return prefabs.ToArray();
        }

        static private void Construct(PathPrefabData[] prefabDatas, out RotationEntry[] outRotations, out PathData[] outPaths, float radiusReference)
        {
            outPaths = new PathData[prefabDatas.Length];
            outRotations = new RotationEntry[16];

            HashSet<int> untouchedMasks = new HashSet<int>(16);
            for (int i = 0; i < 16; i++)
            {
                outRotations[i].PathIndices = new List<ushort>();
                outRotations[i].PathRotations = new List<ushort>();
                if (i > 0)
                {
                    untouchedMasks.Add(i);
                }
            }

            int pathIndex = 0;

            try
            {
                foreach (var data in prefabDatas)
                {
                    EditorUtility.DisplayProgressBar("Analyzing Paths", data.gameObject.name, pathIndex / (float)prefabDatas.Length);
                    Log.Msg("Analyzing road {0}", data.gameObject.name);

                    int shiftedMask = 0;

                    for (int i = 0; i < 4; i++)
                    {
                        if (data.Edges[i] == EdgeState.Disconnected)
                        {
                            shiftedMask += 1 << i;
                        }
                    }

                    outPaths[pathIndex] = new PathData()
                    {
                        Sprite = data.Sprite,
                        Rotation = data.transform.localRotation,
                        Scale = radiusReference / data.Radius,
                        FlipX = data.transform.localScale.x < 0
                    };

                    int rotCount = 0;
                    int appliedCount = 0;
                    while (rotCount < 4)
                    {
                        ref RotationEntry entry = ref outRotations[shiftedMask];
                        entry.PathIndices.Add((ushort)pathIndex);
                        entry.PathRotations.Add((ushort)rotCount);
                        // entry.Rotation = (ushort)rotCount; // per filigree needed?
                        outRotations[shiftedMask] = entry;
                        appliedCount++;

                        untouchedMasks.Remove(shiftedMask);

                        shiftedMask = Rotate(shiftedMask);
                        rotCount++;
                    }

                    Log.Msg("filled {0} entries", appliedCount);
                    pathIndex++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (untouchedMasks.Count > 0)
            {
                Log.Warn("{0} untouched masks", untouchedMasks.Count);
                foreach (var mask in untouchedMasks)
                {
                    // Log.Warn(MaskToString(mask));
                }
            }

            Log.Msg("Path analysis complete");
        }

        private const int BitCount = 4;
        private const int BitMask = (1 << BitCount) - 1;

        /*
        static private string MaskToString(int value)
        {
            return new TileAdjacencyMask(value << 1).ToString();
        }
        */

        static private int Rotate(int value)
        {
            return ((value >> 1) | (value << (BitCount - 1))) & BitMask;
        }

#endif // UNITY_EDITOR
    }
}