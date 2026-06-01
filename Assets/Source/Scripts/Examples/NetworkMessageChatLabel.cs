using TMPro;
using UnityEngine;

public sealed class NetworkMessageChatLabel : MonoBehaviour, IChatView
{
    [SerializeField] private TMP_Text _label;

    private void Start()
    {
        Clear();
    }

    public void Clear()
    {
        if (_label != null)
        {
            _label.text = string.Empty;
        }
    }

    public void AddLine(string text)
    {
        if (_label == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(_label.text))
        {
            _label.text = text;
            return;
        }

        _label.text = $"{_label.text}\n{text}";
    }
}
