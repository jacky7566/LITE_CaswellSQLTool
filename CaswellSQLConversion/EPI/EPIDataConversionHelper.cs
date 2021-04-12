using AutoMapper;
using CaswellSQLConversion.Utils;
using CaswellSQLConversion.XML;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using SystemLibrary.Utility;
using static CaswellSQLConversion.XML.TestStep;

namespace CaswellSQLConversion.EPI
{
    public class EPIDataConversionHelper
    {
        public bool ConversionByRun(cls_tdsmodel xmlResultRoot, Run_View run, string tempFolder,
            ref List<string> xmlToUploadList, ref List<string> attachToUploadList, ref DateTime lastTxnDate)
        {
            EpitaxySQLEntities db = new EpitaxySQLEntities();
            var objCfg = Program.Obj_config;
            try
            {
                string reactorName = run.Reactor.Reactor_Name;
                lastTxnDate = run.Run_Date.Value;
                //Summary Info
                TestStep summaryStep = new TestStep();
                summaryStep.Name = "EPI_RUN_SUMMARY";
                summaryStep.Status = "Done";
                //Set XML Header
                xmlResultRoot.Result = "Done";
                if (run.Start_Time == null)
                    run.Start_Time = run.Run_Date.Value;
                xmlResultRoot.startDateTime = run.Start_Time.ToString();
                if (run.End_Time == null)
                    run.End_Time = run.Start_Time.Value.AddHours(1);
                xmlResultRoot.endDateTime = run.End_Time.ToString();
                //Test time
                var runStartTime = TestTimeConversion(run.Run_Date.Value, run.Start_Time.Value, run.End_Time.Value, true);
                var runEndTime = TestTimeConversion(run.Run_Date.Value, run.Start_Time.Value, run.End_Time.Value, false);
                xmlResultRoot.startDateTime = runStartTime.ToString("s");
                xmlResultRoot.endDateTime = runEndTime.ToString("s");
                summaryStep.startDateTime = runStartTime.ToString("s");
                summaryStep.endDateTime = runEndTime.ToString("s");
                //Set Header Info
                //var partNumber = (from pocket in db.Pockets
                //                   where pocket.Run_ID == run.Run_ID
                //                   select pocket.PROMIS_Part.Part_Number).Where(r=> r != null).ToList().FirstOrDefault();
                //if (string.IsNullOrEmpty(partNumber)) partNumber = "NA";
                var partNumber = Program.Obj_config.PartNumber;

                Header header = GenerateHeaderInfo(run, runStartTime.ToString("s"), runEndTime.ToString("s"), partNumber);
                xmlResultRoot.Header = header;

                //EPI Run Info
                string runCsvFileName = "EPIRUN_" + reactorName + run.Descriptor + "_" + runStartTime.ToString("yyyy-MM-ddTHH.mm.ss") + ".csv";
                string localRunFileName = GenRunSummaryFile(run, runCsvFileName, tempFolder);
                string netRunFileName = objCfg.OutputTableImageAttachmentPath + "\\" + runCsvFileName;
                TestStep epiRunTestStep = GenTestStepInfo(run, "EPIRUN_INFORMATION");
                epiRunTestStep.startDateTime = runStartTime.ToString("s");
                epiRunTestStep.endDateTime = runEndTime.ToString("s");
                //epiRunTestStep._DataTable.Add(new DataTable() { Name = objCfg.Target_Table_EPIRUNSUMMARY,
                //    Description = "EPIRUN", Value = netRunFileName, Status = "Done" });
                xmlResultRoot.TestStep.Add(epiRunTestStep);
                attachToUploadList.Add(localRunFileName); //File Upload
                //Layer Settings               
                var layerSettingList = GenLayerSettingList(run.Run_ID);
                if (layerSettingList.Count() > 0)
                {
                    string layerSettingsCsvFileName = "EPIRUN_" + reactorName + run.Descriptor
                    + "_LayerSettings_" + runStartTime.ToString("yyyy-MM-ddTHH.mm.ss") + ".csv";
                    string localLayerSettingsFileName = GenRunSummaryFile(layerSettingList, layerSettingsCsvFileName, tempFolder);
                    string netLayerSettingsFileName = objCfg.OutputTableImageAttachmentPath + "\\" + layerSettingsCsvFileName;
                    attachToUploadList.Add(localLayerSettingsFileName); //File Upload            
                    summaryStep._DataTable.Add(new DataTable()
                    { Name = Program.Obj_config.Target_Table_REAGENTSUMMARY, Status = "Done", Description = "EPI_WAFER_INFORMATION", Value = netLayerSettingsFileName });
                    //Add Layer Settings Test Step
                    xmlResultRoot.TestStep.AddRange(this.BuildLayerSettingTestSteps(layerSettingList, runStartTime, runEndTime));
                }
                //Layer Result Info
                var layerResList = GenLayerResultList(run.Run_ID);
                if (layerResList.Count() > 0)
                {
                    string layerResCsvFileName = "EPIRUN_" + reactorName + run.Descriptor
                        + "_LayerResults_" + runStartTime.ToString("yyyy-MM-ddTHH.mm.ss") + ".csv";
                    string localLayerResultFileName = GenRunSummaryFile(layerResList, layerResCsvFileName, tempFolder);
                    string netLayerResultFileName = objCfg.OutputTableImageAttachmentPath + "\\" + layerResCsvFileName;
                    attachToUploadList.Add(localLayerResultFileName); //File Upload
                    summaryStep._DataTable.Add(new DataTable()
                    { Name = Program.Obj_config.Target_Table_LAYERSUMMARY, Status = "Done", Description = "EPI_WAFER_INFORMATION", Value = netLayerResultFileName });

                    var layResDic = layerResList.GroupBy(x => x.nIndex).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
                    TestStep layerResStep;
                    foreach (var kvp in layResDic)
                    {
                        layerResStep = new TestStep();
                        layerResStep.Name = "EPIRUN_LAYERRESULTS_" + kvp.Key.ToString();
                        layerResStep.Status = "Done";
                        layerResStep.startDateTime = runStartTime.ToString("s");
                        layerResStep.endDateTime = runEndTime.ToString("s");
                        var layResGrpList = kvp.Value;
                        foreach (var layItem in layResGrpList)
                        {
                            var curParamName = (string.IsNullOrEmpty(layItem.Technique) == false ? layItem.Technique + ":" : "")
                                + layItem.Type;
                            var sameParamItem = layerResStep._DataNumerics.Where(r => r.Name.Contains(curParamName))
                                .OrderByDescending(r => r.Name).FirstOrDefault();
                            this.CheckDuplicateParam(ref sameParamItem, ref curParamName);

                            layerResStep._DataNumerics.Add(new TestStep.DataNumeric()
                            {
                                Name = curParamName,
                                Units = layItem.Units,
                                Value = layItem.Value.ToString(),
                                CompOperator = "LOG",
                                Status = "Done",
                                Comments = layItem.Comment
                            });
                        }
                        xmlResultRoot.TestStep.Add(layerResStep);
                    }

                }
                //Wafer Info
                var waferList = GenWaferList(run.Run_ID);
                if (waferList.Count > 0)
                {
                    UnitGenealogy ug = new UnitGenealogy();
                    foreach (var item in waferList)
                    {
                        if (ug._Item.Where(r=> r.SerialNumber == item.Wafer_ID).Count() == 0)
                        {
                            ug._Item.Add(new UnitGenealogy.Item()
                            {
                                PartNumber = string.IsNullOrEmpty(item.Part_Number) ? "NA" : item.Part_Number,
                                SerialNumber = item.Wafer_ID,
                                PartType = "WAFER",
                                Rev = "NA"
                            });
                        }
                    }
                    xmlResultRoot.UnitGenealogy = ug;
                    xmlResultRoot.TestStep.AddRange(BuildPocketPosTestSteps(waferList, runStartTime, runEndTime));
                    string wafersCsvFileName = "EPIRUN_" + reactorName + run.Descriptor
                        + "_Wafers_" + runStartTime.ToString("yyyy-MM-ddTHH.mm.ss") + ".csv";
                    string localWafersFileName = GenRunSummaryFile(waferList, wafersCsvFileName, tempFolder);
                    string netWafersFileName = objCfg.OutputTableImageAttachmentPath + "\\" + wafersCsvFileName;
                    attachToUploadList.Add(localWafersFileName); //File Upload
                    summaryStep._DataTable.Add(new DataTable()
                    { Name = Program.Obj_config.Target_Table_WAFERSUMMARY, Status = "Done", Description = "EPI_WAFER_INFORMATION", Value = netWafersFileName });
                }

                //Set summary
                xmlResultRoot.TestStep.Add(summaryStep);

                //Build Xml Info
                string xmlStr = string.Empty;
                if (xmlResultRoot.GetXML(ref xmlStr, ref Program.Error_Message) == false)
                {
                    return false;
                }
                else
                {
                    var operation = objCfg.Operation_Prefix + run.Category;
                    if (string.IsNullOrEmpty(run.Category))
                        operation = "NA";

                    string xmlResultFileName = tempFolder
                        + string.Format(@"\Site={0},ProductFamily={1},Operation={2},PartNumber={3},SerialNumber={4},TestDate={5}.xml",
                        objCfg.Site, objCfg.ProductFamily, operation, partNumber,
                        xmlResultRoot.Header.Serialnumber, runStartTime.ToString("yyyy-MM-ddTHH.mm.ss"));
                    if (File.Exists(xmlResultFileName))
                        File.Delete(xmlResultFileName);
                    File.WriteAllText(xmlResultFileName, xmlStr);
                    xmlToUploadList.Add(xmlResultFileName);
                }
            }
            catch (Exception ex)
            {
                Program.Error_Message = "Run Id: " + run.Run_ID + " - \r\r Error Message:" + ExceptionHelper.GetAllFootprints(ex);
                return false;
            }
            return true;
        }
        private Header GenerateHeaderInfo(Run_View run, string startDateTime, string endDateTime, string partNumber)
        {
            Header header = new Header();
            try
            {
                var reactorName = run.Reactor.Reactor_Name;
                header.Devicetype = "RUN";
                header.Site = Program.Obj_config.Site;
                header.Operation = Program.Obj_config.Operation_Prefix + XmlValueCleanup(run.Category);
                header.OperatorId = "NA";
                header.Teststation = reactorName;
                header.Comments = XmlValueCleanup(run.Purpose);
                header.Lotnumber = "NA";
                header.Serialnumber = reactorName + run.Descriptor;
                header.Waferid = "NA";
                header.Partnumber = partNumber;
                header.Starttime = startDateTime;
                header.Endtime = endDateTime;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return header;
        }

        #region Build Object Info
        public List<Run_View> GetAllRunList(DateTime fromDate, DateTime toDate)
        {
            List<Run> runList = new List<Run>();
            Program.ManualRunIds = ConfigurationManager.AppSettings["ManualRunIds"];
            EpitaxySQLEntities epiDb = new EpitaxySQLEntities();

            //Manual Re Process List
            if (string.IsNullOrEmpty(Program.ManualRunIds) == false)
            {
                var manualRunIdList = Program.ManualRunIds.Split(',').ToList();
                runList = (from run in epiDb.Runs
                           where manualRunIdList.Contains(run.Run_ID.ToString())
                           select run).ToList();
            }
            else
            {
                runList = (from run in epiDb.Runs
                           where
                           run.Start_Time != null && run.End_Time != null
                           //b.Trigger a TDS upload if (RunDate + EndTime fields + 2 days) is greater than the last time TDS was updated 
                           && (DbFunctions.AddDays(DbFunctions.CreateDateTime(run.Run_Date.Value.Year, run.Run_Date.Value.Month,
                           run.Run_Date.Value.Day, run.End_Time.Value.Hour, run.End_Time.Value.Minute,
                           run.End_Time.Value.Second), 2) > fromDate
                           && DbFunctions.AddDays(DbFunctions.CreateDateTime(run.Run_Date.Value.Year, run.Run_Date.Value.Month,
                           run.Run_Date.Value.Day, run.End_Time.Value.Hour, run.End_Time.Value.Minute,
                           run.End_Time.Value.Second), 2) <= toDate)
                           //a. Trigger a TDS upload if TDS_UploadDate is greater than the last time TDS was updated 
                           || (run.TDS_UploadDate != null && run.TDS_UploadDate > fromDate && run.TDS_UploadDate <= toDate)
                           //where run.Run_ID == 850008365//785347274//785347274
                           select run).Take(int.Parse(Program.Obj_config.Maximum_row_each_run)).OrderBy(r => r.Run_Date).ToList();
            }


            if (runList.Count() > 0)
            {
                var config = new MapperConfiguration(cfg => cfg.CreateMap<Run, Run_View>());
                var mapper = new Mapper(config);
                var runInfoList = mapper.Map<List<Run>, List<Run_View>>(runList);
                return runInfoList.OrderBy(r=> r.Run_Date).ToList();
            }
            return null;
        }
        private string GenRunSummaryFile(object obj, string fileName, string tempFolder)
        {
            string fileFullPath = tempFolder + "\\" + fileName;
            try
            {
                var colNames = string.Empty;

                StringBuilder colValueSB = new StringBuilder();
                if (IsList(obj))
                {
                    List<object> listO = ((IEnumerable<object>)obj).ToList();
                    colNames = listO.FirstOrDefault().GetType().GetProperties()
                        .Where(p => !p.GetGetMethod().IsVirtual).Select(r => r.Name).Aggregate((cur, next) => cur + "," + next);
                    foreach (var objItem in listO)
                    {
                        colValueSB.AppendLine(GetObjValuesAsStr(",", objItem));
                    }
                }
                else
                {
                    colNames = obj.GetType().GetProperties()
                        .Where(p => !p.GetGetMethod().IsVirtual).Select(r => r.Name).Aggregate((cur, next) => cur + "," + next);
                    colValueSB.Append(GetObjValuesAsStr(",", obj));
                }

                if (File.Exists(fileFullPath))
                    File.Delete(fileFullPath);
                File.WriteAllText(fileFullPath, colNames + Environment.NewLine + colValueSB.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return fileFullPath;
        }
        private string GetObjValuesAsStr(string delimiter, object obj)
        {
            StringBuilder colValueSB = new StringBuilder();
            var tempColValue = string.Empty;
            foreach (var item in obj.GetType().GetProperties().Where(p => !p.GetGetMethod().IsVirtual))
            {
                try
                {
                    if (item.PropertyType == typeof(Byte[]))
                    {
                        tempColValue = string.Empty;
                    }
                    else
                    {
                        tempColValue = item.GetValue(obj, null) != null
                            ? item.GetValue(obj, null).ToString() : null;
                    }
                }
                catch (Exception)
                {
                    tempColValue = string.Empty;
                }
                if (tempColValue != null && tempColValue.Length > 100)
                    colValueSB.Append(TextCleanUp(tempColValue, 100) + delimiter);
                else
                    colValueSB.Append(TextCleanUp(tempColValue) + delimiter);
            }
            colValueSB = colValueSB.Replace(Environment.NewLine, " ");
            if (colValueSB.Length > 0) colValueSB.Length--; //Remove Extra Comma
            return colValueSB.ToString();
        }
        private TestStep GenTestStepInfo(object obj, string name)
        {
            TestStep testStep = new TestStep();
            testStep.Name = name;
            try
            {
                foreach (var pi in obj.GetType().GetProperties().Where(p => !p.GetGetMethod().IsVirtual))
                {
                    if (pi.CanRead == false) continue;
                    if (pi.PropertyType == typeof(Byte[])) continue;
                    var tempValue = pi.GetValue(obj, null);

                    if (pi.PropertyType == typeof(int) || pi.PropertyType == typeof(double)
                    || pi.PropertyType == typeof(Nullable<int>) || pi.PropertyType == typeof(Nullable<double>))
                    {
                        if (tempValue == null)
                            tempValue = 0;
                        testStep._DataNumerics.Add(new TestStep.DataNumeric()
                        {
                            Name = pi.Name,
                            CompOperator = "LOG",
                            Value = tempValue.ToString(),
                            Status = "Done"
                        });
                    }
                    else
                    {
                        if (tempValue == null) tempValue = "NA";
                        //var tempValueStr = XmlValueCleanup(tempValue.ToString());
                        var tempValueStr = tempValue.ToString().Replace(",", "`");
                        if (tempValueStr.Length > 100)
                            tempValueStr = XmlValueCleanup(TextCleanUp(tempValueStr, 100));
                        else
                            tempValueStr = XmlValueCleanup(TextCleanUp(tempValueStr));
                        testStep._DataString.Add(new TestStep.DataString()
                        {
                            Name = pi.Name,
                            CompOperator = "LOG",
                            Value = tempValueStr,
                            Status = "Done"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return testStep;
        }
        private List<LayerSetting_View> GenLayerSettingList(int runId)
        {
            EpitaxySQLEntities db = new EpitaxySQLEntities();
            try
            {
                var result = (from run in db.Runs
                              join lay in db.Layers on run.Run_ID equals lay.Run_ID
                              join rgs in db.Reagent_States on lay.Layer_ID equals rgs.Layer_ID
                              into rgsl from rgs in rgsl.DefaultIfEmpty()
                              join ret in db.Reagents on rgs.Reagent_ID equals ret.Reagent_ID
                              into retl from ret in retl.DefaultIfEmpty()
                              where run.Run_ID == runId
                              select new LayerSetting_View
                              {
                                  Run_ID = runId,
                                  Layer_ID = lay.Layer_ID,
                                  nIndex = lay.nIndex,
                                  Growth_Time = lay.Growth_Time,
                                  Growth_Temperature = lay.Growth_Temperature,
                                  Material = lay.Material,
                                  Reagent_Name = ret.Reagent_Name,
                                  Source = rgs.Source,
                                  Dilution = rgs.Dilution,
                                  Pusher = rgs.Pusher,
                                  Inject = rgs.Inject,
                                  Pressure = rgs.Pressure,
                                  EPISON_Control = rgs.EPISON_Control
                              }).OrderByDescending(r => r.nIndex).ToList<LayerSetting_View>();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private List<TestStep> BuildLayerSettingTestSteps(List<LayerSetting_View> layerSettingList, DateTime runStartTime, DateTime runEndTime)
        {
            List<TestStep> list = new List<TestStep>();
            try
            {
                var laySetDic = layerSettingList.GroupBy(x => x.nIndex).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
                TestStep layerSetStep;
                foreach (var kvp in laySetDic)
                {
                    layerSetStep = new TestStep();
                    layerSetStep.Name = "EPIRUN_LAYERSETTINGS_" + kvp.Key.ToString();
                    layerSetStep.Status = "Done";
                    layerSetStep.startDateTime = runStartTime.ToString("s");
                    layerSetStep.endDateTime = runEndTime.ToString("s");

                    if (kvp.Value.FirstOrDefault().Growth_Temperature.HasValue)
                        layerSetStep.Temperature = kvp.Value.FirstOrDefault().Growth_Temperature.Value.ToString();

                    layerSetStep.MiscInfo = kvp.Value.FirstOrDefault().Material;

                    //layerSetStep._DataString.Add(new TestStep.DataString() { Name = "Material", Value = XmlValueCleanup(kvp.Value.FirstOrDefault().Material) });
                    layerSetStep._DataNumerics.Add(new TestStep.DataNumeric()
                    {
                        Name = "Growth_Time",
                        Units = "Sec",
                        Value = kvp.Value.FirstOrDefault().Growth_Time.HasValue
                        ? kvp.Value.FirstOrDefault().Growth_Time.Value.ToString() : "0",
                        CompOperator = "LOG",
                        Status = "Done"
                    });
                    //layerSetStep._DataNumerics.Add(new TestStep.DataNumeric()
                    //{
                    //    Name = "Growth_Temperature",
                    //    Value = kvp.Value.FirstOrDefault().Growth_Temperature.Value.ToString()
                    //});
                    foreach (var item in kvp.Value)
                    {
                        foreach (var pi in item.GetType().GetProperties())
                        {
                            if (pi.Name == "Run_ID" || pi.Name == "Layer_ID"
                                || pi.Name == "nIndex" || pi.Name == "Growth_Time"
                                || pi.Name == "Growth_Temperature" || pi.Name == "Material"
                                || pi.Name == "Reagent_Name") continue;

                            var output = pi.GetValue(item, null);
                            var val = string.Empty;
                            if (output == null)
                                continue;
                            val = XmlValueCleanup(output.ToString());
                            val = TextCleanUp(val);
                            if (pi.PropertyType == typeof(int?) || pi.PropertyType == typeof(double?))
                            {
                                layerSetStep._DataNumerics.Add(new TestStep.DataNumeric()
                                {
                                    Name = item.Reagent_Name + "_" + pi.Name,
                                    Units = "NA",
                                    Value = val,
                                    CompOperator = "LOG",
                                    Status = "Done"
                                });
                            }
                            else
                            {
                                layerSetStep._DataString.Add(new TestStep.DataString()
                                {
                                    Name = item.Reagent_Name + "_" + pi.Name,
                                    Units = "NA",
                                    Value = val,
                                    CompOperator = "LOG",
                                    Status = "Done"
                                });
                            }
                        }
                    }
                    list.Add(layerSetStep);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return list;
        }
        private List<LayerResult_View> GenLayerResultList(int runId)
        {
            EpitaxySQLEntities db = new EpitaxySQLEntities();
            try
            {
                var result = (from run in db.Runs
                              join lay in db.Layers on run.Run_ID equals lay.Run_ID
                              join lys in db.Layer_Results on lay.Layer_ID equals lys.Layer_ID
                              where run.Run_ID == runId
                              select new LayerResult_View
                              {
                                  Run_ID = runId,
                                  Layer_ID = lay.Layer_ID,
                                  nIndex = lay.nIndex.Value,
                                  Material = lay.Material,
                                  Value = lys.Value,
                                  Units = lys.Units,
                                  Type = lys.Type,
                                  Technique = lys.Technique,
                                  Exclude = lys.Exclude,
                                  Comment = lys.Comment
                              }).ToList<LayerResult_View>();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private List<Wafer_View> GenWaferList(int runId)
        {
            EpitaxySQLEntities db = new EpitaxySQLEntities();
            try
            {
                var result = (from run in db.Runs
                              join poc in db.Pockets on run.Run_ID equals poc.Run_ID
                              //into pocl from poc in pocl.DefaultIfEmpty()
                              join xray in db.QW_Xrays on poc.Pocket_ID equals xray.Pocket_ID
                              into xrayl
                              from xray in xrayl.DefaultIfEmpty()
                              join wmap in db.Wafer_Maps on poc.Pocket_ID equals wmap.Pocket_ID
                              into wmapl
                              from wmap in wmapl.DefaultIfEmpty()
                              where run.Run_ID == runId
                              select new Wafer_View
                              {
                                  Run_ID = run.Run_ID,
                                  Pocket_ID = poc.Pocket_ID,
                                  Pocket_Pos = poc.Pocket_Pos.Value,
                                  PROMIS_Lot_Name = poc.PROMIS_Lot_Name,
                                  PROMIS_Component = poc.PROMIS_Component,
                                  Wafer_ID = poc.Wafer_ID,
                                  Wafer_Prep = poc.Wafer_Prep,
                                  Surfscan_Density = poc.Surfscan_Density,
                                  Surfscan_Count = poc.Surfscan_Count,
                                  Release_Batch = poc.Release_Batch,
                                  Substrate_Part_ID = poc.Substrate_Part_ID,
                                  Part_Number = poc.PROMIS_Part.Part_Number,
                                  QWQB = xray.QWQB,
                                  ZOM = xray.ZOM,
                                  Center_Val = wmap.Center_Val,
                                  Average_Val = wmap.Average_Val,
                                  SD = wmap.SD,
                                  Edge_Exclusion = wmap.Edge_Exclusion,
                                  Spec_Band = wmap.Spec_Band,
                                  Area_in_Spec = wmap.Area_in_Spec,
                                  Type = wmap.Type
                              }).OrderByDescending(r => r.Pocket_Pos).ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private List<TestStep> BuildPocketPosTestSteps(List<Wafer_View> WaferList, DateTime runStartTime, DateTime runEndTime)
        {
            List<TestStep> list = new List<TestStep>();
            List<string> typeParamList = ConfigurationManager.AppSettings["WaferTypeParameters"].Split(',').ToList();
            try
            {
                TestStep waferStep;
                bool isDuplicate;
                foreach (var item in WaferList)
                {
                    isDuplicate = false;
                    var waferStepName = "EPIRUN_POCKETPOS_" + item.Pocket_Pos;
                    waferStep = list.Where(r => r.Name == waferStepName).FirstOrDefault();
                    if (waferStep == null)
                    {
                        waferStep = new TestStep();
                        waferStep.Name = waferStepName;
                        waferStep.Status = "Done";
                        waferStep.startDateTime = runStartTime.ToString("s");
                        waferStep.endDateTime = runEndTime.ToString("s");
                    }
                    else isDuplicate = true;

                    foreach (var pi in item.GetType().GetProperties())
                    {
                        if (pi.Name == "Run_ID" || pi.Name == "Pocket_ID"
                            || pi.Name == "Pocket_Pos" || pi.Name == "Type") continue;

                        var output = pi.GetValue(item, null);
                        var val = string.Empty;
                        if (output == null) continue;
                        else
                        {
                            val = XmlValueCleanup(output.ToString());
                            val = TextCleanUp(val);
                            val = val.Replace(";", "`");
                        }

                        var curParamName = pi.Name;
                        if (typeParamList.Contains(pi.Name))
                        {
                            curParamName = item.Type + ":" + pi.Name;

                            var sameParamItem = waferStep._DataNumerics.Where(r => r.Name.Contains(curParamName))
                                .OrderByDescending(r => r.Name).FirstOrDefault();
                            if (sameParamItem == null)
                            {
                                var dsSameParamItem = waferStep._DataString.Where(r => r.Name.Contains(curParamName))
                                    .OrderByDescending(r => r.Name).FirstOrDefault();
                                this.CheckDuplicateParamForDs(ref dsSameParamItem, ref curParamName);
                            }
                            else
                            {
                                this.CheckDuplicateParam(ref sameParamItem, ref curParamName);
                            }
                        }
                        
                        if (pi.PropertyType == typeof(int?) || pi.PropertyType == typeof(double?))
                        {
                            if (waferStep._DataNumerics.Where(r=> r.Name == curParamName).Count() == 0)
                            {
                                waferStep._DataNumerics.Add(new TestStep.DataNumeric()
                                {
                                    Name = curParamName,
                                    Units = "NA",
                                    Value = val,
                                    CompOperator = "LOG",
                                    Status = "Done"
                                });
                            }
                        }
                        else
                        {
                            if (waferStep._DataString.Where(r => r.Name == curParamName).Count() == 0)
                            {
                                waferStep._DataString.Add(new TestStep.DataString()
                                {
                                    Name = curParamName,
                                    Units = "NA",
                                    Value = val,
                                    CompOperator = "LOG",
                                    Status = "Done"
                                });
                            }                                
                        }
                    }
                    if (isDuplicate == false) list.Add(waferStep);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return list;
        }
        #endregion

        #region Utilities
        private DateTime TestTimeConversion(DateTime runDate, DateTime runStartTime, DateTime runEndTime, bool isStartDT)
        {
            DateTime dt = new DateTime();
            string tempDtStr = string.Empty;

            if (isStartDT) //Start Time
            {
                tempDtStr = runDate.ToString("MM/yyyy/dd") + " " + runStartTime.ToString("HH:mm:ss");
            }
            else //End Time
            {
                if (runStartTime > runEndTime)
                {
                    tempDtStr = runDate.AddDays(1).ToString("MM/yyyy/dd") + " " + runEndTime.ToString("HH:mm:ss");
                }
                else
                {
                    tempDtStr = runDate.ToString("MM/yyyy/dd") + " " + runEndTime.ToString("HH:mm:ss");
                }
            }
            dt = DateTime.Parse(tempDtStr);
            return dt;
        }
        public string TextCleanUp(string fromText, int limitSize = 0)
        {
            if (string.IsNullOrEmpty(fromText)) return string.Empty;
            string toText = fromText;
            toText = toText.Replace(",", ";");
            toText = toText.Replace("  ", " ");
            toText = toText.Replace("  ", " ");
            toText = toText.Replace("  ", " ");
            toText = toText.Replace("  ", " ");
            toText = toText.Replace(Environment.NewLine, " ");

            if (limitSize > 0 && toText.Length > limitSize)
            {
                toText = toText.Substring(0, limitSize - 3) + "...";
            }
            return toText;
        }
        public string XmlValueCleanup(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            if (input.IndexOf("&") > -1)
                input = input.Replace("&", "&amp;");
            if (input.IndexOf("<") > -1)
                input = input.Replace("<", "&lt;");
            if (input.IndexOf(">") > -1)
                input = input.Replace(">", "&gt;");
            if (input.IndexOf("\"") > -1)
                input = input.Replace("\"", "&quot;");
            if (input.IndexOf("'") > -1)
                input = input.Replace("'", "&apos;");
            input = input.Replace("  ", " ");
            input = input.Replace("  ", " ");
            input = input.Replace("  ", " ");
            input = input.Replace("  ", " ");
            input = input.Replace("  ", " ");
            return input;
        }
        public bool FileUpload(List<string> fileUploadList, string outputFilePath)
        {
            try
            {
                //Copy
                foreach (var uploadFile in fileUploadList)
                {
                    var toFilePath = outputFilePath + Path.DirectorySeparatorChar + Path.GetFileName(uploadFile);
                    //LogHelper.WriteLine(string.Format("Uploading file: {0}", Path.GetFileName(uploadFile)));
                    Console.WriteLine(string.Format("Uploading file: {0}", Path.GetFileName(uploadFile)));
                    if (File.Exists(toFilePath))
                        File.Delete(toFilePath);
                    File.Copy(uploadFile, toFilePath);
                }

                //Delete
                foreach (var deleteFile in fileUploadList)
                {
                    //LogHelper.WriteLine(string.Format("Delete after upload file: {0}", Path.GetFileName(deleteFile)));
                    Console.WriteLine(string.Format("Delete after upload file: {0}", Path.GetFileName(deleteFile)));
                    if (File.Exists(deleteFile))
                        File.Delete(deleteFile);
                }
            }
            catch (Exception ex)
            {
                Program.Error_Message = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                return false;
            }
            return true;
        }
        private static bool IsList(object o)
        {
            var oType = o.GetType();
            return (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
        }

        private void CheckDuplicateParam(ref DataNumeric sameParamItem, ref string curParamName)
        {
            if (sameParamItem != null && string.IsNullOrEmpty(sameParamItem.Name) == false)
            {
                var suffix = sameParamItem.Name.Substring(sameParamItem.Name.Length - 1, 1);
                if (int.TryParse(suffix, out int seq))
                {
                    seq = seq + 1;
                    curParamName = curParamName + "_" + seq.ToString();
                }
                else
                {
                    sameParamItem.Name = sameParamItem.Name + "_1";
                    curParamName = curParamName + "_2";
                }
            }
        }

        private void CheckDuplicateParamForDs(ref DataString sameParamItem, ref string curParamName)
        {
            if (sameParamItem != null && string.IsNullOrEmpty(sameParamItem.Name) == false)
            {
                var suffix = sameParamItem.Name.Substring(sameParamItem.Name.Length - 1, 1);
                if (int.TryParse(suffix, out int seq))
                {
                    seq = seq + 1;
                    curParamName = curParamName + "_" + seq.ToString();
                }
                else
                {
                    sameParamItem.Name = sameParamItem.Name + "_1";
                    curParamName = curParamName + "_2";
                }
            }
        }
        #endregion
    }
}
