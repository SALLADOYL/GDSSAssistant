using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;

using Android.Database.Sqlite;
using SQLite;
using System.IO;
using GDSSAssistant.Activities;

namespace GDSSAssistant.DataManager
{
    class DBManagerHelper : SQLiteOpenHelper
    {
        // specifies the database name
        //private const string DatabaseName1 = "BBSITCommAssistant_DB.db";
        //specifies the database version (increment this number each time you make database related changes)
        private const int DatabaseVersion = 3;
        public string DatabasePath;

        public DBManagerHelper(Context context) : base(context, Application.Context.GetString(Resource.String.CurDBFilename), null, DatabaseVersion)
        {
            this.DatabasePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), Application.Context.GetString(Resource.String.CurDBFilename));

        }
        
        public override void OnCreate(SQLiteDatabase db)
        {

            DatabasePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), Application.Context.GetString(Resource.String.CurDBFilename));
            
            using (var Objdb = new SQLiteConnection(DatabasePath))
            {
                //if (Objdb.TableMappings.Count() == 0) {
                var tblEmailEntity = Objdb.GetTableInfo("EmailEntity");
                if (tblEmailEntity.Count==0) {
                    Objdb.CreateTable<EmailEntity>();
                }

                var tblErrorLogEntity = Objdb.GetTableInfo("ErrorLogEntity");
                if (tblErrorLogEntity.Count == 0)
                {
                    Objdb.CreateTable<ErrorLogEntity>();
                }

                var tblContactEntity = Objdb.GetTableInfo("ContactEntity");
                if (tblContactEntity.Count == 0)
                {
                    Objdb.CreateTable<ContactEntity>();
                }

                var tblConfigEntity = Objdb.GetTableInfo("ConfigEntity");
                if (tblConfigEntity.Count == 0)
                {
                    Objdb.CreateTable<ConfigEntity>();
                    //populate configentity table /Application.Context.GetString(Resource.String.CurDBFilename)
                    Objdb.Insert(new ConfigEntity
                    {
                        Name = "GDSSEMailServerPort",
                        Value = Application.Context.GetString(Resource.String.GDSSEMailServerPort),
                        Type = "string"
                    });
                    Objdb.Insert(new ConfigEntity
                    {
                        Name = "GDSSEMailServerHost",
                        Value = Application.Context.GetString(Resource.String.GDSSEMailServerHost),
                        Type = "string"
                    });
                    Objdb.Insert(new ConfigEntity {
                        Name = "GDSSEMailAccount",
                        Value = Application.Context.GetString(Resource.String.GDSSEMailAccount),
                        Type ="string"
                    });
                    Objdb.Insert(new ConfigEntity
                    {
                        Name = "GDSSEMailPwd",
                        Value = Application.Context.GetString(Resource.String.GDSSEMailPwd),
                        Type = "string"
                    });

                }
            }
        }

        private void PopulateConfigEntity() {
        }

        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
            if (oldVersion < 2)
            {
                //perform any database upgrade tasks for versions prior to  version 2              
            }
            if (oldVersion < 3)
            {
                //perform any database upgrade tasks for versions prior to  version 3
            }
        }
    }

    public class DatabaseUpdates
    {
        private DBManagerHelper _helper;

        public void SetContext(Context context)
        {
            if (context != null)
            {
                _helper = new DBManagerHelper(context);
            }
        }

        public List<EmailObject> EmailOBJ() {
            List<EmailObject> retVal = new List<EmailObject>();
            string EmailID = "";
            string EmailFrom = "";
            string EmailSubject = "";
            string EmailBody = "";
            string EmailCC = "";
            DateTime EmailDate = DateTime.Parse( DateTime.Now.ToString() ); //= item.EmailDate;
            
            
            var db = new SQLiteConnection(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), 
                Application.Context.GetString(Resource.String.CurDBFilename)));
            {
                try
                    {
                        var qry = db.Query<EmailEntity>("SELECT * FROM EmailEntity ORDER BY EmailDate DESC");
                        
                        foreach (EmailEntity item in qry) {

                             EmailID = item.EmailID.ToString() ?? "";
                             EmailFrom = item.EmailFrom ?? "";
                             EmailSubject = item.EmailSubject ?? "";
                             EmailBody = item.EmailBody ?? "";
                             EmailCC = item.EmailCC ?? "";
                             EmailDate = item.EmailDate;

                        //Console.WriteLine(EmailID);
                        
                            EmailObject curItem = new EmailObject(EmailID, EmailFrom, EmailSubject, EmailBody, EmailCC, EmailDate);
                        
                            retVal.Add(curItem);
                        }
                        return retVal;
                    }
                    catch (Exception ex)
                    {
                    //exception handling code to go here
                    string errOut = "";
                    errOut += "EmailID:" + EmailID;
                    errOut += "EmailFrom:" + EmailFrom;
                    errOut += "EmailSubject:" + EmailSubject;
                    
                    Console.WriteLine(ex.Source + "|" + ex.Message + "|" + errOut);
                    return retVal;
                    }
            }
        }

        public List<ConfigObject> GetConfigList()
        {
            List<ConfigObject> retVal = new List<ConfigObject>();
            string ConfigName = "";
            string ConfigValue = "";
            string ConfigType = "";
            var db = new SQLiteConnection(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                Application.Context.GetString(Resource.String.CurDBFilename)));
            {
                try
                {
                    var qry = db.Query<ConfigEntity>("SELECT * FROM ConfigEntity ORDER BY Name ASC");

                    foreach (ConfigEntity item in qry)
                    {
                        ConfigName = item.Name  ?? "";
                        ConfigValue = item.Value ?? "";
                        ConfigType = item.Type ?? "";

                        ConfigObject curItem = new ConfigObject(ConfigName, ConfigValue, ConfigType);
                        retVal.Add(curItem);
                    }
                    return retVal;
                }
                catch (Exception ex)
                {
                    //exception handling code to go here
                    string errOut = "";
                    errOut += "Name:" + ConfigName;
                    errOut += "Value:" + ConfigValue;
                    errOut += "Type:" + ConfigType;

                    Console.WriteLine(ex.Source + "|" + ex.Message + "|" + errOut);
                    return retVal;
                }
            }
        }

        public bool IsEmailExist(string ThisEmailID) {
            var db = new SQLiteConnection(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), 
                Application.Context.GetString(Resource.String.CurDBFilename)));
            {
                try
                {
                    var qry = db.Query<EmailEntity>("select * from EmailEntity where EmailID='" + ThisEmailID + "'");
                   
                    if (qry.Count > 0) { return true; }
                    else { return false;  }

                }
                catch (Exception ex)
                {
                    //exception handling code to go here
                    Console.WriteLine(ex.Source + "|" + ex.Message);
                    return true;
                }
            }
        }

        public string GetConfigValue(string ConfigName) {
            var db = new SQLiteConnection(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                Application.Context.GetString(Resource.String.CurDBFilename)));
            {
                try
                {
                    var qry = db.Query<ConfigEntity>("select * from ConfigEntity where Name='" + ConfigName + "'");

                    if (qry.Count > 0)
                    {
                        foreach (var item in qry) {
                            return item.Value;
                        }
                        return null;
                    } else {
                        return null;
                    }

                }
                catch (Exception ex)
                {
                    //exception handling code to go here
                    Console.WriteLine(ex.Source + "|" + ex.Message);
                    return null;
                }
            }
        }
        
        public long AddEmail(EmailEntity addEmail)
        {
            var db = new SQLiteConnection(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), Application.Context.GetString(Resource.String.CurDBFilename)));
            
                try
                {
                    return db.Insert(addEmail);
                }
                catch (Exception ex)
                {
                //exception handling code to go here
                Console.WriteLine(ex.Source + "|" + ex.Message);
                    return -1;
                }
        }

        public long AddConfig(ConfigEntity addConfigEntry) {
            var db = new SQLiteConnection(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), Application.Context.GetString(Resource.String.CurDBFilename)));

            try
            {
                return db.Insert(addConfigEntry);
            }
            catch (Exception ex)
            {
                //exception handling code to go here
                Console.WriteLine(ex.Source + "|" + ex.Message);
                return -1;
            }
        }

        public long ClearMail() {
            var db = new SQLiteConnection(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), Application.Context.GetString(Resource.String.CurDBFilename)));

            try
            {
                return db.Execute("delete from EmailEntity");
            }
            catch (Exception ex)
            {
                //exception handling code to go here
                Console.WriteLine(ex.Source + "|" + ex.Message);
                return -1;
            }
        }

        public long UpdateEmail(EmailEntity updateEmail)
        {
            var db = new SQLiteConnection(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), Application.Context.GetString(Resource.String.CurDBFilename)));
            
                try
                {
                    return db.Update(updateEmail);
                }
                catch (Exception ex)
                {
                //exception handling code to go here
                Console.WriteLine(ex.Source + "|" + ex.Message);
                return -1;
                }
        }

        public long UpdateConfig(ConfigEntity updateConfig)
        {
            var db = new SQLiteConnection(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), Application.Context.GetString(Resource.String.CurDBFilename)));

            try
            {
                return db.Update(updateConfig);
            }
            catch (Exception ex)
            {
                //exception handling code to go here
                Console.WriteLine(ex.Source + "|" + ex.Message);
                return -1;
            }
        }

        public long DeleteEmail(EmailEntity deleteEmail)
{
            var db = new SQLiteConnection(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), Application.Context.GetString(Resource.String.CurDBFilename)));
    
             try
            {
                return db.Delete(deleteEmail);
            }
            catch (Exception ex)
            {
                //exception handling code to go here
                Console.WriteLine(ex.Source + "|" + ex.Message);
                return -1;
            }
        }

        public long DeleteConfig(ConfigEntity deleteConfig)
        {
            var db = new SQLiteConnection(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), Application.Context.GetString(Resource.String.CurDBFilename)));

            try
            {
                return db.Delete(deleteConfig);
            }
            catch (Exception ex)
            {
                //exception handling code to go here
                Console.WriteLine(ex.Source + "|" + ex.Message);
                return -1;
            }
        }

        public long AddErrorLog(ErrorLogEntity addErrLog)
        {
            var db = new SQLiteConnection(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), Application.Context.GetString(Resource.String.CurDBFilename)));
            
                try
                {
                    return db.Insert(addErrLog);
                }
                catch (Exception ex)
                {
                //exception handling code to go here
                Console.WriteLine(ex.Source + "|" + ex.Message);
                return -1;
                }
            }
    }

    #region "Entities"
    public class EmailEntity {
        [PrimaryKey, AutoIncrement, Column("_Id")] public int Id { get; set; }
        public String EmailID { get; set; }
        public String EmailFrom { get; set; }
        public String EmailCC { get; set; }
        public DateTime EmailDate { get; set; }
        public String EmailSubject { get; set; }
        public String EmailBody { get; set; }
    }
    public class ErrorLogEntity { 
        [PrimaryKey, AutoIncrement, Column("_Id")] public int Id { get; set; }
        public String ErrSource { get; set; }
        public String ErrMessage { get; set; }
    }

    public class ContactEntity {
        [PrimaryKey, AutoIncrement, Column("_Id")]
        public int Id { get; set; }
        public String Name { get; set; }
        public String EmailAddress { get; set; }
    }

    public class ConfigEntity
    {
        [PrimaryKey, AutoIncrement, Column("_Id")]
        public string Name { get; set; }
        public String Value { get; set; }
        public String Type { get; set; }
    }

    #endregion
}