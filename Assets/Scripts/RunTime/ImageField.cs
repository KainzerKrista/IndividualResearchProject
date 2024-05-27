using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.U2D.Animation;

public class ImageField : VisualElement
{
    private SpriteLibraryManager _spriteLibrary;
    private ObjectField spriteField;
    private Image previewSprite;
    private SpriteLibraryAsset spriteLibraryAsset;

    public ImageField()
    {
        //Creates new sprite field for nodes
        spriteField = new ObjectField("Select Sprite")
        {
            objectType = typeof(Sprite),
            allowSceneObjects = false
        };

        //CHANGE OBJECT FIELD TO SPRITE

        //Changes sprite valu
        spriteField.RegisterValueChangedCallback(evt => OnSpriteChanged(evt.newValue as Sprite));
        Add(spriteField);

        //Set preview sprite image in text field
        previewSprite = new Image();
        Add(previewSprite);
        previewSprite.AddToClassList("sprite-field");
    }

    private void OnSpriteChanged(Sprite selectedSprite)
    {
        //Checks if selected sprite exists
        if(selectedSprite != null)
        {
            //If sprite exists, converts sprite texture to image type
            previewSprite.image = selectedSprite.texture;
        }
        else
        {
            previewSprite.image = null;
        }

    }
}
