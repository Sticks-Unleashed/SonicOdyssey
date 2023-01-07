using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class AxisMapper : MonoBehaviour
{
    InputHandler _input;
    public InputPoll poll;
    public int EditIndex;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI KeyboardPositive;
    public TextMeshProUGUI KeyboardNegative;
    public TextMeshProUGUI GamepadName;

    public Button PositivePoll;
    public Button NegativePoll;
    public Button GamepadPoll;

    private void Start()
    {
        _input = InputHandler.instance;
    }

    private void Update()
    {
        PositivePoll.onClick.AddListener(PollPositive);
        NegativePoll.onClick.AddListener(PollNegative);
        GamepadPoll.onClick.AddListener(PollGamepad);

        KeyboardPositive.text = _input.axisInputs[EditIndex].Positive;
        KeyboardNegative.text = _input.axisInputs[EditIndex].Negative;
        GamepadName.text = _input.axisInputs[EditIndex].GamepadAxis;
    }

    void PollPositive()
    {
        poll.BeginDigitalAxisPoll(true, EditIndex);
    }
    void PollNegative()
    {
        poll.BeginDigitalAxisPoll(false, EditIndex);
    }
    void PollGamepad()
    {
        poll.BeginAnalogAxisPoll(EditIndex);
    }


}
