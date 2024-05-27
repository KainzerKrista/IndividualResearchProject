using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

public class DialoguePraser : MonoBehaviour
{/*
    public DialogueGraphView graph;
    Coroutine _reader;

    [SerializeField] public TMP_Text speakerName;
    [SerializeField] public TMP_Text dialogueLine;
    [SerializeField] public Image speakerImg;

    private DialogueGraphView _targetGraphView;
    private DialogueContainer _containerCache;

    private void Start()
    {
        foreach(var nodeData in _containerCache.DialogueNodeData)
        {
            var currentNode = _targetGraphView.
        }
        _reader = StartCoroutine(ReaderNode());
    }

    IEnumerator ReaderNode()
    {
        DialogueNodeTest dialogueNode = graph.current;
        string data = dialogueNode.GetString();
        string[] dataParts = data;
        switch (dataParts.ToString())
        {
            //Start Node
            case "Start":
                NextNode("exit");
                break;

            case "DialogueNode":
                //Run DIalogue Process
                speaker.text = dataParts[1];
                dialogue.text = dataParts[2];
                speakerImg.sprite = b.GetSprite();

                //Waits for player action before running the next dialogue line.
                yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
                yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
                NextNode("exit");
                break;
        }
    }

    //Field name refers to the input or output port of a node.
    public void NextNode(string fieldName)
    {
        //Stop the enum from running
        if (_reader != null)
        {
            StopCoroutine(_reader);
            _reader = null;
        }

        foreach (NodePort port in graph.current.Ports)
        {
            //Checks if it's the correct input and output port to call.
            if (port.fieldName == fieldName)
            {
                graph.current = port.Connection.node as DialogueNodeTest;
                break;
            }
        }
        _reader = StartCoroutine(ReaderNode());
    }*/
}
