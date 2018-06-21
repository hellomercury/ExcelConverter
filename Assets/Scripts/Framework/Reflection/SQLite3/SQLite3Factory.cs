using System.IO;
using UnityEngine;

namespace Framework.Reflection.SQLite3Helper
{
    public static class SQLite3Factory
    {
        /// <summary>
        /// Open a sqlite3 database that exists in the persistentDataPath directory as read-only.
        /// </summary>
        /// <param name="InDbName">The name of the sqlite3 database.</param>
        /// <returns>Operation sqlite3 database handle.</returns>
        public static SQLite3Operate OpenToRead(string InDbName)
        {
            string persistentDbPath = Path.Combine(Application.persistentDataPath, InDbName);

            return File.Exists(persistentDbPath) ? new SQLite3Operate(persistentDbPath, SQLite3OpenFlags.ReadOnly) : null;
        }

        /// <summary>
        /// If there is no database in the persistentDataPath directory,
        /// Then copy the database from the streamingAssetsPath directory to the persistentDataPath directory and open the database in read-only mode
        /// Else if need to match detection
        ///        then If the incoming Md5 is not empty, it is determined whether the database Md5 of the persistentDataPath directory matches.
        ///                else  the incoming Md5 is empty, determine whether the database in the persistentDataPath directory is the same as the streamingAssetsPath directory.
        ///        Else Open the existing database.
        /// </summary>
        /// <param name="InDbName">The name of the sqlite3 database.</param>
        /// <param name="InNeedMatchDetection">Whether need to match detection.</param>
        /// <param name="InMd5"></param>
        /// <returns>Operation sqlite3 database handle.</returns>
        public static SQLite3Operate OpenToRead(string InDbName, bool InNeedMatchDetection, string InMd5 = null)
        {
            string persistentDbPath = Path.Combine(Application.persistentDataPath, InDbName);

#if !UNITY_EDITOR && UNITY_ANDROID
            string streamDbPath = Path.Combine("jar:file://" + Application.dataPath + "!/assets/", InDbName);
#elif UNITY_IOS
            string streamDbPath = Path.Combine(Application.dataPath + "/Raw/", InDbName);
#else
            string streamDbPath = Path.Combine(Application.streamingAssetsPath, InDbName);
#endif

            bool isNeedOverride = false;
            byte[] dbBytes = null;
            if (File.Exists(persistentDbPath))
            {
                if (InNeedMatchDetection)
                {
                    if (string.IsNullOrEmpty(InMd5))
                    {
#if !UNITY_EDITOR && UNITY_ANDROID
                        using (WWW www = new WWW(streamDbPath))
                        {
                            while (!www.isDone)
                            {
                            }

                            if (string.IsNullOrEmpty(www.error))
                            {
                                dbBytes = www.bytes;
                                isNeedOverride = !SQLite3Utility.GetBytesMD5(dbBytes).Equals(SQLite3Utility.GetFileMD5(persistentDbPath));
                            }
                            else isNeedOverride = true;
                        }
#else
                        dbBytes = File.ReadAllBytes(streamDbPath);
                        isNeedOverride = !MD5Utility.GetBytesMD5(dbBytes).Equals(MD5Utility.GetFileMD5(persistentDbPath));
#endif
                    }
                    else isNeedOverride = !InMd5.Equals(persistentDbPath);
                }
            }
            else isNeedOverride = true;

            if (isNeedOverride)
            {
                if (null == dbBytes)
                {
#if !UNITY_EDITOR && UNITY_ANDROID
                    using (WWW www = new WWW(streamDbPath))
                    {
                        while (!www.isDone)
                        {
                        }

                        if (string.IsNullOrEmpty(www.error)) dbBytes = www.bytes;
                        else Debug.LogError("Copy database from streamingAssetsPath to persistentDataPath error. " + www.error);
                    }
#else
                    dbBytes = File.ReadAllBytes(streamDbPath);
#endif
                }

                File.WriteAllBytes(persistentDbPath, dbBytes);
            }

            return new SQLite3Operate(persistentDbPath, SQLite3OpenFlags.ReadOnly);
        }

        /// <summary>
        /// Open a sqlite3 database that exists in the persistentDataPath directory as read-write,
        /// If the database does not exist, create an empty database.
        /// </summary>
        /// <param name="InDbName">The name of the sqlite3 database.</param>
        /// <returns>Operation sqlite3 database handle.</returns>
        public static SQLite3Operate OpenToWrite(string InDbName)
        {
            string persistentDbPath = Path.Combine(Application.persistentDataPath, InDbName);

            return new SQLite3Operate(persistentDbPath, SQLite3OpenFlags.Create | SQLite3OpenFlags.ReadWrite);
        }
    }
}