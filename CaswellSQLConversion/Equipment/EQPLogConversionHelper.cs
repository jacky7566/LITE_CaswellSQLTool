using CaswellSQLConversion.EPI;
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

namespace CaswellSQLConversion.Equipment
{
    class EQPLogConversionHelper
    {
        string _TempFolder;
        List<string> _TableImagesAttachList;
        List<string> _XmlAttachList;
        List<string> _FailFileList;
        List<string> _SuccessFileList;

        public bool EQPMain()
        {
            try
            {
                string eqpType = ConfigurationManager.AppSettings["Equipment Type"].ToString();
                string summaryFolder = ConfigurationManager.AppSettings[string.Format("{0} Summary Source Folder", eqpType)].ToString();
                bool result = false;
                this._TableImagesAttachList = new List<string>();
                this._XmlAttachList = new List<string>();
                this._FailFileList = new List<string>();
                this._SuccessFileList = new List<string>();
                List<FileInfo> sumFileList = new List<FileInfo>();
                var filePatterns = ConfigurationManager.AppSettings["FilePatterns"].ToString().Split(',').ToList();
                foreach (var pattern in filePatterns)
                {
                    sumFileList.AddRange(new DirectoryInfo(summaryFolder).GetFiles(pattern, SearchOption.TopDirectoryOnly).ToList());
                }

                //File Count
                NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
                double n = sumFileList.Count();
                double i = 0; //counter
                foreach (var fi in sumFileList.OrderByDescending(r => r.Name))
                {
                    i++;
                    LogHelper.WriteLine(string.Format("{0}/{1} ({2}):{3}", i, n, (i / n).ToString("P", nfi), fi.Name));

                    switch (eqpType)
                    {
                        case "COMEGA02":
                            result = CreateXML_COMEGA02(fi.FullName);
                            break;
                        case "CSTEAG01":
                            result = CreateXML_CSTEAG01(fi.FullName);
                            break;
                        default:
                            break;
                    }

                    if (result == false)
                    {
                        this._FailFileList.Add(fi.FullName);
                    }
                    else
                    {
                        this._SuccessFileList.Add(fi.FullName);
                    }

                    //Upload all files
                    if (this.EQPFilesUpload() == false)
                    {
                        return false;
                    }
                    this._TableImagesAttachList.Clear();
                    this._XmlAttachList.Clear();
                    this._FailFileList.Clear();
                    this._SuccessFileList.Clear();
                }                
            }
            catch (Exception ex)
            {
                Program.Error_Message = ExceptionHelper.GetAllFootprints(ex);
                return false;
            }
            return true;
        }

