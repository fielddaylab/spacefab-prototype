using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Components;
using System;
using UnityEngine;

namespace SpaceFab.SupplyChain {
    public sealed class PortDetailsDisplayState : BatchedComponent {
        public PathNode Node;
        public PathNodeHighlight Highlight;
        public Vector2 TooltipOffset = new Vector2(0, 1.5f);

        [NonSerialized] public PortDetailsMode CurrentMode;
    }

    public enum PortDetailsMode {
        Off,
        Hover,
        Interactive
    }

    static public partial class PortUtility {
        static public void SetDetailsMode(PortDetailsDisplayState displayer, PortDetailsMode mode, PortPools pools, SupplyChainSprites sprites) {
            if (displayer.CurrentMode == mode) {
                return;
            }

            displayer.CurrentMode = mode;
            var port = displayer.Node.Port;
            if (port) {
                if (mode == PortDetailsMode.Off) {
                    Pool.TryFree(port.Visuals.CurrentDetails);
                    port.Visuals.CurrentDetails = null;
                } else {
                    if (!port.Visuals.CurrentDetails) {
                        PortDetailsDisplay details = AllocateDisplayForPort(pools, port);
                        MoveDisplayToBestLocation(details, port.transform, displayer);
                        PopulatePortDetails(details, port, sprites);
                        port.Visuals.CurrentDetails = details;
                    }
                }
            }
        }

        static public PortDetailsDisplay AllocateDisplayForPort(PortPools pools, Port port) {
            switch (port.Type) {
                case PortType.Supply:
                    return pools.SupplyPort.Alloc();
                case PortType.Conversion:
                    return pools.ConversionPort.Alloc();
                default:
                    Assert.Fail("cannot allocate display for port");
                    return null;
            }
        }

        static public void MoveDisplayToBestLocation(PortDetailsDisplay display, Transform portPosition, PortDetailsDisplayState portState) {
            Vector2 localOffset = portState.TooltipOffset;
            display.transform.SetPosition(portPosition.position + (Vector3) localOffset, Axis.XY);
        }

        static public void PopulatePortDetails(PortDetailsDisplay display, Port source, SupplyChainSprites sprites) {
            display.Parent = source;
            
            RouteNode route = source.GetComponent<RouteNode>();
            display.Defense.sprite = sprites.DefenseSprite((int) route.Reliability);

            switch (source.Type) {
                case PortType.Supply: {
                    display.Materials[0].sprite = sprites.MaterialSprite(source.GetComponent<SupplyNode>().Material);
                    break;
                }
                case PortType.Conversion: {
                    ConversionNode conversion = source.GetComponent<ConversionNode>();
                    display.Materials[0].sprite = sprites.MaterialSprite(conversion.Input);
                    display.Materials[1].sprite = sprites.MaterialSprite(conversion.Output);
                    break;
                }
            }

            using(PooledStringBuilder psb = PooledStringBuilder.Create()) {
                psb.Builder.Append('$').AppendNoAlloc(route.Cost);
                display.Cost.SetText(psb);

                psb.Builder.Clear().AppendNoAlloc(route.ProductionTime);
                display.Time.SetText(psb);
            }

            if (source.Owner != null) {
                display.Outline.color = source.Owner.LineColor;
                display.Outline.enabled = true;
            } else {
                display.Outline.enabled = false;
            }
        }
    }
}