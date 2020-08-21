using System;
using UnityEngine;

namespace LuDK.Toolkit.L2D
{
    public class TileMapTools
    {

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/LuDK/2D/Block")]
        public static GameObject CreateBlock()
        {
            GameObject newObj = new GameObject();
            newObj.name = "Block";
            var sr = newObj.AddComponent<SpriteRenderer>();
            sr.sprite = (Sprite)UnityEditor.Selection.activeObject;
            sr.sortingLayerName = PlayerController2D.LAYER_MIDDLEGROUND;
            newObj.AddComponent<BoxCollider2D>();
            UnityEditor.Selection.activeObject = newObj;
            return newObj;
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Block", true)]
        private static bool ValidationBlock()
        {
            return UnityEditor.Selection.activeObject is Sprite && UnityEditor.Selection.objects.Length == 1;
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/MoveableBlock")]
        public static GameObject CreateMoveableBlock()
        {
            GameObject newObj = CreateBlock();
            newObj.name = "MoveableBlock";
            var rb = newObj.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 0;
            return newObj;
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/MoveableBlock", true)]
        private static bool ValidationMoveableBlock()
        {
            return UnityEditor.Selection.activeObject is Sprite && UnityEditor.Selection.objects.Length == 1;
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Animated/Block")]
        public static GameObject CreateAnimatedBlock()
        {
            GameObject newObj = AnimatedSprite.CreateSprite();
            newObj.name = "AnimatedBlock";
            newObj.transform.position = PlayerController2D.GetMiddleOfScreenWorldPos();
            var sr = newObj.GetComponent<SpriteRenderer>();
            sr.sortingLayerName = PlayerController2D.LAYER_MIDDLEGROUND;
            newObj.AddComponent<BoxCollider2D>();
            return newObj;
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Animated/MoveableBlock")]
        public static GameObject CreateAnimatedMoveableBlock()
        {
            GameObject newObj = CreateAnimatedBlock();
            newObj.name = "AnimatedMoveableBlock";
            newObj.transform.position = PlayerController2D.GetMiddleOfScreenWorldPos();
            var rb = newObj.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 0;
            return newObj;
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Animated/MoveableBlock", true)]
        [UnityEditor.MenuItem("Assets/LuDK/2D/Animated/Block", true)]
        private static bool Validation()
        {
            if (null == UnityEditor.Selection.objects || UnityEditor.Selection.objects.Length <= 1)
                return false;
            foreach (var oneObj in UnityEditor.Selection.objects)
            {
                if (!(oneObj is Sprite))
                {
                    return false;
                }
            }
            return true;
        }

        [UnityEditor.MenuItem("GameObject/LuDK/2D/CreateTileMaps", false, 0)]
        public static void CreateTileMaps()
        {
            GameObject newGrid = new GameObject();
            newGrid.name = "Grid";
            newGrid.AddComponent<Grid>();

            CreateTileMap(newGrid, PlayerController2D.LAYER_BACKGROUND);
            GameObject mGround = CreateTileMap(newGrid, PlayerController2D.LAYER_MIDDLEGROUND);
            mGround.AddComponent<UnityEngine.Tilemaps.TilemapCollider2D>();
            CreateTileMap(newGrid, PlayerController2D.LAYER_FOREGROUND);

            UnityEditor.Selection.activeObject = newGrid;
        }

        private static GameObject CreateTileMap(GameObject newGrid, string name)
        {
            CreateSortingLayer(name);
            GameObject newTileMap = new GameObject();
            newTileMap.transform.parent = newGrid.transform;
            newTileMap.name = name;
            var tmr = newTileMap.AddComponent<UnityEngine.Tilemaps.TilemapRenderer>();
            tmr.sortingLayerName = name;
            return newTileMap;

        }

        public static void CreateSortingLayer(string layerName)
        {
            var tagManager = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
            var sortingLayers = tagManager.FindProperty("m_SortingLayers");
            for (int i = 0; i < sortingLayers.arraySize; i++)
                if (sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue.Equals(layerName))
                    return;
            sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
            var newLayer = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
            newLayer.FindPropertyRelative("name").stringValue = layerName;
            newLayer.FindPropertyRelative("uniqueID").intValue = Math.Abs((int)layerName.GetHashCode());
            tagManager.ApplyModifiedProperties();
            UnityEditor.AssetDatabase.SaveAssets();
        }

#endif
    }
}