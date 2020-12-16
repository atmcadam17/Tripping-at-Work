using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is a simplified version of the following commonly used Unity singleton.
// http://wiki.unity3d.com/index.php/Singleton
// The original version avoids some common bugs but is more complex to understand.

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance; // Backing variable

    public static T Instance // Property
    {
        get // This property only has a Getter, not a Setter
        {
            if (_instance == null) 
            {
                // The contents of this 'if' statement are slow but will only be run once.

                _instance = FindObjectOfType(typeof(T)) as T; // Check that we don't already have an instance of this singleton somewhere else.

                if (_instance == null) // If there is no instance in the hierarchy, create one in code.
                {
                    T obj = new GameObject().AddComponent<T>();
                    _instance = obj as T;
                }

                DontDestroyOnLoad(_instance); // Make instance persistent between scenes.

                Debug.Log("Created Singleton");
            }

            return _instance;
        }
    }
}