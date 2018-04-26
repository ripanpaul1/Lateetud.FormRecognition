using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Diagnostics;
using System.Configuration;
using System.Web.Services;
//using FormOcrWcf;
using Leadtools.Forms.Processing;
using Leadtools.Forms.Auto;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using System.Web.Script.Services;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Collections;
using System.Reflection;
using FormRecognitionUtility;
using DAL;
using BLL;

namespace OCR_DLL_Invoker
{
    /// <summary>
    /// Summary description for RuleEngineService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class RuleEngineService : System.Web.Services.WebService
    {

        Process proc = null;
        private static string GetEnvironmentVariable(string latFormHome)
        {
            if (latFormHome == null)
            {
                return null;
            }

            var value = Environment.GetEnvironmentVariable(latFormHome);

            return value;
        }

        [WebMethod]
        public string OCRInvoker_BPMSERVER()
        {

            string output = "";
            string DirPath = GetEnvironmentVariable("SMART");
            AutoFormsRecognizeFormResult result = new AutoFormsRecognizeFormResult();

            //output = FormOcrWcf.Program.ProcessForms(DirPath);
            result = FormRecognitionUtility.RuleEngineUtility.ProcessForms(DirPath);
            output = PrintToString(result);
            return output;
        }
        [WebMethod]
        public string GetResultInXML(string FileName)
        {

            string strStatus = "START";
            long runID;
            string str = string.Empty;
            DateTime dtServiceStart = DateTime.Now;

            MasterFormApplicationSummaryHandler applicationHandler = new MasterFormApplicationSummaryHandler();
            APICallHistoryHandler objAPICallHistoryHandler = new APICallHistoryHandler();
            APICallHistory objRuleSummary = objAPICallHistoryHandler.GetRunID();
            runID = Convert.ToInt64(objRuleSummary.RunID);
           
            try
            {

                string DirPath = GetEnvironmentVariable("SMART");
                AutoFormsRecognizeFormResult result = new AutoFormsRecognizeFormResult();
                DataTable dtOutput = new DataTable();
                if (string.IsNullOrEmpty(FileName))//If File Name doesn`t exists
                {
                   
                    result = FormRecognitionUtility.RuleEngineUtility.ProcessForms(DirPath, runID);
                    //dtOutput = ResultToDataTable(result);
                }
                else//If File Name provided
                {
                    var targetDocInput = Path.Combine(DirPath, "OCRInput");
                    //result = FormRecognitionUtility.RuleEngineUtility.ProcessFiles(targetDocInput, new[] { FileName }, runID);
                    dtOutput = FormRecognitionUtility.RuleEngineUtility.ProcessFile(targetDocInput, new[] { FileName }, runID);
                }

               
                //dtOutput = applicationHandler.GetResultByRunID(runID); // Get Result By Current Run ID
               
                dtOutput.TableName = "LateetudRuleApplication";
                str = ConvertDatatableToXML(dtOutput);
                #region Entry into APICallHistory
                if (dtOutput.Rows.Count > 0)
                {
                    strStatus = dtOutput.Rows.Count + " Rows Successfully returned";
                }
                else
                {
                    strStatus = "No Records returned";
                }

                APICallHistory apiCallHst = new APICallHistory();

                apiCallHst.RunID = Convert.ToInt64(runID);
                apiCallHst.StartTime = dtServiceStart;
                apiCallHst.EndTime = DateTime.Now;
                apiCallHst.Comment = "Call For XML";
                apiCallHst.Status = strStatus;//"START";

                APICallHistoryHandler callHistoryHandler = new APICallHistoryHandler();

                callHistoryHandler.AddNew(apiCallHst);


                #endregion

                #region Entry into Summary
                MasterFormApplicationSummary applicationSummary = new MasterFormApplicationSummary();

                foreach (DataRow row in dtOutput.Rows)
                {
                    applicationSummary.RunID = Convert.ToInt64(runID);
                    applicationSummary.EntryDate = DateTime.Now;
                    applicationSummary.FileName = string.IsNullOrEmpty(Convert.ToString(row["FileName"])) ? "" : Convert.ToString(row["FileName"]);
                    applicationSummary.FieldKey = string.IsNullOrEmpty(Convert.ToString(row["FieldName"])) ? "" : Convert.ToString(row["FieldName"]);
                    applicationSummary.FieldValue = string.IsNullOrEmpty(Convert.ToString(row["FieldValue"])) ? "" : Convert.ToString(row["FieldValue"]);
                    MasterFormApplicationSummaryHandler applicationSummaryHandler = new MasterFormApplicationSummaryHandler();

                    applicationSummaryHandler.AddNew(applicationSummary);

                }

                
                #endregion


            }
            catch (Exception ex)
            {
                #region Error Log
                ExceptionLog log = new ExceptionLog();
                log.ErrorTime = DateTime.Now;
                log.ErrorMessage = Convert.ToString(ex.Message);
                log.Comments = "Error at Call For XML Method for Run ID: " + runID + "";
                log.ErrorNumber = "0";
                ExceptionLogHandler exceptionLogHandler = new ExceptionLogHandler();

                exceptionLogHandler.AddNew(log);

                #endregion

                #region Update History table
                APICallHistory apiCallHst = new APICallHistory();

                apiCallHst.RunID = Convert.ToInt64(runID);
                apiCallHst.StartTime = dtServiceStart;
                apiCallHst.EndTime = DateTime.Now;
                apiCallHst.Comment = "Call For XML";
                apiCallHst.Status = strStatus;//"START";

                APICallHistoryHandler callHistoryHandler = new APICallHistoryHandler();

                callHistoryHandler.AddNew(apiCallHst);

                #endregion

                WriteToFile("RuleEngine Error on: {0} " + ex.Message + ex.StackTrace);
            }
            return str;
        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetResultInJSON(string FileName)
        {
            string strStatus = "START";
            long runID;
            string jsonResult = string.Empty;
            DateTime dtServiceStart = DateTime.Now;

            MasterFormApplicationSummaryHandler applicationHandler = new MasterFormApplicationSummaryHandler();
            APICallHistoryHandler objAPICallHistoryHandler = new APICallHistoryHandler();
            APICallHistory objRuleSummary = objAPICallHistoryHandler.GetRunID();
            runID = Convert.ToInt64(objRuleSummary.RunID);

            try
            {




                string DirPath = GetEnvironmentVariable("SMART");
                AutoFormsRecognizeFormResult result = new AutoFormsRecognizeFormResult();
                DataTable dtOutput = new DataTable();
                if (string.IsNullOrEmpty(FileName))//If File Name doesn`t exists
                {
                   
                    result = FormRecognitionUtility.RuleEngineUtility.ProcessForms(DirPath, runID);
                }
                else//If File Name provided
                {
                    var targetDocInput = Path.Combine(DirPath, "OCRInput");
                    //result = FormRecognitionUtility.RuleEngineUtility.ProcessFiles(targetDocInput, new[] { FileName }, runID);
                    dtOutput = FormRecognitionUtility.RuleEngineUtility.ProcessFile(targetDocInput, new[] { FileName }, runID);
                    
                }


                //dtOutput = applicationHandler.GetResultByRunID(runID); // Get Result By Current Run ID

                dtOutput.TableName = "LateetudRuleApplication";
                JsonSerializerSettings jss = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                jsonResult = JsonConvert.SerializeObject(dtOutput, Newtonsoft.Json.Formatting.Indented, jss);


                #region Entry into APICallHistory
                if (dtOutput.Rows.Count > 0)
                {
                    strStatus = dtOutput.Rows.Count + " Rows Successfully returned";
                }
                else
                {
                    strStatus = "No Records returned";
                }

                APICallHistory apiCallHst = new APICallHistory();

                apiCallHst.RunID = Convert.ToInt64(runID);
                apiCallHst.StartTime = dtServiceStart;
                apiCallHst.EndTime = DateTime.Now;
                apiCallHst.Comment = "Call For JSON";
                apiCallHst.Status = strStatus;//"START";

                APICallHistoryHandler callHistoryHandler = new APICallHistoryHandler();

                callHistoryHandler.AddNew(apiCallHst);


                #endregion

                
                #region Entry into Summary
                MasterFormApplicationSummary applicationSummary = new MasterFormApplicationSummary();

                foreach (DataRow row in dtOutput.Rows)
                {
                    applicationSummary.RunID = Convert.ToInt64(runID);
                    applicationSummary.EntryDate = DateTime.Now;
                    applicationSummary.FileName = string.IsNullOrEmpty(Convert.ToString(row["FileName"])) ? "" : Convert.ToString(row["FileName"]); 
                    applicationSummary.FieldKey = string.IsNullOrEmpty(Convert.ToString(row["FieldName"])) ? "" : Convert.ToString(row["FieldName"]); 
                    applicationSummary.FieldValue =string.IsNullOrEmpty(Convert.ToString(row["FieldValue"]))?"": Convert.ToString(row["FieldValue"]);
                    MasterFormApplicationSummaryHandler applicationSummaryHandler = new MasterFormApplicationSummaryHandler();

                    applicationSummaryHandler.AddNew(applicationSummary);

                }

                //foreach (DataRow row in dtOutput.Rows)
                //{
                //    applicationSummary.RunID = Convert.ToInt64(runID);
                //    applicationSummary.EntryDate = DateTime.Now;
                //    applicationSummary.FieldKey = Convert.ToString(row["FieldName"]);
                //    applicationSummary.FieldValue = Convert.ToString(row["FieldValue"]);
                //    using (DBEntities db = new DBEntities())
                //    {
                //        db.MasterFormApplicationSummaries.Add(applicationSummary);
                //        db.SaveChanges();
                //    }

                //}

                #endregion
            }
            catch (Exception ex)
            {
                #region Error Log
                ExceptionLog log = new ExceptionLog();
                log.ErrorTime = DateTime.Now;
                log.ErrorMessage = Convert.ToString(ex.Message);
                log.Comments = "Error at Call For JSON Method for Run ID: " + runID + "";
                log.ErrorNumber = "0";
                ExceptionLogHandler exceptionLogHandler = new ExceptionLogHandler();

                exceptionLogHandler.AddNew(log);

                #endregion

                #region Update History table
                APICallHistory apiCallHst = new APICallHistory();

                apiCallHst.RunID = Convert.ToInt64(runID);
                apiCallHst.StartTime = dtServiceStart;
                apiCallHst.EndTime = DateTime.Now;
                apiCallHst.Comment = "Call For JSON";
                apiCallHst.Status = strStatus;//"START";

                APICallHistoryHandler callHistoryHandler = new APICallHistoryHandler();

                callHistoryHandler.AddNew(apiCallHst);

                #endregion

                WriteToFile("RuleEngine Error on: {0} " + ex.Message + ex.StackTrace);
            }

            return jsonResult;

        }
        private static string PrintToString(AutoFormsRecognizeFormResult result)
        {
            string fieldname = string.Empty;
            string fieldvalue = "value,";
            string pagenumber = "pagenumber,";
            string confidencevalue = "confidence,";
            string boundx = "x,";
            string boundy = "y,";
            string boundwidth = "width,";
            string boundheight = "height,";
            string tableinfo = "";

            string ocrResult = String.Empty;
            string openBrac = "[[[";
            string closeBrac = "]]]";
            string resDenoter = ":::";

            foreach (var formPage in result.FormPages)
            {
                foreach (var pageResultItem in formPage)
                {
                    var textField = pageResultItem as TextFormField;
                    var omrField = pageResultItem as OmrFormField;
                    var tablefield = pageResultItem as TableFormField;

                    if (textField != null)
                    {


                        ((TextFormFieldResult)textField.Result).Text = ((TextFormFieldResult)textField.Result).Text?.Replace(",", " ");
                        ((TextFormFieldResult)textField.Result).Text = ((TextFormFieldResult)textField.Result).Text?.Replace(System.Environment.NewLine, " ");
                        fieldname = textField.Name;
                        fieldvalue = ((TextFormFieldResult)(textField.Result)).Text?.Trim();

                        ocrResult += openBrac + fieldname + resDenoter + fieldvalue + closeBrac;

                    }
                    else if (omrField != null)
                    {
                        if (((OmrFormFieldResult)(omrField.Result)).Text == "0")
                        {
                            ((OmrFormFieldResult)(omrField.Result)).Text = ((OmrFormFieldResult)(omrField.Result)).Text?.Replace("0", "False");
                        }
                        else

                        {
                            ((OmrFormFieldResult)(omrField.Result)).Text = ((OmrFormFieldResult)(omrField.Result)).Text?.Replace("1", "True");
                        }

                        fieldname = omrField.Name;
                        fieldvalue = ((OmrFormFieldResult)(omrField.Result)).Text;

                        ocrResult += openBrac + fieldname + resDenoter + fieldvalue + closeBrac;
                    }
                }
            }
            return ocrResult;
        }


        private static DataTable ResultToDataTable(AutoFormsRecognizeFormResult result)
        {
            string fieldname = string.Empty;
            string fieldvalue = "value,";
            string pagenumber = "pagenumber,";
            string confidencevalue = "confidence,";
            string boundx = "x,";
            string boundy = "y,";
            string boundwidth = "width,";
            string boundheight = "height,";
            string tableinfo = "";

            //string ocrResult = String.Empty;
            //string openBrac = "[[[";
            //string closeBrac = "]]]";
            //string resDenoter = ":::";

            DataTable dtResult = new DataTable("Result");
            dtResult.Columns.Add("FieldName", typeof(string));
            dtResult.Columns.Add("FieldValue", typeof(string));

            foreach (var formPage in result.FormPages)
            {

                foreach (var pageResultItem in formPage)
                {

                    var textField = pageResultItem as TextFormField;
                    var omrField = pageResultItem as OmrFormField;
                    var tablefield = pageResultItem as TableFormField;

                    if (textField != null)
                    {


                        ((TextFormFieldResult)textField.Result).Text = ((TextFormFieldResult)textField.Result).Text?.Replace(",", " ");
                        ((TextFormFieldResult)textField.Result).Text = ((TextFormFieldResult)textField.Result).Text?.Replace(System.Environment.NewLine, " ");
                        fieldname = textField.Name;
                        fieldvalue = ((TextFormFieldResult)(textField.Result)).Text?.Trim();

                        DataRow dr = dtResult.NewRow();
                        dr["FieldName"] = fieldname;
                        dr["FieldValue"] = fieldvalue;
                        dtResult.Rows.Add(dr);
                        //ocrResult += openBrac + fieldname + resDenoter + fieldvalue + closeBrac;

                    }
                    else if (omrField != null)
                    {
                        if (((OmrFormFieldResult)(omrField.Result)).Text == "0")
                        {
                            ((OmrFormFieldResult)(omrField.Result)).Text = ((OmrFormFieldResult)(omrField.Result)).Text?.Replace("0", "False");
                        }
                        else

                        {
                            ((OmrFormFieldResult)(omrField.Result)).Text = ((OmrFormFieldResult)(omrField.Result)).Text?.Replace("1", "True");
                        }

                        fieldname = omrField.Name;
                        fieldvalue = ((OmrFormFieldResult)(omrField.Result)).Text;

                        //ocrResult += openBrac + fieldname + resDenoter + fieldvalue + closeBrac;
                        DataRow dr = dtResult.NewRow();
                        dr["FieldName"] = fieldname;
                        dr["FieldValue"] = fieldvalue;
                        dtResult.Rows.Add(dr);
                    }
                }
            }
            return dtResult;
        }

        private string ConvertDatatableToXML(DataTable dt)
        {
            MemoryStream str = new MemoryStream();
            dt.WriteXml(str, true);
            str.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(str);
            string xmlstr;
            xmlstr = sr.ReadToEnd();
            return (xmlstr);
        }

        private void WriteToFile(string text)
        {
            string filePath = ConfigurationManager.AppSettings["LogFile"].ToString();
            //string path = "E:\\ServiceLog.txt";
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                writer.Close();
            }
        }
        public static DataTable CreateDataTable(IEnumerable source)
        {
            var table = new DataTable();
            int index = 0;
            var properties = new List<PropertyInfo>();
            foreach (var obj in source)
            {
                if (index == 0)
                {
                    foreach (var property in obj.GetType().GetProperties())
                    {
                        if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                        {
                            continue;
                        }
                        properties.Add(property);
                        table.Columns.Add(new DataColumn(property.Name, property.PropertyType));
                    }
                }
                object[] values = new object[properties.Count];
                for (int i = 0; i < properties.Count; i++)
                {
                    values[i] = properties[i].GetValue(obj);
                }
                table.Rows.Add(values);
                index++;
            }
            return table;
        }

        public class CurrentRun
        {
            public long RunID { get; set; }

        }
        public class Result
        {
            public string FileName { get; set; }
            public string FieldKey { get; set; }
            public string FieldValue { get; set; }

        }
    }
}
