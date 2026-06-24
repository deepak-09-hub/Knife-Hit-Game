using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BGColorTransition : MonoBehaviour
{
    public Image image;
    public Color[] colors;
    public int currentColorIndex = 0;
    public Vector2 colorChangeDelayMinMax = new Vector2(3, 8);

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Start()
    {
        StopCoroutine("ColorTransitionCoroutine");
        StartCoroutine("ColorTransitionCoroutine");
    }

    IEnumerator ColorTransitionCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(colorChangeDelayMinMax.x, colorChangeDelayMinMax.y));

        currentColorIndex = Random.Range(0, colors.Length);
        image.DOColor(colors[currentColorIndex], 1f);

        StartCoroutine("ColorTransitionCoroutine");
    }
}
