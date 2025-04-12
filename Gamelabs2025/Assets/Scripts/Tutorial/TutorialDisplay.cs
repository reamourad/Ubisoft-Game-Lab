using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class RoleBasedTutorialUI : MonoBehaviour
{

    [Header("Role")]
    public PlayerRole playerRole;

    [Header("Input Actions")]
    public InputActionReference pickUpAction;
    public InputActionReference releaseAction;
    public InputActionReference useItemAction;
    public InputActionReference scanAction;
    public InputActionReference openDoorAction;

    [Header("UI Elements")]
    public TMP_Text Page1Text;
    public TMP_Text Page2Text;
    public TMP_Text Page3Text;
    public TMP_Text Page4Text;

    void Start()
    {
        ShowTutorial();
    }

    private void ShowTutorial()
    {
        string pickUp = pickUpAction.action.GetBindingDisplayString();
        string release = releaseAction.action.GetBindingDisplayString();
        string useItem = useItemAction.action.GetBindingDisplayString();
        string scan = scanAction.action.GetBindingDisplayString();
        string openDoor = openDoorAction.action.GetBindingDisplayString();

        if (playerRole == PlayerRole.Hider)
        {
            Page1Text.text =
                $"Your goal is to hide long enough until the house kicks the ghost hunter out.\n" +
                $"The house is already angry you have <color=red>(5 minutes)</color> to hide.\n" +
                $"The house is very sensitive to noise, set traps for the ghost hunter that will trigger noise.\n" +
                $"this takes <color=red>(30 seconds)</color> off the timer.\n\n";

            Page2Text.text =
                $"Press <color=red>({pickUp})</color> to pick up an object, it will show you in blue where it would be placed, move the camera to put it in your desired location.s\n" +
                $"Press <color=red>({release})</color> again to release the object.\n\n";

            Page3Text.text =
                $"Smoke bombs under smoke detectors will make noise (reaction object).\n" +
                $"Fan under windchimes will make noise (reaction object).\n" +
                $"Jack in the box automatically makes noises.\n\n";
            Page4Text.text =
                $"You need the seeker to fall into a trigger item that is connected to a reaction object.\n" +
                $"To connect both, make sure your object is within the blue area of another.\n\n" +

                $"To find objects, press <color=red>({scan})</color> to scan a small area.\n" +
                $"Press <color=red>({openDoor})</color> to open doors.";
        }
        else if (playerRole == PlayerRole.Seeker)
        {
            Page1Text.text =
                $"Your goal is to find the ghost and vacuum him before the house kicks you out\n" +
                $"The house is already angry you have <color=red>(5 minutes)</color> to find the ghost\n" +
                $"The house is very sensitive to noise, the ghost is laying traps around the house. If you fall for them, you lose <color=red>(30 seconds)</color>\n\n";
            Page2Text.text =
                $"You have access to items in your room. Press <color=red>({pickUp})</color> to pick them up\n" +
                $"To use an item press <color=red>({useItem})</color>\n\n";
            Page3Text.text =
                $"The vacuum sucks the ghost in and you win the game\n" +
                $"The thermometer shows you if the ghost is in the direction you are pointing. When successful, it shows <color=green>\"low\"</color> and an approximate direction\n" +
                $"You can view the cameras around the house with the tablet\n" +
                $"Press <color=red>({openDoor})</color> to open door";
        }
    }


    public enum PlayerRole
    {
        Hider,
        Seeker
    }

}
