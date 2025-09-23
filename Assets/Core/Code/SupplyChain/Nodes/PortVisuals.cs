using System;
using System.Collections.Generic;
using BeauPools;
using BeauUtil;
using FieldDay;
using FieldDay.Components;
using FieldDay.Scenes;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class PortVisuals : BatchedComponent, IScenePreload {
        [Required] public Port Port;
        public SpriteRenderer Background;
        public SpriteRenderer Outline;
        public SpriteRenderer[] Icons;

        [NonSerialized] public PortDetailsDisplay CurrentDetails;

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            SupplyChainSprites sprites = Find.GlobalAsset<SupplyChainSprites>();
            switch (Port.Type) {
                case PortType.Supply: {
                    SupplyNode supply = Port.GetComponent<SupplyNode>();
                    Icons[0].sprite = sprites.MaterialSpriteTiny(supply.Material);
                    break;
                }
                case PortType.Conversion: {
                    ConversionNode conversion = Port.GetComponent<ConversionNode>();
                    Icons[0].sprite = sprites.MaterialSpriteTiny(conversion.Input);
                    Icons[1].sprite = sprites.MaterialSpriteTiny(conversion.Output);
                    break;
                }
            }
            return null;
        }
    }
}