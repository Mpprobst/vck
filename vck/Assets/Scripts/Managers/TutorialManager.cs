using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] Image button;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Sprite keySprite, wideKeySprite;

    public UnityEvent tutorialComplete;

    private bool bouncing;
    private int step;

    // Start is called before the first frame update
    void Start()
    {
        step = 0;
        ChangeInfo("A", keySprite);
    }

    // Update is called once per frame
    void Update()
    {
        if (step == 0)
        {
            if (Input.GetAxis("Horizontal") < 0f)
            {
                step++;
                ChangeInfo("D", keySprite);
            }
        }
        else if (step == 1)
        {
            if (Input.GetAxis("Horizontal") > 0f)
            {
                step++;
                ChangeInfo("space", wideKeySprite);
            }
        }
        else if (step == 2)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                step++;
                Destroy(gameObject);
            }
        }

    }

    public void ChangeInfo(string info, Sprite buttonSprite)
    {
        bouncing = false;
        button.sprite = buttonSprite;
        text.text = info;

        if (buttonSprite == wideKeySprite)
        {
            button.rectTransform.sizeDelta = new Vector2(button.rectTransform.rect.height * 2f, button.rectTransform.rect.height);
        }
        else
        {
            button.rectTransform.sizeDelta = new Vector2(button.rectTransform.rect.height, button.rectTransform.rect.height);
        }

        StartCoroutine(ButtonBounce());
    }

    private IEnumerator ButtonBounce()
    {
        float bounceTime = 1f;
        float bounceHeight = 5f;
        bouncing = true;
        while (bouncing)
        {
            float t = 0;

            while (t < bounceTime)
            {
                float h = Mathf.Cos(2f * t / bounceTime * Mathf.PI) * bounceHeight;
                button.rectTransform.anchoredPosition = new Vector2(0, h);
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
            }
            /*
            t = 0;
            while (t < bounceTime / 2f)
            {
                float h = Mathf.Cos(t / bounceTime * Mathf.PI) * bounceHeight / 2f;
                button.rectTransform.anchoredPosition = new Vector2(0, h);
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
            }

            t = 0;
            while (t < bounceTime / 4)
            {
                float h = Mathf.Cos(t / bounceTime * Mathf.PI) * bounceHeight / 4f;
                button.rectTransform.anchoredPosition = new Vector2(0, h);
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
            }

            yield return new WaitForSeconds(1f);*/
        }
    }

    public void TutorialComplete()
    {
        tutorialComplete.Invoke();
        Destroy(gameObject);
    }
}
