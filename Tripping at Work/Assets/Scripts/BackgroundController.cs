using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class BackgroundController : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private List<Background> backgrounds;
    private Dictionary<string, GameObject> _backgroundDict = new Dictionary<string, GameObject>();
    private GameObject _currentBackground;
    
    void Start()
    {
        InitializeBackgroundDict();
        dialogueRunner.AddCommandHandler("switch_background", SwitchBackground);
    }
    
    void Update()
    {
        
    }

    // takes any bg name in list of backgrounds
    // if it's not in the list, clear bg
    void SwitchBackground(string[] parameters)
    {
        var bgName = parameters[0];
        Destroy(_currentBackground);

        if (_backgroundDict.ContainsKey(bgName))
        {
            var prefab = _backgroundDict[bgName];
            var newBg = Instantiate(prefab).transform;
            newBg.SetParent(gameObject.transform);
            
            _currentBackground = newBg.gameObject;
        }
    }

    void InitializeBackgroundDict()
    {
        foreach (var bg in backgrounds)
        {
            _backgroundDict.Add(bg.name, bg.prefab);
        }
    }
}
