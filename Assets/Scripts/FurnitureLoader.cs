using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FurnitureLoader : MonoBehaviour
{
    [FormerlySerializedAs("Button")] [Tooltip("The button prefab to spawn for each furniture object")]
    public GameObject button;
    [FormerlySerializedAs("Content")] [Tooltip("The content object to spawn the buttons in")]
    public GameObject content;
    [FormerlySerializedAs("Title")] [Tooltip("The title object to change the title of the menu")]
    public GameObject title;

    [FormerlySerializedAs("FurnitureObjects")] [Tooltip("The list of furniture objects")]
    public List<GameObject> furnitureObjects;

    [FormerlySerializedAs("FurniturePreview")] [Tooltip("The list of furniture object previews")]
    public List<Texture2D> furniturePreview;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Set up the preview images
        furniturePreview = new List<Texture2D>();

        RuntimePreviewGenerator.OrthographicMode = true;
        foreach (GameObject furniture in furnitureObjects)
        {
            furniturePreview.Add(RuntimePreviewGenerator.GenerateModelPreview(furniture.transform));
        }
        LoadFurniture(true);
    }

    /// <summary>
    /// Loads the furniture objects into the menu
    /// </summary>
    /// <param name="value">The value of the "Alle Objekte" button</param>
    public void LoadFurniture(bool value)
    {
        title.GetComponent<TextMeshProUGUI>().text = "Alle Möbel";
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject furniture in furnitureObjects)
        {
            GameObject currentButton = Instantiate(button, content.transform);
            currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = furniture.name;
            currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = furniturePreview[furnitureObjects.IndexOf(furniture)];
            currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddFurniture(furniture);});
        }
    }
    /// <summary>
    /// Loads the beds into the menu
    /// </summary>
    /// <param name="value">The value of the "Betten" button</param>
    public void LoadBeds(bool value)
    {
        title.GetComponent<TextMeshProUGUI>().text = "Betten";
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject furniture in furnitureObjects)
        {
            if (furniture.name.Contains("Bett"))
            {
                GameObject currentButton = Instantiate(button, content.transform);
                currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = furniture.name;
                currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = furniturePreview[furnitureObjects.IndexOf(furniture)];
                currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddFurniture(furniture);});
            }
        }
    }

    /// <summary>
    /// Load the benches into the menu
    /// </summary>
    /// <param name="value">The value of the "Bänke" button</param>
    public void LoadBenches(bool value)
    {
        title.GetComponent<TextMeshProUGUI>().text = "Bänke";
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject furniture in furnitureObjects)
        {
            if (furniture.name.Contains("Bank"))
            {
                GameObject currentButton = Instantiate(button, content.transform);
                currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = furniture.name;
                currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = furniturePreview[furnitureObjects.IndexOf(furniture)];
                currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddFurniture(furniture);});
            }
        }
    }

    /// <summary>
    /// Loads the tables into the menu
    /// </summary>
    /// <param name="value">The value of the "Tische" button</param>
    public void LoadTables(bool value)
    {
        title.GetComponent<TextMeshProUGUI>().text = "Tische";
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject furniture in furnitureObjects)
        {
            if (furniture.name.Contains("Tisch"))
            {
                GameObject currentButton = Instantiate(button, content.transform);
                currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = furniture.name;
                currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = furniturePreview[furnitureObjects.IndexOf(furniture)];
                currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddFurniture(furniture);});
            }
        }
    }

    /// <summary>
    /// Loads the wardrobes into the menu
    /// </summary>
    /// <param name="value">The value of the "Schränke" button</param>
    public void LoadWardrobes(bool value)
    {
        title.GetComponent<TextMeshProUGUI>().text = "Schränke";
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject furniture in furnitureObjects)
        {
            if (furniture.name.Contains("Schrank"))
            {
                GameObject currentButton = Instantiate(button, content.transform);
                currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = furniture.name;
                currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = furniturePreview[furnitureObjects.IndexOf(furniture)];
                currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddFurniture(furniture);});
            }
        }
    }

    /// <summary>
    /// Load the dressers into the menu
    /// </summary>
    /// <param name="value">The value of the "Kommoden" button</param>
    public void LoadDressers(bool value)
    {
        title.GetComponent<TextMeshProUGUI>().text = "Kommoden";
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject furniture in furnitureObjects)
        {
            if (furniture.name.Contains("Kommode"))
            {
                GameObject currentButton = Instantiate(button, content.transform);
                currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = furniture.name;
                currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = furniturePreview[furnitureObjects.IndexOf(furniture)];
                currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddFurniture(furniture);});
            }
        }
    }

    /// <summary>
    /// Load the chairs into the menu
    /// </summary>
    /// <param name="value">The value of the "Stühle" button</param>
    public void LoadChairs(bool value)
    {
        title.GetComponent<TextMeshProUGUI>().text = "Stühle";
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject furniture in furnitureObjects)
        {
            if (furniture.name.Contains("Stuhl"))
            {
                GameObject currentButton = Instantiate(button, content.transform);
                currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = furniture.name;
                currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = furniturePreview[furnitureObjects.IndexOf(furniture)];
                currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddFurniture(furniture);});
            }
        }
    }

    /// <summary>
    /// Load the shelves into the menu
    /// </summary>
    /// <param name="value">The value of the "Regale" button</param>
    public void LoadShelves(bool value)
    {
        title.GetComponent<TextMeshProUGUI>().text = "Regale";
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject furniture in furnitureObjects)
        {
            if (furniture.name.Contains("Regal"))
            {
                GameObject currentButton = Instantiate(button, content.transform);
                currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = furniture.name;
                currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = furniturePreview[furnitureObjects.IndexOf(furniture)];
                currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddFurniture(furniture);});
            }
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
        foreach (GameObject furniture in furnitureObjects)
        {
            if (!furniture.name.Contains("Bett") && !furniture.name.Contains("Bank") && !furniture.name.Contains("Tisch") && !furniture.name.Contains("Schrank") && !furniture.name.Contains("Kommode") && !furniture.name.Contains("Stuhl") && !furniture.name.Contains("Regal"))
            {
                GameObject currentButton = Instantiate(button, content.transform);
                currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = furniture.name;
                currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = furniturePreview[furnitureObjects.IndexOf(furniture)];
                currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddFurniture(furniture);});
            }
        }
    }

    public void LoadImages(bool value)
    {
        title.GetComponent<TextMeshProUGUI>().text = "Bilder";
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject furniture in furnitureObjects)
        {
            if (furniture.name.Contains("Bild"))
            {
                GameObject currentButton = Instantiate(button, content.transform);
                currentButton.transform.Find("Content/Background/Elements/Label").GetComponentInChildren<TextMeshProUGUI>().text = furniture.name;
                currentButton.transform.Find("Content/Background/Elements/Space").GetComponent<RawImage>().texture = furniturePreview[furnitureObjects.IndexOf(furniture)];
                currentButton.transform.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RoomManager.Instance.AddFurniture(furniture);});
            }
        }
    }
    /// <summary>
    /// Deactivates the Furniture Menu
    /// </summary>
    /// <param name="value">The value of the "X" button</param>
    public void CloseMenu(bool value)
    {
        //Deactivate the furniture menu
        gameObject.SetActive(false);
    }
}
