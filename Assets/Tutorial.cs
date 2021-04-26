using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Tutorial : MonoBehaviour
{
    public int targetPage;
    //public Canvas tutorialCanvas;
    public RectTransform AllPages;

    float screenResolution = 800f;

    float targetPosition = 0f;

    [HideInInspector]
    public List<RectTransform> tutorialPanels;

    // Start is called before the first frame update
    void Start()
    {
        //List<Image> images = tutorialCanvas.transform.GetComponentsInChildren<Image>().ToList();
        //foreach (Image image in images)
        //{
        //    tutorialPanels.Add(image.gameObject.GetComponent<RectTransform>());
        //}
    }

    // Update is called once per frame
    void Update()
    {
        targetPosition = Mathf.Lerp(targetPosition, screenResolution * targetPage * -1f, .5f);
        AllPages.anchoredPosition = new Vector2(targetPosition, AllPages.anchoredPosition.y);

        //foreach (RectTransform rect in tutorialPanels)
        //{
        //    rect.anchoredPosition = new Vector2(targetPosition, rect.anchoredPosition.y);
        //}

        
    }

    public void advancePage()
    {
        targetPage += 1;
    }

    public void backPage()
    {
        targetPage -= 1;
    }
}
