using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Core.Save
{
    public interface ISaveable
    {
        string SaveKey { get; }

        Type SaveStateType { get; }

        object CaptureState();

        void RestoreState(object state);
    }
}
