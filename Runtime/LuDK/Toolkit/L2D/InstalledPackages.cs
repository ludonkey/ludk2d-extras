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
#endif
    }
}