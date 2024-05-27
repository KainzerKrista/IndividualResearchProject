using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.U2D.Animation;
using UnityEditor;
using UnityEditor.UIElements;

public class SpriteLibraryManager : EditorWindow
{
    public ObjectField spriteLibraryAssetField;
    public SpriteLibraryAsset spriteLibraryAsset;
    public DropdownField categoryDropdown;

    private Dictionary<string, Dictionary<string, Sprite>> spriteDictionary;
    public List<SpriteData> spriteCategories = new List<SpriteData>();

    [MenuItem("Window/Sprite Library Editor")]
    public static void ShowSpriteLibrary()
    {
        SpriteLibraryManager window = GetWindow<SpriteLibraryManager>();
        window.titleContent = new GUIContent("Sprite Library Editor");
    }

    private void Start()
    {
        if (spriteLibraryAsset != null)
        {
            spriteDictionary = new Dictionary<string, Dictionary<string, Sprite>>();
            LoadSprites();
        }
        else
        {
            Debug.Log("Error SLA");
        }
    }

    private void LoadSprites()
    {

        var categories = spriteLibraryAsset.GetCategoryNames();
        foreach(var category in categories)
        {
            var labels = spriteLibraryAsset.GetCategoryLabelNames(category);
            var labelDict = new Dictionary<string, Sprite>();

            foreach(var label in labels)
            {
                Sprite sprite = spriteLibraryAsset.GetSprite(category, label);
                labelDict[label] = sprite;
            }

            spriteDictionary[category] = labelDict;
        }
    }

    public Sprite GetSprite(string category, string label)
    {
        if(spriteDictionary.ContainsKey(category) && spriteDictionary[category].ContainsKey(label))
        {
            return spriteDictionary[category][label];
        }
        return null;
    }

    public List<string> GetCategories()
    {
        return new List<string>(spriteDictionary.Keys);
    }

    public List<string> GetLabels(string category)
    {
        if(spriteDictionary.ContainsKey(category))
        {
            return new List<string>(spriteDictionary[category].Keys);
        }
        return null;
    }
}

    /*public SpriteLibraryAsset GetSpriteLibrary()
    {
        if (spriteLibraryAsset == null)
        {
            Debug.Log("Error: Sprite Library not found!");
            return null;
        }
        return spriteLibraryAsset;
    }

    public List<string> GetSpriteCategory()
    {
        //Get list of category names in assigned sprite library
        List<string> names = new List<string>();
        foreach(var category in spriteCategories)
        {
            names.Add(category.categoryName);
        }
        return names;
    }

    public Sprite GetSprite(string categoryName)
    {
        var category = spriteCategories.Find(c => c.categoryName == categoryName);
        return category != null ? category.sprite : null;
    }*/

/*
        //Stores sprite in a disctionary
        Dictionary<string, List <Sprite>> spriteLibrary = new Dictionary<string, List<Sprite>>();


        //Gets list of category, label and sprites
        foreach(string category in categoryNames)
        {
            List<string> spriteNames = new List<string>(spriteLibraryAsset.GetCategoryLabelNames(category));
            List<Sprite> spriteList = new List<Sprite>();
            
            foreach(string spriteName in spriteNames)
            {
                Sprite sprite = spriteLibraryAsset.GetSprite(category, spriteName);
                if (sprite != null)
                {
                    spriteList.Add(sprite);
                }
            }

            spriteLibrary[category] = spriteList;
        }
*/
