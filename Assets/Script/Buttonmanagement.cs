using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public Button[] buttons;          // 拖入所有按钮
    public Color pressedColor = Color.grey; // 按下颜色
    public Color defaultColor = Color.white; // 默认颜色

    private Button lastPressedButton; // 记录上一个按下的按钮

    void Start()
    {
        // 为每个按钮添加点击事件
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => OnButtonClicked(btn));
            SetButtonColor(btn, defaultColor); // 初始化颜色
        }
    }

    private void OnButtonClicked(Button currentButton)
    {
        // 恢复上一个按钮的默认颜色
        if (lastPressedButton != null && lastPressedButton != currentButton)
        {
            SetButtonColor(lastPressedButton, defaultColor);
        }

        // 设置当前按钮为按下颜色
        SetButtonColor(currentButton, pressedColor);
        lastPressedButton = currentButton; // 更新最后按下的按钮
    }

    private void SetButtonColor(Button button, Color color)
    {
        // 获取按钮的图像组件
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = color;
        }
    }
}