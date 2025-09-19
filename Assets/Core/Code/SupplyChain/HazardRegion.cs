using BeauPools;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Components;
using System;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    [DisallowMultipleComponent]
    public sealed class HazardRegion : BatchedComponent {
        public HazardType Type;
        public int TariffCost;
        public PathNodeHighlight Highlight;
    }

    public enum HazardType {
        Risky,
        TimeDialation,
        Tariff,
    }
}