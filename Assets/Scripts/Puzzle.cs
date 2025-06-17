using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    public bool hasBeenHinted;
    public Color hintColor;
    public int pieceNum;

    public void Hint()
    {
        foreach(SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            sr.color = hintColor;
            hasBeenHinted = true;
        }
    }
}
