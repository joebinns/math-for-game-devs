using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    public void Stop(float duration = 0.15f)
    {
        StartCoroutine(PauseCoroutine(duration));
    }
    
    private IEnumerator PauseCoroutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}
