using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace CaswellSQLConversion.Utils
{
    /// <summary>
    /// 執行Excel VBA
    /// </summary>
    public class ExcelMacroHelper
    {
        /// <summary>
        /// 執行Excel中的module
        /// </summary>
        /// <param name="excelFilePath">Excel文件路徑</param>
        /// <param name="macroName">module名稱</param>
        /// <param name="parameters">module參數組</param>
        /// <param name="rtnValue">module返回值</param>
        /// <param name="isShowExcel">執行時是否顯示Excel</param>
        public void RunExcelMacro(string excelFilePath, string macroName,
            object[] parameters, out object rtnValue, bool isShowExcel)
        {
            try
            {
                #region 檢查入參

                // 檢查文件是否存在
                if (!File.Exists(excelFilePath))
                {
                    throw new System.Exception(excelFilePath + " 文件不存在");
                }

                // 檢查是否輸入module名稱
                if (string.IsNullOrEmpty(macroName))
                {
                    throw new System.Exception("請輸入module的名稱");
                }

                #endregion

                #region 調用module處理

                // 準備打開Excel文件時的缺省參數對象
                object oMissing = System.Reflection.Missing.Value;

                // 根據參數組是否為空，準備參數組對象
                object[] paraObjects;

                if (parameters == null)
                {
                    paraObjects = new object[] { macroName };
                }
                else
                {
                    // module參數組長度
                    int paraLength = parameters.Length;

                    paraObjects = new object[paraLength + 1];

                    paraObjects[0] = macroName;
                    for (int i = 0; i < paraLength; i++)
                    {
                        paraObjects[i + 1] = parameters[i];
                    }
                }

                // 創建Excel對象示例
                Excel.Application oExcel = new Excel.Application();

                // 判斷是否要求執行時Excel可見
                if (isShowExcel)
                {
                    // 使創建的對象可見
                    oExcel.Visible = true;
                }

                // 創建Workbooks對象
                Excel.Workbooks oBooks = oExcel.Workbooks;

                // 創建Workbook對象
                Excel._Workbook oBook = null;

                // 打開指定的Excel文件
                oBook = oBooks.Open(
                                        excelFilePath,
                                        oMissing,
                                        oMissing,
                                        oMissing,
                                        oMissing,
                                        oMissing,
                                        oMissing,
                                       oMissing,
                                       oMissing,
                                       oMissing,
                                       oMissing,
                                       oMissing,
                                       oMissing,
                                       oMissing,
                                       oMissing
                                  );

                // 執行Excel中的module
                rtnValue = this.RunMacro(oExcel, paraObjects);

                // 保存更改
                oBook.Save();

                // 退出Workbook
                oBook.Close(false, oMissing, oMissing);

                #endregion

                #region 釋放對象

                // 釋放Workbook對象
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oBook);
                oBook = null;

                // 釋放Workbooks對象
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oBooks);
                oBooks = null;

                // 關閉Excel
                oExcel.Quit();

                // 釋放Excel對象
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oExcel);
                oExcel = null;

                // 調用垃圾回收
                GC.Collect();

                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 執行module
        /// </summary>
        /// <param name="oApp">Excel對象</param>
        /// <param name="oRunArgs">參數（第一個參數為指定module名稱，後面為指定module的參數值）</param>
        /// <returns>module返回值</returns>
        private object RunMacro(object oApp, object[] oRunArgs)
        {
            try
            {
                // 聲明一個返回對象
                object objRtn;

                // 反射方式執行module
                objRtn = oApp.GetType().InvokeMember(
                                                        "Run",
                                                        System.Reflection.BindingFlags.Default |
                                                        System.Reflection.BindingFlags.InvokeMethod,
                                                        null,
                                                        oApp,
                                                        oRunArgs
                                                     );

                // 返回值
                return objRtn;

            }
            catch (Exception ex)
            {
                // 如果有底層異常，拋出底層異常
                if (ex.InnerException.Message.ToString().Length > 0)
                {
                    throw ex.InnerException;
                }
                else
                {
                    throw ex;
                }
            }
        }
    }
}
