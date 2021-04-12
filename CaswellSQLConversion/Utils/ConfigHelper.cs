using CaswellSQLConversion.EPI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SystemLibrary.Utility;

namespace CaswellSQLConversion
{
    public static class ConfigHelper
    {
        public static string AppPath()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetSoftwareVersion()
        {
            string title = Assembly.GetExecutingAssembly().GetName().Name;
            string major = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductMajorPart.ToString();
            string minor = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductMinorPart.ToString();
            string revision = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductBuildPart.ToString();
            return string.Format("{0} V{1}.{2}{3}", title, major, minor, revision);
        }

        public static bool ReadingConfiguration(ref string errorMessage)
        {
            try
            {
                LogHelper.WriteLine("Start reading ini configruation");
                Program.Obj_config = new EPIConfigurationClass();
                //Target Table
                Program.Obj_config.Target_Table_EPIRUNSUMMARY = ConfigurationManager.AppSettings["Target Table(RUN)"].ToString();
                Program.Obj_config.Target_Table_WAFERSUMMARY = ConfigurationManager.AppSettings["Target Table(WAFER)"].ToString();
                Program.Obj_config.Target_Table_REAGENTSUMMARY = ConfigurationManager.AppSettings["Target Table(REAGENT)"].ToString();
                Program.Obj_config.Target_Table_LAYERSUMMARY = ConfigurationManager.AppSettings["Target Table(LAYER)"].ToString();
                Program.Obj_config.Target_Table_COMEGA02_1 = ConfigurationManager.AppSettings["Target Table Name1"].ToString();
                Program.Obj_config.Target_Table_COMEGA02_2 = ConfigurationManager.AppSettings["Target Table Name2"].ToString();
                //Sql Condition
                Program.Obj_config.Default_Start_Date = ConfigurationManager.AppSettings["Default Start Date"].ToString();
                Program.Obj_config.Maximum_row_each_run = ConfigurationManager.AppSettings["Maximum row each run"].ToString();
                if (string.IsNullOrEmpty(Program.Obj_config.Maximum_row_each_run)) //Set default value
                    Program.Obj_config.Maximum_row_each_run = "100";
                Program.Obj_config.Number_of_hour_each_run = ConfigurationManager.AppSettings["Number of hour of each run"].ToString();
                if (string.IsNullOrEmpty(Program.Obj_config.Number_of_hour_each_run))
                    Program.Obj_config.Number_of_hour_each_run = "24"; //Set default value
                //Configurations
                Program.Obj_config.Site = ConfigurationManager.AppSettings["Site"].ToString();
                Program.Obj_config.ProductFamily = ConfigurationManager.AppSettings["Product Family"].ToString();
                Program.Obj_config.Operation_Prefix = ConfigurationManager.AppSettings["Operation Prefix"].ToString();
                Program.Obj_config.TestStation = ConfigurationManager.AppSettings["TestStation"].ToString();
                Program.Obj_config.PartNumber = ConfigurationManager.AppSettings["Part Number"].ToString();
                Program.Obj_config.DeviceType = ConfigurationManager.AppSettings["Device Type"].ToString();
                Program.Obj_config.TestStepName = ConfigurationManager.AppSettings["Test Step Name"].ToString();                
                //File Configurations
                Program.Obj_config.OutputXMLPath = ConfigurationManager.AppSettings["Output XML Path"].ToString();
                Program.Obj_config.OutputTableImageAttachmentPath = ConfigurationManager.AppSettings["Output Table/Image/Attachment Path"].ToString();
                Program.Obj_config.BackFilePath = ConfigurationManager.AppSettings["Backup File Path"].ToString();
                Program.Obj_config.ErrorFilePath = ConfigurationManager.AppSettings["Error File Path"].ToString();
                //Error Checking
                if (string.IsNullOrEmpty(Program.Obj_config.Site))
                {
                    errorMessage = "Site is empty";
                    return false;
                }
                if (string.IsNullOrEmpty(Program.Obj_config.ProductFamily))
                {
                    errorMessage = "Product Family is empty";
                    return false;
                }
                if (string.IsNullOrEmpty(Program.Obj_config.PartNumber))
                {
                    errorMessage = "Partnumber name is empty";
                    return false;
                }
                if (string.IsNullOrEmpty(Program.Obj_config.Operation_Prefix))
                {
                    errorMessage = "Operation name is empty";
                    return false;
                }
                if (Directory.Exists(Program.Obj_config.OutputXMLPath) == false)
                {
                    errorMessage = string.Format("Path: {0} does not exist.", Program.Obj_config.OutputXMLPath);
                    return false;
                }
                if (Directory.Exists(Program.Obj_config.OutputTableImageAttachmentPath) == false)
                {
                    errorMessage = string.Format("Path: {0} does not exist.", Program.Obj_config.OutputTableImageAttachmentPath);
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorMessage = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                return false;
            }
            return true;
        }

        #region Get/Set LastTxnKey
        public static string GetKey(string default_start_date)
        {
            string keyName = "LastRunDate";
            string keyFolder = AppPath() + @"\key";
            string startKey = default_start_date;
            try
            {
                if (Directory.Exists(keyFolder) == false)
                    Directory.CreateDirectory(keyFolder);
                string fileName = string.Format(@"{0}\{1}.txt", keyFolder, keyName);
                if (File.Exists(fileName) == false)
                {
                    File.WriteAllText(fileName, startKey);
                    return startKey;
                }
                else
                {
                    string fileContents = File.ReadAllText(fileName);
                    if (IsDate(fileContents))
                        return fileContents;
                    else
                        return startKey;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public static bool SetKey(string maxKey)
        {
            string keyName = "LastRunDate";
            string keyFolder = AppPath() + @"\key";
            try
            {
                if (Directory.Exists(keyFolder) == false)
                    Directory.CreateDirectory(keyFolder);
                string fileName = string.Format(@"{0}\{1}.txt", keyFolder, keyName);
                File.WriteAllText(fileName, maxKey);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        #endregion

        public static bool IsDate(this string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                DateTime dt;
                return (DateTime.TryParse(input, out dt));
            }
            else
            {
                return false;
            }
        }
    }
}
