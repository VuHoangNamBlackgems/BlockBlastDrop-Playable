using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimationHand : MonoBehaviour
{

    [SerializeField] private Sprite spDown;
    [SerializeField] private Sprite spUp;

    private Image im;
    // Start is called before the first frame update
    void Start()
    {
        im = GetComponent<Image>(); 
    }


    public void ClickDown()
    {
        im.sprite = spDown;
    }
    public void ClickUp()
    {

        im.sprite = spUp;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
