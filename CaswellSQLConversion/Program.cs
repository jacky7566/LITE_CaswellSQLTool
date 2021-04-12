using CaswellSQLConversion.EPI;
using CaswellSQLConversion.Equipment;
using CaswellSQLConversion.Utils;
using CaswellSQLConversion.XML;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SystemLibrary.Utility;

namespace CaswellSQLConversion
{   
    class Program
    {
        public static string GB_software_version;
        public static EPIConfigurationClass Obj_config;
        public static string Error_Message = string.Empty;
        public static string ManualRunIds = string.Empty;
        static void Main(string[] args)
        {
            //Get Function
            string function = ConfigurationManager.AppSettings["Function"].ToString();
            //Get Software Version
            GB_software_version = ConfigHelper.GetSoftwareVersion();
            //Start
            LogHelper.WriteLine("Start");
            if (ConfigHelper.ReadingConfiguration(ref Error_Message))
            {
                var conResult = false;
                switch (function)
                {
                    case "EPI":
                        conResult = EPIConversionMain();
                        LogHelper.WriteLine("EPI Run Conversion Success");
                        break;
                    case "EQP":
                        conResult = EquipmentConversionMain();
                        LogHelper.WriteLine("EQP Conversion Success");
                        break;
                    default:
                        break;
                }

                if (conResult == false)
                {
                    MailHelper.LogAndSendMail(Error_Message, null, true, "ReadingConfiguration_" + function );
                }
                else
                {
                    LogHelper.WriteLine("Finish");
                }
            }
            else
            {
                MailHelper.LogAndSendMail(Error_Message, null, true, "ReadingConfiguration");          
            }
        }

        /// <summary>
        /// 1. Collect data from MDB to object list
        //  2. Convert ech data into CSV and save into temp location
        //  3. Construct XML and use above CSV as datatype table. and save into temp location
        //  4. Once all done. copy all result from temp location into it's location(defined in ini)
        /// </summary>
        /// <returns></returns>
        private static bool EPIConversionMain()
        {
            try
            {
                EpitaxySQLEntities epiDb = new EpitaxySQLEntities();
                EPIDataConversionHelper conversionHelper = new EPIDataConversionHelper();
                List<string> tableImageUploadList = new List<string>();
                List<string> xmlUploadList = new List<string>();
                string tempFolder = ConfigHelper.AppPath() + "\\Temp";
                //Create Temp Folder if not exists
                if (Directory.Exists(tempFolder) == false) Directory.CreateDirectory(tempFolder); 
                
                string xDate = ConfigHelper.GetKey(Obj_config.Default_Start_Date);
                DateTime fromDate = DateTime.Parse(xDate);
                DateTime toDate = fromDate.AddHours(double.Parse(Obj_config.Number_of_hour_each_run));
                DateTime lastTxnDate = new DateTime();

                var mesMaxDate = epiDb.Runs.Select(r => r.Run_Date).Max().Value;
                if (toDate > mesMaxDate)
                    toDate = mesMaxDate;

                //var runInfoList
                var runList = conversionHelper.GetAllRunList(fromDate, toDate);

                if (runList.Count() == 0)
                {
                    LogHelper.WriteLine("No Run between {0} and {1}", fromDate.ToString(), toDate.ToString());
                    ConfigHelper.SetKey(toDate.ToString());
                    return true;
                }
                LogHelper.WriteLine("Run count: {0} running between {1} and {2}", runList.Count(), fromDate.ToString(), toDate.ToString());
                double n = runList.Count();
                double i = 0;
                StringBuilder errorRunIds = new StringBuilder();
                NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
                foreach (var run in runList)
                {
                    i++;
                    LogHelper.WriteLine(string.Format("{0}/{1} ({2})", i, n, (i / n).ToString("P", nfi)));
                    cls_tdsmodel xmlResultRoot = new cls_tdsmodel();
                    LogHelper.WriteLine("Process Run ID: " + run.Run_ID);
                    if (conversionHelper.ConversionByRun(xmlResultRoot, run, tempFolder, ref xmlUploadList, ref tableImageUploadList, ref lastTxnDate) == false)
                    {                        
                        errorRunIds.Append(run.Run_ID.ToString());
                        errorRunIds.AppendLine();
                    }
                    if (conversionHelper.FileUpload(xmlUploadList, Program.Obj_config.OutputXMLPath) == false)
                        return false;
                    if (conversionHelper.FileUpload(tableImageUploadList, Program.Obj_config.OutputTableImageAttachmentPath) == false)
                        return false;
                    xmlUploadList.Clear();
                    tableImageUploadList.Clear();
                    if (string.IsNullOrEmpty(ManualRunIds))
                        ConfigHelper.SetKey(lastTxnDate.ToShortDateString());
                    //var updateRun = epiDb.Runs.Where(r => r.Run_ID == run.Run_ID).FirstOrDefault();
                    //if (updateRun != null)
                    //{
                    //    updateRun.TDS_UploadDate = DateTime.Now;
                    //    epiDb.Entry(updateRun);
                    //    epiDb.SaveChanges();
                    //}
                }

                if (errorRunIds.Length > 0)
                {
                    LogHelper.WriteLine("Process Run ID: \n\r" + errorRunIds.ToString() + " Failed!");      
                }
            }
            catch (Exception ex)
            {
                Error_Message = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                return false;
            }

            return true;
        }

        private static bool EquipmentConversionMain()
        {
            List<string> toBeProcessedFileList = new List<string>();
            string tempFolder = ConfigHelper.AppPath() + "\\Temp";            
            try
            {
                EQPLogConversionHelper eqpHelper = new EQPLogConversionHelper();
                //bool result = false;
                //string eqpType = ConfigurationManager.AppSettings["Equipment Type"].ToString();
                //string summaryFolder = ConfigurationManager.AppSettings[string.Format("{0} Summary Source Folder", eqpType)].ToString();
                //Create Temp Folder if not exists
                if (Directory.Exists(tempFolder) == false) Directory.CreateDirectory(tempFolder);
                return eqpHelper.EQPMain();
            }
            catch (Exception ex)
            {
                Error_Message = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                return false;
            }
        }
    }
}
