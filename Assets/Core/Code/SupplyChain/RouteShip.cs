using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Assets;
using FieldDay.Components;
using FieldDay.SharedState;
using System;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    [CreateAssetMenu(menuName = "SupplyChain/Ship")]
    public sealed class RouteShip : NamedAsset {
        public string DisplayName;
        public Sprite Icon;

        [Header("Stats")]
        [Range(1, 4)] public int Speed;
        [Range(1, 4)] public int Capacity;
        [Range(1, 4)] public int Defense;
        [Range(1, 4)] public int Cost;
    }
}