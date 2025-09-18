using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Audio;
using FieldDay.Components;
using FieldDay.HID;
using FieldDay.Scenes;
using FieldDay.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class ResearchToolButton : BatchedComponent, IScenePreload {
        public SpriteRenderer Image;
        public Collider2D Region;
        public CursorHint Cursor;
        public ResearchTool Tool;

        [Header("Colors")]
        public Color32 UnselectedColor;
        public Color32 SelectedColor;

        [NonSerialized] public bool Locked;

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            Cursor.onClick.Register(() => {
                Sfx.Play("Research.Tool.Switch");
                ResearchToolUtility.SetCurrentTool(Tool);
            });
            return null;
        }
    }

    static public partial class ResearchToolUtility {
    }
}