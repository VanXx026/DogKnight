using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//传送的目的地
public class TransitionDestination : MonoBehaviour
{
    public enum DestinationTag //传送目的地的tag，传送点设为ENTER
    {
        ENTER, A, B, C, D
    }

    public DestinationTag destinationTag; //传送目的地tag
}
