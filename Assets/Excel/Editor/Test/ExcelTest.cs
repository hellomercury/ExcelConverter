using System.Collections;
using System.Collections.Generic;
using ExcelConverter.Excel.Editor;
using UnityEditor;
using UnityEngine;

namespace ExcelConverter.Excel.Test
{
    public class ExcelTest : MonoBehaviour
    {
        [MenuItem("Tools/Excel/ReadTest")]
       static void ReadTest()
        {
            string excelPath = @"D:\Unity\Self\ExcelConverter\Assets\Excel\Editor\Excel\Item.xlsx";
            ExcelData[] data = ExcelReader.GetSingleExcelData(excelPath);

            int length = data.Length;
            for (int i = 0; i < length; ++i)
            {
                Debug.LogError("Excel or Sheet name = " + data[i].SheetName);

                for (int j = 0; j < data[i].DataColumnLen; ++j)
                {
                    string headStr = string.Empty;
                    for (int k = 0; k < data[i].HeadRowLen; ++k)
                    {
                    }
                }
            }
        }

        static string getStr(string InStr, int InLen)
        {
            int length = InStr.Length;

            if (length >= InLen) return InStr;
            else
            {
                length = InLen - length;
                for (int i = 0; i < length; ++i)
                {
                    InStr = " " + InStr;
                }
            }
            return InStr;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

