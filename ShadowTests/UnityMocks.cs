using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace UnityEngine
{
    public class Object
    {
        public string name;
        public static void Destroy(Object obj)
        {
             DestroyImmediate(obj);
        }
        public static void DontDestroyOnLoad(Object target) { }

        public static void DestroyImmediate(Object obj)
        {
             if (obj == null) return;

             // 1. Invoke OnDestroy
             MethodInfo onDestroy = obj.GetType().GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
             if (onDestroy != null)
             {
                 onDestroy.Invoke(obj, null);
             }

             // 2. If GameObject, destroy all components
             if (obj is GameObject go)
             {
                 // Create a copy to avoid modification during iteration issues
                 var comps = new List<object>(go.components);
                 foreach (var c in comps)
                 {
                     if (c is Object co) DestroyImmediate(co);
                 }
                 go.components.Clear();
             }
        }
    }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject
        {
            return Activator.CreateInstance<T>();
        }
    }

    public class Component : Object
    {
        public GameObject gameObject;
        public Transform transform;

        public T GetComponent<T>()
        {
            return gameObject.GetComponent<T>();
        }

        public void SendMessage(string methodName)
        {
            MethodInfo method = this.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
            {
                method.Invoke(this, null);
            }
        }
    }

    public class Transform : Component
    {
    }

    public class GameObject : Object
    {
        public List<object> components = new List<object>();

        public GameObject(string name)
        {
            this.name = name;
            // Add Transform
             var t = new Transform();
            t.gameObject = this;
            components.Add(t);
        }

        public T AddComponent<T>() where T : Component, new()
        {
            T comp = new T();
            comp.gameObject = this;
            comp.transform = GetComponent<Transform>();
            comp.name = this.name;
            components.Add(comp);

            // Simulate Awake
            MethodInfo awake = typeof(T).GetMethod("Awake", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (awake != null) awake.Invoke(comp, null);

            return comp;
        }

        public T GetComponent<T>()
        {
            foreach (var c in components)
            {
                if (c is T) return (T)c;
            }
            return default(T);
        }
    }

    public class MonoBehaviour : Component
    {
        public Coroutine StartCoroutine(System.Collections.IEnumerator routine)
        {
            if (routine.MoveNext())
            {
            }
            return new Coroutine();
        }
    }

    public class Coroutine {}

    public class WaitForSeconds
    {
        public WaitForSeconds(float seconds) {}
    }

    public static class Time
    {
        public static float deltaTime = 0.1f;
        public static float time = 0f;
    }

    public static class Random
    {
        private static System.Random rng = new System.Random();
        public static int Range(int min, int max)
        {
            return rng.Next(min, max);
        }
        public static float Range(float min, float max)
        {
            return (float)rng.NextDouble() * (max - min) + min;
        }
        public static float value => (float)rng.NextDouble();
    }

    public static class Mathf
    {
        public static float Max(float a, float b) => Math.Max(a, b);
        public static int Max(int a, int b) => Math.Max(a, b);
        public static float Min(float a, float b) => Math.Min(a, b);
        public static int Min(int a, int b) => Math.Min(a, b);
        public static int FloorToInt(float f) => (int)Math.Floor(f);
        public static float Pow(float f, float p) => (float)Math.Pow(f, p);
        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }

    public static class Debug
    {
        public static void Log(object message)
        {
            Console.WriteLine($"[Debug.Log] {message}");
        }
        public static void LogError(object message)
        {
            Console.WriteLine($"[Debug.LogError] {message}");
        }
    }

    public static class Application
    {
        public static string persistentDataPath = System.IO.Directory.GetCurrentDirectory();
    }

    public static class JsonUtility
    {
        public static string ToJson(object obj, bool prettyPrint)
        {
            return System.Text.Json.JsonSerializer.Serialize(obj, new System.Text.Json.JsonSerializerOptions { WriteIndented = prettyPrint, IncludeFields = true });
        }

        public static T FromJson<T>(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, new System.Text.Json.JsonSerializerOptions { IncludeFields = true });
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class HeaderAttribute : Attribute
    {
        public HeaderAttribute(string header) {}
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CreateAssetMenuAttribute : Attribute
    {
        public string fileName;
        public string menuName;
    }

    public class TextAsset : Object
    {
        public string text;
        public TextAsset(string content)
        {
            this.text = content;
        }
    }

    public static class Resources
    {
        public static T Load<T>(string path) where T : Object
        {
            if (typeof(T) == typeof(TextAsset))
            {
                string root = FindAssetsRoot();
                if (root == null)
                {
                    Debug.LogError("Resources.Load: Could not find 'Assets' directory in parents.");
                    return null;
                }

                string filePath = Path.Combine(root, "Assets", "Resources", path + ".json");

                if (File.Exists(filePath))
                {
                    string content = File.ReadAllText(filePath);
                    return new TextAsset(content) as T;
                }
                else
                {
                    Debug.LogError($"Resources.Load: File not found at {filePath}. Root: {root}");
                }
            }
            return null;
        }

        private static string FindAssetsRoot()
        {
            string current = Directory.GetCurrentDirectory();
            // Safety check: Prevent infinite loop if file system root is reached
            for (int i = 0; i < 10; i++)
            {
                if (Directory.Exists(Path.Combine(current, "Assets")))
                {
                    return current;
                }
                DirectoryInfo parent = Directory.GetParent(current);
                if (parent == null) break;
                current = parent.FullName;
            }
            return null;
        }
    }
}
