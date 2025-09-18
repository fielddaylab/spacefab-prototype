using BeauPools;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Components;
using System;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class HazardRegion : BatchedComponent {
        public HazardType Type;
    }

    public enum HazardType {
        Risky,
        TimeDialation,
        Tariff,
    }
}