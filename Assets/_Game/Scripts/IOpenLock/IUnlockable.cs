using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnlockable 
{
    bool TryUnlock();
    bool IsFullyUnlocked();
    Transform GetTransform(); 
}
