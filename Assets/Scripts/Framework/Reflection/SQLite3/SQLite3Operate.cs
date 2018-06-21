﻿/* Copyright (c) 2017 ShenZhaoNan. All rights reserved.
 ************************************************************************
 * SQLite3Helper - Convert Excel data to SQLite3 Database table.
 ************************************************************************
 * Filename: SQLite3Operate.cs
 * Date: 2017/12/11
 * Author: ShenZhaoNan
 * Email: hellomercury@vip.qq.com
 * Blog: shenzhaonan.cn
 ************************************************************************
 * Describe:
 *   
 *   
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using SQLite3DbHandle = System.IntPtr;
using SQLite3Statement = System.IntPtr;
using Framework.Reflection.Config;
using Framework.Reflection.Sync;
using Framework.Tools;
using UnityEngine.Assertions;

namespace Framework.Reflection.SQLite3Helper
{
    public class SQLite3Operate
    {
        #region Field
        /// <summary>
        /// SQLite3 data handle for operating the database.
        /// </summary>
        private SQLite3DbHandle handle;

        /// <summary>
        /// The StringBuilder used to connect the string.
        /// </summary>
        private StringBuilder stringBuilder;

        private int SBLength;
        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:SQLite3Helper.SQLite3Operate"/> class.
        /// </summary>
        /// <param name="InDatabasePath">In database path.</param>
        /// <param name="InSQLite3OpenFlags">In SQLite3 open flags.</param>
        public SQLite3Operate(string InDatabasePath, SQLite3OpenFlags InSQLite3OpenFlags)
        {
            if (string.IsNullOrEmpty(InDatabasePath)) throw new ArgumentNullException();

            if (SQLite3Result.OK == SQLite3.Open(ConvertTypeUtility.StringToUTF8Bytes(InDatabasePath),
                out handle, (int)InSQLite3OpenFlags))
            {
                stringBuilder = new StringBuilder(256);
            }
            else
            {
                ShowMsg(SQLite3.GetErrmsg(handle));
                SQLite3.Close(handle);
                handle = SQLite3DbHandle.Zero;
                ShowMsg("Database failed to open.");
            }
        }
        #endregion

        //        #region Open
        //        /// <summary>
        //        /// Open a database to read the data, you can not make any changes to the database. 
        //        /// If database is not exist there will throw a FileLoadException.
        //        /// when first opened database program will copy it from streamingAssetsPath directory to the persistentDataPath directory,
        //        /// </summary>
        //        /// <returns>The to read.</returns>
        //        /// <param name="InDbName">In db name.</param>
        //        public static SQLite3Operate LoadToRead(string InDbName)
        //        {
        //            return Load(InDbName, SQLite3OpenFlags.ReadOnly);
        //        }

        //        /// <summary>
        //        /// Open an existing database for data read and write, 
        //        /// if the database does not exist will throw a FileLoadException. 
        //        /// If the database exists in the streamingAssetsPath directory, 
        //        /// the database will be copied to the persistentDataPath directory, 
        //        /// any operation on the database will not affect the database under the streamingAssetsPath directory.
        //        /// Note that the database may be modified by players, please check the correctness of the data before use.
        //        /// </summary>
        //        /// <returns>The to write.</returns>
        //        /// <param name="InDbName">In db name.</param>
        //        public static SQLite3Operate LoadToWrite(string InDbName)
        //        {
        //            return Load(InDbName, SQLite3OpenFlags.ReadWrite);
        //        }

        //        /// <summary>
        //        /// Create or open a database for data read and write, 
        //        /// if the database does not exist will create a new database on persistentDataPath directory. 
        //        /// Note that the database may be modified by players, please check the correctness of the data before use.
        //        /// </summary>
        //        /// <returns>The SQLite3Operate object.</returns>
        //        /// <param name="InDbName">In db name.</param>
        //        public static SQLite3Operate CreateAndWrite(string InDbName)
        //        {
        //            string destinationPath = Path.Combine(Application.persistentDataPath, InDbName);
        //            return new SQLite3Operate(destinationPath, SQLite3OpenFlags.Create | SQLite3OpenFlags.ReadWrite);
        //        }

        //        /// <summary>
        //        /// Copy a exist database from the StreamingAssets path to PrersistentDataPath.
        //        /// And open it according the open flags.
        //        /// </summary>
        //        /// <returns>The SQLite3Operate object.</returns>
        //        /// <param name="InDbName">In db name.</param>
        //        /// <param name="InSQLite3OpenFlags">In SQLite3 open flags.</param>
        //        private static SQLite3Operate Load(string InDbName, SQLite3OpenFlags InSQLite3OpenFlags)
        //        {
        //            string destinationPath = Path.Combine(Application.persistentDataPath, InDbName);

        //            if (!File.Exists(destinationPath))
        //            {
        //#if UNITY_ANDROID
        //#if UNITY_EDITOR
        //                string streamPath = Path.Combine("file:///" + Application.streamingAssetsPath, InDbName);
        //#else
        //                string streamPath = Path.Combine("jar:file://" + Application.dataPath + "!/assets/", InDbName);
        //#endif
        //#elif UNITY_IOS
        //                string streamPath = Application.dataPath + "/Raw/";
        //#else
        //                string streamPath = Application.streamingAssetsPath + "/";
        //#endif

        //#if UNITY_ANDROID
        //                using (WWW www = new WWW(streamPath))
        //                {
        //                    while (!www.isDone) { }
        //                    if (string.IsNullOrEmpty(www.error)) File.WriteAllBytes(destinationPath, www.bytes);
        //                    else ShowMsg(www.error);
        //                }
        //#else
        //                string sourcePath = Path.Combine(streamPath, InDbName);

        //                File.Copy(sourcePath, destinationPath, true);
        //#endif
        //            }

        //            return new SQLite3Operate(destinationPath, InSQLite3OpenFlags);
        //        }
        //        #endregion

        #region Check Exists
        /// <summary>
        ///  Check the database table exists.
        /// </summary>
        /// <returns><c>true</c>, if table exists, <c>false</c> otherwise.</returns>
        /// <typeparam name="T">The Subclass of SyncBase.</typeparam>
        public bool TableExists<T>() where T : SyncBase
        {
            return TableExists(SyncFactory.GetSyncProperty(typeof(T)).ClassName);
        }

        /// <summary>
        /// Check the database table exists.
        /// </summary>
        /// <returns><c>true</c>, if table exists, <c>false</c> otherwise.</returns>
        /// <param name="InTableName">In table name.</param>
        public bool TableExists(string InTableName)
        {
            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("SELECT * FROM sqlite_master WHERE type = 'table' AND name = '")
                         .Append(InTableName)
                         .Append("'");

            return CheckExists(stringBuilder.ToString());
        }

        /// <summary>
        /// According to a specific value to determine whether the data is in the database
        /// </summary>
        /// <param name="InTableName">Table name</param>
        /// <param name="InProperty"></param>
        /// <param name="InValue"></param>
        /// <returns></returns>
        public bool ValueExists(string InTableName, string InProperty, object InValue)
        {
            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("SELECT * FROM ")
                         .Append(InTableName)
                         .Append(" WHERE ")
                         .Append(InProperty)
                         .Append(" = ")
                         .Append(ConvertTypeUtility.ChangeType(InValue));

            return CheckExists(stringBuilder.ToString());
        }

        /// <summary>
        /// Query whether the incoming object exists in the database
        /// </summary>
        /// <typeparam name="T">Subclass of SyncBase</typeparam>
        /// <param name="InObject">Query object</param>
        /// <param name="InPropertyIndex">property index in TEnum</param>
        /// <returns></returns>
        public bool ValueExists<T>(T InObject, int InPropertyIndex) where T : SyncBase
        {
            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            SyncProperty property = SyncFactory.GetSyncProperty(InObject);
            stringBuilder.Append("SELECT * FROM ")
                         .Append(property.ClassName)
                         .Append(" WHERE ")
                         .Append(property.Infos[InPropertyIndex].Name)
                         .Append(" = ")
                         .Append(property.Infos[InPropertyIndex].GetValue(InObject, null));

            return CheckExists(stringBuilder.ToString());
        }

        public bool ValueExists<T, U>(int InPropertyIndex, U InValue) where T : SyncBase
        {
            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            SyncProperty property = SyncFactory.GetSyncProperty<T>();
            stringBuilder.Append("SELECT * FROM ")
                         .Append(property.ClassName)
                         .Append(" WHERE ")
                         .Append(property.Infos[InPropertyIndex].Name)
                         .Append(" = ")
                         .Append(InValue);

            return CheckExists(stringBuilder.ToString());
        }

        /// <summary>
        /// According to the SQL statement to check whether the specified data exists
        /// </summary>
        /// <param name="InSQLStatement">SQL statement</param>
        /// <returns></returns>
        public bool ValueExists(string InSQLStatement)
        {
            return CheckExists(InSQLStatement);
        }

        /// <summary>
        /// Check the table exists in the field
        /// </summary>
        /// <typeparam name="T">Subclass of SyncBase</typeparam>
        /// <param name="InFieldName">Field Name</param>
        /// <returns></returns>
        public bool FieldExists<T>(string InFieldName) where T : SyncBase
        {
            return FieldExists(SyncFactory.GetSyncProperty<T>().ClassName, InFieldName);
        }

        /// <summary>
        /// Check the table exists in the field
        /// </summary>
        /// <typeparam name="T">Subclass of SyncBase</typeparam>
        /// <param name="InIndex">The index of the property in the class</param>
        /// <returns></returns>
        public bool FieldExists<T>(int InIndex) where T : SyncBase
        {
            SyncProperty property = SyncFactory.GetSyncProperty<T>();
            if (null == property || InIndex < 0 || InIndex >= property.InfosLength) return false;
            else return FieldExists(property.ClassName, property.Infos[InIndex].Name);
        }

        /// <summary>
        /// Check the table exists in the field
        /// </summary>
        /// <param name="InTableName">Database table name</param>
        /// <param name="InFieldName">Field name</param>
        /// <returns></returns>
        public bool FieldExists(string InTableName, string InFieldName)
        {
            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("SELECT * FROM sqlite_master WHERE name = '")
                .Append(InTableName)
                .Append("' AND sql like '%")
                .Append(InFieldName)
                .Append("%'");

            return CheckExists(stringBuilder.ToString());
        }

        /// <summary>
        /// According to the SQL statement query results
        /// </summary>
        /// <param name="InSqlStatement">SQL statement</param>
        /// <returns></returns>
        private bool CheckExists(string InSqlStatement)
        {
            SQLite3Statement stmt;

            if (ExecuteQuery(InSqlStatement, out stmt))
            {
                bool isExists = false;

                SQLite3Result result = SQLite3.Step(stmt);

                if (SQLite3Result.Row == result) isExists = true;
                else if (SQLite3Result.Done != result) ShowMsg(SQLite3.GetErrmsg(stmt));

                SQLite3.Finalize(stmt);

                return isExists;
            }
            return false;
        }
        #endregion

        #region Create
        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="InSQLStetement">SQL Statement.</param>
        public bool CreateTable(string InSQLStetement)
        {
            return Exec(InSQLStetement);
        }

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="InTableName">In table name.</param>
        /// <param name="InColumnNameAndType">In column name and type.</param>
        /// <param name="InOverWrite">Whether to overwrite the original form</param>
        public bool CreateTable(string InTableName, string[] InColumnNameAndType, bool InOverWrite = true)
        {
            if (InOverWrite) Exec("DROP TABLE IF EXISTS " + InTableName);

            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("CREATE TABLE ").Append(InTableName).Append(" (");
            int length = InColumnNameAndType.Length;
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(InColumnNameAndType[i]).Append(", ");
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(")");

            return Exec(stringBuilder.ToString());
        }

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        public bool CreateTable<T>(bool InOverWrite = true) where T : SyncBase
        {
            SyncProperty property = SyncFactory.GetSyncProperty(typeof(T));

            Assert.IsNotNull(property);

            if (InOverWrite) Exec("DROP TABLE IF EXISTS " + property.ClassName);

            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("CREATE TABLE ").Append(property.ClassName).Append("(");
            int length = property.Infos.Length;
            for (int i = 0; i < length; ++i)
            {
                if (null == property.Infos[i]) Debug.LogError(property.ClassName + i);
                stringBuilder.Append(property.Infos[i].Name);

                Type type = property.Infos[i].PropertyType;

                if (type == ReadonlyData.INT_TYPE
                    || type == ReadonlyData.BOOL_TYPE
                    || type == ReadonlyData.LONG_TYPE
                    || type == ReadonlyData.SHORT_TYPE)
                {
                    stringBuilder.Append(" INTEGER ");
                }
                else if (type == ReadonlyData.STRING_TYPE)
                {
                    stringBuilder.Append(" TEXT ");
                }
                else if (type == ReadonlyData.FLOAT_TYPE
                    || type == ReadonlyData.DOUBLE_TYPE)
                {
                    stringBuilder.Append(" REAL ");
                }
                else
                {
                    stringBuilder.Append(" BLOB ");
                }

                object[] objs = property.Infos[i].GetCustomAttributes(typeof(SQLite3ConstraintAttribute), false);
                if (objs.Length == 1 && objs[0] is SQLite3ConstraintAttribute)
                    stringBuilder.Append((objs[0] as SQLite3ConstraintAttribute).Constraint);

                stringBuilder.Append(", ");
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(")");

            return Exec(stringBuilder.ToString());
        }
        #endregion

        #region Alter

        public void AlterAddColumn(string InTableName, string InColumnName, SQLite3DataType InColumnType, SQLite3Constraint InConstraint = SQLite3Constraint.Default)
        {
            if (TableExists(InTableName))
            {
                if (!FieldExists(InTableName, InColumnName))
                {
                    if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
                    stringBuilder.Append("ALTER TABLE ")
                        .Append(InTableName)
                        .Append(" ADD ")
                        .Append(InColumnName)
                        .Append(" ")
                        .Append(InColumnType)
                        .Append(SQLite3Utility.ConvertSQLite3ConstraintToStr(InConstraint));

                    Exec(stringBuilder.ToString());
                }
            }
        }
        #endregion

        #region Insert
        /// <summary>
        /// Execute insert SQL statement.
        /// </summary>
        /// <param name="InSQLstatement">In SQL statement.</param>
        public bool Insert(string InSQLstatement)
        {
            return Exec(InSQLstatement);
        }

        /// <summary>
        /// Execute insert SQL statement Through the assembly parameters into SQL statements.
        /// </summary>
        /// <param name="InTableName">In table name.</param>
        /// <param name="InData">The data inserted to the table.</param>
        public bool Insert(string InTableName, params object[] InData)
        {
            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("INSERT INTO ").Append(InTableName).Append(" VALUES(");

            int length = InData.Length;
            for (int i = 0; i < length; ++i)
            {
                stringBuilder.Append(ConvertTypeUtility.ChangeType(InData[i])).Append(", ");
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(")");

            return Exec(stringBuilder.ToString());
        }

        /// <summary>
        /// Insert subclass of SyncBase into the table.
        /// </summary>
        /// <param name="InValue">Subclass of SyncBase object.</param>
        /// <typeparam name="T">Subclass of SyncBase</typeparam>
        public bool InsertT<T>(T InValue) where T : SyncBase
        {
            SyncProperty property = SyncFactory.GetSyncProperty(typeof(T));

            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("INSERT INTO ").Append(property.ClassName).Append(" VALUES(");

            int length = property.Infos.Length;
            PropertyInfo info;
            for (int i = 0; i < length; i++)
            {
                info = property.Infos[i];
                stringBuilder.Append(ConvertTypeUtility.ChangeType(info.GetValue(InValue, null), info.PropertyType)).Append(", ");
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(")");

            return Exec(stringBuilder.ToString());
        }

        /// <summary>
        /// Insert some SyncBase subclasses into the table.
        /// </summary>
        /// <param name="InValue">Some SyncBase subclasses list.</param>
        /// <typeparam name="T">subclass of SyncBase.</typeparam>
        public bool InsertAllT<T>(List<T> InValue) where T : SyncBase
        {
            if (null == InValue) throw new ArgumentNullException();
            int count = InValue.Count, length;
            PropertyInfo info;
            if (count > 0)
            {
                SyncProperty property = SyncFactory.GetSyncProperty(typeof(T));

                for (int i = 0; i < count; ++i)
                {
                    if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
                    stringBuilder.Append("INSERT INTO ").Append(property.ClassName).Append(" VALUES(");

                    length = property.Infos.Length;
                    for (int j = 0; j < length; j++)
                    {
                        info = property.Infos[j];
                        stringBuilder.Append(ConvertTypeUtility.ChangeType(info.GetValue(InValue[i], null), info.PropertyType))
                            .Append(", ");
                    }
                    stringBuilder.Remove(stringBuilder.Length - 2, 2);
                    stringBuilder.Append(")");

                    if (!Exec(stringBuilder.ToString())) return false;
                }
            }

            return true;
        }
        #endregion

        #region Update
        /// <summary>
        /// According to the SQL statement to update the table. 
        /// </summary>
        /// <param name="InSQLStatement">In SQL statement.</param>
        public bool Update(string InSQLStatement)
        {
            return Exec(InSQLStatement);
        }

        public bool Update(string InTableName, string InUpdateFieldName, object InUpdateValue,
            string InConditionFieldName, object InConditionValue)
        {
            return Update(InTableName, InUpdateFieldName, InUpdateValue,
                InConditionFieldName + " = " + ConvertTypeUtility.ChangeType(InConditionValue));
        }

        public bool Update(string InTableName, string InUpdateFieldName, object InUpdateValue, string InCondition)
        {
            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("UPDATE ").Append(InTableName)
                .Append(" SET ").Append(InUpdateFieldName)
                .Append(" = ")
                .Append(ConvertTypeUtility.ChangeType(InUpdateValue))
                .Append(" WHERE ").Append(InCondition);

            return Exec(stringBuilder.ToString());
        }

        /// <summary>
        /// Execute update SQL statement Through the assembly parameters into SQL statements.
        /// </summary>
        /// <param name="InTableName">In table name.</param>
        /// <param name="InCondition">Analyzing conditions.</param>
        /// <param name="InFieldName">UpdateMultiValue field name</param>
        /// <param name="InValues">Field value</param>
        public bool UpdateMultiValue(string InTableName, string[] InFieldName, object[] InValues, string InCondition)
        {
            if (null == InFieldName || null == InValues || 0 == InFieldName.Length || 0 == InValues.Length) throw new ArgumentNullException();
            if (InFieldName.Length != InValues.Length) throw new ArgumentException("Number of field can not matched value.");

            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("UPDATE ").Append(InTableName).Append(" SET ");

            int length = InFieldName.Length;
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(InFieldName[i])
                    .Append(" = ")
                    .Append(ConvertTypeUtility.ChangeType(InValues[i]))
                    .Append(", ");
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(" WHERE ").Append(InCondition);

            return Exec(stringBuilder.ToString());
        }

        /// <summary>
        /// According to the subclass of SyncBase to update the table.
        /// </summary>
        /// <param name="InValue">SyncBase object.</param>
        /// <typeparam name="T">subclass of SyncBase</typeparam>
        public bool UpdateT<T>(T InValue) where T : SyncBase
        {
            if (null == InValue) throw new ArgumentNullException();

            SyncProperty property = SyncFactory.GetSyncProperty(typeof(T));

            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("UPDATE ").Append(property.ClassName).Append(" SET ");

            PropertyInfo info;
            int length = property.Infos.Length;
            for (int i = 1; i < length; i++)
            {
                info = property.Infos[i];
                stringBuilder.Append(property.Infos[i].Name)
                    .Append(" = ")
                    .Append(ConvertTypeUtility.ChangeType(info.GetValue(InValue, null), info.PropertyType))
                    .Append(", ");
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(" WHERE ID = ").Append(property.Infos[0].GetValue(InValue, null));

            return Exec(stringBuilder.ToString());
        }

        /// <summary>
        /// The value obtained by singletonKey Reflection updates the table
        /// </summary>
        /// <param name="InIndex">The index of the object property.</param>
        /// <param name="InValue">SyncBase subclass object</param>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        public bool UpdateTByKeyValue<T>(int InIndex, T InValue) where T : SyncBase
        {
            if (null == InValue) throw new ArgumentNullException();
            SyncProperty property = SyncFactory.GetSyncProperty(typeof(T));
            if (InIndex < 0 || InIndex >= property.InfosLength) throw new ArgumentOutOfRangeException();

            PropertyInfo info = property.Infos[InIndex];
            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("UPDATE ")
                .Append(property.ClassName)
                .Append(" SET ")
                .Append(info.Name)
                .Append(" = ")
                 .Append(ConvertTypeUtility.ChangeType(info.GetValue(InValue, null), info.PropertyType))
                .Append(" WHERE ID = ")
                .Append(property.Infos[0].GetValue(InValue, null));

            return Exec(stringBuilder.ToString());
        }

        /// <summary>
        /// According to the SyncBase subclass object updates the table or insert into the table.
        /// </summary>
        /// <param name="InT">SyncBase subclass object.</param>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        public void UpdateOrInsert<T>(T InT) where T : SyncBase
        {
            if (null == InT) throw new ArgumentNullException();

            if (ValueExists(InT, 0)) UpdateT(InT);
            else InsertT(InT);
        }
        #endregion

        #region Select
        /// <summary>
        /// According to the ID from the table to read multiple data.
        /// SELECT $InColumnName$ FROM $InTableName$ WHERE 
        /// $InConditionFieldName$ $InOperator$ $InConditionFieldName$ 
        /// $InConnectOperator$ 
        /// $InConditionFieldName$ $InOperator$ $InConditionFieldName$
        /// ...
        /// </summary>
        /// <returns>Results from the database query.</returns>
        /// <param name="InColumnName">In column name.</param>
        /// <param name="InTableName">In table name.</param>
        /// <param name="InConditionFieldName">In compare name.</param>
        /// <param name="InOperator">In Operator</param>
        /// <param name="InConditionValue">In condition.</param>
        /// <param name="InConnectOperators">In connect operator.</param>
        public List<object[]> SelectObject(string InColumnName, string InTableName,
                                              string[] InConditionFieldName, string[] InOperator, string[] InConditionValue,
                                           string[] InConnectOperators = null)
        {
            int length = InConditionFieldName.Length;
            int connectLength = InConnectOperators == null ? 0 : InConnectOperators.Length;
            if (length != InOperator.Length || length != InConditionValue.Length || length - 1 != connectLength)
                throw new ArgumentException("Parameter length does not match.");

            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("SELECT ")
                         .Append(InColumnName)
                         .Append(" FROM ")
                         .Append(InTableName)
                         .Append(" WHERE ");

            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(InConditionFieldName[i])
                        .Append(" ")
                        .Append(InOperator[i])
                        .Append(" ")
                        .Append(ConvertTypeUtility.ChangeType(InConditionValue[i]));
                if (i < connectLength)
                    stringBuilder.Append(" ")
                                 .Append(InConnectOperators[i])
                                 .Append(" ");
            }



            SQLite3Statement stmt;

            if (ExecuteQuery(stringBuilder.ToString(), out stmt))
            {
                List<object[]> obj = new List<object[]>();
                int count = SQLite3.ColumnCount(stmt);
                SQLite3Result sqlite3Result;
                while (true)
                {
                    sqlite3Result = SQLite3.Step(stmt);
                    if (SQLite3Result.Row == sqlite3Result)
                    {
                        object[] objs = new object[count];
                        for (int i = 0; i < count; ++i)
                        {
                            objs[i] = GetObject(stmt, i);
                        }
                        obj.Add(objs);
                    }
                    else if (SQLite3Result.Done == sqlite3Result) break;
                    else
                    {
                        ShowMsg(SQLite3.GetErrmsg(stmt));
                        break;
                    }
                }

                SQLite3.Finalize(stmt);

                return obj;
            }
            else return null;
        }

        /// <summary>
        /// Resolve the database results.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="InStmt">In sqlite statement.</param>
        /// <param name="InIndex">In index of result.</param>
        private object GetObject(SQLite3Statement InStmt, int InIndex)
        {
            switch (SQLite3.ColumnType(InStmt, InIndex))
            {
                case SQLite3DataType.Integer:
                    return SQLite3.ColumnInt(InStmt, InIndex);

                case SQLite3DataType.Real:
                    return SQLite3.ColumnDouble(InStmt, InIndex);

                case SQLite3DataType.Text:
                    return SQLite3.ColumnText(InStmt, InIndex);

                case SQLite3DataType.Blob:
                    return SQLite3.ColumnBlob(InStmt, InIndex);

                default:
                    return null;
            }
        }

        public object SelectSingleObject(string InColumnName, string InTableName, string InConditionFieldName,
            string InConditionValue)
        {
            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("SELECT ")
                         .Append(InColumnName)
                         .Append(" FROM ")
                         .Append(InTableName)
                         .Append(" WHERE ")
                         .Append(InConditionFieldName)
                         .Append(" = ")
                         .Append(ConvertTypeUtility.ChangeType(InConditionValue));

            SQLite3Statement stmt;

            object obj = null;
            if (ExecuteQuery(stringBuilder.ToString(), out stmt))
            {
                if (1 == SQLite3.ColumnCount(stmt))
                {
                    SQLite3Result sqlite3Result = SQLite3.Step(stmt);
                    if (SQLite3Result.Row == sqlite3Result) obj = GetObject(stmt, 0);
                    else if (SQLite3Result.Done != sqlite3Result) ShowMsg(SQLite3.GetErrmsg(stmt));
                }

                SQLite3.Finalize(stmt);
            }

            return obj;
        }

        /// <summary>
        /// Query the object from the database by ID.
        /// Only in the absence of primary key or primary key type is not an integer
        /// </summary>
        /// <returns>SyncBase subclass object.</returns>
        /// <param name="InID">In table id as key.</param>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        public T SelectTByID<T>(int InID) where T : SyncBase, new()
        {
            return SelectTBySQLCommand<T>("SELECT * FROM "
                              + SyncFactory.GetSyncProperty(typeof(T)).ClassName
                              + " WHERE ID = " + InID);
        }

        /// <summary>
        /// Query the object from the database by index.
        /// </summary>
        /// <returns>SyncBase subclass object.</returns>
        /// <param name="InIndex">In index as key, the index value is automatically generated by the database.</param>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        public T SelectTByIndex<T>(int InIndex) where T : SyncBase, new()
        {
            return SelectTBySQLCommand<T>("SELECT * FROM "
                              + SyncFactory.GetSyncProperty(typeof(T)).ClassName
                              + " WHERE rowid = " + (InIndex + 1));    //SQLite3 rowid begin with 1.
        }

        /// <summary>
        /// Query the object from the database by property index and perperty's value.
        /// </summary>
        /// <returns>SyncBase subclass object.</returns>
        /// <param name="InPropertyIndex">In property index, The index value is specified by the SyncAttribute.</param>
        /// <param name="InExpectedValue">Expected values.</param>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        public T SelectTByKeyValue<T>(int InPropertyIndex, object InExpectedValue) where T : SyncBase, new()
        {
            SyncProperty property = SyncFactory.GetSyncProperty(typeof(T));
            if (InPropertyIndex < 0 || InPropertyIndex >= property.InfosLength) throw new IndexOutOfRangeException();

            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("SELECT * FROM ")
                         .Append(property.ClassName)
                         .Append(" WHERE ")
                         .Append(property.Infos[InPropertyIndex].Name)
                         .Append(" = '")
                         .Append(ConvertTypeUtility.ChangeType(InExpectedValue))
                         .Append("'");

            return SelectTBySQLCommand<T>(stringBuilder.ToString());
        }

        /// <summary>
        /// Query the object from the database by property name and perperty's value.
        /// </summary>
        /// <returns>SyncBase subclass object.</returns>
        /// <param name="InPropertyName">In property name.</param>
        /// <param name="InExpectedValue">Expected values.</param>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        public T SelectTByKeyValue<T>(string InPropertyName, object InExpectedValue) where T : SyncBase, new()
        {
            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("SELECT * FROM ")
                         .Append(SyncFactory.GetSyncProperty(typeof(T)).ClassName)
                         .Append(" WHERE ")
                         .Append(InPropertyName)
                         .Append(" = '")
                         .Append(ConvertTypeUtility.ChangeType(InExpectedValue))
                         .Append("'");

            return SelectTBySQLCommand<T>(stringBuilder.ToString());
        }

        /// <summary>
        /// Query the object from the database by sql statement.
        /// </summary>
        /// <returns>SyncBase subclass object.</returns>
        /// <param name="InSQLStatement">In sql statement.</param>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        public T SelectTBySQLCommand<T>(string InSQLStatement) where T : SyncBase, new()
        {
            SyncProperty property = SyncFactory.GetSyncProperty(typeof(T));

            SQLite3Statement stmt;

            if (ExecuteQuery(InSQLStatement, out stmt))
            {
                T t = default(T);
                SQLite3Result result = SQLite3.Step(stmt);
                switch (result)
                {
                    case SQLite3Result.Row:
                        t = GetT(new T(), property.Infos, stmt, property.InfosLength);
                        break;

                    case SQLite3Result.Error:
                        if (!TableExists(property.ClassName)) ShowMsg("No such table : " + property.ClassName);
                        else ShowMsg(SQLite3.GetErrmsg(stmt));
                        break;
                }

                SQLite3.Finalize(stmt);

                return t;
            }
            else return default(T);
        }

        /// <summary>
        /// Query the database by property indexes and expected value and return the dictionary.
        /// </summary>
        /// <returns>Returns the result of the query as a dictionary.</returns>
        /// <param name="InIndexes">property indexes, The index value is specified by the SyncAttribute.</param>
        /// <param name="InOperators">Operators between properties and expected values.</param>
        /// <param name="InExpectedValues">Expected values.</param>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        /// <param name="InConnectOperators">In connect operator.</param>
        public Dictionary<int, T> SelectDictT<T>(int[] InIndexes, string[] InOperators, object[] InExpectedValues,
                                                 string[] InConnectOperators = null) where T : SyncBase, new()
        {
            if (null == InIndexes || null == InOperators || null == InExpectedValues) throw new ArgumentNullException();
            int length = InIndexes.Length;
            int connectLength = InConnectOperators == null ? 0 : InConnectOperators.Length;
            if (length != InOperators.Length || length != InExpectedValues.Length || length - 1 != connectLength)
                throw new ArgumentException("Parameter length does not match.");

            SyncProperty property = SyncFactory.GetSyncProperty(typeof(T));
            string[] propertyNames = new string[length];
            for (int i = 0; i < length; i++)
            {
                propertyNames[i] = property.Infos[InIndexes[i]].Name;
            }

            return SelectDictT<T>(propertyNames, InOperators, InExpectedValues, InConnectOperators);
        }

        /// <summary>
        /// Query the database by property names and expected value and return the dictionary.
        /// </summary>
        /// <returns>Returns the result of the query as a dictionary.</returns>
        /// <param name="InPropertyNames">property names.</param>
        /// <param name="InOperators">Operators between properties and expected values.</param>
        /// <param name="InExpectedValues">Expected values.</param>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        /// <param name="InConnectOperators">In connect operator.</param>
        public Dictionary<int, T> SelectDictT<T>(string[] InPropertyNames, string[] InOperators, object[] InExpectedValues,
                                                 string[] InConnectOperators = null) where T : SyncBase, new()
        {
            if (null == InPropertyNames || null == InOperators || null == InExpectedValues)
                throw new ArgumentNullException();
            int length = InPropertyNames.Length;
            int connectLength = InConnectOperators == null ? 0 : InConnectOperators.Length;
            if (length != InOperators.Length || length != InExpectedValues.Length || length - 1 != connectLength)
                throw new ArgumentException("Parameter length does not match.");

            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(InPropertyNames[i])
                             .Append(" ")
                             .Append(InOperators[i])
                             .Append(" ")
                             .Append(InExpectedValues[i]);
                if (i < connectLength)
                    stringBuilder.Append(" ")
                                 .Append(InConnectOperators[i])
                                 .Append(" ");
            }

            return SelectDictT<T>(stringBuilder.ToString());
        }

        /// <summary>
        /// Query the dictionary from the database by sql statement.
        /// </summary>
        /// <returns>Returns the result of the query as a dictionary.</returns>
        /// <param name="InSQLStatement">In SQL Statement</param>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        public Dictionary<int, T> SelectDictT<T>(string InSQLStatement = "") where T : SyncBase, new()
        {
            SyncProperty property = SyncFactory.GetSyncProperty(typeof(T));

            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("SELECT * FROM ")
                .Append(property.ClassName)
                .Append(" WHERE ")
                .Append(InSQLStatement);

            SQLite3Statement stmt;
            if (ExecuteQuery(stringBuilder.ToString(), out stmt))
            {
                Dictionary<int, T> resultDict = new Dictionary<int, T>();
                int count = SQLite3.ColumnCount(stmt), id;
                SQLite3Result result;
                while (true)
                {
                    result = SQLite3.Step(stmt);
                    if (SQLite3Result.Row == result)
                    {
                        T t = GetT(new T(), property.Infos, stmt, count);
                        id = (int)property.Infos[0].GetValue(t, null);
                        if (!resultDict.ContainsKey(id)) resultDict.Add(id, t);
                    }
                    else if (SQLite3Result.Done == result) break;
                    else
                    {
                        if (!TableExists(property.ClassName)) ShowMsg("No such table : " + property.ClassName);
                        else ShowMsg(SQLite3.GetErrmsg(stmt));
                        break;
                    }
                }

                SQLite3.Finalize(stmt);

                return resultDict;
            }
            else return null;
        }

        /// <summary>
        /// Query the array by property indexes and expected value and return the dictionary.
        /// </summary>
        /// <returns>Returns the result of the query as a array.</returns>
        /// <param name="InIndexes">property indexes, The index value is specified by the SyncAttribute.</param>
        /// <param name="InOperators">Operators between properties and expected values.</param>
        /// <param name="InExpectedValues">Expected values.</param>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        public T[] SelectArrayT<T>(int[] InIndexes, string[] InOperators, object[] InExpectedValues) where T : SyncBase, new()
        {
            int length = InIndexes.Length;
            SyncProperty property = SyncFactory.GetSyncProperty(typeof(T));
            string[] propertyNames = new string[length];
            for (int i = 0; i < length; i++)
            {
                propertyNames[i] = property.Infos[InIndexes[i]].Name;
            }

            return SelectArrayT<T>(propertyNames, InOperators, InExpectedValues);
        }

        /// <summary>
        /// Query the database by property names and expected value and return the array.
        /// </summary>
        /// <returns>Returns the result of the query as a dictionary.</returns>
        /// <param name="InPropertyNames">property names.</param>
        /// <param name="InOperators">Operators between properties and expected values.</param>
        /// <param name="InExpectedValues">Expected values.</param>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        /// <param name="InConnectOperators">In connect operator.</param>
        public T[] SelectArrayT<T>(string[] InPropertyNames, string[] InOperators, object[] InExpectedValues,
                                   string[] InConnectOperators = null) where T : SyncBase, new()
        {
            if (null == InPropertyNames || null == InOperators || null == InExpectedValues) throw new ArgumentNullException();
            int length = InPropertyNames.Length;
            int connectLength = null == InConnectOperators ? 0 : InConnectOperators.Length;
            if (length != InOperators.Length || length != InExpectedValues.Length || length - 1 != connectLength)
                throw new ArgumentException("Parameter length does not match.");

            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("WHERE ");
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(InPropertyNames[i])
                             .Append(" ")
                             .Append(InOperators[i])
                             .Append(" ")
                             .Append(InExpectedValues[i]);
                if (i < connectLength)
                    stringBuilder.Append(" ")
                                 .Append(InConnectOperators[i])
                                 .Append(" ");
            }

            return SelectArrayT<T>(stringBuilder.ToString());
        }

        /// <summary>
        /// Query the database by sql statement and return the array.
        /// </summary>
        /// <returns>Returns the result of the query as a array.</returns>
        /// <param name="InCondition">In Condition.</param>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        public T[] SelectArrayT<T>(string InCondition = "") where T : SyncBase, new()
        {
            SyncProperty property = SyncFactory.GetSyncProperty(typeof(T));
            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("SELECT * FROM ")
                         .Append(property.ClassName)
                         .Append(" ")
                         .Append(InCondition);

            SQLite3Statement stmt;
            if (ExecuteQuery(stringBuilder.ToString(), out stmt))
            {
                List<T> resultList = new List<T>();
                SQLite3Result sqlite3Result;
                int count = SQLite3.ColumnCount(stmt);
                while (true)
                {
                    sqlite3Result = SQLite3.Step(stmt);
                    if (SQLite3Result.Row == sqlite3Result)
                        resultList.Add(GetT(new T(), property.Infos, stmt, count));
                    else if (SQLite3Result.Done == sqlite3Result) break;
                    else
                    {
                        if (!TableExists(property.ClassName)) ShowMsg("No such table : " + property.ClassName);
                        else ShowMsg(SQLite3.GetErrmsg(stmt));
                        break;
                    }
                }
                SQLite3.Finalize(stmt);

                return resultList.ToArray();
            }
            else return null;
        }

        /// <summary>
        /// Convert query result from database to SyncBase subclass object.
        /// </summary>
        /// <returns>SyncBase subclass object.</returns>
        /// <param name="InBaseSubclassObj">In SyncBase subclass object.</param>
        /// <param name="InPropertyInfos">In SyncBase subclass property infos.</param>
        /// <param name="InStmt">SQLite3 result address.</param>
        /// <param name="InCount">In sqlite3 result count.</param>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        private T GetT<T>(T InBaseSubclassObj, IList<PropertyInfo> InPropertyInfos, SQLite3Statement InStmt, int InCount) where T : SyncBase, new()
        {
            Type type;
            for (int i = 0; i < InCount; ++i)
            {
                type = InPropertyInfos[i].PropertyType;

                if (ReadonlyData.INT_TYPE == type)
                {
                    InPropertyInfos[i].SetValue(InBaseSubclassObj, SQLite3.ColumnInt(InStmt, i), null);
                }
                else if (ReadonlyData.BOOL_TYPE == type)
                {
                    switch (SQLite3.ColumnType(InStmt, i))
                    {
                        case SQLite3DataType.Integer:
                            InPropertyInfos[i].SetValue(InBaseSubclassObj, SQLite3.ColumnInt(InStmt, i) == 1, null);
                            break;

                        case SQLite3DataType.Real:
                            InPropertyInfos[i].SetValue(InBaseSubclassObj, (int)SQLite3.ColumnDouble(InStmt, i) == 1, null);
                            break;

                        case SQLite3DataType.Text:
                            InPropertyInfos[i].SetValue(InBaseSubclassObj, SQLite3.ColumnText(InStmt, i).ToLower().Equals("true"), null);
                            break;

                        default:
                            InPropertyInfos[i].SetValue(InBaseSubclassObj, false, null);
                            break;
                    }
                }
                else if (ReadonlyData.LONG_TYPE == type)
                {
                    InPropertyInfos[i].SetValue(InBaseSubclassObj, SQLite3.ColumnInt64(InStmt, i), null);
                }
                else if (ReadonlyData.FLOAT_TYPE == type)
                {
                    InPropertyInfos[i].SetValue(InBaseSubclassObj, (float)SQLite3.ColumnDouble(InStmt, i), null);
                }
                else if (ReadonlyData.DOUBLE_TYPE == type)
                {
                    InPropertyInfos[i].SetValue(InBaseSubclassObj, SQLite3.ColumnDouble(InStmt, i), null);
                }
                else if (ReadonlyData.STRING_TYPE == type)
                {
                    InPropertyInfos[i].SetValue(InBaseSubclassObj, ConvertTypeUtility.ChangeType(SQLite3.ColumnText(InStmt, i), type), null);
                }
                else if (type.IsArray)
                {
                    InBaseSubclassObj.OnSyncOne(i, SQLite3.ColumnText(InStmt, i));
                }
                else
                {
                    throw new FormatException("Can not convert this type.");
                }
            }

            return InBaseSubclassObj;
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes the data by identifier.
        /// </summary>
        /// <param name="InTableName">In table name.</param>
        /// <param name="InID">In identifier of data.</param>
        public bool DeleteByID(string InTableName, int InID)
        {
            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("DELETE FROM ").Append(InTableName).Append(" WHERE ID = ").Append(InID);

            bool result = Exec(stringBuilder.ToString());

            Exec("VACUUM"); //rebuild the built-in index.

            return result;
        }

        /// <summary>
        /// Deletes the data by SyncBase subclass object.
        /// </summary>
        /// <param name="InValue">In Subclass object id.</param>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        public bool DeleteT<T>(T InValue) where T : SyncBase
        {
            if (null != InValue)
            {
                SyncProperty property = SyncFactory.GetSyncProperty(typeof(T));

                return Delete(property.ClassName, "ID", property.Infos[0].GetValue(InValue, null));
            }
            return false;
        }

        public bool Delete(string InTableName, string InConditionFieldName, object InConditionValue)
        {
            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("DELETE FROM ").Append(InTableName)
                .Append(" WHERE ").Append(InConditionFieldName).Append(" = ")
                .Append(ConvertTypeUtility.ChangeType(InConditionValue));

            bool result = Exec(stringBuilder.ToString());

            if (result) Exec("VACUUM"); //rebuild the built-in index.

            return result;
        }

        public bool Delete(string InTableName, string InCondition)
        {
            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("DELETE FROM ").Append(InTableName)
                .Append(" WHERE ").Append(InCondition);

            bool result = Exec(stringBuilder.ToString());

            if (result) Exec("VACUUM"); //rebuild the built-in index.

            return result;
        }

        /// <summary>
        /// Clear table data by SyncBase of subclass.
        /// </summary>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        public bool DeleteAllT<T>() where T : SyncBase
        {
            SyncProperty property = SyncFactory.GetSyncProperty(typeof(T));

            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("DELETE FROM ").Append(property.ClassName);

            bool result = Exec(stringBuilder.ToString());

            if (result) Exec("VACUUM"); //rebuild the built-in index.

            return result;
        }

        public bool DeleteAll(string InTableName)
        {
            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("DELETE FROM ").Append(InTableName);

            bool result = Exec(stringBuilder.ToString());

            if (result) Exec("VACUUM"); //rebuild the built-in index.

            return result;
        }
        #endregion

        #region Drop
        /// <summary>
        /// Drop the table.
        /// </summary>
        /// <typeparam name="T">Subclass of SyncBase.</typeparam>
        public bool DropTable<T>()
        {
            return DropTable(SyncFactory.GetSyncProperty(typeof(T)).ClassName);
        }

        /// <summary>
        /// Drop the table.
        /// </summary>
        /// <param name="InTableName">In table name.</param>
        public bool DropTable(string InTableName)
        {
            if (0 != (SBLength = stringBuilder.Length)) stringBuilder.Remove(0, SBLength);
            stringBuilder.Append("DROP TABLE IF EXISTS ").Append(InTableName);

            return Exec(stringBuilder.ToString());
        }
        #endregion

        #region Close
        /// <summary>
        /// Closes the database.
        /// </summary>
        public void CloseDB()
        {
            if (SQLite3DbHandle.Zero != handle)
            {
                if (SQLite3Result.OK == SQLite3.Close(handle))
                {
                    handle = SQLite3DbHandle.Zero;
                }
                else
                {
                    ShowMsg(SQLite3.GetErrmsg(handle));
                }
            }
        }

        #endregion

        #region Common Function

        /// <summary>
        /// Executed the SQL statement and return the address of sqlite3.
        /// </summary>
        /// <returns>the address of sqlite3.</returns>
        /// <param name="InSQLStatement">In sql statement.</param>
        /// <param name="OutStmt">Out the SQLite3 handle.</param>
        private bool ExecuteQuery(string InSQLStatement, out SQLite3Statement OutStmt)
        {
            if (SQLite3Result.OK != SQLite3.Prepare2(handle, InSQLStatement, GetUTF8ByteCount(InSQLStatement), out OutStmt))
            {
                ShowMsg(InSQLStatement + "\nError: " + SQLite3.GetErrmsg(OutStmt));
                SQLite3.Finalize(OutStmt);

                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Executed the SQL statement.
        /// </summary>
        /// <returns>The exec.</returns>
        /// <param name="InSQLStatement">In SQL Statement.</param>
        public bool Exec(string InSQLStatement)
        {
            bool result = true;
            SQLite3Statement stmt;

            if (SQLite3Result.OK == SQLite3.Prepare2(handle, InSQLStatement, GetUTF8ByteCount(InSQLStatement), out stmt))
            {
                if (SQLite3Result.Done != SQLite3.Step(stmt))
                {
                    ShowMsg(InSQLStatement + "\nError: " + SQLite3.GetErrmsg(stmt));
                    result = false;
                }
            }
            else
            {
                ShowMsg(InSQLStatement + "\nError: " + SQLite3.GetErrmsg(stmt));
                result = false;
            }

            SQLite3.Finalize(stmt);

            return result;
        }

        /// <summary>
        /// get utf8 bytes length of string.
        /// </summary>
        /// <returns>The UTF 8 bytes count.</returns>
        /// <param name="InStr">In original string.</param>
        private int GetUTF8ByteCount(string InStr)
        {
            return Encoding.UTF8.GetByteCount(InStr);
        }

        private static void ShowMsg(string InMsg)
        {
            Debug.LogError(InMsg);
        }
        #endregion
    }
}