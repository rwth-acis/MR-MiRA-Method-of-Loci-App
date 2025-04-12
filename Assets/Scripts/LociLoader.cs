using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LociLoader : MonoBehaviour
{
    [FormerlySerializedAs("Button")] [Tooltip("The button prefab to spawn for each loci object")]
    public GameObject button;
    [FormerlySerializedAs("Content")] [Tooltip("The content object to spawn the buttons in")]
    public GameObject content;
    [FormerlySerializedAs("Title")] [Tooltip("The title object to change the title of the menu")]
    public GameObject title;

    [FormerlySerializedAs("LociObjects")] [Tooltip("The list of loci objects")]
    public List<GameObject> lociObjects;
    [FormerlySerializedAs("LociOther")] [Tooltip("The list of objects categorized as 'Other'")]
    public List<GameObject> lociOther;
    [FormerlySerializedAs("LociFood")] [Tooltip("The list of objects categorized as 'Food'")]
    public List<GameObject> lociFood;
    [FormerlySerializedAs("LociCars")] [Tooltip("The list of objects categorized as 'Cars'")]
    public List<GameObject> lociCars;
    [FormerlySerializedAs("LociPeople")] [Tooltip("The list of objects categorized as 'People'")]
    public List<GameObject> lociPeople;
    [FormerlySerializedAs("LociAnimals")] [Tooltip("The list of objects categorized as 'Animals'")]
    public List<GameObject> lociAnimals;
    [FormerlySerializedAs("Recommendations")] [Tooltip("The list of recommendations")]
    public List<GameObject> recommendations;

    [FormerlySerializedAs("LociPreview")] [Tooltip("The list of loci object previews")]
    public List<Texture2D> lociPreview;
    [FormerlySerializedAs("LociOtherPreview")] [Tooltip("The list of 'Other' object previews")]
    public List<Texture2D> lociOtherPreview;
    [FormerlySerializedAs("LociFoodPreview")] [Tooltip("The list of 'Food' object previews")]
    public List<Texture2D> lociFoodPreview;
    [FormerlySerializedAs("LociCarsPreview")] [Tooltip("The list of 'Cars' object previews")]
    public List<Texture2D> lociCarsPreview;
    [FormerlySerializedAs("LociPeoplePreview")] [Tooltip("The list of 'People' object previews")]
    public List<Texture2D> lociPeoplePreview;
    [FormerlySerializedAs("LociAnimalsPreview")] [Tooltip("The list of 'Animals' object previews")]
    public List<Texture2D> lociAnimalsPreview;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Set up the preview images
        lociPreview = new List<Texture2D>();
        lociOtherPreview = new List<Texture2D>();
        lociFoodPreview = new List<Texture2D>();
        lociCarsPreview = new List<Texture2D>();
        lociPeoplePreview = new List<Texture2D>();
        lociAnimalsPreview = new List<Texture2D>();
        RuntimePreviewGenerator.OrthographicMode = true;
        foreach (GameObject loci in lociObjects)
        {
            lociPreview.Add(RuntimePreviewGenerator.GenerateModelPreview(loci.transform));
        }
        foreach (GameObject other in lociOther)
        {
            lociOtherPreview.Add(RuntimePreviewGenerator.GenerateModelPreview(other.transform));
        }
        foreach (GameObject food in lociFood)
        {
            lociFoodPreview.Add(RuntimePreviewGenerator.GenerateModelPreview(food.transform));
        }
        foreach (GameObject cars in lociCars)
        {
            lociCarsPreview.Add(RuntimePreviewGenerator.GenerateModelPreview(cars.transform));
        }
        foreach (GameObject people in lociPeople)
        {
            lociPeoplePreview.Add(RuntimePreviewGenerator.GenerateModelPreview(people.transform));
        }
        foreach (GameObject animals in lociAnimals)
        {
            lociAnimalsPreview.Add(RuntimePreviewGenerator.GenerateModelPreview(animals.transform));
        }
        LoadLoci(true);
    }

    /// <summary>
    /// Loads the loci objects into the menu
    /// </summary>
    /// <param name="value">The value of the "Alle Objekte" button</param>
    public void LoadLoci(bool value)
    {
        title.GetComponent<TextMeshProUGUI>().text = "Alle Objekte";
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject loci in lociObjects)
        {
            GameObject currentButton = Instantiate(button, content.transform);
            currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = loci.name;
            currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = lociPreview[lociObjects.IndexOf(loci)];
            currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddLoci(loci);});
        }
    }

    /// <summary>
    /// Loads the 'Other' objects into the menu
    /// </summary>
    /// <param name="value">The value of the "Sonstige" button</param>
    public void LoadOther(bool value)
    {
        title.GetComponent<TextMeshProUGUI>().text = "Sonstige";
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject other in lociOther)
        {
            GameObject currentButton = Instantiate(button, content.transform);
            currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = other.name;
            currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = lociOtherPreview[lociOther.IndexOf(other)];
            currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddLoci(other);});
        }
    }

    /// <summary>
    /// Loads the 'Food' objects into the menu
    /// </summary>
    /// <param name="value">The value of the "Essen" button</param>
    public void LoadFood(bool value)
    {
        title.GetComponent<TextMeshProUGUI>().text = "Essen";
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject food in lociFood)
        {
            GameObject currentButton = Instantiate(button, content.transform);
            currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = food.name;
            currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = lociFoodPreview[lociFood.IndexOf(food)];
            currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddLoci(food);});
        }
    }

    /// <summary>
    /// Loads the 'Cars' objects into the menu
    /// </summary>
    /// <param name="value">The value of the "Fahrzeuge" button</param>
    public void LoadCars(bool value)
    {
        title.GetComponent<TextMeshProUGUI>().text = "Fahrzeuge";
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject car in lociCars)
        {
            GameObject currentButton = Instantiate(button, content.transform);
            currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = car.name;
            currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = lociCarsPreview[lociCars.IndexOf(car)];
            currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddLoci(car);});
        }
    }

    /// <summary>
    /// Loads the 'People' objects into the menu
    /// </summary>
    /// <param name="value">The value of the "Menschen" button</param>
    public void LoadPeople(bool value)
    {
        title.GetComponent<TextMeshProUGUI>().text = "Menschen";
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject person in lociPeople)
        {
            GameObject currentButton = Instantiate(button, content.transform);
            currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = person.name;
            currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = lociPeoplePreview[lociPeople.IndexOf(person)];
            currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddLoci(person);});
        }
    }

    /// <summary>
    /// Loads the 'Animals' objects into the menu
    /// </summary>
    /// <param name="value">The value of the "Tiere" button</param>
    public void LoadAnimals(bool value)
    {
        title.GetComponent<TextMeshProUGUI>().text = "Tiere";
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject animal in lociAnimals)
        {
            GameObject currentButton = Instantiate(button, content.transform);
            currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = animal.name;
            currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = lociAnimalsPreview[lociAnimals.IndexOf(animal)];
            currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddLoci(animal);});
        }
    }

    /// <summary>
    /// Deactivates the Loci Menu
    /// </summary>
    /// <param name="value">The value of the "X" button</param>
    public void CloseMenu(bool value)
    {
        //Deactivate the loci menu
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Loads the recommendations into the menu
    /// </summary>
    /// <param name="value">The value of the "Vorschläge"" button</param>
    public void LoadRecommendations(bool value)
    {
        title.GetComponent<TextMeshProUGUI>().text = "Vorschläge";
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        int index = RoomManager.Instance.LoadRecommendations();
        for (int i = index * 3 ; i < (index*3)+3 ; i++)
        {
            GameObject currentButton = Instantiate(button, content.transform);
            currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = recommendations[i].name;
            currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = lociPreview[lociObjects.IndexOf(recommendations[i])];
            currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddLoci(recommendations[i]);});
        }
    }
}
