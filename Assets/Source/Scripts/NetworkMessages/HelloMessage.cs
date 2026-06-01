using System;

[Serializable]
public struct HelloMessage
{
    public string Text;

    public HelloMessage(string text)
    {
        Text = text;
    }
}
