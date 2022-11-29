﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Security.Cryptography;

namespace GRIDLibraries.Libraries
{
    
    partial class GridLib
    {
       
        //public List<gridQAQuestionnaireForm> GetQALob1()
        //{
        //    var temp = new List<gridQAQuestionnaireForm>();

        //    if (this.OpenMainAHSQAConnection())
        //    {
        //        SqlDataReader dr;
        //        this.gridMainDbCommand.CommandText = "SELECT * FROM dbo.[tblLOB_V1] WHERE [Id] NOT IN (1, 2, 4);";


        //        dr = this.gridMainDbCommand.ExecuteReader();


        //        while (dr.Read())
        //            temp.Add(new gridQAQuestionnaireForm() { Id = (int)dr["Id"], LOBName = (string)dr["LineOfBusinessValue"] });
        //        dr.Close();
        //        this.CloseDbConnection();
        //    }


        //    return temp;

        //}

        public DataTable GetQALob()
        {
            var temp = new DataTable();

            var da = new SqlDataAdapter();

            da = new SqlDataAdapter("SELECT * FROM dbo.[tblQAForm];", conStringAHS_QA);
            da.SelectCommand.CommandTimeout = 1000;
            try
            {
                da.Fill(temp);
            }
            catch (Exception ex)
            {
            }

            return temp;
        }

        public DataTable GetQAQuestionnaires()
        {
            var temp = new DataTable();

            var da = new SqlDataAdapter();

            da = new SqlDataAdapter("SELECT DISTINCT Id,FormId,Question,ObjectType,Remarks FROM [dbo].[vQuery_QA_Questionnaire];", conStringAHS_QA);
            da.SelectCommand.CommandTimeout = 1000;
            try
            {
                da.Fill(temp);
            }
            catch (Exception ex)
            {
            }

            return temp;
        }

        public DataTable GetQASelection()
        {          
            var temp = new DataTable();
            var da = new SqlDataAdapter();

            da = new SqlDataAdapter("SELECT * FROM[dbo].[vQuery_QA_Questionnaire];", conStringAHS_QA);
            da.SelectCommand.CommandTimeout = 1000;
            try
            {
                da.Fill(temp);
            }
            catch (Exception ex)
            {
            }
            return temp;
        }

        public DataTable GetQAMarkdown()
        {
            var temp = new DataTable();
            var da = new SqlDataAdapter();

            da = new SqlDataAdapter("SELECT * FROM [dbo].[vQuery_QA_Markdown];", conStringAHS_QA);
            da.SelectCommand.CommandTimeout = 1000;
            try
            {
                da.Fill(temp);
            }
            catch (Exception ex)
            {
            }
            return temp;
        }

