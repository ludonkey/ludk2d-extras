#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
#endif

namespace LuDK.Toolkit.L2D
{
    public static class InstalledPackages
    {
#if UNITY_EDITOR
        

        private static List<string> sortedlayers = new List<string>() { 
            PlayerController2D.LAYER_BACKGROUND,
            PlayerController2D.LAYER_MIDDLEGROUND,
            PlayerController2D.LAYER_FOREGROUND,
        };

        private static List<int> sortedlayersId = new List<int>() {
            332834558,
            1893382100,
            812876595,
        };

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            CreateLayerIfNotExists(PlayerController2D.GROUND_DEFAULT_LAYER);
            var sortedLayers = GetSortingLayerNames().ToList();
            for (int i = 0; i < sortedlayers.Count; i ++) 
            {
                if (!sortedLayers.Contains(sortedlayers[i]))
                {
                    CreateSortingLayer(sortedlayers[i], sortedlayersId[i]);
                }
            }          
        }

        private static void CreateSortingLayer(string layerName, int hash)
        {
            var serializedObject = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
            var sortingLayers = serializedObject.FindProperty("m_SortingLayers");
            for (int i = 0; i < sortingLayers.arraySize; i++)
                if (sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue.Equals(layerName))
                    return;
            sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
            var newLayer = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
            newLayer.FindPropertyRelative("name").stringValue = layerName;
            newLayer.FindPropertyRelative("uniqueID").intValue = hash;//layerName.GetHashCode(); /* some unique number */
            serializedObject.ApplyModifiedProperties();
        }

        public static string[] GetSortingLayerNames()
        {
            Type internalEditorUtilityType = typeof(InternalEditorUtility);
            PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
            return (string[])sortingLayersProperty.GetValue(null, new object[0]);
        }

        public static void CreateTag(string tag)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset");
            if (asset != null)
            { // sanity checking
                var so = new SerializedObject(asset);
                var tags = so.FindProperty("tags");

                var numTags = tags.arraySize;
                // do not create duplicates
                for (int i = 0; i < numTags; i++)
                {
                    var existingTag = tags.GetArrayElementAtIndex(i);
                    if (existingTag.stringValue == tag) return;
                }

                tags.InsertArrayElementAtIndex(numTags);
                tags.GetArrayElementAtIndex(numTags).stringValue = tag;
                so.ApplyModifiedProperties();
                so.Update();
            }
        }

        private static int _maxLayersNumber = 32;

        public static bool CreateLayerIfNotExists(string layerName)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
            var layersProp = tagManager.FindProperty("layers");
            if (!PropertyExists(layersProp, 0, _maxLayersNumber, layerName))
            {
                // Start at layer 9th index -> 8 (zero based) => first 8 reserved for unity / greyed out
                for (int i = _maxLayersNumber - 1; i > 8; i--)
                {
                    var sp = layersProp.GetArrayElementAtIndex(i);
                    if (sp.stringValue == "")
                    {
                        sp.stringValue = layerName;
                        tagManager.ApplyModifiedProperties();
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        private static bool PropertyExists(UnityEditor.SerializedProperty property, int start, int end, string value)
        {
            for (int i = start; i < end; i++)
            {
                SerializedProperty t = property.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }
#endif
    }
}