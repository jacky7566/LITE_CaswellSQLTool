using CaswellSQLConversion.EPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CaswellSQLConversion.XML
{
    public class cls_tdsmodel
    {
        public string startDateTime { get; set; }
        public string endDateTime { get; set; }
        public string Result { get; set; }
        public Header Header;
        public HeaderMisc HeaderMisc;
        public UnitGenealogy UnitGenealogy;
        public List<TestStep> TestStep = new List<TestStep>();
        public bool GetXML(ref string xml, ref string errorMessage)
        {
            StringBuilder textXmlSB = new StringBuilder();
            EPIDataConversionHelper conversionHelper = new EPIDataConversionHelper();
            try
            {
                foreach (var pi in this.GetType().GetProperties())
                {
                    try
                    {
                        var tempValueStr = pi.GetValue(this, null).ToString();
                        if (string.IsNullOrEmpty(tempValueStr)) continue;
                        tempValueStr = conversionHelper.TextCleanUp(tempValueStr);
                        tempValueStr = conversionHelper.XmlValueCleanup(tempValueStr);
                        textXmlSB.AppendFormat("{0}=\"{1}\" ", pi.Name, pi.GetValue(this, null));
                    }
                    catch (Exception)
                    {                       
                    }
                }
                string resultXml = string.Format("<Result {0} >", textXmlSB.ToString());

                var headerXml = string.Empty;
                if (Header != null && Header.GetXML(ref headerXml, ref errorMessage) == false)
                    return false;
                var headerMiscXml = string.Empty;
                if (HeaderMisc != null && HeaderMisc.GetXML(ref headerMiscXml, ref errorMessage) == false)
                    return false;
                var uniGenealogyXml = string.Empty;
                if (UnitGenealogy != null && UnitGenealogy.GetXML(ref uniGenealogyXml, ref errorMessage) == false)
                    return false;
                StringBuilder fullXmlSB = new StringBuilder();
                fullXmlSB.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
                fullXmlSB.AppendLine(@"<Results xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">");
                fullXmlSB.AppendLine("\t" + resultXml);
                fullXmlSB.AppendLine("\t\t" + headerXml);
                fullXmlSB.AppendLine("\t\t" + headerMiscXml);
                fullXmlSB.AppendLine("\t\t" + uniGenealogyXml);

                if (TestStep != null)
                {
                    foreach (var obj in TestStep)
                    {
                        var tempXml = string.Empty;
                        if (obj.GetXML(ref tempXml, ref errorMessage))
                        {
                            fullXmlSB.AppendLine(tempXml);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                fullXmlSB.AppendLine("\t</Result>");
                fullXmlSB.AppendLine("</Results>");
                xml = fullXmlSB.ToString();
                try
                {
                    var xDoc = new XmlDocument();
                    xDoc.LoadXml(xml);
                }
                catch (Exception ex)
                {
                    errorMessage = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
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
    }
    public class Header
    {
        public string Serialnumber {get;set;}
        public string Customerserialnumber {get;set;}
        public string Waferid {get;set;}
        public string Batchnumber {get;set;}
        public string Lotnumber {get;set;}
        public string Workordernumber {get;set;}
        public string Shipmentid {get;set;}
        public string Partnumber {get;set;}
        public string Site {get;set;}
        public string Gate {get;set;}
        public string Productrevision {get;set;}
        public DateTime Productreleasedate { get; set; }
        public string Productfirmware {get;set;}
        public string Operation {get;set;}
        public string Parentprocessstep {get;set;}
        public string OperatorId {get;set;}
        public string Purpose {get;set;}
        public string Specificationrevision {get;set;}
        public string Softwareaplicationname {get;set;}
        public string Softwareaplicationversion {get;set;}
        public string Softwarereleasestate {get;set;}
        public string Softwarerevision {get;set;}
        public string Validated {get;set;}
        public string Datalock {get;set;}
        public string Testplanid {get;set;}
        public string Resultcode {get;set;}
        public string Failuremode {get;set;}
        public int Failurecount { get; set; }
        public int Errorcount { get; set; }
        public string Reworkassigment {get;set;}
        public string Testspecificationsreleasestate {get;set;}
        public int Testspecificationsrevid { get; set; }
        public string Testspecsdatalock {get;set;}
        public int Stoponfirstfail { get; set; }
        public int Stoponfirsterror { get; set; }
        public string Teststation {get;set;}
        public int Devicexlocation { get; set; }
        public int Deviceylocation { get; set; }
        public string Starttime {get;set;}
        public string Endtime {get;set;}
        public double Cycletime_M { get; set; }
        public double Automatedtime_M { get; set; }
        public double Manualactivetime_M { get; set; }
        public double Manualidletime_M { get; set; }
        public double Faultactivetime_M { get; set; }
        public double Faultidletime_M { get; set; }
        public int Measurementcount { get; set; }
        public int Testequipmentcount { get; set; }
        public int Testconditionid { get; set; }
        public string Testsequencefile {get;set;}
        public string Result {get;set;}
        public string Filename {get;set;}
        public string mesoperationname {get;set;}
        public string mesoperationdesc {get;set;}
        public string mesParameterSetName {get;set;}
        public string mesWorkflowName {get;set;}
        public string mesWorkflowRevision {get;set;}
        public string mesWorkflowStepName {get;set;}
        public string Rackid {get;set;}
        public string Drawerid {get;set;}
        //Publicstringy Pass {get;set;}
        public string Recipename {get;set;}
        public string Recipeversion {get;set;}
        public string Hwrevision {get;set;}
        public string Paretocodename {get;set;}
        public string Processname {get;set;}
        public string Devicetype {get;set;}
        public string Misc {get;set;}
        public string Comments {get;set;}
        public string Node {get;set;}
        public bool GetXML(ref string xml, ref string errorMessage)
        {
            try
            {
                StringBuilder textXmlSB = new StringBuilder();
                foreach (var pi in this.GetType().GetProperties())
                {
                    if (pi.CanRead)
                    {
                        try
                        {
                            var value = pi.GetValue(this, null) != null ? pi.GetValue(this, null).ToString() : "";
                            if (string.IsNullOrEmpty(value) || value == "0") continue;
                            var name = pi.Name;
                            if (pi.Name.Trim().ToUpper() == "OPERATORID")
                                name = "Operator";
                            if (string.IsNullOrEmpty(value) == false)
                            {
                                textXmlSB.AppendFormat("{0}=\"{1}\" ", name, value);
                            }
                        }
                        catch (Exception)
                        {
                            
                        }
                    }
                }
                xml = string.Format("<Header {0} />", textXmlSB.ToString());
            }
            catch (Exception ex)
            {
                errorMessage = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                return false;
            }
            return true;
        }
    }
    public class HeaderMisc
    {
        public List<Item> _Item = new List<Item>();

        public bool GetXML(ref string xml, ref string errorMessage)
        {
            try
            {
                StringBuilder textXmlSB = new StringBuilder("<HeaderMisc>");
                foreach (var obj in _Item)
                {
                    var tempXml = string.Empty;
                    if (obj.GetXML(ref tempXml, ref errorMessage))
                    {
                        textXmlSB.Append(tempXml);
                    }
                    else
                    {
                        return false;
                    }
                }
                textXmlSB.Append("\t\t</HeaderMisc>");
                xml = textXmlSB.ToString();
            }
            catch (Exception ex)
            {
                errorMessage = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                return false;
            }
            return true;
        }

        public class Item
        {
            public string Description { get; set; }
            public string Value { get; set; }
            public bool GetXML(ref string xml, ref string errorMessage)
            {
                StringBuilder textXmlSB = new StringBuilder();
                try
                {
                    textXmlSB.AppendFormat("<Item Description=\"{0}\">{1}</Item>", Description, Value);
                    xml = textXmlSB.ToString();
                }
                catch (Exception ex)
                {
                    errorMessage = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                    return false;
                }
                return true;
            }
        }
    }    
    public class UnitGenealogy
    {
        public List<Item> _Item = new List<Item>();
        public bool GetXML(ref string xml, ref string errorMessage)
        {
            try
            {
                StringBuilder textXmlSB = new StringBuilder();
                textXmlSB.AppendLine("<UnitGenealogy>");
                foreach (var obj in _Item)
                {
                    var tempXml = string.Empty;
                    if (obj.GetXML(ref tempXml, ref errorMessage))
                    {
                        textXmlSB.AppendLine(tempXml);
                    }
                    else
                    {
                        return false;
                    }
                }
                textXmlSB.AppendLine("\t\t</UnitGenealogy>");
                xml = textXmlSB.ToString();
            }
            catch (Exception ex)
            {
                errorMessage = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                return false;
            }
            return true;
        }
        public class Item
        {
            public string PartType { get; set; }
            public string PartNumber { get; set; }
            public string SerialNumber { get; set; }
            public string Rev { get; set; }
            public bool GetXML(ref string xml, ref string errorMessage)
            {
                StringBuilder textXmlSB = new StringBuilder();
                try
                {
                    foreach (var pi in this.GetType().GetProperties())
                    {
                        try
                        {
                            textXmlSB.AppendFormat("{0}=\"{1}\" ", pi.Name, pi.GetValue(this, null));
                        }
                        catch (Exception)
                        {
                        }
                    }
                    xml = string.Format("\t\t\t<Item {0}/>", textXmlSB.ToString());
                }
                catch (Exception ex)
                {
                    errorMessage = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                    return false;
                }
                return true;
            }
        }
    }
    public class TestStep
    {
        public string Name { get; set; }
        public string startDateTime { get; set; }
        public string endDateTime { get; set; }
        public string Status { get; set; }
        public string Temperature { get; set; }
        public string TestType { get; set; }
        public string SampleRate { get; set; }
        public string MiscInfo { get; set; }

        public List<DataNumeric> _DataNumerics = new List<DataNumeric>();
        public List<DataString> _DataString = new List<DataString>();
        public List<DataImage> _DataImage = new List<DataImage>();
        public List<DataAttachment> _DataAttachment = new List<DataAttachment>();
        public List<DataTable> _DataTable = new List<DataTable>();
        public List<DataSerial> _DataSerial = new List<DataSerial>();
        public List<DataArray> _DataArray = new List<DataArray>();
        public bool GetXML(ref string xml, ref string errorMessage)
        {
            StringBuilder textXmlSB = new StringBuilder();
            try
            {
                EPIDataConversionHelper conversionHelper = new EPIDataConversionHelper();
                foreach (var pi in this.GetType().GetProperties())
                {
                    try
                    {
                        var tempValueStr = pi.GetValue(this, null) != null ? pi.GetValue(this, null).ToString() : "";
                        tempValueStr = conversionHelper.TextCleanUp(tempValueStr);
                        tempValueStr = conversionHelper.XmlValueCleanup(tempValueStr);
                        textXmlSB.AppendFormat("{0}=\"{1}\" ", pi.Name, tempValueStr);
                    }
                    catch (Exception)
                    {
                    }
                }
                string resultXml = string.Format("\t\t<TestStep {0} >", textXmlSB.ToString());
                StringBuilder fullXmlSB = new StringBuilder();
                fullXmlSB.AppendLine(resultXml);
                var tempXml = string.Empty;

                foreach (var obj in _DataNumerics)
                {
                    tempXml = string.Empty;
                    if (obj.GetXML(ref tempXml, ref errorMessage))
                    {
                        fullXmlSB.AppendLine(tempXml);
                    }
                    else
                    {
                        return false;
                    }
                }

                foreach (var obj in _DataString)
                {
                    tempXml = string.Empty;
                    if (obj.GetXML(ref tempXml, ref errorMessage))
                    {
                        fullXmlSB.AppendLine(tempXml);
                    }
                    else
                    {
                        return false;
                    }
                }

                foreach (var obj in _DataImage)
                {
                    tempXml = string.Empty;
                    if (obj.GetXML(ref tempXml, ref errorMessage))
                    {
                        fullXmlSB.AppendLine(tempXml);
                    }
                    else
                    {
                        return false;
                    }
                }

                foreach (var obj in _DataAttachment)
                {
                    tempXml = string.Empty;
                    if (obj.GetXML(ref tempXml, ref errorMessage))
                    {
                        fullXmlSB.AppendLine(tempXml);
                    }
                    else
                    {
                        return false;
                    }
                }

                foreach (var obj in _DataTable)
                {
                    tempXml = string.Empty;
                    if (obj.GetXML(ref tempXml, ref errorMessage))
                    {
                        fullXmlSB.AppendLine(tempXml);
                    }
                    else
                    {
                        return false;
                    }
                }

                foreach (var obj in _DataSerial)
                {
                    tempXml = string.Empty;
                    if (obj.GetXML(ref tempXml, ref errorMessage))
                    {
                        fullXmlSB.AppendLine(tempXml);
                    }
                    else
                    {
                        return false;
                    }
                }

                foreach (var obj in _DataArray)
                {
                    tempXml = string.Empty;
                    if (obj.GetXML(ref tempXml, ref errorMessage))
                    {
                        fullXmlSB.AppendLine(tempXml);
                    }
                    else
                    {
                        return false;
                    }
                }

                fullXmlSB.AppendLine("\t\t</TestStep>");
                xml = fullXmlSB.ToString();
            }
            catch (Exception ex)
            {
                errorMessage = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                return false;
            }
            return true;
        }
        public class DataNumeric
        {
            public string Name { get; set; }
            public string Units { get; set; }
            public string CompOperator { get; set; }
            public string Status { get; set; }
            public string SpecMin { get; set; }
            public string SpecMax { get; set; }
            public string Value { get; set; }
            public string Comments { get; set; }
            public bool GetXML(ref string xml, ref string errorMessage)
            {
                StringBuilder textXmlSB = new StringBuilder();
                try
                {
                    foreach (var pi in this.GetType().GetProperties())
                    {
                        try
                        {
                            textXmlSB.AppendFormat("{0}=\"{1}\" ", pi.Name, pi.GetValue(this, null));
                        }
                        catch (Exception)
                        {
                        }
                    }
                    xml = string.Format("\t\t\t<Data DataType=\"Numeric\" {0} />", textXmlSB.ToString());
                }
                catch (Exception ex)
                {
                    errorMessage = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                    return false;
                }
                return true;
            }
        }
        public class DataString
        {
            public string Name { get; set; }
            public string Units { get; set; }
            public string CompOperator { get; set; }
            public string Status { get; set; }
            public string SpecMin { get; set; }
            public string SpecMax { get; set; }
            public string Value { get; set; }
            public string Comments { get; set; }
            public bool GetXML(ref string xml, ref string errorMessage)
            {
                StringBuilder textXmlSB = new StringBuilder();
                try
                {
                    foreach (var pi in this.GetType().GetProperties())
                    {
                        try
                        {
                            textXmlSB.AppendFormat("{0}=\"{1}\" ", pi.Name, pi.GetValue(this, null));
                        }
                        catch (Exception)
                        {
                        }
                    }
                    xml = string.Format("\t\t\t<Data DataType=\"String\" {0} />", textXmlSB.ToString());
                }
                catch (Exception ex)
                {
                    errorMessage = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                    return false;
                }
                return true;
            }
        }
    }
    public class DataTable
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public bool GetXML(ref string xml, ref string errorMessage)
        {
            StringBuilder textXmlSB = new StringBuilder();
            try
            {
                foreach (var pi in this.GetType().GetProperties())
                {
                    try
                    {
                        textXmlSB.AppendFormat("{0}=\"{1}\" ", pi.Name, pi.GetValue(this, null));
                    }
                    catch (Exception)
                    {
                    }
                }
                xml = string.Format("\t\t\t<Data DataType=\"Table\" {0} />", textXmlSB.ToString());
            }
            catch (Exception ex)
            {
                errorMessage = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                return false;
            }
            return true;
        }
    }
    public class DataAttachment
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Value { get; set; }
        public bool GetXML(ref string xml, ref string errorMessage)
        {
            StringBuilder textXmlSB = new StringBuilder();
            try
            {
                foreach (var pi in this.GetType().GetProperties())
                {
                    try
                    {
                        textXmlSB.AppendFormat("{0}=\"{1}\" ", pi.Name, pi.GetValue(this, null));
                    }
                    catch (Exception)
                    {
                    }
                }
                xml = string.Format("\t\t\t<Data DataType=\"Attachment\" {0} />", textXmlSB.ToString());
            }
            catch (Exception ex)
            {
                errorMessage = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                return false;
            }
            return true;
        }
    }
    public class DataImage
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Value { get; set; }
        public bool GetXML(ref string xml, ref string errorMessage)
        {
            StringBuilder textXmlSB = new StringBuilder();
            try
            {
                foreach (var pi in this.GetType().GetProperties())
                {
                    try
                    {
                        textXmlSB.AppendFormat("{0}=\"{1}\" ", pi.Name, pi.GetValue(this, null));
                    }
                    catch (Exception)
                    {
                    }
                }
                xml = string.Format("\t\t\t<Data DataType=\"Image\" {0} />", textXmlSB.ToString());
            }
            catch (Exception ex)
            {
                errorMessage = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                return false;
            }
            return true;
        }
    }
    public class DataSerial
    {
        public string Value { get; set; }
        public string PartType { get; set; }
        public string PartNumber { get; set; }
        public bool GetXML(ref string xml, ref string errorMessage)
        {
            StringBuilder textXmlSB = new StringBuilder();
            try
            {
                foreach (var pi in this.GetType().GetProperties())
                {
                    try
                    {
                        textXmlSB.AppendFormat("{0}=\"{1}\" ", pi.Name, pi.GetValue(this, null));
                    }
                    catch (Exception)
                    {
                    }
                }
                xml = string.Format("\t\t\t<Data DataType=\"Serial\" {0} />", textXmlSB.ToString());
            }
            catch (Exception ex)
            {
                errorMessage = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                return false;
            }
            return true;
        }
    }
    public class DataArray
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Units { get; set; }
        public string Value { get; set; }
        public bool GetXML(ref string xml, ref string errorMessage)
        {
            try
            {
                xml = string.Format("\t\t\t<Data DataType=\"Array\" Name=\"{0}\" Units=\"{1}\" Status=\"{2}\">{3}</Data> ",
                    Name, Units, Status, Value);
            }
            catch (Exception ex)
            {
                errorMessage = System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message;
                return false;
            }
            return true;
        }
    }
}