        public bool AddQAForm(string _OALOBName)
        {
            

            bool temp = false;
            this.grdMutexx.WaitOne();

            if (this.OpenMainAHSQAConnection())
            {
                this.gridMainDbCommand.CommandText = "SELECT COALESCE(MAX(ID)+1,1) AS MAXID FROM [dbo].[tblQAForm];";
                try
                {
                    SqlDataReader dr = this.gridMainDbCommand.ExecuteReader();
                    while (dr.Read())
                    {                      
                        this.grdData.QuestionForm.MaxLOBId = Convert.ToInt32(dr["MAXID"]);                       
                    }
                    dr.Close();

                    this.gridMainDbCommand.Parameters.Clear();
                    this.gridMainDbCommand.Parameters.AddWithValue("@Name", _OALOBName);
                    this.gridMainDbCommand.Parameters.AddWithValue("@ModifiedBy", this.grdData.CurrentUser.EID);
                    this.gridMainDbCommand.Parameters.AddWithValue("@ModifiedDate", this.ConvertTimeZone(this.grdData.TeamInfo.OffSet));
                    this.gridMainDbCommand.Parameters.AddWithValue("@Status", true);

                    this.gridMainDbCommand.CommandText = "INSERT INTO [dbo].[tblQAForm] ([Name],[ModifiedBy],[ModifiedDate],[Status]) VALUES (@Name,@ModifiedBy,@ModifiedDate,@Status);";

                    try
                    {
                      if (this.gridMainDbCommand.ExecuteNonQuery() > 0)
                               temp = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                catch (Exception ex)
                {
                }
                this.CloseMainDbConnection();
            }
            this.grdMutexx.ReleaseMutex();

            return temp;

        }

        public bool AddQATemplate(int _FormId, string _Category, string _QAQuestion, string _QAObjType, string _QARemarks, DataTable dt1, DataTable dt2)
        {
            bool temp = false;
            this.grdMutexx.WaitOne();

            if (this.OpenMainAHSQAConnection())
            {
                this.gridMainDbCommand.CommandText = "SELECT COALESCE(MAX(FormId)+1,1) AS MAXQID FROM [dbo].[tblQAQuestions]";
                try
                {
                    SqlDataReader dr = this.gridMainDbCommand.ExecuteReader();
                    while (dr.Read())
                    {
                        this.grdData.QuestionForm.MaxQID = Convert.ToInt32(dr["MAXQID"]);
                    }
                    dr.Close();

                    this.gridMainDbCommand.Parameters.Clear();
                    this.gridMainDbCommand.Parameters.AddWithValue("@Id", this.grdData.QuestionForm.LOBId);
                    this.gridMainDbCommand.Parameters.AddWithValue("@FormId", this.grdData.QuestionForm.MaxQID);
                    this.gridMainDbCommand.Parameters.AddWithValue("@Category", _Category);
                    this.gridMainDbCommand.Parameters.AddWithValue("@Question", _QAQuestion);
                    this.gridMainDbCommand.Parameters.AddWithValue("@ObjType", _QAObjType);
                    this.gridMainDbCommand.Parameters.AddWithValue("@Remarks", _QARemarks);
                    this.gridMainDbCommand.Parameters.AddWithValue("@ModifiedBy", this.grdData.CurrentUser.EID);
                    this.gridMainDbCommand.Parameters.AddWithValue("@ModifiedDate", this.ConvertTimeZone(grdData.TeamInfo.OffSet));
                    this.gridMainDbCommand.Parameters.AddWithValue("@Status", true);

                    this.gridMainDbCommand.CommandText = "INSERT INTO [dbo].[tblQAQuestions] ([Id],[FormId],[Category],[Question],[ObjectType],[Remarks],[ModifiedBy],[ModifiedDate],[Status])" +
                                " VALUES (@Id,@FormId,@Category,@Question,@ObjType,@Remarks,@ModifiedBy,@ModifiedDate,@Status);";
                    try
                    {
                        if (this.gridMainDbCommand.ExecuteNonQuery() > 0)
                            temp = true;
                    }
                    catch (Exception)
                    {
                    }

                    foreach (DataRow row in dt1.Rows)
                    {
                        this.grdData.QuestionForm.SelectionValue = row["Value"].ToString();
                        this.grdData.QuestionForm.Score = Convert.ToInt32(row["Score"].ToString());

                        this.gridMainDbCommand.CommandText = "INSERT INTO [dbo].[tblQAScoring] ([QID],[Value],[Score])" +
                                        " VALUES ('" + this.grdData.QuestionForm.MaxQID + "','" + this.grdData.QuestionForm.SelectionValue + "','" + this.grdData.QuestionForm.Score + "');";
                        try
                        {
                            this.gridMainDbCommand.ExecuteNonQuery();
                        }
                        catch (Exception)
                        {
                        }
                    }

                    foreach (DataRow row2 in dt2.Rows)
                    {
                        this.grdData.QuestionForm.MarkdownType = row2["Value"].ToString();

                        this.gridMainDbCommand.CommandText = "INSERT INTO [dbo].[tblQAMarkdown] ([QID],[Value])" +
                                        " VALUES ('" + this.grdData.QuestionForm.MaxQID + "','" + this.grdData.QuestionForm.MarkdownType + "');";
                        try
                        {
                            this.gridMainDbCommand.ExecuteNonQuery();
                        }
                        catch (Exception)
                        {
                        }
                    }

                }
                catch (Exception ex)
                {
                }
                this.CloseMainDbConnection();
            }
            this.grdMutexx.ReleaseMutex();

            return temp;
        }

        public bool UpdateQATemplate(int _LOBId, string _Category, string _QAQuestion, string _QAObjType, string _QARemarks)
        {
            bool temp = false;
            this.grdMutexx.WaitOne();

            if (this.OpenMainAHSQAConnection())
            {
                try
                {
                    this.gridMainDbCommand.Parameters.Clear();
                    this.gridMainDbCommand.Parameters.AddWithValue("@Id", this.grdData.QuestionForm.LOBId);
                    this.gridMainDbCommand.Parameters.AddWithValue("@FormId", this.grdData.QuestionForm.QID);
                    this.gridMainDbCommand.Parameters.AddWithValue("@Category", _Category);
                    this.gridMainDbCommand.Parameters.AddWithValue("@Question", _QAQuestion);
                    this.gridMainDbCommand.Parameters.AddWithValue("@ObjType", _QAObjType);
                    this.gridMainDbCommand.Parameters.AddWithValue("@Remarks", _QARemarks);
                    this.gridMainDbCommand.Parameters.AddWithValue("@ModifiedBy", this.grdData.CurrentUser.EID);
                    this.gridMainDbCommand.Parameters.AddWithValue("@ModifiedDate", this.ConvertTimeZone(grdData.TeamInfo.OffSet));
                    this.gridMainDbCommand.Parameters.AddWithValue("@Status", true);

                    this.gridMainDbCommand.CommandText = "UPDATE [dbo].[tblQAQuestions] SET [Category]=@Category,[Question]=@Question,[ObjectType]=@ObjType," +
                                "[Remarks]=@Remarks,[ModifiedBy]=@ModifiedBy,[ModifiedDate]=ModifiedDate,[Status]=@Status WHERE [Id]=@Id AND FormId=@FormId;";

                    try
                    {
                        if (this.gridMainDbCommand.ExecuteNonQuery() > 0)
                            temp = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                catch (Exception ex)
                {
                }
                this.CloseMainDbConnection();
            }
            this.grdMutexx.ReleaseMutex();

            return temp;

        }

        public bool AddScore(int _QID, string _SValue, int _SScore)
        {
            bool temp = false;
            this.grdMutexx.WaitOne();

            if (this.OpenMainAHSQAConnection())
            {
                try
                {
                    this.gridMainDbCommand.Parameters.Clear();
                    this.gridMainDbCommand.Parameters.AddWithValue("@QID", _QID);
                    this.gridMainDbCommand.Parameters.AddWithValue("@Value", _SValue);
                    this.gridMainDbCommand.Parameters.AddWithValue("@Score", _SScore);

                    this.gridMainDbCommand.CommandText = "INSERT INTO [dbo].[tblQAScoring] ([QID],[Value],[Score])" +
                                    " VALUES (@QID,@Value,@Score);";
                    try
                    {
                        this.gridMainDbCommand.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                    }

                }
                catch (Exception)
                {
                }
                this.CloseMainDbConnection();
            }
            this.grdMutexx.ReleaseMutex();
            return temp;
        }

        public bool AddMarkdown(int _QID, string _MValue)
        {
            bool temp = false;
            this.grdMutexx.WaitOne();

            if (this.OpenMainAHSQAConnection())
            {
                try
                {
                    this.gridMainDbCommand.Parameters.Clear();
                    this.gridMainDbCommand.Parameters.AddWithValue("@QID", _QID);
                    this.gridMainDbCommand.Parameters.AddWithValue("@Value", _MValue);

                    this.gridMainDbCommand.CommandText = "INSERT INTO [dbo].[tblQAMarkdown] ([QID],[Value])" +
                                    " VALUES (@QID,@Value);";
                    try
                    {
                        this.gridMainDbCommand.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                    }

                }
                catch (Exception)
                {
                }
                this.CloseMainDbConnection();
            }
            this.grdMutexx.ReleaseMutex();
            return temp;
        }

        public bool DeleteScore(int _Id)
        {
            bool temp = false;
            this.grdMutexx.WaitOne();

            if (this.OpenMainAHSQAConnection())
            {
                try
                {
                    this.gridMainDbCommand.Parameters.Clear();
                    this.gridMainDbCommand.Parameters.AddWithValue("@Id", _Id);
                    this.gridMainDbCommand.CommandText = "DELETE FROM [dbo].[tblQAScoring] WHERE [Id]=@Id";
                    if (this.gridMainDbCommand.ExecuteNonQuery() > 0)
                        temp = true;

                }
                catch (Exception)
                {
                }
                this.CloseMainDbConnection();
            }
            this.grdMutexx.ReleaseMutex();
            return temp;
        }

        public bool DeleteMarkdown(int _Id)
        {
            bool temp = false;
            this.grdMutexx.WaitOne();

            if (this.OpenMainAHSQAConnection())
            {
                try
                {
                    this.gridMainDbCommand.Parameters.Clear();
                    this.gridMainDbCommand.Parameters.AddWithValue("@Id", _Id);
                    this.gridMainDbCommand.CommandText = "DELETE FROM [dbo].[tblQAMarkdown] WHERE [Id]=@Id";
                    if (this.gridMainDbCommand.ExecuteNonQuery() > 0)
                        temp = true;

                }
                catch (Exception)
                {
                }
                this.CloseMainDbConnection();
            }
            this.grdMutexx.ReleaseMutex();
            return temp;
        }

        public bool DeleteQATemplate(int _QID)
        {
            bool temp = false;
            this.grdMutexx.WaitOne();

            if (this.OpenMainAHSQAConnection())
            {
                try
                {
                    this.gridMainDbCommand = new SqlCommand("sp_QA_DelTemplate", gridMainDbConnection);
                    this.gridMainDbCommand.CommandType = CommandType.StoredProcedure;
                    this.gridMainDbCommand.Parameters.AddWithValue("@QID", _QID);
                    this.gridMainDbCommand.ExecuteNonQuery();                                            
                }
                catch (Exception)
                {
                }
                this.CloseMainDbConnection();
            }
            this.grdMutexx.ReleaseMutex();
            return temp;
        }
    }
}


        
