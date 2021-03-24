using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleFloat : MonoBehaviour
{
    [SerializeField] private float time = 0;
    [SerializeField] private float height = 0;

    private float timeElapsed = 0;
    private float topY;
    private float bottomY;
    void Start()
    {
        topY = transform.position.y + (.5f * height);
        bottomY = transform.position.y - (.5f * height);
        StartCoroutine(FloatUp());
    }

    void Update()
    {
    }
    
    // easing function
    public static float EaseOutBack(float start, float end, float value)
    {
        float s = 1.70158f;
        end -= start;
        value = (value) - 1;
        return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
    }

    IEnumerator FloatUp()
    {
        var startPos = transform.position.y;
        timeElapsed = 0;
        while (timeElapsed <= time - (time*.06f))
        {
            timeElapsed += Time.deltaTime;
            var pos = transform.position;
            var y = EaseOutBack(startPos,topY,timeElapsed / time);
            transform.position = new Vector3(pos.x, y, pos.z);
            yield return null;
        }
        
        StartCoroutine(FloatDown());
        yield return null;
    }

    IEnumerator FloatDown()
    {
        var startPos = transform.position.y;
        timeElapsed = 0;
        while (timeElapsed <= time - (time*.06f))
        {
            timeElapsed += Time.deltaTime;
            var pos = transform.position;
            var y = EaseOutBack(startPos,bottomY,timeElapsed / time);
            transform.position = new Vector3(pos.x, y, pos.z);
            yield return null;
        }

        StartCoroutine(FloatUp());
        yield return null;
    }
}
