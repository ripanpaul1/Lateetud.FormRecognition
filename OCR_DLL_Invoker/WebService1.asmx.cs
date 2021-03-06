﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Diagnostics;
using System.Configuration;
using System.Web.Services;
using FormOcrWcf;
using Leadtools.Forms.Processing;
using Leadtools.Forms.Auto;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using System.Web.Script.Services;
using System.Data.Entity;
using System.Data.SqlClient;

namespace OCR_DLL_Invoker
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
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
            result = FormOcrWcf.Program.ProcessForms(DirPath);
            output = PrintToString(result);
            return output;
        }
        [WebMethod]
        public string GetResultInXML()
        {


            long runID;
            string str = string.Empty;
            using (DBEntities context = new DBEntities())
            {
                IEnumerable<RunResult> details = context.Database.SqlQuery
                                                                              <RunResult>("exec proc_APICallHistory_GetRunID").ToList();
                runID = Convert.ToInt64(details.FirstOrDefault().RunID);
            }
            try
            {
                #region Entry into APICallHistory


                APICallHistory apiCallHstStart = new APICallHistory();

                apiCallHstStart.RunID = Convert.ToInt64(runID);
                apiCallHstStart.RunTime = DateTime.Now;
                apiCallHstStart.Comment = "Call For XML";
                apiCallHstStart.Status = "START";
                using (DBEntities db = new DBEntities())
                {
                    db.APICallHistories.Add(apiCallHstStart);
                    db.SaveChanges();
                }
                #endregion


                string DirPath = GetEnvironmentVariable("SMART");
                AutoFormsRecognizeFormResult result = new AutoFormsRecognizeFormResult();
                DataTable dtOutput = new DataTable();
                result = FormOcrWcf.Program.ProcessForms(DirPath);
                dtOutput = ResultToDataTable(result);

                dtOutput.TableName = "LateetudRuleApplication";
                str = ConvertDatatableToXML(dtOutput);
                #region End Entry into APICallHistory
                //APICallHistory apiCallHstStart = new APICallHistory();

                apiCallHstStart.RunID = Convert.ToInt64(runID);
                apiCallHstStart.RunTime = DateTime.Now;
                apiCallHstStart.Comment = "Call For XML";
                apiCallHstStart.Status = "END";
                using (DBEntities db = new DBEntities())
                {
                    db.APICallHistories.Add(apiCallHstStart);
                    db.SaveChanges();
                }
                #endregion

                #region Entry into Summary
                MasterFormApplicationSummary applicationSummary = new MasterFormApplicationSummary();

                foreach (DataRow row in dtOutput.Rows)
                {
                    applicationSummary.RunID = Convert.ToInt64(runID);
                    applicationSummary.EntryDate = DateTime.Now;
                    applicationSummary.FieldKey = Convert.ToString(row["FieldName"]);
                    applicationSummary.FieldValue = Convert.ToString(row["FieldValue"]);
                    using (DBEntities db = new DBEntities())
                    {
                        db.MasterFormApplicationSummaries.Add(applicationSummary);
                        db.SaveChanges();
                    }

                }

                #endregion

                
            }
            catch (Exception ex)
            {
                #region Error Log
                ExceptionLog log = new ExceptionLog();
                log.ErrorTime = DateTime.Now;
                log.ErrorMessage = Convert.ToString(ex.Message);
                log.Comments = "Error at Call For XML Method for Run ID: "+runID+"";

                using (DBEntities db = new DBEntities())
                {
                    db.ExceptionLogs.Add(log);
                    db.SaveChanges();
                }

                #endregion

                WriteToFile("RuleEngine Error on: {0} " + ex.Message + ex.StackTrace);
            }
            return str;
        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetResultInJSON()
        {

            long runID;
            string jsonResult = string.Empty;

            using (DBEntities context = new DBEntities())
            {
                IEnumerable<RunResult> details = context.Database.SqlQuery
                                                                              <RunResult>("exec proc_APICallHistory_GetRunID").ToList();
                runID = Convert.ToInt64(details.FirstOrDefault().RunID);
            }
            try
            {
                #region Entry into APICallHistory


                APICallHistory apiCallHstStart = new APICallHistory();

                apiCallHstStart.RunID = Convert.ToInt64(runID);
                apiCallHstStart.RunTime = DateTime.Now;
                apiCallHstStart.Comment = "Call For JSON";
                apiCallHstStart.Status = "START";
                using (DBEntities db = new DBEntities())
                {
                    db.APICallHistories.Add(apiCallHstStart);
                    db.SaveChanges();
                }
                #endregion

                string DirPath = GetEnvironmentVariable("SMART");
                AutoFormsRecognizeFormResult result = new AutoFormsRecognizeFormResult();
                DataTable dtOutput = new DataTable();
                result = FormOcrWcf.Program.ProcessForms(DirPath);
                dtOutput = ResultToDataTable(result);

                dtOutput.TableName = "LateetudRuleApplication";
                JsonSerializerSettings jss = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                jsonResult = JsonConvert.SerializeObject(dtOutput, Newtonsoft.Json.Formatting.Indented, jss);

                #region End Entry into APICallHistory
                //APICallHistory apiCallHstStart = new APICallHistory();

                apiCallHstStart.RunID = Convert.ToInt64(runID);
                apiCallHstStart.RunTime = DateTime.Now;
                apiCallHstStart.Comment = "Call For JSON";
                apiCallHstStart.Status = "END";
                using (DBEntities db = new DBEntities())
                {
                    db.APICallHistories.Add(apiCallHstStart);
                    db.SaveChanges();
                }
                #endregion

                #region Entry into Summary
                MasterFormApplicationSummary applicationSummary = new MasterFormApplicationSummary();

                foreach (DataRow row in dtOutput.Rows)
                {
                    applicationSummary.RunID = Convert.ToInt64(runID);
                    applicationSummary.EntryDate = DateTime.Now;
                    applicationSummary.FieldKey = Convert.ToString(row["FieldName"]);
                    applicationSummary.FieldValue = Convert.ToString(row["FieldValue"]);
                    using (DBEntities db = new DBEntities())
                    {
                        db.MasterFormApplicationSummaries.Add(applicationSummary);
                        db.SaveChanges();
                    }

                }

                #endregion
            }
            catch (Exception ex)
            {
                #region Error Log
                ExceptionLog log = new ExceptionLog();
                log.ErrorTime = DateTime.Now;
                log.ErrorMessage = Convert.ToString(ex.Message);
                log.Comments = "Error at Call For JSON Method for Run ID: " + runID + "";

                using (DBEntities db = new DBEntities())
                {
                    db.ExceptionLogs.Add(log);
                    db.SaveChanges();
                }

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


        public class RunResult
        {
            public long RunID { get; set; }

        }
    }
}
