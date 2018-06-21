using System;

public class PlayerPrefsKeyAttribute : Attribute
{
    public string Keys { get;}

    public PlayerPrefsKeyAttribute(string InKey)
    {
        Keys = InKey;
    }
}
