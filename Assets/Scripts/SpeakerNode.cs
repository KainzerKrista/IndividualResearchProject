using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D.Animation;

public class SpeakerNode : Node
{
    public string guid;
    public string name;
    public string title;
    public string dialogue;
    public Sprite speakerImg;
    public bool entryPoint = false;
    public SpriteLibraryAsset SpriteLibrary;

}
