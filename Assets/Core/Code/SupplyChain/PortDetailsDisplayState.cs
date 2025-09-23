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
            if (mode == PortDetailsMode.Off) {
                foreach(var port in displayer.Node.Ports) {
                    Pool.TryFree(port.Visuals.CurrentDetails);
                    port.Visuals.CurrentDetails = null;
                }
            } else {
                foreach(var port in displayer.Node.Ports) {
                    if (!port.Visuals.CurrentDetails) {
                        PortDetailsDisplay details = AllocateDisplayForPort(pools, port);
                        MoveDisplayToBestLocation(details, port.transform);
                        PopulatePortDetails(details, port, sprites);
                        port.Visuals.CurrentDetails = details;
                    }

                    port.Visuals.CurrentDetails.Cursor.enabled = mode == PortDetailsMode.Interactive;
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

        static public void MoveDisplayToBestLocation(PortDetailsDisplay display, Transform portPosition) {
            Vector2 localOffset = portPosition.localPosition;
            localOffset.Normalize();

            localOffset *= display.Size / 2;

            display.transform.SetPosition(portPosition.position + (Vector3) localOffset, Axis.XY);
        }

        static public void PopulatePortDetails(PortDetailsDisplay display, Port source, SupplyChainSprites sprites) {
            display.Parent = source;
            
            RouteNode route = source.GetComponent<RouteNode>();
            display.DisplayName.SetText(route.DisplayName);
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

                psb.Builder.Clear().AppendNoAlloc(route.ProductionTime).Append("C:");
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