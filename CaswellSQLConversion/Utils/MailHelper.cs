using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;
using SystemLibrary.Utility;

namespace CaswellSQLConversion.Utils
{
    public class MailHelper
    {
        /// <summary>
        /// 寄信標題
        /// </summary>
        static string _configMailSubject = ConfigurationManager.AppSettings["mailTitle"].Trim();
        /// <summary>
        /// 寄信人Email
        /// </summary>
        static string _sendMail = ConfigurationManager.AppSettings["sendMail"].Trim();
        /// <summary>
        /// 收信人Email(多筆用逗號隔開)
        /// </summary>
        static string _receiveBccMails = ConfigurationManager.AppSettings["receiveMails"].Trim();
        /// <summary>
        /// 寄信smtp server
        /// </summary>
        static string _smtpServer = ConfigurationManager.AppSettings["smtpServer"].Trim();
        /// <summary>
        /// For Test Enviroment Special Suffix usage
        /// </summary>
        static string _subjectSuffix = ConfigurationManager.AppSettings["SubjectSuffix"].Trim();

        /// <summary>
        /// 完整的寄信功能
        /// </summary>
        /// <param name="MailFrom">寄信人E-mail Address</param>
        /// <param name="MailToList">收信人E-mail Address</param>
        /// <param name="mailSubject">主旨</param>
        /// <param name="MailBody">信件內容</param>
        /// <param name="isBodyHtml">是否採用HTML格式</param>
        /// <param name="filePaths">附檔在WebServer檔案總管路徑</param>
        /// <param name="deleteFileAttachment">是否刪除在WebServer上的附件</param>
        /// <param name="CcList">副本E-mail Address</param>
        /// <returns>是否成功</returns>
        public static bool SendMail(string MailFrom, List<string> MailToList, string mailSubject, string MailBody, bool isBodyHtml, string[] filePaths, bool deleteFileAttachment, List<string> CcList = null)
        {
            try
            {
                //防呆
                if (string.IsNullOrEmpty(MailFrom))
                {//※有些公司的Mail Server會規定寄信人的Domain Name要是該Mail Server的Domain Name
                    MailFrom = "KaizenTDSConverstion@lumentum.com";
                }

                //命名空間： System.Web.Mail已過時，http://msdn.microsoft.com/zh-tw/library/system.web.mail.mailmessage(v=vs.80).aspx
                //建立MailMessage物件
                System.Net.Mail.MailMessage mms = new System.Net.Mail.MailMessage();
                //指定一位寄信人MailAddress
                mms.From = new MailAddress(MailFrom);
                //信件主旨
                if (string.IsNullOrEmpty(mailSubject))
                {
                    mms.Subject = _subjectSuffix + _configMailSubject;
                }
                else
                {
                    mms.Subject = _subjectSuffix + mailSubject;
                }
                //信件內容
                mms.Body = MailBody;
                //信件內容 是否採用Html格式
                mms.IsBodyHtml = isBodyHtml;

                if (MailToList != null)//防呆
                {
                    foreach (var mail in MailToList)
                    {
                        mms.To.Add(new MailAddress(mail.Trim()));
                    }

                    //CC List
                    if (CcList != null && CcList.Count() > 0)
                    {
                        foreach (var mail in CcList)
                        {
                            mms.CC.Add(new MailAddress(mail));
                        }
                    }
                    //Default System receivers BCC List
                    string[] receivers = _receiveBccMails.Split(new char[2] { ';', ',' });
                    foreach (var rec in receivers)
                    {
                        mms.Bcc.Add(new MailAddress(rec));
                    }
                }
                else
                {
                    string[] receivers = _receiveBccMails.Split(new char[2] { ';', ',' });
                    foreach (var rec in receivers)
                    {
                        mms.To.Add(new MailAddress(rec));
                    }
                }
                //End if (MailTos !=null)//防呆


                if (filePaths != null)//防呆
                {//有夾帶檔案
                    for (int i = 0; i < filePaths.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(filePaths[i].Trim()))
                        {
                            Attachment file = new Attachment(filePaths[i].Trim());
                            //加入信件的夾帶檔案
                            mms.Attachments.Add(file);
                        }
                    }

                }//End if (filePaths!=null)//防呆

                using (SmtpClient client = new SmtpClient(_smtpServer))//或公司、客戶的smtp_server
                {
                    //if (!string.IsNullOrEmpty(mailAccount) && !string.IsNullOrEmpty(mailPwd))//.config有帳密的話
                    //{//寄信要不要帳密？眾說紛紜Orz，分享一下經驗談....

                    //    //網友阿尼尼:http://www.dotblogs.com.tw/kkc123/archive/2012/06/26/73076.aspx
                    //    //※公司內部不用認證,寄到外部信箱要特別認證 Account & Password

                    //    //自家公司MIS:
                    //    //※要看smtp server的設定呀~

                    //    //結論...
                    //    //※程式在客戶那邊執行的話，問客戶，程式在自家公司執行的話，問自家公司MIS，最準確XD
                    //    client.Credentials = new NetworkCredential(mailAccount, mailPwd);//寄信帳密
                    //}
                    client.Send(mms);//寄出一封信
                }//end using 
                 //釋放每個附件，才不會Lock住
                 //if (mms.Attachments != null && mms.Attachments.Count > 0)
                 //{
                 //    for (int i = 0; i < mms.Attachments.Count; i++)
                 //    {
                 //        mms.Attachments[i].Dispose();

                //    }
                //}

                //#region 要刪除附檔
                //if (deleteFileAttachment && filePaths != null && filePaths.Length > 0)
                //{

                //    foreach (string filePath in filePaths)
                //    {
                //        File.Delete(filePath.Trim());
                //    }

                //}
                //#endregion
                return true;//成功
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void LogAndSendMail( string message, List<string> mailList, bool isError, string funcName, string subject = "")
        {
            if (isError) LogHelper.WriteLine(message);
            var isTestMail = ConfigurationManager.AppSettings["TestMail"].ToString() == "Y" ? true : false;
            if (isTestMail)
                mailList = null;
            if (isError)
            {
                MailHelper.SendMail(string.Empty, mailList, string.Format("KaizenTDSConverstion [{0}] Exception Alert", funcName), message, true, null, false);
            }
            else
            {
                MailHelper.SendMail(string.Empty, mailList, subject, message, true, null, false);
            }
            
        }
    }
}