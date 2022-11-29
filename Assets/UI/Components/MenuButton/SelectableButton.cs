using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class SelectableButton : MonoBehaviour
{
    public Color StaticColor;
    public Color PressColor;

    private Color CurrentColor;
    private Image Img;
    private bool Selected;
    private float minScale = 1.0f;
    private float maxScale = 7.5f;
    private float currentScale;

    void Start()
    {
        Img = GetComponent<Image>();
        CurrentColor = StaticColor;
        Selected = false;
        currentScale = minScale;
    }

    private void LateUpdate() 
    {
        if (Selected) CurrentColor = Color.Lerp(CurrentColor, PressColor, 5 * Time.deltaTime); 
            else CurrentColor = Color.Lerp(CurrentColor, StaticColor, 5 * Time.deltaTime);

        if (Selected)
        {
            currentScale = Mathf.Lerp(currentScale, maxScale, 5 * Time.deltaTime);
        } else 
        {
            currentScale = Mathf.Lerp(currentScale, minScale, 5 * Time.deltaTime);
        }
        RectTransform trans = GetComponent<RectTransform>();
        trans.localScale = new Vector3(currentScale, trans.localScale.y, trans.localScale.z);

        // Reset Layout Hack
        gameObject.SetActive(false);
        gameObject.SetActive(true);

        print(Img.color);
        Img.color = CurrentColor;
    }

    public void startMouseOver()
    {
        Selected = true;
    }
    
    public void endMouseOver()
    {
        Selected = false;
    }
}
