using BeauPools;
using BeauUtil;
using FieldDay.SharedState;
using FieldDay.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.SupplyChain {
    public sealed class RouteDrawerUI : SharedPanel {
        public CanvasGroup Group;
        public Graphic Background;

        public override void Show() {
            Group.gameObject.SetActive(true);
        }

        public override void Hide() {
            Group.gameObject.SetActive(false);
        }
    }
}