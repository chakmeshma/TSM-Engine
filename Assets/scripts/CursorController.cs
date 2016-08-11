using UnityEngine;
using System.Collections;

public class CursorController : MonoBehaviour
{
    static private CursorController _instance;
    static public CursorController instance
    {
        get
        {
            return _instance;
        }
    }

    private CursorType _lastSetCursor = CursorType.Default;

    public CursorType lastSetCursor
    {
        get
        {
            return _lastSetCursor;
        }
    }
    public enum CursorType { Default = -1, ResizeVertical, ResizeHorizontal, ResizeSlash, ResizeBackslash, Move }
    public Texture2D[] cursorTextures;

    void Awake()
    {
        _instance = this;
    }

    public void SetCursor(CursorType cursorType)
    {
        if(cursorType == CursorType.Default)
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        else
            Cursor.SetCursor(cursorTextures[(int)cursorType], new Vector2(50.0F, 50.0F), CursorMode.Auto);

        _lastSetCursor = cursorType;
    }

    public void ResetCursor()
    {
        SetCursor(CursorType.Default);
    }
}
