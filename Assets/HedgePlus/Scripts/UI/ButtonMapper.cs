using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ButtonMapper : MonoBehaviour
{
    InputHandler _input;
    public InputPoll poll;
    public int EditIndex;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI KeyboardName;
    public TextMeshProUGUI GamepadName;

    public Button KeyboardPoll;
    public Button GamepadPoll;

    private void Start()
    {
        _input = InputHandler.instance;
    }

    private void Update()
    {
        KeyboardPoll.onClick.AddListener(PollKeyboardInput);
        GamepadPoll.onClick.AddListener(PollGamepadInput);

        KeyboardName.text = _input.buttonInputs[EditIndex].KeyboardInput;
        GamepadName.text = _input.buttonInputs[EditIndex].GamepadInput;
    }

    void PollKeyboardInput()
    {
        poll.BeginButtonInputPoll(false, EditIndex);
    }
    void PollGamepadInput()
    {
        poll.BeginButtonInputPoll(true, EditIndex);
    }
}
