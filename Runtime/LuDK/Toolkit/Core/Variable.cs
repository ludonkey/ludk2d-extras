using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace LuDK.Toolkit.Core
{
    public class Variable : MonoBehaviour
    {
        [System.Serializable]
        public class Backup
        {
            public string name;
            public DateTime creationDateTime;
            public DateTime lastSavedDateTime;
            public Dictionary<string, float> vars;

            public Backup(string saveName)
            {
                name = saveName;
                creationDateTime = new DateTime();
                vars = new Dictionary<string, float>();
            }

            public float Get(string name)
            {
                if (Contains(name))
                {
                    return vars[name];
                }
                return 0;
            }

            public void Set(string name, float val)
            {
                vars[name] = val;
            }

            public bool Contains(string name)
            {
                return vars.ContainsKey(name);
            }

            public void Flush()
            {
                vars.Clear();
            }
        }

        private static string DEFAULT_SAVENAME = "Default";
        private static string SAVE_FILE_PREFIX = "Bak_";
        private static string VAR_PREFIX_KEY = "LuDKVar_";

        private static Dictionary<string, float> GLOBAL_VARIABLES = new Dictionary<string, float>();
        private static Backup currentSave;

        public enum VarType
        {
            local,
            global,
            playerPrefs
        }

        [SerializeField]
        private VarType type = VarType.local;

        [SerializeField]
        private bool saveable = false;

        [SerializeField]
        private float currentValue;

        private string Key { 
            get {
                string oneKey = VAR_PREFIX_KEY;
                if (type == VarType.local)
                {
                    oneKey += "#" + SceneManager.GetActiveScene().name + "#_";
                }
                return oneKey + gameObject.name; 
            } 
        }

        [SerializeField]
        public float value { 
            get
            {
                float val;
                switch (type)
                {
                    default:
                    case VarType.local:
                        if (saveable)
                        {
                            val = currentSave.Get(Key);
                        } else
                        {
                            val = currentValue;
                        }                      
                        break;
                    case VarType.global:                       
                        if (saveable)
                        {
                            val = currentSave.Get(Key);
                        }
                        else
                        {
                            val = GLOBAL_VARIABLES[Key];
                        }
                        break;
                    case VarType.playerPrefs:
                        val = PlayerPrefs.GetFloat(Key);
                        break;
                }
                return FloatWith3Decimals(val);
            }            

            private set
            {
                float val = FloatWith3Decimals(value);
                switch (type)
                {
                    default:
                    case VarType.local:
                        if (saveable)
                        {
                            currentSave.Set(Key, val);
                        }
                        else
                        {
                            currentValue = val;
                        }
                        break;
                    case VarType.global:                       
                        if (saveable)
                        {
                            currentSave.Set(Key, val);
                        }
                        else
                        {
                            GLOBAL_VARIABLES[Key] = val;
                        }
                        break;
                    case VarType.playerPrefs:
                        PlayerPrefs.SetFloat(Key, val);
                        PlayerPrefs.Save();
                        break;
                }
                currentValue = val;                         
            }
        }

        private static float FloatWith3Decimals(float val)
        {
            return (float)Math.Round(val * 1000f) / 1000f;
        }

        internal void RemoveOnChangeListener(UnityAction listener)
        {
            OnChange.RemoveListener(listener);
        }

        internal void AddOnChangeListener(UnityAction listener)
        {
            OnChange.AddListener(listener);
        }

        [SerializeField]
        private bool constraintMinValue = false;
        [SerializeField]
        private float minValue = 0;
        [SerializeField]
        private bool constraintMaxValue = false;
        [SerializeField]
        private float maxValue = 0;
        [SerializeField]
        private UnityEvent OnChange;
        [SerializeField]
        private float valueToReach = -1;
        [SerializeField]
        private UnityEvent OnReach;

        private void Awake()
        {
            // Do it here because it doesn't allow to use Application.persistentDataPath in static block
            if (currentSave == null)
            {
                Load(DEFAULT_SAVENAME);
            }

            if (!HasKey(Key))
            {
                value = currentValue;
            }
        }

        private bool HasKey(string key)
        {
            switch (type)
            {
                default:
                case VarType.local:
                    if (saveable)
                    {
                        return currentSave.Contains(key);
                    } else
                    {
                        return false;
                    }
                case VarType.global:
                    if (saveable)
                    {
                        return currentSave.Contains(key);
                    }
                    else
                    {
                        return GLOBAL_VARIABLES.ContainsKey(key);
                    }
                case VarType.playerPrefs:
                    return PlayerPrefs.HasKey(key);                   
            }
        }

        public void Start()
        {
            NotifyListenerIfNeeds(false);
        }

        public void Add(int add)
        {
            Add((float)add);
        }

        /// <summary>
        /// To add (can be a negative value) a value to the current value.
        /// It will invoke OnReach when it will equal to valueToReach.
        /// </summary>
        /// <param name="add"></param>
        public void Add(float add)
        {
            Set(value + add);
        }

        /// <summary>
        /// To set a value to the current value.
        /// It will invoke OnReach when it will equal to valueToReach.
        /// </summary>
        /// <param name="newVal"></param>
        public void Set(int newVal)
        {
            Set((float)newVal);
        }

        /// <summary>
        /// To set a value to the current value.
        /// It will invoke OnReach when it will equal to valueToReach.
        /// </summary>
        /// <param name="newVal"></param>
        public void Set(float newVal)
        {
            float valToSet = newVal;
            if (constraintMinValue && valToSet < minValue)
            {
                valToSet = minValue;
            }
            if (constraintMaxValue && valToSet > maxValue)
            {
                valToSet = maxValue;
            }
            value = valToSet;
            NotifyListenerIfNeeds(true);
        }

        private void NotifyListenerIfNeeds(bool justChanged)
        {
            if (justChanged && OnChange != null)
            {
                OnChange.Invoke();
            }
            if (value == valueToReach)
            {
                if (OnReach != null)
                {
                    OnReach.Invoke();
                }
            }
        }

        public static void Load(string saveName)
        {
            string filepath = GetSavedFilePath(saveName);
            Debug.Log("[Variable] Loading save from:" + filepath);
            if (!File.Exists(filepath))
            {
                currentSave = new Backup(saveName);
                return;
            }
            try
            {
                using (FileStream file = File.Open(filepath, FileMode.Open))
                {
                    object loadedData = new BinaryFormatter().Deserialize(file);
                    currentSave = (Backup)loadedData;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                currentSave = new Backup(saveName);
                return;
            }
        }

        public void FlushAll()
        {
            Flush();
        }

        public static void Flush()
        {
            currentSave.Flush();
            Save();
        }

        public void SaveAll()
        {
            Save();
        }

        public static void Save()
        {
            SaveAs(currentSave.name);
        }

        public void SaveAllAs(string name)
        {
            SaveAs(name);
        }

        public static void SaveAs(string name)
        {
            try
            {
                currentSave.name = name;
                currentSave.lastSavedDateTime = new DateTime();
                string filepath = GetSavedFilePath(name);
                Debug.Log("[Variable] Saving save to:" + filepath);
                using (FileStream file = File.Create(filepath))
                {
                    new BinaryFormatter().Serialize(file, currentSave);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }
        }

        private static string GetSavedFilePath(string saveName)
        {
            return Application.persistentDataPath + "/" + SAVE_FILE_PREFIX + saveName + ".dat";
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/LuDK/Core/CreateVariable", false, 0)]
        public static void CreateEmptyDesination()
        {
            GameObject newObj = new GameObject();
            newObj.name = "Variable";
            newObj.AddComponent<Variable>();
            if (UnityEditor.Selection.activeObject is GameObject)
            {
                newObj.transform.parent = ((GameObject)UnityEditor.Selection.activeObject).transform;
            }
        }   
#endif
    }
}