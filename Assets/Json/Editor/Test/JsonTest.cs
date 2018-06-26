using System;
using ExcelConverter.Json.DataTemplate;
using LitJson;
using UnityEditor;
using UnityEngine;

namespace ExcelConverter.Json.Test
{
    public class NewClass
    {
        [MenuItem("Tools/ExcelToJson/Test")]
        static void Test()
        {
            JsonData data = JsonMapper.ToObject("{\"Data\":[20500005,\"Card_Back\",\"Card_Back_1\",0,0,0]}");

            Item item = new Item(data["Data"]);
            Debug.LogError(item);
        }
    }
}
