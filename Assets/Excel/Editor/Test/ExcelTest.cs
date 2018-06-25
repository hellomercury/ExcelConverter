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
            string excelPath = Application.dataPath + "/Excel/Editor/Excel/Item.xlsx";
            ExcelData[] data = ExcelReader.GetSingleExcelData(excelPath);

            int length = data.Length;
            for (int i = 0; i < length; ++i)
            {
                Debug.LogError("Excel or Sheet name = " + data[i].SheetName);

                for (int j = 0; j < data[i].HeadRowLen; ++j)
                {
                    string headStr = string.Empty;
                    for (int k = 0; k < data[i].DataColumnLen; ++k)
                    {
                        headStr += data[i].Head[j][k] + ", ";
                    }
                    Debug.LogError(headStr);
                }

                for (int j = 0; j < data[i].BodyRowLen; j++)
                {
                    string bodyStr = string.Empty;
                    for (int k = 0; k < data[i].DataColumnLen; k++)
                    {
                        bodyStr += data[i].Body[j][k] + ", ";
                    }
                    Debug.LogError(bodyStr);
                }
            }
        }
    }
}

