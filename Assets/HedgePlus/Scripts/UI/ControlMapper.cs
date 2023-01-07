using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ControlMapper : MonoBehaviour
{
    InputHandler _input;
    public PauseMenuControl menuController;
    public InputPoll poll;
    public AxisMapper AxisInputSetting;
    public ButtonMapper ButtonInputSetting;
    public float VerticalPadding;
    float ButtonStartOffset;

    private void Start()
    {
        _input = InputHandler.instance;
        poll._input = _input;
        SetAxisUI();
        SetButtonUI();
        AxisInputSetting.gameObject.SetActive(false);
        ButtonInputSetting.gameObject.SetActive(false);
    }

    private void OnEnable()
    {

    }


    public void SetAxisUI()
    {
        for (int i = 0; i < _input.axisInputs.Length; i++)
        {
            RectTransform InitialTrans = AxisInputSetting.GetComponent<RectTransform>();
            float Padding = InitialTrans.rect.height + VerticalPadding;
            AxisMapper _axis = Instantiate(AxisInputSetting);
            _axis.transform.SetParent(transform, false);
            _axis.Title.text = _input.axisInputs[i].Name;
            _axis.EditIndex = i;
            RectTransform AxisTrans = _axis.GetComponent<RectTransform>();
            AxisTrans.anchoredPosition = InitialTrans.anchoredPosition - new Vector2(0, Padding * i);
            if (i + 1 >= _input.axisInputs.Length)
            {
                ButtonStartOffset = Padding * (i + 1);
            }
        }
    }

    public void SetButtonUI()
    {
        for (int i = 0; i < _input.buttonInputs.Length; i++)
        {
            RectTransform InitialRect = ButtonInputSetting.GetComponent<RectTransform>();
            float Padding = InitialRect.rect.height + VerticalPadding;
            ButtonMapper _button = Instantiate(ButtonInputSetting);
            _button.transform.SetParent(transform, false);
            _button.EditIndex = i;
            _button.Title.text = _input.buttonInputs[i].Name;

            RectTransform ButtonRect = _button.GetComponent<RectTransform>();
            ButtonRect.anchoredPosition = InitialRect.anchoredPosition - new Vector2(0, Padding * i + ButtonStartOffset);
        }
    }
}
