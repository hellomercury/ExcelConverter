using System;
using ExcelConverter.Excel.Editor;
using LitJson;
using UnityEditor;
using UnityEngine;

namespace ExcelConverter.Json.Editor
{
    public class ConvertExcelToJson
    {
        [MenuItem("Tools/ExcelToJson/Convert")]
        public static void Init()
        {
            string excelPath = Application.dataPath + "/Excel/Editor/Excel/Item.xlsx";
            ExcelData[] excelData = ExcelReader.GetSingleExcelData(excelPath);


            for (int i = 0; i < excelData.Length; i++)
            {
                Debug.LogError(excelData[i]);

                JsonData jsonData = new JsonData();
                jsonData["Name"] = excelData[i].SheetName;
                jsonData["Header"] = new JsonData();
                jsonData["Header"]["Name"] = new JsonData();
                jsonData["Header"]["Type"] = new JsonData();
                jsonData["Header"]["Annotation"] = new JsonData();

                int headerRow = excelData[i].HeadRowLen;
                int dataColumnLen = excelData[i].DataColumnLen;
                for (int j = 0; j < dataColumnLen; j++)
                {
                    jsonData["Header"]["Name"].Add(excelData[i].Head[0][j].StringCellValue);
                    jsonData["Header"]["Type"].Add(excelData[i].Head[1][j].StringCellValue);
                    if(headerRow == 3)jsonData["Header"]["Annotation"].Add(excelData[i].Head[2][j].StringCellValue);
                }

                int bodyRow = excelData[i].BodyRowLen;
                jsonData["Body"] = new JsonData();
                for (int j = 0; j < bodyRow; j++)
                {
                    JsonData rowJsonData = new JsonData();
                    for (int k = 0; k < dataColumnLen; k++)
                    {
                        switch ((string)jsonData["Header"]["Type"][k])
                        {
                            case "int":
                                switch (excelData[i].Body[j][k].CellType)
                                {
                                    case NPOI.SS.UserModel.CellType.Boolean:
                                        rowJsonData.Add(excelData[i].Body[j][k].BooleanCellValue ? 1 : 0);
                                        break;

                                    
                                    case NPOI.SS.UserModel.CellType.Numeric:
                                        rowJsonData.Add((int)excelData[i].Body[j][k].NumericCellValue);
                                        break;

                                    case NPOI.SS.UserModel.CellType.String:
                                        string cellData = excelData[i].Body[j][k].StringCellValue;
                                        Debug.LogError(cellData);
                                        rowJsonData.Add(string.IsNullOrEmpty(cellData) ? 0 : Convert.ToInt32(cellData));
                                        break;

                                    case NPOI.SS.UserModel.CellType.Error:

                                    case NPOI.SS.UserModel.CellType.Formula:

                                    case NPOI.SS.UserModel.CellType.Blank:
                                      
                                    case NPOI.SS.UserModel.CellType.Unknown:
                                        rowJsonData.Add(0);
                                        break;
                                }

                            break;

                            case "string":
                                rowJsonData.Add(excelData[i].Body[j][k] == null ? "" : excelData[i].Body[j][k].StringCellValue);
                                break;

                            case "float":
                                break;
                        }
                    }
                    jsonData["Body"].Add(rowJsonData);
                }

                Debug.LogError(jsonData.ToJson());

            }


        }

    } 
}

