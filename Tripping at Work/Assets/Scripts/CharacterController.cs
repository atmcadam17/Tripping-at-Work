using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class CharacterController : MonoBehaviour
{

    [SerializeField] private DialogueRunner _dialogueRunner;
    
    [SerializeField] private List<Transform> _transforms;
    [SerializeField] private Transform _offscreenLeftTransform;
    [SerializeField] private Transform _offscreenRightTransform;
    
    [SerializeField] private List<GameObject> _characterObjects;
    public Dictionary<string, GameObject> _characterDictionary = new Dictionary<string, GameObject>(); // name corresponds w/ character object

    [SerializeField] private float _easeEnterDuration;
    [SerializeField] private float _easeExitDuration;
    
    [SerializeField] private float _lerpEnterDuration;
    [SerializeField] private float _lerpExitDuration;

    private bool _moving = false;

    public void Start()
    {
        // creates new command "switch_sprite"
        _dialogueRunner.AddCommandHandler(
            "switch_sprite", // the name of the command
            switchSprite // the method to run
        );
        
        InitializeCharacterDictionary();
    }

    // blocking command that runs doSwitch
    private void switchSprite(string[] parameters, System.Action onComplete)
    {
        // paramaters: position (int), character (string) OR "none", expression name (string), move type (instant, lerp, or ease)
        StartCoroutine(doSwitch(parameters, onComplete));
    }

    private IEnumerator doSwitch(string[] parameters, System.Action onComplete)
    {
        var switched = false;
        var positionNumber = Convert.ToInt32(parameters[0]);
        var targetTransform = _transforms[positionNumber];
        GameObject characterObject;
        var expression = parameters[2];
        GameObject child = null;
        var moveType = parameters[3];

        // get character
        if (_characterDictionary.ContainsKey(parameters[1]))
        {
            characterObject = _characterDictionary[parameters[1]];
        } else if (parameters[1] == "none")
        {
            characterObject = null;
        }
        else
        {
            characterObject = null;
            Debug.Log("invalid character name");
        }
        
        if (targetTransform.childCount > 0)
        {
            child = targetTransform.GetChild(0).gameObject; // gets character in that position (if any)
        }

        // check if the desired position is filled
        if (child != null)
        {
            // check if the desired character is already in the spot
            // if so change character expression (get from object's CharacterInfo)
            if (characterObject != null)
            {
                if (child.name == characterObject.name)
                {
                    // get correct expression (sprite)/ get expressions
                    // set switched to true once it's in position
                    Sprite newExpression = GetExpression(expression, child);
                
                    // change the sprite
                    child.GetComponent<SpriteRenderer>().sprite = newExpression;
                
                    switched = true;
                }
            }
            
            if (!switched)
            {
                // otherwise move off screen before destroying
                _moving = true;
                var startX = child.transform.position.x;

                // StartCoroutine(doSwitch(parameters, onComplete));
                StartCoroutine(Leave(startX, positionNumber, child, moveType));
                yield return new WaitUntil(() => !_moving);

                if (characterObject == null)
                {
                    switched = true;
                }
            }
        }
        
        if (!switched)
        {
            // spawn character w/ correct expression
            // spawn offscreen

            var newChar = Instantiate(characterObject);
            newChar.name = characterObject.name;
            newChar.transform.SetParent(targetTransform); // set child
            
            // flip if on right
            if (positionNumber >= 2)
            {
                newChar.GetComponent<SpriteRenderer>().flipX = !this;
            }
            
            Sprite newExpression = GetExpression(expression, newChar);
            newChar.GetComponent<SpriteRenderer>().sprite = newExpression;
            
            // put in place
            _moving = true;
            StartCoroutine(Enter(positionNumber, newChar, moveType, characterObject));
            yield return new WaitUntil(() => !_moving);

            // set switched to true once it's in position
            switched = true;
        }
        
        yield return new WaitUntil(() => switched);
        onComplete();
    }

    // corresponds to _characterObjects list
    private void InitializeCharacterDictionary()
    {
        _characterDictionary.Add("Arnold", _characterObjects[0]);
        _characterDictionary.Add("Boss", _characterObjects[1]);
        _characterDictionary.Add("Folder", _characterObjects[2]);
        _characterDictionary.Add("Stapler", _characterObjects[3]);
        _characterDictionary.Add("Henry", _characterObjects[4]);
        _characterDictionary.Add("Jason", _characterObjects[5]);
        _characterDictionary.Add("Karen", _characterObjects[6]);
    }

    private Sprite GetExpression(string expression, GameObject charObject)
    {
        var charSprites = charObject.GetComponent<CharacterInfo>().sprites;
            
        foreach (var info in charSprites)
        {
            if (info.name == expression)
            {
                return info.sprite;
            }
        }
        
        Debug.LogErrorFormat("Can't find sprite named {0}!", expression);
        return null;
    }

    // makes character move off screen
    private IEnumerator Leave(float startPos, int positionNumber, GameObject objToMove, string moveType)
    {
        var gone = false; // is the object as offscreen as it's gonna get?
        float endXPos;

        if (positionNumber <= 1)
        {
            endXPos = _offscreenLeftTransform.transform.position.x;
        }
        else
        {
            endXPos = _offscreenRightTransform.transform.position.x;
        }

        if (moveType == "instant")
        {
            gone = true;
        } else if (moveType == "lerp")
        {
            // lerp into position (while loop)
            var t = 0f;
            var currentPos = startPos;
            
            while (t <= _lerpExitDuration)
            {
                t += Time.deltaTime;
                currentPos = Mathf.Lerp(startPos, endXPos, t/_lerpExitDuration);
                objToMove.transform.position = new Vector3(currentPos, _transforms[positionNumber].position.y, 0);
                yield return null;
            }
            
            gone = true;
        } else if (moveType == "ease")
        {
            var t = 0f;
            var currentPos = startPos;
            
            while (t <= _easeExitDuration)
            {
                t += Time.deltaTime;
                currentPos = EaseOutCubic(startPos, endXPos, t/_easeExitDuration);
                objToMove.transform.position = new Vector3(currentPos, _transforms[positionNumber].position.y, 0);
                yield return null;
            }
            
            gone = true;
        }
        else
        {
            Debug.LogError("move type does not exist");
        }
        
        yield return new WaitUntil(() => gone);
                
        // destroy object
        Destroy(objToMove);
        _moving = false; // all done!
    }

    private IEnumerator Enter(int positionNumber, GameObject objToMove, string moveType, GameObject characterObject)
    {
        float startXPos;
        var inPlace = false;
        var endXPos = _transforms[positionNumber].position.x;
        
        // set startXPos
        if (positionNumber >= _transforms.Count/2)
        {
            startXPos = _offscreenRightTransform.transform.position.x;
        }
        else
        {
            startXPos = _offscreenLeftTransform.transform.position.x;
        }
        
        // scale object correctly
        // TODO: why is this not working :(
        objToMove.transform.localScale = characterObject.transform.localScale;
        
        // check move type, act accordingly
        if (moveType == "instant")
        {
            objToMove.transform.position = new Vector3(endXPos, _transforms[positionNumber].position.y, 0);
            inPlace = true;
        } else if (moveType == "lerp")
        {
            // TODO: lerp into position
            var t = 0f;
            var currentPos = startXPos;
            
            while (t <= _lerpEnterDuration)
            {
                t += Time.deltaTime;
                currentPos = Mathf.Lerp(startXPos, endXPos, t/_lerpEnterDuration);
                objToMove.transform.position = new Vector3(currentPos, _transforms[positionNumber].position.y, 0);
                yield return null;
            }
            
            inPlace = true;
        } else if (moveType == "ease")
        {
            var t = 0f;
            var currentPos = startXPos;
            
            while (t <= _easeEnterDuration)
            {
                t += Time.deltaTime;
                currentPos = EaseOutBack(startXPos, endXPos, t/_easeEnterDuration);
                objToMove.transform.position = new Vector3(currentPos, _transforms[positionNumber].position.y, 0);
                yield return null;
            }

            inPlace = true;
        }
        else
        {
            Debug.LogError("move type does not exist");
        }
        
        yield return new WaitUntil(()=> inPlace);
        _moving = false;
    }
    
    // easing function for moving sprites in
    public static float EaseOutBack(float start, float end, float value)
    {
        float s = 1.70158f;
        end -= start;
        value = (value) - 1;
        return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
    }
    
    // easing function for moving sprites out
    public static float EaseOutCubic(float start, float end, float value)
    {
        value--;
        end -= start;
        return end * (value * value * value + 1) + start;
    }
}
