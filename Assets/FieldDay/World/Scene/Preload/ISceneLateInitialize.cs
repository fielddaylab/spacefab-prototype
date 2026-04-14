using System;
using System.Collections.Generic;
using BeauUtil;

namespace FieldDay.Scenes {
    /// <summary>
    /// Contains a scene late initialize callback.
    /// </summary>
    public interface ISceneLateInitialize {
        void LateInitialize();
    }

    /// <summary>
    /// Marks a late-initialized component 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class LateInitializeOrderAttribute : Attribute {
        public readonly int Order;
        
        public LateInitializeOrderAttribute(int order) {
            Order = order;
        }
    }
}