        #region COMEGA02
        public bool COMEGA02Processing(string summaryFolder)
        {
            try
            {
                this._TableImagesAttachList = new List<string>();
                this._XmlAttachList = new List<string>();
                this._FailFileList = new List<string>();
                this._SuccessFileList = new List<string>();
                List<FileInfo> sumFileList = new List<FileInfo>();
                var filePatterns = ConfigurationManager.AppSettings["FilePatterns"].ToString().Split(',').ToList();
                foreach (var pattern in filePatterns)
                {
                    sumFileList.AddRange(new DirectoryInfo(summaryFolder).GetFiles(pattern, SearchOption.TopDirectoryOnly).ToList());
                }
                //File Count
                NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
                double n = sumFileList.Count();
                double i = 0; //counter
                foreach (var fi in sumFileList.OrderByDescending(r => r.Name))
                {
                    i++;
                    LogHelper.WriteLine(string.Format("{0}/{1} ({2}):{3}", i, n, (i / n).ToString("P", nfi), fi.Name));
                    if (CreateXML_COMEGA02(fi.FullName) == false)
                    {
                        this._FailFileList.Add(fi.FullName);
                    }
                    else
                    {
                        this._SuccessFileList.Add(fi.FullName);
                    }

                    //Upload all files
                    if (this.EQPFilesUpload() == false)
                    {
                        return false;
                    }
                    this._TableImagesAttachList.Clear();
                    this._XmlAttachList.Clear();
                    this._FailFileList.Clear();
                    this._SuccessFileList.Clear();
                }

            }
            catch (Exception ex)
            {
                Program.Error_Message = "Run Id: " + "" + " - \r\r Error Message:" + ExceptionHelper.GetAllFootprints(ex);
                return false;
            }
            return true;
        }
        private bool CreateXML_COMEGA02(string srcFileFullName)
        {
            try
            {
                cls_tdsmodel xmlResultRoot = new cls_tdsmodel()
                {
                    Result = "Done"
                };
                List<COMEGA02_DSClass> comega02DsList = new List<COMEGA02_DSClass>();

                var summaryLines = File.ReadAllLines(srcFileFullName);
                //properties
                var recipe = string.Empty;
                var lotnumber = string.Empty;
                var wafernumber = string.Empty;
                var recipedate = new DateTime();
                var header = string.Empty;
                var cassette = string.Empty;
                var cassette_date = string.Empty;
                var cassette_time = string.Empty;
                var csv_data = string.Empty;
                var recipename = string.Empty;
                var teststep = string.Empty;
                var teststepname = string.Empty;
                var sequence = string.Empty;
                var recipeheader = string.Empty;
                var recipenumber = string.Empty;

                var overAllMaxDate = new DateTime(1990, 1, 1);
                var overAllMinDate = new DateTime(2100, 1, 1);

                foreach (var line in summaryLines.Select(r => r.Replace("\"", ""))) //Replcate all double quote in summary file
                {
                    if (string.IsNullOrEmpty(line) == false)
                    {
                        var cells = line.Split(',');
                        var firstCellStr = cells[0];
                        //firstCellStr.Replace("\"", "");
                        switch (firstCellStr.ToUpper())
                        {
                            case "SEQUENCE":
                                sequence = cells[1];
                                break;
                            case "CASSETTE":
                                cassette = cells[1];
                                lotnumber = cells[2].ToUpper();
                                if (string.IsNullOrEmpty(lotnumber))
                                    lotnumber = "NA";
                                cassette_date = cells[5];
                                cassette_time = cells[6];
                                break;
                            case "WAFER":
                                wafernumber = cells[1];
                                var fileDetailList = new List<string>();
                                var filePreCondList = new List<string>();
                                GetSupportDatafiles(cassette, cassette_date, wafernumber, ref fileDetailList, ref filePreCondList);
                                var res = comega02DsList.Where(r => r.WaferNumber == wafernumber).FirstOrDefault();
                                if (res != null)
                                {
                                    res.FileDetailList = fileDetailList;
                                    res.FileDetailList = filePreCondList;
                                }
                                else
                                {
                                    COMEGA02_DSClass ds = new COMEGA02_DSClass()
                                    {
                                        WaferNumber = wafernumber,
                                        FileDetailList = fileDetailList,
                                        FilePreConditionList = filePreCondList
                                    };
                                    comega02DsList.Add(ds);
                                }
                                break;
                            case "RECIPE":
                                recipedate = ObjectHelper.UK2USDate(cells[6], cells[5]);
                                recipename = cells[2];
                                recipenumber = cells[1];
                                break;
                            case "STEP":
                                var p = line.IndexOf(",Time");
                                var tempHeader = line.Substring(p + 1, line.Length - p - 1);
                                header = "Cassette,Sequence,Lotnumber,Wafer,Date,RecipeNumber,RecipeName,StepNumber,StepName,DataType," + tempHeader;
                                csv_data = header;
                                teststep = cells[1];
                                teststepname = cells[2];
                                break;
                            case "MIN":
                            case "MAX":
                            case "SET":
                                var tempVal = line.Replace("Set,,", "Set");
                                tempVal = tempVal.Replace("Min,,", "Min");
                                tempVal = tempVal.Replace("Max,,", "Max");
                                tempVal = tempVal.Replace("N/A", "");
                                tempVal = tempVal.Replace("N/M", "");
                                var value = cassette + "," + sequence + "," + lotnumber + "," + wafernumber + "," + recipedate + "," + recipenumber + "," + recipename + "," + teststep + "," + teststepname + "," + tempVal;
                                csv_data = csv_data + Environment.NewLine + value.ToUpper();
                                break;
                            case "AVE":
                                var aveTempVal = line.Replace("Ave,,", "Ave");
                                aveTempVal = aveTempVal.Replace("N/A", "");
                                aveTempVal = aveTempVal.Replace("N/M", "");
                                var aveValue = cassette + "," + sequence + "," + lotnumber + "," + wafernumber + "," + recipedate + "," + recipenumber + "," + recipename + "," + teststep + "," + teststepname + "," + aveTempVal;
                                csv_data = csv_data + Environment.NewLine + aveValue.ToUpper();
                                var res2 = comega02DsList.Where(r => r.WaferNumber == wafernumber).FirstOrDefault();
                                if (res2 != null)
                                {
                                    if (res2.CSVObjectList == null)
                                        res2.CSVObjectList = new List<COMEGA02_CSVClass>();
                                    res2.CSVObjectList.Add(new COMEGA02_CSVClass()
                                    {
                                        RecipeName = recipename,
                                        RecipeDate = recipedate,
                                        TestStep = teststep,
                                        CSV = csv_data
                                    });
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }

                //Build Test Step for Result
                xmlResultRoot.TestStep.AddRange(GenerateTestSteps(comega02DsList, ref overAllMaxDate, ref overAllMinDate));

                var objCfg = Program.Obj_config;
                var startDateTime = ObjectHelper.UK2USDate(cassette_date, cassette_time);
                //Data Source
                var backupFileName = objCfg.BackFilePath + Path.DirectorySeparatorChar + Path.GetFileName(srcFileFullName);
                TestStep dataSrcTestStep = new TestStep()
                {
                    Name = "DATASOURCE",
                    startDateTime = startDateTime.ToString("s"),
                    endDateTime = overAllMaxDate.ToString("s")
                };
                dataSrcTestStep._DataAttachment.Add(new DataAttachment()
                {
                    Name = "COMEGA02_SUMMARY_SOURCE",
                    Value = backupFileName,
                    Status = "Done"
                });
                xmlResultRoot.TestStep.Add(dataSrcTestStep);

                //Header                                
                var xmlHeader = this.GenerateXmlHeader(lotnumber, "COMEGA02", startDateTime, overAllMaxDate);
                xmlResultRoot.Header = xmlHeader;

                xmlResultRoot.startDateTime = startDateTime.ToString("s");
                xmlResultRoot.endDateTime = overAllMaxDate.ToString("s");

                //Build Xml Info
                string xmlStr = string.Empty;
                if (xmlResultRoot.GetXML(ref xmlStr, ref Program.Error_Message) == false)
                {
                    return false;
                }
                else
                {
                    string xmlResultFileName = this._TempFolder
                            + string.Format(@"\Site={0},ProductFamily={1},Operation={2},PartNumber={3},SerialNumber={4},TestDate={5}.xml",
                            objCfg.Site, objCfg.ProductFamily, xmlHeader.Operation, xmlHeader.Partnumber,
                            xmlHeader.Serialnumber, startDateTime.ToString("yyyy-MM-ddTHH.mm.ss"));
                    if (File.Exists(xmlResultFileName))
                        File.Delete(xmlResultFileName);
                    xmlStr = xmlStr.Replace("\"12:00:00 AM\"", "");
                    File.WriteAllText(xmlResultFileName, xmlStr);
                    this._XmlAttachList.Add(xmlResultFileName);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }
        private bool GetSupportDatafiles(string cassette, string cassetDate, string wafer, ref List<string> fileDetailList, ref List<string> filePreCondList)
        {
            try
            {
                var dd = int.Parse(cassetDate.Substring(0, 2));
                var m = int.Parse(cassetDate.Substring(3, 2));
                var yyyy = int.Parse("20" + cassetDate.Substring(6, 2));
                var xDate = new DateTime(yyyy, m, dd);
                var logFolder = string.Format(@"\\cas-share.int.oclaro.com\casfab\ToolData\COMEGA02\{0}\Logs\DATALOG\FULLMV\{1}\DAY{2}",
                    yyyy, xDate.ToString("MMM", CultureInfo.InvariantCulture).ToUpper(), dd);

                var di = new DirectoryInfo(logFolder);
                //Pattern1 - Get Detail File          
                var waferStr1 = string.Empty;
                if (ObjectHelper.Val(wafer) >= 10)
                    waferStr1 = wafer;
                else
                    waferStr1 = "0" + ObjectHelper.Val(wafer);
                var logFilePattern1 = string.Format("C{0}{1}??.LOG", cassette, waferStr1);
                var detailFile = di.GetFiles(logFilePattern1, SearchOption.TopDirectoryOnly);
                //Pattern2 - Get Pre Condition file
                var waferStr2 = string.Empty;
                if (ObjectHelper.Val(wafer) >= 10)
                    waferStr2 = "0" + ObjectHelper.Val(wafer);
                else
                    waferStr2 = "00" + ObjectHelper.Val(wafer);
                var logFilePattern2 = string.Format("C{0}-{1}.LOG", cassette, waferStr2);
                var preCondFile = di.GetFiles(logFilePattern2, SearchOption.TopDirectoryOnly);

                if (detailFile.Count() > 0)
                    fileDetailList.AddRange(detailFile.Select(r => r.FullName).ToList());
                if (preCondFile.Count() > 0)
                    filePreCondList.AddRange(preCondFile.Select(r => r.FullName).ToList());
            }
            catch (Exception ex)
            {
                Program.Error_Message = ExceptionHelper.GetAllFootprints(ex);
                return false;
            }
            return true;
        }
        private List<TestStep> GenerateTestSteps(List<COMEGA02_DSClass> dsList, ref DateTime overAllMaxDate, ref DateTime overAllMinDate)
        {
            List<TestStep> testStepList = new List<TestStep>();
            this._TempFolder = ConfigHelper.AppPath() + "\\Temp";
            //Create Temp Folder if not exists
            if (Directory.Exists(this._TempFolder) == false) Directory.CreateDirectory(this._TempFolder);
            try
            {
                foreach (var ds in dsList)
                {
                    TestStep testStep = new TestStep()
                    {
                        Name = "WAFER" + ds.WaferNumber
                    };

                    int i = 0;
                    //Set DS Detail List into TestStep
                    foreach (var dFile in ds.FileDetailList)
                    {
                        i++;
                        testStep._DataAttachment.Add(new DataAttachment()
                        {
                            Name = "DETAIL_LOGSTEP" + i,
                            Value = dFile,
                            Status = "Done"
                        });
                    }

                    //Set DS PreCondition List into TestStep
                    foreach (var pFile in ds.FilePreConditionList)
                    {
                        testStep._DataAttachment.Add(new DataAttachment()
                        {
                            Name = "PRE_CONDITION",
                            Value = pFile,
                            Status = "Done"
                        });
                    }


                    var stepMaxDate = new DateTime(1990, 1, 1);
                    var stepMinDate = new DateTime(2100, 1, 1);
                    foreach (var csvObj in ds.CSVObjectList)
                    {
                        var csvFileName = string.Format("{0}_{1}_{2}_{3}.csv", ds.WaferNumber, csvObj.RecipeName,
                            csvObj.TestStep, Convert.ToDateTime(csvObj.RecipeDate).ToString("MM.dd.yyyy.HHmmss"));
                        var localCsvFileName = this._TempFolder + Path.DirectorySeparatorChar + csvFileName;
                        var netCsvFileName = Program.Obj_config.OutputTableImageAttachmentPath + Path.DirectorySeparatorChar + csvFileName;
                        if (File.Exists(localCsvFileName)) File.Delete(localCsvFileName);
                        File.WriteAllText(localCsvFileName, csvObj.CSV);
                        testStep._DataTable.Add(new DataTable()
                        {
                            Name = Program.Obj_config.Target_Table_COMEGA02_1,
                            Description = "SUMMARY DATA",
                            Value = netCsvFileName,
                            Status = "Done"
                        });
                        this._TableImagesAttachList.Add(localCsvFileName);
                        if (csvObj.RecipeDate > stepMaxDate) stepMaxDate = csvObj.RecipeDate;
                        if (csvObj.RecipeDate < stepMinDate) stepMinDate = csvObj.RecipeDate;
                        if (csvObj.RecipeDate > overAllMaxDate) overAllMaxDate = csvObj.RecipeDate;
                        if (csvObj.RecipeDate < overAllMinDate) overAllMinDate = csvObj.RecipeDate;
                    }
                    testStep.startDateTime = stepMinDate.ToString("s");
                    testStep.endDateTime = stepMaxDate.ToString("s");
                    testStepList.Add(testStep);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return testStepList;
        }
        private Header GenerateCOMEGA02XmlHeader(string lotNumber, DateTime cassetteStartDate, DateTime overAllMaxDate)
        {
            Header header = new Header();
            try
            {
                var objCfg = Program.Obj_config;
                header.Partnumber = objCfg.PartNumber;
                header.Devicetype = objCfg.DeviceType;
                header.Site = Program.Obj_config.Site;
                header.Operation = "COMEGA02";
                header.Teststation = "COMEGA02";
                header.Serialnumber = lotNumber;
                header.OperatorId = "NA";
                header.Result = "Done";
                header.Starttime = cassetteStartDate.ToString("s");
                header.Endtime = overAllMaxDate.ToString("s");
            }
            catch (Exception)
            {
                throw;
            }
            return header;
        }
        private bool EQPFilesUpload()
        {
            EPIDataConversionHelper conversionHelper = new EPIDataConversionHelper();
            try
            {
                if (conversionHelper.FileUpload(this._XmlAttachList, Program.Obj_config.OutputXMLPath) == false)
                    return false;
                if (conversionHelper.FileUpload(this._TableImagesAttachList, Program.Obj_config.OutputTableImageAttachmentPath) == false)
                    return false;
                if (conversionHelper.FileUpload(this._SuccessFileList, Program.Obj_config.BackFilePath) == false)
                    return false;
                if (conversionHelper.FileUpload(this._FailFileList, Program.Obj_config.ErrorFilePath) == false)
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }
        #endregion

        #region CSTEGA01
        public bool CreateXML_CSTEAG01(string srcFileFullName)
        {
            try
            {
                cls_tdsmodel xmlResultRoot = new cls_tdsmodel()
                {
                    Result = "Done"
                };
                var cSTEAG = this.GetCSTEAGInfo(srcFileFullName);
                if (cSTEAG.CSVDicList.Count() > 0)
                {
                    this._TempFolder = ConfigHelper.AppPath() + "\\Temp";
                    var csv = this.GenerateCSTEAGCsv(cSTEAG);
                    var csvFileName = string.Format("{0}_{1}_{2}.csv", cSTEAG.SerialNumber, cSTEAG.Recipe,
                        cSTEAG.StartDateTime.ToString("MM.dd.yyyy.HHmmss"));
                    var localCsvFileName = this._TempFolder + Path.DirectorySeparatorChar + csvFileName;
                    var netCsvFileName = Program.Obj_config.OutputTableImageAttachmentPath + Path.DirectorySeparatorChar + csvFileName;
                    if (File.Exists(localCsvFileName)) File.Delete(localCsvFileName);
                    File.WriteAllText(localCsvFileName, csv);

                    TestStep testStep = new TestStep()
                    {
                        Name = "SN_" + cSTEAG.SerialNumber,
                        startDateTime = cSTEAG.StartDateTime.ToString("s"),
                        endDateTime = cSTEAG.StartDateTime.ToString("s"),
                        Status = "Done"
                    };

                    testStep._DataTable.Add(new DataTable()
                    {
                        Name = Program.Obj_config.Target_Table_COMEGA02_1,
                        Description = "SUMMARY DATA",
                        Value = netCsvFileName,
                        Status = "Done"
                    });
                    xmlResultRoot.TestStep.Add(testStep);

                    //Header                                
                    var xmlHeader = this.GenerateXmlHeader(cSTEAG.SerialNumber, "CSTEAG01", cSTEAG.StartDateTime, cSTEAG.StartDateTime);
                    xmlResultRoot.Header = xmlHeader;

                    //Build Xml Info
                    var objCfg = Program.Obj_config;
                    string xmlStr = string.Empty;
                    if (xmlResultRoot.GetXML(ref xmlStr, ref Program.Error_Message) == false)
                    {
                        return false;
                    }
                    else
                    {
                        string xmlResultFileName = this._TempFolder
                                + string.Format(@"\Site={0},ProductFamily={1},Operation={2},PartNumber={3},SerialNumber={4},TestDate={5}.xml",
                                objCfg.Site, objCfg.ProductFamily, objCfg.Operation_Prefix, "NA",
                                cSTEAG.SerialNumber, cSTEAG.StartDateTime.ToString("yyyy-MM-ddTHH.mm.ss"));
                        if (File.Exists(xmlResultFileName))
                            File.Delete(xmlResultFileName);
                        File.WriteAllText(xmlResultFileName, xmlStr);
                        this._XmlAttachList.Add(xmlResultFileName);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }

        private CSTEAGClass GetCSTEAGInfo(string srcFileFullName)
        {
            CSTEAGClass cSTEAG = new CSTEAGClass();
            cSTEAG.CSVDicList = new Dictionary<string, List<double>>();
            try
            {                                
                string lineStr = string.Empty;
                string[] cells = null;
                var summaryLines = File.ReadAllLines(srcFileFullName);
                //For Content
                double rate = 0;
                double number = 0;
                int addRowCount = 0;
                //All Content start from _samples key
                int contentStartRow = Array.IndexOf(summaryLines, summaryLines.Where(r => r.Contains("_samples")).FirstOrDefault());
                //Get Header
                for (int i = 0; i < summaryLines.Count(); i++)
                {
                    lineStr = summaryLines[i];
                    if (lineStr.TrimStart(' ').Equals("$"))
                    {
                        i++;
                        continue;
                    }
                    if (string.IsNullOrEmpty(lineStr) == false && lineStr.Contains("$"))
                    {
                        lineStr = lineStr.TrimStart(' ').TrimStart('$');
                        lineStr = lineStr.Replace("$", ",").Replace(", ,", ",");
                        cells = lineStr.Split(',');
                        switch (cells[0])
                        {
                            case "_matid":
                                cSTEAG.SerialNumber = cells[1];
                                break;
                            case "_recipe":
                                cSTEAG.Recipe = cells[1];
                                break;
                            case "_operator":
                                cSTEAG.Operator = cells[1];
                                break;
                            case "_time":
                                cSTEAG.StartDateTime = Convert.ToDateTime(cells[1]);
                                break;
                            default:
                                if (i > contentStartRow) //Content
                                {
                                    var resList = new List<double>();
                                    var timeList = new List<double>();
                                    var dList = new List<double>();
                                    rate = double.Parse(new string(cells[1].Where(c => char.IsDigit(c)).ToArray()));
                                    number = int.Parse(new string(cells[2].Where(c => char.IsDigit(c)).ToArray()));
                                    addRowCount = Convert.ToInt32(Math.Ceiling(number / 10)); //each row data has 10 columns
                                    i = i + 2; //add 2 rows to find the data content
                                    var unit = rate / 1000; //ms to sec
                                    for (double j = 0; j < number * unit; j = j + unit) //Get Timing by number * unit
                                    {
                                        if (j == 0)
                                            timeList.Add(j);
                                        timeList.Add(j + unit);
                                    }
                                    timeList.RemoveRange(Convert.ToInt32(number), 1); //Remove extra 1 data
                                    cSTEAG.CSVDicList.Add(cells[0].ToUpper().Replace(" ", "_") + "_TIMING", timeList);
                                    var data = summaryLines.ToList().GetRange(i, addRowCount); //i=30 -> addRowCount = 100
                                    foreach (var item in data) //Get all data and combine them into one list
                                    {
                                        dList = item.TrimEnd(',').Split(',').Select(x => double.Parse(x)).ToList();
                                        resList.AddRange(dList);
                                    }
                                    cSTEAG.CSVDicList.Add(cells[0].ToUpper().Replace(" ", "_") + "_SAMPLING", resList);
                                    i = i + addRowCount; //Move to next Test Parameter by using row count
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return cSTEAG;
        }

        private string GenerateCSTEAGCsv(CSTEAGClass cSTEAG)
        {
            var dic = cSTEAG.CSVDicList;
            StringBuilder csv = new StringBuilder();
            try
            {
                var maxRow = dic.Select(r => r.Value.Count()).Max(); //Max Row 11011
                csv.Append("SerialNumber,");
                csv.AppendLine(String.Join(",", dic.Select(r => r.Key))); //Header
                for (int i = 0; i < maxRow; i++)
                {
                    csv.Append(cSTEAG.SerialNumber + ",");
                    csv.AppendLine(String.Join(",", dic.Select(r => r.Value.Count > i ? r.Value[i].ToString() : "")));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return csv.ToString();
        }        
        private void TestMarco()
        {
            //string marcoPath = ConfigHelper.AppPath() + "\\SteagReader.xlsm";
            //ExcelMacroHelper excelMacroHelper = new ExcelMacroHelper();
            //object[] inputObj = new object[] { @"C:\Users\lic67888\Box\jacky.li\Documents\Public\20 Kaizen TDS\Caswell\00 Doc\Equipment\CSTEAG01_02\cqr0460.1.000" };
            //object rtnVal;
            //excelMacroHelper.RunExcelMacro(marcoPath, "OpenSteagLog", inputObj, out rtnVal, false);
        }
        #endregion

        #region Utilities
        private Header GenerateXmlHeader(string serialNum, string testStation, DateTime startDate, DateTime overAllMaxDate)
        {
            Header header = new Header();
            try
            {
                var objCfg = Program.Obj_config;
                header.Partnumber = objCfg.PartNumber;
                header.Devicetype = objCfg.DeviceType;
                header.Site = Program.Obj_config.Site;
                header.Operation = Program.Obj_config.Operation_Prefix;
                header.Teststation = testStation;
                header.Serialnumber = serialNum;
                header.OperatorId = "NA";
                header.Result = "Done";
                header.Starttime = startDate.ToString("s");
                header.Endtime = overAllMaxDate.ToString("s");
            }
            catch (Exception)
            {
                throw;
            }
            return header;
        }
        #endregion
    }
}
