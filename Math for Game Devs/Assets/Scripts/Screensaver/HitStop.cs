using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    [Range(0f, 1f)] [SerializeField] private float pauseTimeScale = 0f;
    
    public void Stop(float duration = 0.15f)
    {
        StartCoroutine(PauseCoroutine(duration));
    }
    
    private IEnumerator PauseCoroutine(float duration)
    {
        Time.timeScale = pauseTimeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}
