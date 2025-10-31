using BeauPools;
using BeauRoutine;
using FieldDay.Components;
using System.Collections;
using UnityEngine;

namespace SpaceFab.Research {
    public sealed class VfxInstance : BatchedComponent, IPoolAllocHandler {
        public ParticleSystem[] Particles;
        public Routine Animation;

        void IPoolAllocHandler.OnAlloc() {
        }

        void IPoolAllocHandler.OnFree() {
            VfxUtility.StopAndClear(this);
        }
    }

    static public partial class VfxUtility {
        static public VfxInstance PlayFromPool(IPool<VfxInstance> pool, Transform position) {
            VfxInstance instance = pool.Alloc();
            instance.transform.SetPosition(position.position, Axis.XY, Space.World);
            Play(instance);
            return instance;
        }

        static public void Play(VfxInstance instance) {
            foreach(var particleSys in instance.Particles) {
                particleSys.Play();
            }
        }

        static public void Play(VfxInstance instance, IEnumerator routine) {
            instance.Animation.Replace(instance, routine);
            foreach (var particleSys in instance.Particles) {
                particleSys.Play();
            }
        }

        static public bool IsPlaying(VfxInstance instance) {
            if (instance.Animation) {
                return true;
            }
            foreach (var particleSys in instance.Particles) {
                if (particleSys.isEmitting || particleSys.particleCount > 0) {
                    return true;
                }
            }
            return false;
        }

        static public void Stop(VfxInstance instance) {
            instance.Animation.Stop();
            foreach (var particle in instance.Particles) {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        static public void StopAndClear(VfxInstance instance) {
            instance.Animation.Stop();
            foreach (var particle in instance.Particles) {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        static public void Kill(VfxInstance instance) {
            if (!Pool.TryFree(instance)) {
                VfxUtility.StopAndClear(instance);
            }
        }
    }
}