using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueContainer : ScriptableObject
{
   public List<NodeLinkData> NodeLinkData = new List<NodeLinkData>();
   public List<DialogueNodeData> DialogueNodeData = new List<DialogueNodeData>();
   public List<SpeakerNodeData> SpeakerNodeData = new List<SpeakerNodeData>();
   public List<ExposeProperty> ExposeProperties = new List<ExposeProperty>();
}
