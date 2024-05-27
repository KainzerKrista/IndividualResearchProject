using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.U2D.Animation;

public class DialogueNode : Node
{
    public string guid;
    public string category;
    public string label;
    public string dialogueText;
    public string response;
    public bool entryPoint = false;
    public bool isSelected = false;
}
