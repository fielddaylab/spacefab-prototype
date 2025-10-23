using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace SpaceFab.ChipDesign
{
    public sealed class PathPrefabData : MonoBehaviour
    {
#if UNITY_EDITOR

        public Sprite Sprite;
        public EdgeState[] Edges;
        public float Radius = 1;

        private void Reset()
        {
            // Mesh = GetComponent<MeshFilter>().sharedMesh;
        }

        [CustomEditor(typeof(PathPrefabData))]
        private class Inspector : Editor
        {
            private void OnSceneGUI()
            {
                PathPrefabData prefabData = target as PathPrefabData;
                /*
                if (!FieldDay.Frame.IsActive(prefabData))
                {
                    return;
                }
                */

                Vector3 currentPos = prefabData.transform.position;
                float newRadius = Handles.RadiusHandle(Quaternion.identity, currentPos, prefabData.Radius);
                Update(prefabData, ref prefabData.Radius, newRadius);

                EdgeState[] edges = prefabData.Edges;

                for (int dir = 0; dir < 4; dir++)
                {
                    Vector3 offset = Vector3.zero;
                    if (dir == 0) { offset = currentPos + new Vector3(0, 1, 0) * (newRadius + 0.2f); }
                    if (dir == 1) { offset = currentPos + new Vector3(1, 0, 0) * (newRadius + 0.2f); }
                    if (dir == 2) { offset = currentPos + new Vector3(0, -1, 0) * (newRadius + 0.2f); }
                    if (dir == 3) { offset = currentPos + new Vector3(-1, 0, 0) * (newRadius + 0.2f); }

                    Handles.color = edges[dir] == EdgeState.Connected ? Color.yellow : Color.white;
                    if (Handles.Button(offset, Quaternion.identity, 0.1f, 0.1f, Handles.SphereHandleCap))
                    {
                        if (edges[dir] == EdgeState.Connected)
                        {
                            edges[dir] = EdgeState.Disconnected;
                        }
                        else
                        {
                            edges[dir] = EdgeState.Connected;
                        }
                    }
                }

                Update(prefabData, ref prefabData.Edges, edges);
            }

            static private void Update<T>(UnityEngine.Object host, ref T val, in T newVal)
            {
                Undo.RecordObject(host, "Updating property");
                EditorUtility.SetDirty(host);
                val = newVal;
            }
        }

#endif // UNITY_EDITOR
    }
}