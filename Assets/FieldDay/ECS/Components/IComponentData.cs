using System;
using System.Runtime.CompilerServices;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.UI;
using UnityEngine;

namespace FieldDay.Components {

    /// <summary>
    /// Interface for a component that can be used with a component system.
    /// </summary>
    public interface IComponentData { }

    public interface IComponentTuple {
        int Count { get; }
        bool IsValid { get; }
    }

    /// <summary>
    /// Tuple of two component types.
    /// </summary>
    public struct ComponentTuple<TPrimary, TSecondary> : IComponentTuple
        where TPrimary : class, IComponentData
        where TSecondary : class, IComponentData {

        public TPrimary Primary;
        public TSecondary Secondary;

        public ComponentTuple(TPrimary primary, TSecondary additional) {
            Primary = primary;
            Secondary = additional;
        }

        public readonly int Count {
            get { return 2; }
        }

        public readonly bool IsValid {
            get {
                return !ReferenceEquals(Primary, null)
                    && !ReferenceEquals(Secondary, null);
            }
        }
    }

    /// <summary>
    /// Tuple of three component types.
    /// </summary>
    public struct ComponentTuple<TPrimary, TComponentA, TComponentB> : IComponentTuple
        where TPrimary : class, IComponentData
        where TComponentA : class, IComponentData
        where TComponentB : class, IComponentData {

        public TPrimary Primary;
        public TComponentA ComponentA;
        public TComponentB ComponentB;

        public ComponentTuple(TPrimary primary, TComponentA additionalA, TComponentB additionalB) {
            Primary = primary;
            ComponentA = additionalA;
            ComponentB = additionalB;
        }

        public readonly int Count {
            get { return 3; }
        }

        public readonly bool IsValid {
            get {
                return !ReferenceEquals(Primary, null)
                    && !ReferenceEquals(ComponentA, null)
                    && !ReferenceEquals(ComponentB, null);
            }
        }
    }

    /// <summary>
    /// Tuple of four component types.
    /// </summary>
    public struct ComponentTuple<TPrimary, TComponentA, TComponentB, TComponentC> : IComponentTuple
        where TPrimary : class, IComponentData
        where TComponentA : class, IComponentData
        where TComponentB : class, IComponentData
        where TComponentC : class, IComponentData {

        public TPrimary Primary;
        public TComponentA ComponentA;
        public TComponentB ComponentB;
        public TComponentC ComponentC;

        public ComponentTuple(TPrimary primary, TComponentA additionalA, TComponentB additionalB, TComponentC additionalC) {
            Primary = primary;
            ComponentA = additionalA;
            ComponentB = additionalB;
            ComponentC = additionalC;
        }

        public readonly int Count {
            get { return 4; }
        }

        public readonly bool IsValid {
            get {
                return !ReferenceEquals(Primary, null)
                    && !ReferenceEquals(ComponentA, null)
                    && !ReferenceEquals(ComponentB, null)
                    && !ReferenceEquals(ComponentC, null);
            }
        }
    }

    /// <summary>
    /// Component utility.
    /// </summary>
    static public class ComponentUtility {
        #region Siblings

        /// <summary>
        /// Retrieves the subling of the given type from the given primary component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public TComponent Sibling<TPrimary, TComponent>(this TPrimary primary)
            where TPrimary : UnityEngine.Component, IComponentData
            where TComponent : UnityEngine.Component, IComponentData {
            return primary.GetComponent<TComponent>();
        }

        /// <summary>
        /// Retrieves the subling of the given type from the given primary component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Sibling<TPrimary, TComponent>(TPrimary primary, out TComponent component)
            where TPrimary : UnityEngine.Component, IComponentData
            where TComponent : UnityEngine.Component, IComponentData {
            return primary.TryGetComponent(out component);
        }

        /// <summary>
        /// Retrieves the sublings of the given type from the given primary component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Siblings<TPrimary, TComponentA, TComponentB>(TPrimary primary, out TComponentA componentA, out TComponentB componentB)
            where TPrimary : UnityEngine.Component, IComponentData
            where TComponentA : UnityEngine.Component, IComponentData
            where TComponentB : UnityEngine.Component, IComponentData {
            bool result = primary.TryGetComponent(out componentA);
            result &= primary.TryGetComponent(out componentB);
            return result;
        }

        /// <summary>
        /// Retrieves the sublings of the given type from the given primary component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Siblings<TPrimary, TComponentA, TComponentB, TComponentC>(TPrimary primary, out TComponentA componentA, out TComponentB componentB, out TComponentC componentC)
            where TPrimary : UnityEngine.Component, IComponentData
            where TComponentA : UnityEngine.Component, IComponentData
            where TComponentB : UnityEngine.Component, IComponentData
            where TComponentC : UnityEngine.Component, IComponentData {
            bool result = primary.TryGetComponent(out componentA);
            result &= primary.TryGetComponent(out componentB);
            result &= primary.TryGetComponent(out componentC);
            return result;
        }

        #endregion // Siblings

        #region Tuples

        /// <summary>
        /// Retrieves the subling of the given type from the given primary component.
        /// </summary>
        static public ComponentTuple<TPrimary, TComponent> Tuple<TPrimary, TComponent>(TPrimary primary)
            where TPrimary : UnityEngine.Component, IComponentData
            where TComponent : UnityEngine.Component, IComponentData {
            ComponentTuple<TPrimary, TComponent> result;
            result.Primary = primary;
            primary.TryGetComponent(out result.Secondary);
            return result;
        }

        /// <summary>
        /// Retrieves the subling of the given type from the given primary component.
        /// </summary>
        static public ComponentTuple<TPrimary, TComponentA, TComponentB> Tuple<TPrimary, TComponentA, TComponentB>(TPrimary primary)
            where TPrimary : UnityEngine.Component, IComponentData
            where TComponentA : UnityEngine.Component, IComponentData
            where TComponentB : UnityEngine.Component, IComponentData {
            ComponentTuple<TPrimary, TComponentA, TComponentB> result;
            result.Primary = primary;
            primary.TryGetComponent(out result.ComponentA);
            primary.TryGetComponent(out result.ComponentB);
            return result;
        }

        /// <summary>
        /// Retrieves the subling of the given type from the given primary component.
        /// </summary>
        static public ComponentTuple<TPrimary, TComponentA, TComponentB, TComponentC> Tuple<TPrimary, TComponentA, TComponentB, TComponentC>(TPrimary primary)
            where TPrimary : UnityEngine.Component, IComponentData
            where TComponentA : UnityEngine.Component, IComponentData
            where TComponentB : UnityEngine.Component, IComponentData
            where TComponentC : UnityEngine.Component, IComponentData {
            ComponentTuple<TPrimary, TComponentA, TComponentB, TComponentC> result;
            result.Primary = primary;
            primary.TryGetComponent(out result.ComponentA);
            primary.TryGetComponent(out result.ComponentB);
            primary.TryGetComponent(out result.ComponentC);
            return result;
        }

        #endregion // Tuples

        /// <summary>
        /// Returns if this component is a valid instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsValid<TComponent>(TComponent component) where TComponent : UnityEngine.Component, IComponentData {
            return (bool) component;
        }
    }
}