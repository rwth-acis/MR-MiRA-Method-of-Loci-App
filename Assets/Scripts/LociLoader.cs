using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LociLoader : MonoBehaviour
{
    public GameObject Button;
    public GameObject Content;
    public GameObject Title;

    public List<GameObject> LociObjects;
    public List<GameObject> LociOther;
    public List<GameObject> LociFood;
    public List<GameObject> LociCars;
    public List<GameObject> LociPeople;
    public List<GameObject> LociAnimals;

    public List<Texture2D> LociPreview;
    public List<Texture2D> LociOtherPreview;
    public List<Texture2D> LociFoodPreview;
    public List<Texture2D> LociCarsPreview;
    public List<Texture2D> LociPeoplePreview;
    public List<Texture2D> LociAnimalsPreview;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LociPreview = new List<Texture2D>();
        LociOtherPreview = new List<Texture2D>();
        LociFoodPreview = new List<Texture2D>();
        LociCarsPreview = new List<Texture2D>();
        LociPeoplePreview = new List<Texture2D>();
        LociAnimalsPreview = new List<Texture2D>();
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

    public void LoadCars(bool value)
    {
        Title.GetComponent<TextMeshProUGUI>().text = "Autos";
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
}
