using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class EtchProjectileCollisionHandler : ProjectileCollisionHandlerBase
    {
        public override void HandleCollision(Collision2D collision)
        {
            base.HandleCollision(collision);

            var layer = collision.gameObject.GetComponent<WaferLayer>();

            if (layer)
            {
                switch (layer.LayerType)
                {
                    case WaferLayerType.ResistUndeveloped:
                        // blast
                        Destroy(collision.gameObject);
                        Destroy(this.gameObject);
                        break;
                    case WaferLayerType.ResistDeveloped:
                        // nothing
                        break;
                    case WaferLayerType.Oxide:
                        // blast
                        Destroy(collision.gameObject);
                        Destroy(this.gameObject);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}