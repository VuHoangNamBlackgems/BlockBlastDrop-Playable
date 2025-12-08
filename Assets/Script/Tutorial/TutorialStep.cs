using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialStep : MonoBehaviour
{
    public int stepOrder;
    public StepDir stepDirection;  
}

public enum StepDir
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3
}
