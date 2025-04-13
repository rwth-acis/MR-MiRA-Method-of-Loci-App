using UnityEngine;

public class DevMenuController : MonoBehaviour
{
    [Tooltip("The RoomManager instance")]
    public RoomManager roomManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        //Get the RoomManager instance
        roomManager = RoomManager.Instance;
    }

    /// <summary>
    /// Calls the CreateRoom method in the room manager
    /// </summary>
    /// <param name="value">Value of the corresponding button</param>
    public void CreateRoom(bool value)
    {
        roomManager.CreateRoom(value);
    }

    /// <summary>
    /// Calls the NextScene method in the room manager
    /// </summary>
    /// <param name="value">Value of the corresponding button</param>
    public void NextScene(bool value)
    {
        roomManager.NextRoom(value);
    }

    /// <summary>
    /// Calls the PreviousScene method in the room manager
    /// </summary>
    /// <param name="value">Value of the corresponding button</param>
    public void PreviousScene(bool value)
    {
        roomManager.PreviousRoom(value);
    }

    /// <summary>
    /// Calls the OpenLociMenu method in the room manager
    /// </summary>
    /// <param name="value">Value of the corresponding button</param>
    public void OpenLociMenu(bool value)
    {
        roomManager.OpenLociMenu(value);
    }

    /// <summary>
    /// Calls the NextWallColour method in the room manager
    /// </summary>
    /// <param name="value">Value of the corresponding button</param>
    public void NextWallColour(bool value)
    {
        roomManager.NextWallColour(value);
    }

    /// <summary>
    /// Calls the PreviousWallColour method in the room manager
    /// </summary>
    /// <param name="value">Value of the corresponding button</param>
    public void PreviousWallColour(bool value)
    {
        roomManager.PreviousWallColour(value);
    }

    /// <summary>
    /// Calls the NextFurniture method in the room manager
    /// </summary>
    /// <param name="value">Value of the corresponding button</param>
    public void NextFurniture(bool value)
    {
        roomManager.NextFurniture(value);
    }

    /// <summary>
    /// Calls the PreviousFurniture method in the room manager
    /// </summary>
    /// <param name="value">Value of the corresponding button</param>
    public void PreviousFurniture(bool value)
    {
        roomManager.PreviousFurniture(value);
    }

    /// <summary>
    /// Calls the SelectFurniture method in the room manager
    /// </summary>
    /// <param name="value">Value of the corresponding button</param>
    public void SelectFurniture(bool value)
    {
        roomManager.SelectFurniture(value);
    }

    /// <summary>
    /// Calls the FurniturePhase method in the room manager
    /// </summary>
    /// <param name="value">Value of the corresponding button</param>
    public void FurniturePhase(bool value)
    {
        roomManager.FurniturePhase(value);
    }

    /// <summary>
    /// Calls the ListPhase method in the room manager
    /// </summary>
    /// <param name="value">Value of the corresponding button</param>
    public void ListPhase(bool value)
    {
        roomManager.ListPhase(value);
    }

    /// <summary>
    /// Calls the StoryPhase method in the room manager
    /// </summary>
    /// <param name="value">Value of the corresponding button</param>
    public void StoryPhase(bool value)
    {
        roomManager.StoryPhase(value);
    }

    /// <summary>
    /// Calls the NumberPhase method in the room manager
    /// </summary>
    /// <param name="value">Value of the corresponding button</param>
    public void NumberPhase(bool value)
    {
        roomManager.NumberPhase(value);
    }

    /// <summary>
    /// Calls the button Confirmation method in the room manager to confirm the placement of the current object
    /// </summary>
    /// <param name="value">Value of the corresponding button</param>
    public void OnConfirmButtonClick(bool value)
    {
        roomManager.OnConfirmButtonClick(value);
    }

    /// <summary>
    /// Deletes the current user's json file
    /// </summary>
    /// <param name="value">The value of the corresponding button</param>
    public void DeleteJSON(bool value)
    {
        roomManager.DeleteJSON(value);
    }

    /// <summary>
    /// Closes the dev menu
    /// </summary>
    /// <param name="value">The value of the corresponding button</param>
    public void CloseMenu(bool value)
    {
        roomManager.CloseDevMenu(value);
    }

    /// <summary>
    /// Opens the furniture menu
    /// </summary>
    /// <param name="value">The value of the corresponding button</param>
    public void OpenFurnitureMenu(bool value)
    {
        roomManager.OpenFurnitureMenu(value);
    }
}
