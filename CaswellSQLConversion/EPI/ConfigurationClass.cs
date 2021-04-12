using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CaswellSQLConversion.EPI
{
    class EPIConfigurationClass
    {
        private Thread Mythread { get; set; }
        public string Status { get; set; }
        public string Error_Message { get; set; }
        public string Connection_String { get; set; }
        public string Target_Table_EPIRUNSUMMARY { get; set; }
        public string EPI_RUN_SQL { get; set; }
        public string Target_Table_WAFERSUMMARY { get; set; }
        public string Target_Table_REAGENTSUMMARY { get; set; }
        public string Target_Table_LAYERSUMMARY { get; set; }
        public string Target_Table_COMEGA02_1 { get; set; }
        public string Target_Table_COMEGA02_2 { get; set; }
        public string Number_of_hour_each_run { get; set; }
        public string Default_Start_Date { get; set; }
        public string Maximum_row_each_run { get; set; }
        public string Site { get; set; }
        public string ProductFamily { get; set; }
        public string Operation_Prefix { get; set; }
        public string TestStation { get; set; }
        public string PartNumber { get; set; }
        public string DeviceType { get; set; }
        public string TestStepName { get; set; }
        public string OutputXMLPath { get; set; }
        public string OutputTableImageAttachmentPath { get; set; }
        public string BackFilePath { get; set; }
        public string ErrorFilePath { get; set; }
        public string Email_server { get; set; }
        public string Email_sender { get; set; }
        public string email_to { get; set; }
        public string Email_subject { get; set; }
        public string Email_bcc { get; set; }
        public string Email_cc { get; set; }
    }
}
