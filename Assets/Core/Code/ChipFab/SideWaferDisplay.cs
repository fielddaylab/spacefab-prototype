using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class SideWaferDisplay : MonoBehaviour
    {
        public SpriteRenderer Resist;
        public SpriteRenderer Metallization;
        public SpriteRenderer Oxide;
        public SpriteRenderer Semiconductor;

        public void UpdateDisplay(WaferData data)
        {
            // Resist Layer
            switch (data.ResistLayer.State)
            {
                case ResistState.Empty:
                    Resist.sprite = null;
                    break;
                case ResistState.Full:
                    Resist.sprite = GameDB.Instance.ResistFull;
                    break;
                case ResistState.Developed:
                    Resist.sprite = GameDB.Instance.ResistDeveloped;
                    break;
                default:
                    break;
            }

            // Metal Layer
            switch (data.MetallizationLayer.State)
            {
                case MetallizationState.Empty:
                    Metallization.sprite = null;
                    break;
                case MetallizationState.Full:
                    Metallization.sprite = GameDB.Instance.MetalFull;
                    break;
                case MetallizationState.Stripped:
                    Metallization.sprite = GameDB.Instance.MetalStripped;
                    break;
                case MetallizationState.OxideFilled:
                    Metallization.sprite = GameDB.Instance.MetalOxideFilled;
                    break;
                default:
                    break;
            }

            // Oxide Layer
            switch (data.OxideLayer.State)
            {
                case OxideState.Empty:
                    Oxide.sprite = null;
                    break;
                case OxideState.Full:
                    Oxide.sprite = GameDB.Instance.OxideFull;
                    break;
                case OxideState.Stripped:
                    Oxide.sprite = GameDB.Instance.OxidePatterned;
                    break;
                default:
                    break;
            }

            // Semiconductor Layer
            var semiState = SemiconductorState.Blank;
            if (data.SemiconductorLayer.DopingPatterns != null)
            {
                foreach (var pattern in data.SemiconductorLayer.DopingPatterns)
                {
                    if (pattern.DopingType == DopingType.N)
                    {
                        semiState = semiState == SemiconductorState.DopedP ? SemiconductorState.DopedNP : SemiconductorState.DopedN;
                    }
                    else if (pattern.DopingType == DopingType.P)
                    {
                        semiState = semiState == SemiconductorState.DopedN ? SemiconductorState.DopedNP : SemiconductorState.DopedP;
                    }
                }
            }

            switch (semiState)
            {
                case SemiconductorState.Blank:
                    Semiconductor.sprite = GameDB.Instance.SemiEmpty;
                    break;
                case SemiconductorState.DopedN:
                    Semiconductor.sprite = GameDB.Instance.SemiDopedN;
                    break;
                case SemiconductorState.DopedP:
                    Semiconductor.sprite = GameDB.Instance.SemiDopedP;
                    break;
                case SemiconductorState.DopedNP:
                    Semiconductor.sprite = GameDB.Instance.SemiDopedNP;
                    break;
                default:
                    break;
            }
        }
    }
}