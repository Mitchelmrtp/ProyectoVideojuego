using UnityEngine;
using System.Collections;

public class TutorialTrigger : MonoBehaviour
{
    public SpriteRenderer tutorialSprite;
    public string playerTag = "Player";
    public float fadeDuration = 1f;
    private bool hasShown = false;

    private void Start()
    {
        if (tutorialSprite != null)
            tutorialSprite.color = new Color(1, 1, 1, 0); // invisible
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && !hasShown)
        {
            StartCoroutine(FadeIn());
            hasShown = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && hasShown)
        {
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeIn()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            tutorialSprite.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float progress = t / fadeDuration;
            float alpha = Mathf.Lerp(1f, 0f, progress * progress);
           
            tutorialSprite.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }
}
