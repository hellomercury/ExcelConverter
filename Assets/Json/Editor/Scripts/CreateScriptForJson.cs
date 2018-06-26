using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NPOI.SS.UserModel;
using UnityEngine;

namespace ExcelConverter.Json.Editor
{
    public class CreateScriptForJson
    {

        private static void Write(string InSavePath, string InScriptName, ICell[][] InHeader)
        {
            string fullPath = Path.Combine(InSavePath, InScriptName);

            StringBuilder stringBuilder = new StringBuilder(512);

            stringBuilder.Append("namespace ExcelConverter.Json.Template");
            stringBuilder.Append("\n{");
            stringBuilder.Append("\n    public class ").Append(InScriptName);
            stringBuilder.Append("\n    {");

            stringBuilder.Append("\n    }");
            stringBuilder.Append("\n}");

        }
    }
}

