using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class InputPoll : MonoBehaviour
{
    [HideInInspector] public InputHandler _input;
    public PauseMenuControl menu;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI CounterText;
    enum ActionMap { Axis, Button } //Determines what action mapping we are looking for
    ActionMap actionMap;
    bool IsGamepadInput; //Determines if we are looking for keyboard or gamepad input
    bool IsPositive; //Determines if we are looking for the positive input on digital axes
    int EditingIndex;
    public float InputTimeout = 5;
    float t;
    bool IsPolling;

    private void Update()
    {
        if (IsPolling)
        {
            t -= Time.unscaledDeltaTime;
            CounterText.text = t.ToString("F0");
            if (t <= 0)
            {
                menu.SetMenu(4);
                IsPolling = false;
            }
            else
            {
                switch (actionMap)
                {
                    case ActionMap.Axis:
                        if (IsGamepadInput)
                        {
                            for (int i = 0; i < 9; i++)
                            {
                                if (Input.GetAxis("joystick axis " + i) != 0)
                                {
                                    _input.axisInputs[EditingIndex].GamepadAxis = "joystick axis " + i;
                                    menu.SetMenu(4);
                                    IsPolling = false;
                                }
                            }
                        }
                        else
                        {
                            foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
                            {
                                if (Input.GetKeyDown(k))
                                {
                                    if (IsPositive)
                                    {
                                        string inp = k.ToString().ToLower();
                                        //Add an extra check to remove "arrow" so arrow keys work
                                        if (inp.Contains("arrow"))
                                            inp = inp.Replace("arrow", "");
                                        _input.axisInputs[EditingIndex].Positive = inp;
                                        
                                    } else
                                    {
                                        string inp = k.ToString().ToLower();
                                        //Add an extra check to remove "arrow" so arrow keys work
                                        if (inp.Contains("arrow"))
                                            inp = inp.Replace("arrow", "");
                                        _input.axisInputs[EditingIndex].Negative = inp;
                                    }
                                    menu.SetMenu(4);
                                    IsPolling = false;
                                }
                            }
                        }
                        break;
                    case ActionMap.Button:
                        if (IsGamepadInput)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                if (Input.GetKeyDown("joystick button " + i))
                                {
                                    _input.buttonInputs[EditingIndex].GamepadInput = "joystick button " + i;
                                    menu.SetMenu(4);
                                    IsPolling = false;
                                }
                            }
                        }
                        else
                        {
                            foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
                            {
                                if (Input.GetKeyDown(k))
                                {
                                    _input.buttonInputs[EditingIndex].GamepadInput = k.ToString().ToLower();
                                    menu.SetMenu(4);
                                    IsPolling = false;
                                }
                            }
                        }
                        break;
                }
            }
        }
    }

    public void BeginButtonInputPoll(bool isGamepad, int buttonIndex)
    {
        actionMap = ActionMap.Button;
        IsGamepadInput = isGamepad;
        EditingIndex = buttonIndex;
        t = InputTimeout;
        TitleText.text = "Press a " + (isGamepad ? "button " : "key ") + "to assign to " + _input.buttonInputs[buttonIndex].Name + ".";
        menu.SetMenu(5);
        IsPolling = true;
    }

    public void BeginDigitalAxisPoll(bool positive, int axisIndex)
    {
        actionMap = ActionMap.Axis;
        IsGamepadInput = false;
        IsPositive = positive;
        EditingIndex = axisIndex;
        TitleText.text = "Press a key to assign to " + (positive ? "Positive " : "Negative ") + _input.axisInputs[axisIndex].Name + ".";
        t = InputTimeout;
        menu.SetMenu(5);
        IsPolling = true;
    }
    public void BeginAnalogAxisPoll(int axisIndex)
    {
        actionMap = ActionMap.Axis;
        IsGamepadInput = true;
        EditingIndex = axisIndex;
        TitleText.text = "Move an axis to assign to "  + _input.axisInputs[axisIndex].Name + ".";
        t = InputTimeout;
        menu.SetMenu(5);
        IsPolling = true;
    }


}
