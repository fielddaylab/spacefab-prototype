using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.UI.Animation;
using UnityEngine;

[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]

namespace SpaceFab
{
    public sealed class SpaceFabGame : Game
    {

        [InvokePreBoot]
        static private void OnPreBoot()
        {
            SetEventDispatcher(new EventDispatcher<EvtArgs>());
            Rendering.EnableAspectClamping(4, 3);
        }
    }
}