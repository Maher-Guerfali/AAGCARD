using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Card : MonoBehaviour
{
    public int faceId;
    public Image frontImage;
    public Image backImage;
    public float flipTime = 0.28f;
    public bool IsMatched { get; private set; } = false;
    public bool IsRevealed { get; private set; } = false;

    private bool isAnimating = false;

    public void Initialize(int id, Sprite frontSprite, Sprite backSprite)
    {
        faceId = id;
        frontImage.sprite = frontSprite;
        backImage.sprite = backSprite;
        IsMatched = false;
        IsRevealed = false;
        transform.localScale = Vector3.one;
        frontImage.gameObject.SetActive(false);
        backImage.gameObject.SetActive(true);
    }

    public void OnClicked()
    {
        if (isAnimating || IsMatched) return;
        if (IsRevealed) return;
        Reveal();
        GameManager.Instance.RegisterFlip(this);
        SoundManager.Instance.PlayFlip();
    }

    public void Reveal()
    {
        if (isAnimating || IsRevealed) return;
        StartCoroutine(Flip(true));
    }

    public void HideInstant()
    {
        if (IsMatched) return;
        StartCoroutine(Flip(false));
    }

    public void MarkMatched()
    {
        IsMatched = true;
        // small animation or disable interaction
        // keep revealed
        StartCoroutine(MatchAnim());
    }

    private IEnumerator MatchAnim()
    {
        // pop scale then disable collider
        var tr = transform;
        Vector3 target = tr.localScale * 1.08f;
        float t = 0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            tr.localScale = Vector3.Lerp(tr.localScale, target, t / 0.12f);
            yield return null;
        }
        yield return new WaitForSeconds(0.08f);
        // optionally remove or fade
    }

    private IEnumerator Flip(bool showFront)
    {
        isAnimating = true;
        float t = 0f;
        var start = transform.localScale.x;
        // we will scale X to simulate flip - cleaner than rotating in UI space
        while (t < flipTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / flipTime);
            float s = Mathf.Lerp(start, 0.001f, p);
            transform.localScale = new Vector3(s, 1, 1);
            yield return null;
        }

        // swap visible images
        frontImage.gameObject.SetActive(showFront);
        backImage.gameObject.SetActive(!showFront);
        IsRevealed = showFront;

        t = 0f;
        while (t < flipTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / flipTime);
            float s = Mathf.Lerp(0.001f, 1f, p);
            transform.localScale = new Vector3(s, 1, 1);
            yield return null;
        }
        isAnimating = false;
    }
}
