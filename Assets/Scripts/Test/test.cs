using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    [XLua.CSharpCallLua]
    public delegate double LuaMax(double a, double b);

    // Start is called before the first frame update
    void Start()
    {
        XLua.LuaEnv luaEnv = new XLua.LuaEnv();
        var max = luaEnv.Global.GetInPath<LuaMax>("math.max");
        Debug.Log("max:" + max(32, 12));
        // luaEnv.DoString("CS.UnityEngine.Debug.Log('hello world')");
        luaEnv.Dispose();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
