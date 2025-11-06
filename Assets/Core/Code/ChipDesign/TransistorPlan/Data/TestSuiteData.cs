using ChipFab.ChipDesign;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipDesign
{
    [Serializable]
    public struct TestData
    {
        public FlowState InVal;
        public FlowState AVal;
        public FlowState BVal;
        public FlowState OutVal;
        public FlowState OutXVal;
        public FlowState OutYVal;
    }

    [CreateAssetMenu(menuName = "Chip Design/Test Suite Data")]
    public class TestSuiteData : ScriptableObject
    {
        public TestData[] Tests;
    }

    public static class EvalUtility { 
        public static FlowState GetTestValBySubType(string subtype, TestData testData)
        {
            if (subtype.Equals(GameConsts.IN_SUBTYPE))
            {
                return testData.InVal;
            } 
            else if (subtype.Equals(GameConsts.A_SUBTYPE))
            {
                return testData.AVal;
            }
            else if (subtype.Equals(GameConsts.B_SUBTYPE))
            {
                return testData.BVal;
            }
            else if (subtype.Equals(GameConsts.OUT_SUBTYPE))
            {
                return testData.OutVal;
            }
            else if (subtype.Equals(GameConsts.X_SUBTYPE))
            {
                return testData.OutXVal;
            }
            else if (subtype.Equals(GameConsts.Y_SUBTYPE))
            {
                return testData.OutYVal;
            }
            else if (subtype.Equals(GameConsts.VPLUS_SUBTYPE))
            {
                return FlowState.Hi;
            }
            else if (subtype.Equals(GameConsts.VMINUS_SUBTYPE))
            {   
                return FlowState.Lo;
            }

            return FlowState.Empty;
        }
    }

}