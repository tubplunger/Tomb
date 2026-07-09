using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomb.Core.Save
{
    public interface ISaveable
    {
        string SaveKey { get; }

        object CaptureState();

        void RestoreState(object state);
    }
}
