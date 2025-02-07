using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LociLoader : MonoBehaviour
{
    [Tooltip("The button prefab to spawn for each loci object")]
    public GameObject Button;
    [Tooltip("The content object to spawn the buttons in")]
    public GameObject Content;
    [Tooltip("The title object to change the title of the menu")]
    public GameObject Title;

    [Tooltip("The list of loci objects")]
    public List<GameObject> LociObjects;
    [Tooltip("The list of objects categorized as 'Other'")]
    public List<GameObject> LociOther;
    [Tooltip("The list of objects categorized as 'Food'")]
    public List<GameObject> LociFood;
    [Tooltip("The list of objects categorized as 'Cars'")]
    public List<GameObject> LociCars;
    [Tooltip("The list of objects categorized as 'People'")]
    public List<GameObject> LociPeople;
    [Tooltip("The list of objects categorized as 'Animals'")]
    public List<GameObject> LociAnimals;

    [Tooltip("The list of loci object previews")]
    public List<Texture2D> LociPreview;
    [Tooltip("The list of 'Other' object previews")]
    public List<Texture2D> LociOtherPreview;
    [Tooltip("The list of 'Food' object previews")]
    public List<Texture2D> LociFoodPreview;
    [Tooltip("The list of 'Cars' object previews")]
    public List<Texture2D> LociCarsPreview;
    [Tooltip("The list of 'People' object previews")]
    public List<Texture2D> LociPeoplePreview;
    [Tooltip("The list of 'Animals' object previews")]
    public List<Texture2D> LociAnimalsPreview;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set up the preview images
        LociPreview = new List<Texture2D>();
        LociOtherPreview = new List<Texture2D>();
        LociFoodPreview = new List<Texture2D>();
        LociCarsPreview = new List<Texture2D>();
        LociPeoplePreview = new List<Texture2D>();
        LociAnimalsPreview = new List<Texture2D>();
        RuntimePreviewGenerator.OrthographicMode = true;
        foreach (GameObject loci in LociObjects)
        {
            LociPreview.Add(RuntimePreviewGenerator.GenerateModelPreview(loci.transform));
        }
        foreach (GameObject other in LociOther)
        {
            LociOtherPreview.Add(RuntimePreviewGenerator.GenerateModelPreview(other.transform));
        }
        foreach (GameObject food in LociFood)
        {
            LociFoodPreview.Add(RuntimePreviewGenerator.GenerateModelPreview(food.transform));
        }
        foreach (GameObject cars in LociCars)
        {
            LociCarsPreview.Add(RuntimePreviewGenerator.GenerateModelPreview(cars.transform));
        }
        foreach (GameObject people in LociPeople)
        {
            LociPeoplePreview.Add(RuntimePreviewGenerator.GenerateModelPreview(people.transform));
        }
        foreach (GameObject animals in LociAnimals)
        {
            LociAnimalsPreview.Add(RuntimePreviewGenerator.GenerateModelPreview(animals.transform));
        }
        LoadLoci(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Loads the loci objects into the menu
    /// </summary>
    /// <param name="value">The value of the "Alle Objekte" button</param>
    public void LoadLoci(bool value)
    {
        Title.GetComponent<TextMeshProUGUI>().text = "Alle Objekte";
        foreach (Transform child in Content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject loci in LociObjects)
        {
            GameObject currentButton = Instantiate(Button, Content.transform);
            currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = loci.name;
            currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = LociPreview[LociObjects.IndexOf(loci)];
            currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddLoci(loci);});
        }
    }

    /// <summary>
    /// Loads the 'Other' objects into the menu
    /// </summary>
    /// <param name="value">The value of the "Sonstige" button</param>
    public void LoadOther(bool value)
    {
        Title.GetComponent<TextMeshProUGUI>().text = "Sonstige";
        foreach (Transform child in Content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject other in LociOther)
        {
            GameObject currentButton = Instantiate(Button, Content.transform);
            currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = other.name;
            currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = LociOtherPreview[LociOther.IndexOf(other)];
            currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddLoci(other);});
        }
    }

    /// <summary>
    /// Loads the 'Food' objects into the menu
    /// </summary>
    /// <param name="value">The value of the "Essen" button</param>
    public void LoadFood(bool value)
    {
        Title.GetComponent<TextMeshProUGUI>().text = "Essen";
        foreach (Transform child in Content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject food in LociFood)
        {
            GameObject currentButton = Instantiate(Button, Content.transform);
            currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = food.name;
            currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = LociFoodPreview[LociFood.IndexOf(food)];
            currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddLoci(food);});
        }
    }

    /// <summary>
    /// Loads the 'Cars' objects into the menu
    /// </summary>
    /// <param name="value">The value of the "Fahrzeuge" button</param>
    public void LoadCars(bool value)
    {
        Title.GetComponent<TextMeshProUGUI>().text = "Fahrzeuge";
        foreach (Transform child in Content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject car in LociCars)
        {
            GameObject currentButton = Instantiate(Button, Content.transform);
            currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = car.name;
            currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = LociCarsPreview[LociCars.IndexOf(car)];
            currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddLoci(car);});
        }
    }

    /// <summary>
    /// Loads the 'People' objects into the menu
    /// </summary>
    /// <param name="value">The value of the "Menschen" button</param>
    public void LoadPeople(bool value)
    {
        Title.GetComponent<TextMeshProUGUI>().text = "Menschen";
        foreach (Transform child in Content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject person in LociPeople)
        {
            GameObject currentButton = Instantiate(Button, Content.transform);
            currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = person.name;
            currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = LociPeoplePreview[LociPeople.IndexOf(person)];
            currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddLoci(person);});
        }
    }

    /// <summary>
    /// Loads the 'Animals' objects into the menu
    /// </summary>
    /// <param name="value">The value of the "Tiere" button</param>
    public void LoadAnimals(bool value)
    {
        Title.GetComponent<TextMeshProUGUI>().text = "Tiere";
        foreach (Transform child in Content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject animal in LociAnimals)
        {
            GameObject currentButton = Instantiate(Button, Content.transform);
            currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = animal.name;
            currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = LociAnimalsPreview[LociAnimals.IndexOf(animal)];
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
}
