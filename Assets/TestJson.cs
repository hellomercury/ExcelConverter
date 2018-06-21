using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LitJson;
using UnityEngine;

public class TestJson : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	    //Type type = typeof (PrefKeys);
	    //FieldInfo[] fieldinfos = type.GetFields(BindingFlags.Public | BindingFlags.Static);
	    //int fieldLen = fieldinfos.Length;

	    //for (int i = 0; i < fieldLen; ++i)
	    //{
	    //    Debug.LogError(fieldinfos[i].GetRawConstantValue());

	    //    object[] attrObjs = fieldinfos[i].GetCustomAttributes(typeof (PlayerPrefsKeyAttribute), false);
     //       if(1 == attrObjs.Length) Debug.LogError((attrObjs[0] as PlayerPrefsKeyAttribute).Keys);
	    //}

        JsonData json = new JsonData();
	    for (int i = 0; i < 10; ++i)
	    {
	        JsonData da = new JsonData();
	        da.Add(((char)('A' + i)).ToString());
	        da.Add(i);
	        da.Add(UnityEngine.Random.Range(0, 5));
	        json.Add(da);
	    }

	    string str = json.ToJson();
        Debug.LogError(str);

	    json = JsonMapper.ToObject(str);
	    for (int i = 0; i < 10; ++i)
	    {
	        Debug.LogError(json[i][0] + "," + json[i][1] + "," + json[i][2]);
	    }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
