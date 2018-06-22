using System;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using UnityEditor;
using UnityEngine;

namespace ExcelConverter.Excel.Editor
{
    public class ExcelData
    {
        /// <summary>
        /// name of exce or sheet.
        /// </summary>
        public string SheetName;

        /// <summary>
        /// The number of rows in the data header, always 3.
        /// 0. Property definitions
        /// 1. Property types
        /// 2. Property comments
        /// </summary>
        public int HeadRowLen;

        /// <summary>
        /// The number of rows in the data body.
        /// </summary>
        public int BodyRowLen;

        /// <summary>
        /// The number of column in the data.
        /// </summary>
        public int DataColumnLen;

        /// <summary>
        /// Property definitions, types, and comments.
        /// </summary>
        public ICell[][] Head;

        /// <summary>
        /// Data array.
        /// </summary>
        public ICell[][] Body;

        /// <summary>
        /// Check and correct data.
        /// </summary>
        /// <returns>Is the data available?</returns>
        public bool CheckData()
        {
            if (string.IsNullOrEmpty(SheetName)) return false;

            if (HeadRowLen == 0 || BodyRowLen == 0 || BodyRowLen == 0) return false;

            if (Head == null || Head[0] == null || Head[1] == null) return false;

            List<int> invalidRows = null, invalidClouns = null;
            //Check header definition, data can be empty but header definition cannot be null
            bool isInvalidData;
            int realDataColumnLen = DataColumnLen;
            for (int i = 0; i < realDataColumnLen; ++i)
            {
                // 0. Property definitions, Can not be empty.
                // 1. Property types, Can not be empty.
                // 2. Property comments, Can be empty.
                if (Head[0][i] == null || Head[1][i] == null) isInvalidData = true;
                else if (Head[0][i].CellType == CellType.String && Head[1][i].CellType == CellType.String)
                    isInvalidData = string.IsNullOrEmpty(Head[0][i].StringCellValue) ||
                                    string.IsNullOrEmpty(Head[1][i].StringCellValue);
                else
                    isInvalidData = true;

                //The column data is missing the attribute name or type and is overwritten with the left data
                if (isInvalidData)
                {
                    if(null == invalidClouns) invalidClouns = new List<int>(realDataColumnLen - i);
                    invalidClouns.Add(i);

                    --realDataColumnLen;

                    for (int j = i; j < realDataColumnLen; ++j)
                    {
                        for (int k = 0; k < HeadRowLen; ++k)
                        {
                            Head[k][j] = Head[k][j + 1];
                        }

                        for (int k = 0; k < BodyRowLen; ++k)
                        {
                            Body[k][j] = Body[k][j + 1];
                        }
                    }
                }
            }

            int realBodyRowLen = BodyRowLen;
            int invalidDataCount = 0;
            for (int i = 0; i < realBodyRowLen; ++i)
            {
                isInvalidData = false;
                if (null == Body[i]) isInvalidData = true;
                else
                {
                    for (int j = 0; j < realDataColumnLen; ++j)
                    {
                        if (null == Body[i][j]) invalidDataCount += 1;
                        else
                        {
                            switch (Body[i][j].CellType)
                            {
                                case CellType.Numeric:
                                    invalidDataCount += Body[i][j].NumericCellValue - 0 == double.Epsilon ? 1 : 0;
                                    break;

                                case CellType.String:
                                    invalidDataCount += string.IsNullOrEmpty(Body[i][j].StringCellValue) ? 1 : 0;
                                    break;

                                case CellType.Formula:
                                case CellType.Unknown:
                                case CellType.Error:
                                case CellType.Blank:
                                    invalidDataCount += 1;
                                    break;
                            }
                        }

                        if (invalidDataCount == j + 1) isInvalidData = true;
                        else break;
                    }
                }

                if (isInvalidData)
                {
                    if(null == invalidRows) invalidRows = new List<int>(realBodyRowLen - i);
                    invalidRows.Add(i + 1 + 3);//The row number index starts at 0 and the Excel index starts at 1. Header file retains three lines.

                    --realBodyRowLen;
                    for (int j = i; j < realBodyRowLen; ++j)
                    {
                        for (int k = 0; k < realDataColumnLen; ++k)
                        {
                            Body[j][k] = Body[j + 1][k];
                        }
                    }
                }
            }

            if (realDataColumnLen != DataColumnLen)
            {
                EditorUtility.DisplayDialog("Warning",
                    SheetName + " The second column is missing the attribute name or the type is covered by the right column", "Confirm");

                DataColumnLen = realDataColumnLen;
                for (int i = 0; i < HeadRowLen; ++i)
                {
                   Array.Resize(ref Head[i], DataColumnLen);
                }

                for (int i = 0; i < realDataColumnLen; ++i)
                {
                    Array.Resize(ref Body[i], DataColumnLen);
                }
            }

            if (realBodyRowLen != BodyRowLen)
            {
                EditorUtility.DisplayDialog("Warning",
                   "All data in a row in the " + SheetName +" is the default value and has been ignored.", "Confirm");

                BodyRowLen = realBodyRowLen;

                Array.Resize(ref Body, BodyRowLen);
            }

            Debug.LogError("Head row = " + Head.Length + "    Cloumn = " + Head[0].Length);
            Debug.LogError("Body row = " + Body.Length + "    Cloumn = " + Body[0].Length);

            return true;
        }
    }
}


