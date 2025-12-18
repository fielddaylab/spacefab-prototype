using BeauUtil;
using FieldDay.Assets;
using System;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    [CreateAssetMenu(menuName = "SupplyChain/Level")]
    public sealed class SupplyChainLevel : NamedAsset {
        public string Label;
        public SceneReference Scene;

        [AssetName(typeof(RouteShip))] public StringHash32[] Ships;
        public FabMaterialSet RequiredMaterials;
        public int SellPrice = 10;
    }
}