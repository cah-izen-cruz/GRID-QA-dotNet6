﻿using GRIDLibraries.Libraries;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic.CompilerServices;
using System.Windows.Threading;
using Microsoft.VisualBasic;
using System;
using Telerik.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using SharpDX.Direct3D10;
using ToastNotifications.Messages;
using ToastNotifications.Core;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;
using SharpDX.Direct3D9;
using Telerik.Windows.Diagrams.Core;
using Convert = System.Convert;
using System.Data;
using System.Windows.Data;
using static GRID.MessagesBox;

namespace GRID.Pages
{
    /// <summary>
    /// Interaction logic for MyActivities.xaml
    /// </summary>
    public partial class MyActivities : Page
    {
        GridLib grd = new GridLib();

        MainWindow MainScrn = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

        gridActivity curAct = default;

        DataTable dtCompleted;

        private long PrevActId = 0L;

        //public DispatcherTimer ActivityElapsedTimer;

        private DispatcherTimer ActivityElapsedTimer;

        public bool IsMyActivitiesActivated = false;
        public bool ReStartActivity = false;

        long OvertimeCounter = 0L;

        public int idleCtr;
        public MyActivities()
        {
            InitializeComponent();



            //notifier.ShowInformation("Info");
            //notifier.ShowSuccess("Success");
            //notifier.ShowWarning("Warning");
            //notifier.ShowError("Error");

            MainScrn.ActivityElapsedTimer.Start();

            grd.grdData.TeamInfo.DBName = grd.grdData.TeamInfo.DBName;

            grd.conString = "Data Source=WPEC5009GRDRP01;" + "Initial Catalog=" + grd.grdData.TeamInfo.DBName + ";" + "Persist Security Info=True;" + "Integrated Security=SSPI;" + "Connect Timeout=3000;";

            lvMyActivities.ItemsSource = null;
            GvProductivity.ItemsSource = null;

            GvProductivity.ItemsSource = grd.grdData._lstProductivityOrig;
            lvMyActivities.ItemsSource = grd.grdData._lstMyActivitiesOrig;

            this.PopulateProcessAndSubProcessMyActivities();
            this.PopulateProcessAndSubProcessProductivity();

            this.MainScrn.DashMyActivities.Visibility = Visibility.Visible;

            if (grd.grdData.ScrContent.IsActivityRunning)
            {
                grd.grdData.ScrContent.IsMyDataChanged = true;
                btnStart.Visibility = Visibility.Collapsed;
                btnStop.Visibility = Visibility.Visible;
                btnPause.Visibility = Visibility.Visible;
                btnChange.Visibility = Visibility.Visible;

                if (grd.grdData.ScrContent.IsBreakStarted)
                {

                    this.GridDynamicObjects.Visibility = Visibility.Collapsed;
                    this.WrapPanelMain.Visibility = Visibility.Collapsed;
                    this.WrapPanelMain2.Visibility = Visibility.Collapsed;
                    btnCloseMyAct.Visibility = Visibility.Collapsed;
                    WrapActivityList.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btnCloseMyAct.Visibility = Visibility.Collapsed;
                    WrapActivityList.Visibility = Visibility.Collapsed;

                    this.GridDynamicObjects.Visibility = Visibility.Collapsed;
                    this.WrapPanelMain.Visibility = Visibility.Collapsed;
                    this.WrapPanelMain2.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                grd.grdData.ScrContent.IsMyDataChanged = false;
                btnStart.Visibility = Visibility.Visible;
                btnStop.Visibility = Visibility.Collapsed;
                btnPause.Visibility = Visibility.Collapsed;
                btnChange.Visibility = Visibility.Collapsed;

                this.GridDynamicObjects.Visibility = Visibility.Collapsed;
                this.WrapPanelMain.Visibility = Visibility.Collapsed;
                this.WrapPanelMain2.Visibility = Visibility.Collapsed;

                btnCloseMyAct.Visibility = Visibility.Collapsed;
                WrapActivityList.Visibility = Visibility.Collapsed;
            }

            ActivityElapsedTimer = new DispatcherTimer();
            ActivityElapsedTimer.Tick += ElapsedTimer_Tick;
            ActivityElapsedTimer.Interval = new TimeSpan(0, 0, 1);

        }

        Notifier notifier = new Notifier(msg =>
        {
            msg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.TopRight,
                offsetX: 10,
                offsetY: 10);   

            msg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(3),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            msg.Dispatcher = Application.Current.Dispatcher;

            
        });

        #region "Populate Process and Subprocess combo boxes"
        private void PopulateProcessAndSubProcessMyActivities() // 11-13
        {

            // ======= My Activities ============
            var lstProcess = new List<string>();

            foreach (var act in grd.grdData._lstMyActivitiesOrig)


                lstProcess.Add(Conversions.ToString(act.Process));

            lstProcess = lstProcess.Distinct().ToList();

            cmbProcess.Items.Clear();

            foreach (var proc in lstProcess)
                cmbProcess.Items.Add(proc);


        }

        private void PopulateProcessAndSubProcessProductivity() // 11-13
        {

            var lstProcess = new List<string>();

            foreach (var act in grd.grdData._lstProductivityOrig)

                lstProcess.Add(Conversions.ToString(act.Process));

            lstProcess = lstProcess.Distinct().ToList();

            cmbProcessProd.Items.Clear();

            foreach (var proc in lstProcess)
                cmbProcessProd.Items.Add(proc);

        }

        #endregion

        #region "Event Handlers"
        private void MainWindowActivated()
        {
            if (grd.grdData.CurrentActivity.Id <= 0)
            {

                switch (grd.grdData.MainWindowAction)
                {

                    case "START":
                        {

                            try
                            {
                                this.StartActivity();
                            }

                            catch (Exception ex)
                            {
                                _eMsgBox(ex.Message);
                            }

                            break;
                        }
                    case "STOP":
                        {

                            try
                            {
                                //this.ResumeActivity();
                            }

                            catch (Exception ex)
                            {
                                _eMsgBox(ex.Message);
                            }

                            break;
                        }

                    case "PAUSE":
                        {

                            try
                            {
                                //this.ResumeActivity();
                            }

                            catch (Exception ex)
                            {
                                _eMsgBox(ex.Message);
                            }

                            break;
                        }

                    case "RESUME":
                        {

                            try
                            {
                                this.ResumeActivity();
                            }

                            catch (Exception ex)
                            {
                                _eMsgBox(ex.Message);
                            }

                            break;
                        }

                    default:
                        {

                            return;
                        }

                }
            }

            else if (grd.grdData.CurrentActivity.Started == true & grd.grdData.CurrentActivity.Id > 0)
            {


                switch (grd.grdData.MainWindowAction)
                {

                    case "CHANGE":
                        {

                            try
                            {
                                this.ChangeActivity();
                            }
                            catch (Exception ex)
                            {
                                _eMsgBox(ex.Message);
                            }

                            break;
                        }

                    case "RESUME":
                        {

                            try
                            {
                                this.ResumeActivity();
                            }

                            catch (Exception ex)
                            {
                                _eMsgBox(ex.Message);
                            }

                            break;
                        }

                    default:
                        {

                            return;
                        }

                }


            }
        }
        #endregion

        #region "MainEvents"
        private void ClickGotoActivityList(object sender, RoutedEventArgs e)
        {


            if (grd.grdData.CurrentActivity.Id > 0)
            {
                Interaction.MsgBox("There's a running activity. Please pause if you want to start another.", Constants.vbInformation, grd.AppName);
                grd.grdData.MainWindowAction = "";
                return;
            }


            try
            {
                //GoToActivitityList();
            }
            catch (Exception ex)
            {
                _eMsgBox("clickGotoActivityList ", ex.Message);
            }
        }
        #endregion

        #region "MAIN PROCESS"
        public void StartActivity()
        {
            grd.grdData.MainWindowAction = "";

            this.ClearWrapPanel();

            idleCtr = 0;

            var newPerf = new gridPerformance();

            newPerf.Id = 1;
            newPerf.UserId = grd.grdData.CurrentUser.EmpNo;
            newPerf.TransDate = Convert.ToDateTime(grd.grdData.CurrentUser.TransactionDate2).ToString("MM/dd/yyyy");
            newPerf.ActivityId = grd.grdData.CurrentActivity.ActivityId;
            newPerf.TimeStart = Strings.Format(grd.grdData.CurrentActivity.TimeStart2, "MM/dd/yyyy h:mm:ss tt").ToString();
            newPerf.TimeEnd = Strings.Format(grd.grdData.CurrentActivity.TimeEnd2, "MM/dd/yyyy h:mm:ss tt").ToString();
            newPerf.TransDate2 = grd.grdData.CurrentUser.TransactionDate2; //grd.grdData.CurrentUser.TransactionDate2;
            newPerf.TimeStart2 = grd.grdData.CurrentActivity.TimeStart2;
            newPerf.TimeEnd2 = grd.grdData.CurrentActivity.TimeEnd2;
            newPerf.TimeElapsed = Strings.LTrim(Strings.RTrim((string)MainScrn.lblTimeElapsed.Content));
            newPerf.IdleTime = grd.grdData.CurrentActivity.IdleTime;
            newPerf.Status = "0";
            newPerf.IsPaused = false;
            newPerf.ReferenceId = grd.grdData.CurrentActivity.ReferenceId;
            newPerf.PerfStatus = 0;


            if (!(grd.grdData.CurrentActivity.Activity.ConfigInfo == null))
            {
                if (grd.grdData.CurrentActivity.Activity.ConfigInfo.Count > 0)
                {
                    this.SetupDynamicConfigInfo();
                }
            }


            newPerf.Id = grd.AddPerformanceInfo(newPerf);

            if (newPerf.Id > 0)
            {
                try
                {
                    grd.grdData.PerformanceList.Add(newPerf);
                }
                catch (Exception ex)
                {
                }
                this.WrapActivityList.Visibility = Visibility.Hidden;
                this.btnCloseMyAct.Visibility = Visibility.Hidden;

                this.GridDynamicObjects.Visibility = Visibility.Visible;
                this.WrapPanelMain.Visibility = Visibility.Visible;
                this.WrapPanelMain2.Visibility = Visibility.Visible;
                this.WrapPanelMain.Width = 380;
                this.WrapPanelMain2.Width = 380;

            }

            grd.grdData.CurrentActivity.Id = newPerf.Id;

            MainScrn.lblActivityName.Content = grd.grdData.CurrentActivity.Activity.ActName;
            MainScrn.lblAHT.Content = grd.grdData.CurrentActivity.Activity.AHT;
            MainScrn.lblProcessName.Content = grd.grdData.CurrentActivity.Activity.Process;
            MainScrn.lblStartTime.Content = grd.grdData.CurrentActivity.TimeStart2.ToLongTimeString();
            MainScrn.MinuteTimer = 0;
            idleCtr = 0;

            notifier.ShowInformation("  Your Activity Started @ " + this.MainScrn.lblStartTime.Content);

        }

        public void StartSameActivity()
        {
            grd.grdData.MainWindowAction = "";

            this.ClearWrapPanel();

            idleCtr = 0;

            grd.grdData.CurrentActivity.ActivityId = (int)PrevActId;
            grd.grdData.CurrentActivity.Activity = null;

            var q = from p in grd.grdData.ActivityList
                    where p.Id == PrevActId
                    select p;

            if (!(q == null))
            {
                if (q.Count() > 0)
                {
                    grd.grdData.CurrentActivity.Activity = q.First();
                }
            }


            var newPerf = new gridPerformance();

            newPerf.Id = 1;
            newPerf.UserId = grd.grdData.CurrentUser.EmpNo;
            newPerf.TransDate = Strings.Format(grd.grdData.CurrentUser.TransactionDate2, "MM/dd/yyyy").ToString();
            newPerf.ActivityId = grd.grdData.CurrentActivity.ActivityId;
            newPerf.TimeStart = Strings.Format(grd.grdData.CurrentActivity.TimeStart2, "MM/dd/yyyy h:mm:ss tt").ToString();
            newPerf.TimeEnd = Strings.Format(grd.grdData.CurrentActivity.TimeEnd2, "MM/dd/yyyy h:mm:ss tt").ToString();
            newPerf.TransDate2 = grd.grdData.CurrentUser.TransactionDate2;
            newPerf.TimeStart2 = grd.grdData.CurrentActivity.TimeStart2;
            newPerf.TimeEnd2 = grd.grdData.CurrentActivity.TimeEnd2;

            newPerf.TimeElapsed = Strings.LTrim(Strings.RTrim((string)MainScrn.lblTimeElapsed.Content));
            newPerf.IdleTime = grd.grdData.CurrentActivity.IdleTime;
            newPerf.Status = "0";
            newPerf.IsPaused = false;

            newPerf.ReferenceId = "";
            newPerf.PerfStatus = 0;


            if (!(grd.grdData.CurrentActivity.Activity.ConfigInfo == null))
            {
                if (grd.grdData.CurrentActivity.Activity.ConfigInfo.Count > 0)
                {
                    this.SetupDynamicConfigInfo();
                }
            }



            for (int z = 0; z <= 4; z++)
            {

                if (newPerf.Id > 1)
                {
                    break;
                }
                else
                {
                    newPerf.Id = grd.AddPerformanceInfo(newPerf);
                }

                if (newPerf.Id > 1)
                {
                    break;
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                }

            }


            if (newPerf.Id > 0)
            {


                try
                {
                    grd.grdData.PerformanceList.Add(newPerf);
                }
                catch (Exception ex)
                {
                }



                //if (grd.MAddPerformanceLocal(newPerf) == false)
                //{

                //    this.WrapPanelMain.Children.Clear();
                //    this.WrapPanelMain2.Children.Clear();
                //    grd.grdData.CurrentActivity.Reset();
                //    this.ResetValues();
                //    Interaction.MsgBox("Cannot start activity. Unable to connect to local database, please try again.", Constants.vbExclamation, grd.AppName);
                //    return;
                //}
            }
            else
            {

                this.WrapPanelMain.Children.Clear();
                this.WrapPanelMain2.Children.Clear();
                grd.grdData.CurrentActivity.Reset();
                this.ResetValues();
                Interaction.MsgBox("Cannot start activity. Unable to connect to the server, please check LAN connection.", Constants.vbExclamation, grd.AppName);
                return;

            }


            this.WrapActivityList.Visibility = Visibility.Collapsed;
            this.btnCloseMyAct.Visibility = Visibility.Collapsed;


            this.GridDynamicObjects.Visibility = Visibility.Visible;
            this.WrapPanelMain.Visibility = Visibility.Visible;
            this.WrapPanelMain2.Visibility = Visibility.Visible;
            this.WrapPanelMain.Width = 380;
            this.WrapPanelMain2.Width = 380;



            grd.grdData.CurrentActivity.Id = newPerf.Id;


            MainScrn.lblActivityName.Content = grd.grdData.CurrentActivity.Activity.ActName;
            MainScrn.lblAHT.Content = grd.grdData.CurrentActivity.Activity.AHT;
            MainScrn.lblProcessName.Content = grd.grdData.CurrentActivity.Activity.Process;
            MainScrn.lblStartTime.Content = grd.grdData.CurrentActivity.TimeStart2.ToLongTimeString();
            MainScrn.MinuteTimer = 0;

            this.MainScrn.MinuteTimer = 0;

            this.changelblbg();

            idleCtr = 0;

        }

        public void StopActivity()
        {

            if (!(grd.grdData.CurrentActivity.Id > 0))
            {
                new MessagesBox("You have no running activity To Stop.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                //Interaction.MsgBox("You have no running activity To Stop.", Constants.vbExclamation, grd.AppName);
                return;
            }

            var curStopPerf = new gridPerformance();
            curStopPerf.PerfConfigData = null;

            var withBlock = curStopPerf;
            withBlock.Id = grd.grdData.CurrentActivity.Id;
            withBlock.UserId = grd.grdData.CurrentUser.EmpNo;
            withBlock.ActivityId = grd.grdData.CurrentActivity.ActivityId;
            withBlock.TransDate = grd.grdData.CurrentUser.TransactionDate;
            withBlock.TimeStart = grd.grdData.CurrentActivity.TimeStart;
            withBlock.TimeEnd = grd.grdData.CurrentActivity.TimeEnd;
            withBlock.TransDate2 = grd.grdData.CurrentUser.TransactionDate2;
            withBlock.TimeStart2 = grd.grdData.CurrentActivity.TimeStart2;
            withBlock.TimeEnd2 = grd.grdData.CurrentActivity.TimeEnd2;
            withBlock.TimeElapsed = (string)MainScrn.lblTimeElapsed.Content;
            withBlock.IdleTime = grd.grdData.CurrentActivity.IdleTime;
            withBlock.Status = "2";
            withBlock.IsPaused = grd.grdData.CurrentActivity.IsPaused;
            withBlock.PerfStatus = 2;
            withBlock.ReferenceId = grd.grdData.CurrentActivity.ReferenceId;



            if (!(grd.grdData.CurrentActivity.Activity.ConfigInfo == null))
            {
                if (grd.grdData.CurrentActivity.Activity.ConfigInfo.Count > 0)
                {
                    if (CheckDynamicConfigData() == false)
                        return;

                    curStopPerf.PerfConfigData = this.GetDynamicConfigData();
                    curStopPerf.PerfConfigData.PerfId = curStopPerf.Id;

                }
            }

            if (grd.MEditPerformanceMain(curStopPerf))
            {
                try
                {
                    var tempPerf = new gridPerformance();
                    var q = from p in grd.gridData.PerformanceList
                            where p.Id == curStopPerf.Id
                            select p;

                    if (!(q == null))
                    {
                        if (q.Count() > 0)
                        {
                            tempPerf = q.First();
                            grd.grdData.PerformanceList.Remove(tempPerf);
                        }
                    }

                    grd.grdData.PerformanceList.Add(curStopPerf); //-look up ng open and completed
                }

                catch (Exception ex)
                {
                }

                //grd.MEditPerformanceLocal(curStopPerf);

                //if (grd.MUpdatePerfInfoLocal((int)curStopPerf.Id, curStopPerf.PerfConfigData))
                //{
                grd.MUpdatePerfInfoMain((int)curStopPerf.Id, curStopPerf.PerfConfigData);
                //}
            }
            else
            {
                new MessagesBox("Activity was Not successfully saved. Please Try again", MessageType.Warning, MessageButtons.Ok).ShowDialog();

                //Interaction.MsgBox("Activity was Not successfully saved. Please Try again", Constants.vbInformation, grd.AppName);

                grd.grdData.MainWindowAction = "";

                return;
            }

            if (grd.grdData.CurrentActivity.Activity.Type.ToUpper() == "PRODUCTIVE" & grd.grdData.CurrentActivity.Activity.IsPublic)
            {
                PrevActId = grd.grdData.CurrentActivity.Activity.Id;
            }
            else
            {
                PrevActId = 0;
            }

            this.ClearWrapPanel();

            grd.grdData.CurrentActivity.Reset();
            this.ResetValues();

            MainScrn.ActivityElapsedTimer.Stop();

            grd.grdData.CurrentActivity.TimeStart = curStopPerf.TimeEnd; // timeEndNow
            grd.grdData.CurrentActivity.TimeEnd = curStopPerf.TimeEnd;
            grd.grdData.CurrentActivity.TimeStart2 = curStopPerf.TimeEnd2;
            grd.grdData.CurrentActivity.TimeEnd2 = curStopPerf.TimeEnd2;
            grd.grdData.CurrentActivity.TransDate2 = curStopPerf.TransDate2;

            MainScrn.lblStartTime.Content = grd.grdData.CurrentActivity.TimeStart2.ToLongTimeString();
            grd.grdData.CurrentActivity.Id = 0;

            grd.grdData.CurrentActivity.IsPaused = false;
            grd.grdData.CurrentActivity.IdleTime = 0;
            grd.grdData.CurrentActivity.Started = false;
            MainScrn.lblTimeElapsed.Content = "00:00:00";
            MainScrn.MinuteTimer = 0;

            MainScrn.ActivityElapsedTimer.Start();

            grd.gridData.MainWindowAction = "";

            this.changelblbg();

            grd.grdData.CurrentUser.LogOut = grd.grdData.CurrentActivity.TimeEnd2;

            this.btnCloseMyAct.Visibility = Visibility.Visible;
            this.GridDynamicObjects.Visibility = Visibility.Collapsed;
            this.WrapPanelMain.Visibility = Visibility.Collapsed;
            this.WrapPanelMain2.Visibility = Visibility.Collapsed;
            this.WrapActivityList.Visibility = Visibility.Visible;

            btnCloseMyAct.Visibility = Visibility.Visible;

            btnStop.Visibility = Visibility.Collapsed;
            btnPause.Visibility = Visibility.Collapsed;
            btnChange.Visibility = Visibility.Collapsed;

            grd.grdData.CurrentActivity.Started = false;
            this.MainScrn.PopulateAgentMetrics();

            notifier.ShowSuccess("Activity successfully saved!");

            new MessagesBox("Activity Successfully Saved.", MessageType.Success, MessageButtons.Ok).ShowDialog();


            idleCtr = 0;

            if (PrevActId > 0)
            {
                //var ans2 = Interaction.MsgBox("Activity successfully saved!" + '\r' + '\r' + "Do you want to load the same activity?", Constants.vbYesNo, grd.AppName);
                


                //var ans2 = Interaction.MsgBox("Do you want to load the same activity?", Constants.vbYesNo, grd.AppName);

                bool? Result = new MessagesBox("Do you want to work on the same activity?", MessageType.Confirmation, MessageButtons.YesNo).ShowDialog();
           
                if (Result.Value)
                {
                    this.StartSameActivity();
                }
                else
                {
                    //this.GoToActivitityList();
                    return;
                }




            }

        }

        public bool PauseActivity(bool _IsAuto)
        {
            bool PauseActivityRet = default;

            PauseActivityRet = false;

            grd.grdData.MainWindowAction = "";

            bool IsCommentsChanged = false;

            grd.grdData.MainWindowAction = "";


            string PrevActType = null;

            var curPausePerf = new gridPerformance();
            curPausePerf.PerfConfigData = null;
            curPausePerf.Id = grd.grdData.CurrentActivity.Id;
            curPausePerf.UserId = grd.grdData.CurrentUser.EmpNo;
            curPausePerf.TransDate = Strings.Format(grd.grdData.CurrentUser.TransactionDate2, "MM/dd/yyyy").ToString();
            curPausePerf.ActivityId = grd.grdData.CurrentActivity.ActivityId;
            curPausePerf.TimeStart = grd.grdData.CurrentActivity.TimeStart;
            curPausePerf.TimeEnd = Strings.Format(grd.grdData.CurrentActivity.TimeEnd2, "MM/dd/yyyy h:mm:ss tt").ToString();
            curPausePerf.TransDate2 = grd.grdData.CurrentUser.TransactionDate2;
            curPausePerf.TimeStart2 = grd.grdData.CurrentActivity.TimeStart2;
            curPausePerf.TimeEnd2 = grd.grdData.CurrentActivity.TimeEnd2;

            curPausePerf.TimeElapsed = Strings.LTrim(Strings.RTrim((string)MainScrn.lblTimeElapsed.Content));
            curPausePerf.IdleTime = grd.grdData.CurrentActivity.IdleTime;
            curPausePerf.Status = "1";
            curPausePerf.IsPaused = true;
            curPausePerf.PerfStatus = 1;


            if (!(grd.grdData.CurrentActivity.Activity.ConfigInfo == null))
            {
                if (grd.grdData.CurrentActivity.Activity.ConfigInfo.Count > 0)
                {
                    curPausePerf.PerfConfigData = this.GetDynamicConfigData();
                    curPausePerf.PerfConfigData.PerfId = curPausePerf.Id;
                }
            }


            if (grd.MEditPerformanceMain(curPausePerf))
            {
                try
                {
                    var tempPerf = new gridPerformance();
                    var q = from p in grd.grdData.PerformanceList
                            where p.Id == curPausePerf.Id
                            select p;

                    if (!(q == null))
                    {
                        if (q.Count() > 0)
                        {
                            tempPerf = q.First();
                            grd.grdData.PerformanceList.Remove(tempPerf);
                        }
                    }

                    grd.grdData.PerformanceList.Add(curPausePerf);
                }

                catch (Exception ex)
                {
                }

                grd.MUpdatePerfInfoMain(Convert.ToInt32(curPausePerf.Id), curPausePerf.PerfConfigData);


                PrevActId = grd.grdData.CurrentActivity.Activity.Id;
                try
                {
                    PrevActType = grd.grdData.CurrentActivity.Activity.Type.ToUpper();
                }
                catch (Exception)
                {
                }

                PauseActivityRet = true;
            }


            else
            {

                if (_IsAuto == false)
                {
                    Interaction.MsgBox("Activity was not successfully Paused. Please try again", Constants.vbInformation, grd.AppName);
                }

                grd.grdData.MainWindowAction = "";
                return false;

            }

            this.ClearWrapPanel();


            grd.gridData.CurrentActivity.Reset();
            this.ResetValues();

            this.MainScrn.ActivityElapsedTimer.Stop();

            grd.gridData.LOBProcess = "";

      

            grd.grdData.CurrentActivity.TimeStart = Strings.Format(curPausePerf.TimeEnd2, "MM/dd/yyyy h:mm:ss tt").ToString();
            grd.grdData.CurrentActivity.TimeStart2 = curPausePerf.TimeEnd2;
            MainScrn.lblStartTime.Content = grd.grdData.CurrentActivity.TimeStart2.ToLongTimeString();
            grd.grdData.CurrentActivity.Id = 0;
            grd.grdData.CurrentActivity.Started = true;
            grd.grdData.CurrentActivity.IsPaused = false;
            grd.grdData.CurrentActivity.IdleTime = 0;

            MainScrn.lblTimeElapsed.Content = "00:00:00";
            MainScrn.MinuteTimer = 0;


            // grd.gridData.CurrentActivity.Activity.ConfigInfo = Nothing

            this.MainScrn.ActivityElapsedTimer.Start();
            this.MainScrn.lblTimeElapsed.Content = "Izen";



            this.changelblbg();

            grd.grdData.CurrentUser.LogOut = grd.grdData.CurrentActivity.TimeEnd2;


            this.MainScrn.PopulateAgentMetrics();

            idleCtr = 0;

            if (_IsAuto == false)
            {
                //this.GoToActivitityList();
            }

            return PauseActivityRet;
        }

        public void ResumeActivity()
        {

        }

        public void ChangeActivity()
        {

        }
        #endregion

        #region "Level4 SetupDynamicConfigInfo"

        private void SetupDynamicConfigInfo()
        {

            #region "Clean"
            this.WrapPanelMain.Children.Clear();
            this.WrapPanelMain2.Children.Clear();
            try
            {
                for (int i = 1; i <= 50; i++)
                {

                    object tempObj = this.FindName("objRF" + Strings.Format(i, "00"));
                    if (!(tempObj == null))
                    {
                        this.UnregisterName("objRF" + Strings.Format(i, "00"));
                    }

                    object tempObj2 = this.FindName("objRF" + Strings.Format(i, "00") + "2");
                    if (!(tempObj2 == null))
                    {
                        this.UnregisterName("objRF" + Strings.Format(i, "00") + "2");
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            #endregion

            int loopCtr = 0;

            for (int i = 0, loopTo = grd.grdData.CurrentActivity.Activity.ConfigInfo.Count - 1; i <= loopTo; i++)
            {
                loopCtr = loopCtr + 1;

                var x = new WrapPanel();
                x.Background = System.Windows.Media.Brushes.Transparent;
                x.Height = 26;
                x.Width = 380;
                x.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                x.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                x.Margin = new Thickness(0, 1, 0, 0);
                x.Orientation = System.Windows.Controls.Orientation.Horizontal;

                var y = new System.Windows.Controls.Label();
                y.Content = grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName;

                try
                {
                    if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].Desc != "")
                    {
                        if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].Desc.Length > 3)
                        {
                            y.ToolTip = grd.grdData.CurrentActivity.Activity.ConfigInfo[i].Desc;
                        }
                    }
                }
                catch (Exception ex)
                {
                }

                y.Width = 110;
                y.Height = 25;
                y.FontSize = 11;
                y.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
                y.Foreground = System.Windows.Media.Brushes.White;
                y.Margin = new Thickness(0, 0, 0, 0);
                y.VerticalAlignment = System.Windows.VerticalAlignment.Center;



                switch (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].ObjectType.ToUpper())
                {
                    #region "Textbox"
                    case "TEXTBOX":

                        var z = new Telerik.Windows.Controls.RadWatermarkTextBox(); // System.Windows.Controls.TextBox

                        z.Width = 200;
                        z.Height = 25;
                        z.FontSize = 11;
                        z.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
                        z.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                        z.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        z.Foreground = System.Windows.Media.Brushes.Black;
                        z.Background = System.Windows.Media.Brushes.White;



                        StyleManager.SetTheme(z, new Expression_DarkTheme());

                        z.Margin = new Thickness(0, 1, 1, 1);

                        z.Name = GetObjectName(i);
                        this.RegisterName(z.Name, z);


                        if (grd.grdData.CurrentActivity.Activity.DailyScorecard == true)
                        {
                            y.Width = 225;
                            y.BorderThickness = new Thickness(0.5d, 0.5d, 0.5d, 0.5d);
                            y.BorderBrush = System.Windows.Media.Brushes.White;
                            y.Height = 22;
                            y.Margin = new Thickness(0, 1, 3, 1);
                            // y.Padding = New Thickness(1)
                            y.FontSize = 10;
                            y.Content = "  " + y.Content;

                            z.Width = 70;
                            z.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;

                        }




                        x.Children.Add(y);
                        x.Children.Add(z);


                        break;
                    #endregion

                    #region "DropDown"
                    case "DROPDOWN":
                        var dd = new Telerik.Windows.Controls.RadComboBox();

                        dd.Width = 200;
                        dd.Height = 25;
                        dd.FontSize = 11;
                        dd.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
                        dd.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                        dd.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        dd.Foreground = System.Windows.Media.Brushes.Black;

                        dd.Margin = new Thickness(0, 1, 1, 1);
                        dd.IsEditable = true;

                        dd.Name = GetObjectName(i);
                        this.RegisterName(dd.Name, dd);


                        x.Children.Add(y);
                        x.Children.Add(dd);

                        break;
                    #endregion

                    #region "Selectionlist"
                    case "SELECTIONLIST":
                        var sl = new System.Windows.Controls.ComboBox();

                        sl.Width = 200;
                        sl.Height = 25;
                        sl.FontSize = 11;
                        sl.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
                        sl.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                        sl.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        sl.Foreground = System.Windows.Media.Brushes.Black;
                        sl.Background = System.Windows.Media.Brushes.White;
                        sl.Margin = new Thickness(0, 1, 1, 1);
                        sl.IsEditable = false;
                        sl.Name = GetObjectName(i);

                        this.RegisterName(sl.Name, sl);


                        x.Children.Add(y);
                        x.Children.Add(sl);
                        break;
                    #endregion

                    #region "DatePicker"
                    case "DATEPICKER":
                        var dp = new RadDatePicker();

                        // Dim z As New RadDateTimePicker
                        // z.InputMode = InputMode.TimePicker

                        dp.Width = 100;
                        dp.Height = 25;
                        dp.FontSize = 11;
                        dp.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
                        dp.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                        dp.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        dp.Foreground = System.Windows.Media.Brushes.Black;
                        dp.Margin = new Thickness(0, 1, 1, 1);
                        dp.Name = GetObjectName(i);

                        try
                        {
                            if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].UseCurrentDate == true)
                            {
                                dp.SelectedDate = grd.ConvertTimeZone(grd.grdData.TeamInfo.OffSet);
                            }
                        }
                        catch (Exception ex)
                        {

                        }



                        this.RegisterName(dp.Name, dp);

                        x.Children.Add(y);
                        x.Children.Add(dp);
                        break;
                    #endregion

                    #region "TimePicker"
                    case "TIMEPICKER":
                        var rdtp = new RadDateTimePicker();
                        rdtp.InputMode = Telerik.Windows.Controls.InputMode.TimePicker;

                        rdtp.Width = 100;
                        rdtp.Height = 25;
                        rdtp.FontSize = 11;
                        rdtp.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
                        rdtp.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                        rdtp.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        rdtp.Foreground = System.Windows.Media.Brushes.Black;
                        rdtp.Margin = new Thickness(0, 1, 1, 1);
                        rdtp.Name = GetObjectName(i);

                        try
                        {
                            if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].UseCurrentDate == true)
                            {
                                rdtp.SelectedDate = grd.ConvertTimeZone(grd.grdData.TeamInfo.OffSet);
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        this.RegisterName(rdtp.Name, rdtp);

                        x.Children.Add(y);
                        x.Children.Add(rdtp);
                        break;
                    #endregion

                    #region "DateTimePicker"
                    case "DATETIMEPICKER":
                        var dtpkr = new RadDateTimePicker();
                        dtpkr.InputMode = Telerik.Windows.Controls.InputMode.DateTimePicker;

                        dtpkr.Width = 100;
                        dtpkr.Height = 25;
                        dtpkr.FontSize = 11;
                        dtpkr.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
                        dtpkr.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                        dtpkr.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        dtpkr.Foreground = System.Windows.Media.Brushes.Black;
                        dtpkr.Margin = new Thickness(0, 1, 1, 1);
                        dtpkr.Name = GetObjectName(i);

                        try
                        {
                            if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].UseCurrentDate == true)
                            {
                                dtpkr.SelectedDate = grd.ConvertTimeZone(grd.grdData.TeamInfo.OffSet);
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        this.RegisterName(dtpkr.Name, dtpkr);

                        x.Children.Add(y);
                        x.Children.Add(dtpkr);
                        break;
                    #endregion

                    #region "Yes/No"
                    case "YES/NO":
                        var cb = new System.Windows.Controls.ComboBox();

                        cb.Width = 200;
                        cb.Height = 25;
                        cb.FontSize = 11;
                        cb.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
                        cb.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                        cb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        cb.Foreground = System.Windows.Media.Brushes.Black;
                        cb.Margin = new Thickness(0, 1, 1, 1);
                        cb.IsEditable = false;
                        cb.Name = GetObjectName(i);
                        this.RegisterName(cb.Name, cb);

                        cb.Items.Add("Yes");
                        cb.Items.Add("No");


                        if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].IsRequired == true)
                        {
                            if (cb.Items.Count > 0)
                            {
                                cb.SelectedIndex = 0;
                            }
                        }

                        x.Children.Add(y);
                        x.Children.Add(cb);
                        break;
                    #endregion

                    #region "True/False"
                    case "TRUE/FALSE":
                        var cbtf = new System.Windows.Controls.ComboBox();

                        cbtf.Width = 200;
                        cbtf.Height = 25;
                        cbtf.FontSize = 11;
                        cbtf.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
                        cbtf.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                        cbtf.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        cbtf.Foreground = System.Windows.Media.Brushes.Black;
                        cbtf.Margin = new Thickness(0, 1, 1, 1);
                        cbtf.IsEditable = false;
                        cbtf.Name = GetObjectName(i);
                        this.RegisterName(cbtf.Name, cbtf);

                        cbtf.Items.Add("True");
                        cbtf.Items.Add("False");

                        if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].IsRequired == true)
                        {
                            if (cbtf.Items.Count > 0)
                            {
                                cbtf.SelectedIndex = 0;
                            }
                        }

                        x.Children.Add(y);
                        x.Children.Add(cbtf);
                        break;
                    #endregion

                    #region "Multiline"
                    case "MULTILINE":
                        //var rt = new Telerik.Windows.Controls.RadRichTextBox(); RichTextBox
                        var rt = new System.Windows.Controls.RichTextBox();
                        rt.Width = 200;
                        rt.Height = 60;
                        rt.FontSize = 10;

                        rt.Document.LineHeight = 1;
                        rt.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                        rt.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
                        rt.VerticalContentAlignment = System.Windows.VerticalAlignment.Top;
                        rt.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        rt.Foreground = System.Windows.Media.Brushes.Black;
                        rt.Background = System.Windows.Media.Brushes.White;
                        rt.Margin = new Thickness(0, 1, 1, 1);

                        rt.Name = GetObjectName(i);
                        this.RegisterName(rt.Name, rt);

                        //rt.TextChanged += RichTexbox_LostFocus;

                        x.Children.Add(y);
                        x.Children.Add(rt);
                        x.Height = 60;
                        break;
                        #endregion
                }


                //if (i == 0)
                //{
                //    if (grd.grdData.TeamInfo.ProjectId == 2)
                //    {

                //        if (grd.grdData.LOBProcess == "GMB" & grd.grdData.CurrentActivity.LOBItemId > 0)
                //        {



                //            var z5 = new Telerik.Windows.Controls.RadButton();

                //            z5.Width = 90;
                //            z5.Height = 27;
                //            z5.FontSize = 11;
                //            z5.Content = "Open Email";
                //            z5.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                //            z5.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                //            z5.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                //            z5.Foreground = System.Windows.Media.Brushes.Black;
                //            z5.Background = System.Windows.Media.Brushes.White;
                //            z5.Margin = new Thickness(5, 1, 1, 1);
                //            z5.BorderBrush = System.Windows.Media.Brushes.White;
                //            z5.BorderThickness = new Thickness(1);
                //            z5.Name = "btnOFOpenEmail";
                //            z5.CornerRadius = new CornerRadius(2);

                //            //z5.Click += btnOFOpenEmail_Click;


                //            this.RegisterName(z5.Name, z5);

                //            x.Children.Add(z5);


                //        }

                //    }


                //}

                if (loopCtr < 12)
                    this.WrapPanelMain.Children.Add(x);
                else
                    this.WrapPanelMain2.Children.Add(x);

            }
        }

        private gridPerfInfo GetDynamicConfigData()
        {

            grd.grdData.CurrentPerfInfo = new gridPerfInfo();
            grd.grdData.CurrentPerfInfo.Count = 0;

            for (int i = 0, loopTo = grd.grdData.CurrentActivity.Activity.ConfigInfo.Count - 1; i <= loopTo; i++)
            {
                grd.grdData.CurrentPerfInfo.Count = grd.grdData.CurrentActivity.Activity.ConfigInfo.Count;
                switch (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].ObjectType)
                {

                    case "Textbox":
                        {

                            System.Windows.Controls.TextBox tb = (TextBox)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
                            if (!(tb == null))
                            {
                                if (tb.Text == "" | tb.Text == null)
                                {
                                }
                                else
                                {
                                    PassValueToRF(i, tb.Text);
                                }
                            }

                            break;
                        }


                    case "Dropdown":
                        {

                            RadComboBox tb = (RadComboBox)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
                            if (!(tb == null))
                            {
                                if (tb.Text == "" | tb.Text == null)
                                {
                                }

                                else
                                {
                                    PassValueToRF(i, tb.Text);
                                }
                            }

                            break;
                        }

                    case "Selectionlist":
                        {

                            ComboBox tb = (ComboBox)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
                            if (!(tb == null))
                            {
                                if (tb.Text == "" | tb.Text == null)
                                {
                                }
                                else
                                {
                                    PassValueToRF(i, tb.Text);
                                }
                            }

                            break;
                        }

                    case "DatePicker":
                        {


                            RadDatePicker tb = (RadDatePicker)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
                            if (!(tb == null))
                            {
                                if (!Information.IsDate(tb.SelectedDate))
                                {
                                }
                                else
                                {
                                    PassValueToRF(i, Strings.Format(tb.SelectedDate, "MM/dd/yyyy"));
                                }
                            }

                            break;
                        }


                    case "TimePicker":
                        {

                            var tb = new RadDateTimePicker();

                            try
                            {
                                tb = (RadDateTimePicker)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
                            }
                            catch (Exception ex)
                            {

                            }

                            if (!(tb == null))
                            {
                                if (!Information.IsDate(tb.SelectedDate))
                                {
                                }
                                else
                                {
                                    PassValueToRF(i, Strings.Format(tb.SelectedDate, "hh:mm:ss AM/PM"));
                                }
                            }

                            break;
                        }

                    case "DateTimePicker":
                        {

                            var tb = new RadDateTimePicker();

                            try
                            {
                                tb = (RadDateTimePicker)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
                            }
                            catch (Exception ex)
                            {

                            }

                            if (!(tb == null))
                            {
                                if (!Information.IsDate(tb.SelectedDate))
                                {
                                }
                                else
                                {
                                    PassValueToRF(i, Strings.Format(tb.SelectedDate, "MM/dd/yyyy hh:mm:ss AM/PM"));
                                }
                            }

                            break;
                        }

                    case "Yes/No":
                        {

                            System.Windows.Controls.ComboBox tb = (ComboBox)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));

                            if (!(tb == null))
                            {
                                if (tb.Text == "" | tb.Text == null)
                                {
                                }
                                else
                                {
                                    PassValueToRF(i, tb.Text);
                                }
                            }

                            break;
                        }

                    case "True/False":
                        {

                            System.Windows.Controls.ComboBox tb = (ComboBox)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
                            if (!(tb == null))
                            {
                                if (tb.Text == "" | tb.Text == null)
                                {
                                }
                                else
                                {
                                    PassValueToRF(i, tb.Text);
                                }
                            }

                            break;
                        }

                    case "Multiline":
                        {

                            RichTextBox tb = (RichTextBox)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));

                            if (!(tb == null))
                            {


                                string richText = null;

                                try
                                {
                                    richText = new TextRange(tb.Document.ContentStart, tb.Document.ContentEnd).Text;


                                    if (richText.EndsWith(Constants.vbCrLf))
                                    {
                                        var oTrim = new char[] { Conversions.ToChar(Constants.vbCr), Conversions.ToChar(Constants.vbLf) };
                                        richText = richText.TrimEnd(oTrim);
                                    }


                                    if (richText.StartsWith(Constants.vbCrLf))
                                    {
                                        var oTrim = new char[] { Conversions.ToChar(Constants.vbCr), Conversions.ToChar(Constants.vbLf) };
                                        richText = richText.TrimStart(oTrim);
                                    }
                                }


                                catch (Exception ex)
                                {

                                }

                                if (string.IsNullOrEmpty(richText) | richText == null)
                                {
                                }

                                else
                                {
                                    PassValueToRF(i, richText);
                                }


                            }

                            break;
                        }

                }

            }

            return grd.grdData.CurrentPerfInfo;

        }

        private bool CheckDynamicConfigData()
        {

            //if (grd.grdData.LOBProcess.ToUpper() == "AtHomeBillingDiabetesGMB".ToUpper() & grd.grdData.CurrentActivity.LOBId == 94 & grd.grdData.CurrentActivity.LOBItemId > 0)
            //{
            //    //if (!(grd.grdData.CurrentActivity.AtHomeBillingDiabetesGMBEmailItem == null))
            //    //{
            //    //    if (grd.grdData.CurrentActivity.Activity.Id == grd.grdData.CurrentActivity.AtHomeBillingDiabetesGMBEmailItem.ActId)
            //    //    {
            //    if (grd.grdData.CurrentActivity.Activity.Id == 3033)
            //    {
            //        for (int i = 0, loopTo = grd.grdData.CurrentActivity.Activity.ConfigInfo.Count - 1; i <= loopTo; i++)
            //        {
            //            if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].Id == 3957)
            //            {
            //                System.Windows.Controls.ComboBox tb = (ComboBox)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
            //                if (!(tb == null))
            //                {
            //                    if (tb.Text.ToUpper().Contains("Inquiry".ToUpper()))
            //                    {
            //                        return true;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    //    }
            //    //}
            //}


            for (int i = 0, loopTo1 = grd.grdData.CurrentActivity.Activity.ConfigInfo.Count - 1; i <= loopTo1; i++)
            {


                switch (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].ObjectType.ToUpper())
                {

                    case var @case when @case == "Textbox".ToUpper():
                        {

                            System.Windows.Controls.TextBox tb = (TextBox)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
                            if (!(tb == null))
                            {



                                if (tb.Text == "" | tb.Text == null)
                                {

                                    if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].IsRequired == true)
                                    {
                                        new MessagesBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                        //Interaction.MsgBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", Constants.vbExclamation, grd.AppName);
                                        return false;
                                    }
                                }


                                else
                                {

                                    switch (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].DataType.ToUpper())
                                    {


                                        case "NUMBER":
                                            {

                                                if (!Information.IsNumeric(tb.Text))
                                                {
                                                    new MessagesBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                                    //Interaction.MsgBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + ": Invalid data type.", Constants.vbExclamation, grd.AppName);
                                                    return false;
                                                }

                                                break;
                                            }

                                        case "DATE":
                                            {

                                                if (!Information.IsDate(tb.Text))
                                                {
                                                    new MessagesBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                                    //Interaction.MsgBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + ": Invalid data type.", Constants.vbExclamation, grd.AppName);
                                                    return false;
                                                }

                                                break;
                                            }


                                    }


                                }


                            }

                            break;
                        }

                    case var case1 when case1 == "DROPDOWN".ToUpper():
                        {
                            ComboBox tb = (ComboBox)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
                            if (!(tb == null))
                            {
                                if (tb.Text == "" | tb.Text == null)
                                {

                                    if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].IsRequired == true)
                                    {
                                        new MessagesBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                        //Interaction.MsgBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", Constants.vbExclamation, grd.AppName);
                                        return false;
                                    }
                                }


                                else
                                {

                                    switch (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].DataType.ToUpper())
                                    {


                                        case "NUMBER":
                                            {

                                                if (!Information.IsNumeric(tb.Text))
                                                {
                                                    new MessagesBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                                    //Interaction.MsgBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + ": Invalid data type.", Constants.vbExclamation, grd.AppName);
                                                    return false;
                                                }

                                                break;
                                            }

                                        case "DATE":
                                            {

                                                if (!Information.IsDate(tb.Text))
                                                {
                                                    new MessagesBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                                    //Interaction.MsgBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + ": Invalid data type.", Constants.vbExclamation, grd.AppName);
                                                    return false;
                                                }

                                                break;
                                            }


                                    }


                                }
                            }

                            break;
                        }

                    case var case2 when case2 == "Selectionlist".ToUpper():
                        {

                            System.Windows.Controls.ComboBox tb = (ComboBox)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
                            if (!(tb == null))
                            {
                                if (tb.Text == "" | tb.Text == null)
                                {
                                    if (tb.Text == "" | tb.Text == null)
                                    {

                                        if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].IsRequired == true)
                                        {
                                            new MessagesBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                            //Interaction.MsgBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", Constants.vbExclamation, grd.AppName);
                                            return false;
                                        }
                                    }


                                    else
                                    {

                                        switch (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].DataType.ToUpper())
                                        {


                                            case "NUMBER":
                                                {

                                                    if (!Information.IsNumeric(tb.Text))
                                                    {
                                                        new MessagesBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                                        //Interaction.MsgBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + ": Invalid data type.", Constants.vbExclamation, grd.AppName);
                                                        return false;
                                                    }

                                                    break;
                                                }

                                            case "DATE":
                                                {

                                                    if (!Information.IsDate(tb.Text))
                                                    {
                                                        new MessagesBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                                        //Interaction.MsgBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + ": Invalid data type.", Constants.vbExclamation, grd.AppName);
                                                        return false;
                                                    }

                                                    break;
                                                }


                                        }


                                    }
                                }
                            }

                            break;
                        }

                    case var case3 when case3 == "DatePicker".ToUpper():
                        {

                            DatePicker tb = (DatePicker)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
                            if (!(tb == null))
                            {

                                if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].IsRequired == true)
                                {
                                    if (!Information.IsDate(tb.SelectedDate))
                                    {
                                        new MessagesBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                        //Interaction.MsgBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", Constants.vbExclamation, grd.AppName);
                                        return false;
                                    }
                                }

                            }

                            break;
                        }

                    case var case4 when case4 == "TimePicker".ToUpper():
                        {

                            var tb = new RadDateTimePicker();

                            try
                            {
                                tb = (RadDateTimePicker)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
                            }
                            catch (Exception ex)
                            {

                            }

                            if (!(tb == null))
                            {

                                if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].IsRequired == true)
                                {
                                    if (!Information.IsDate(tb.SelectedDate))
                                    {
                                        new MessagesBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                        //Interaction.MsgBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", Constants.vbExclamation, grd.AppName);
                                        return false;
                                    }
                                }

                            }

                            break;
                        }

                    case var case5 when case5 == "DateTimePicker".ToUpper():
                        {

                            var tb = new RadDateTimePicker();

                            try
                            {
                                tb = (RadDateTimePicker)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
                            }
                            catch (Exception ex)
                            {

                            }

                            if (!(tb == null))
                            {

                                if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].IsRequired == true)
                                {
                                    if (!Information.IsDate(tb.SelectedDate))
                                    {

                                        new MessagesBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                        // Interaction.MsgBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", Constants.vbExclamation, grd.AppName);
                                        return false;
                                    }
                                }

                            }

                            break;
                        }

                    case var case6 when case6 == "YES/NO".ToUpper():
                        {

                            System.Windows.Controls.ComboBox tb = (ComboBox)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
                            if (!(tb == null))
                            {
                                if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].IsRequired == true)
                                {
                                    if (tb.Text == "" | tb.Text == null)
                                    {

                                        new MessagesBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                        //Interaction.MsgBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", Constants.vbExclamation, grd.AppName);
                                        return false;
                                    }
                                }

                            }

                            break;
                        }

                    case var case7 when case7 == "True/False".ToUpper():
                        {

                            System.Windows.Controls.ComboBox tb = (ComboBox)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
                            if (!(tb == null))
                            {
                                if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].IsRequired == true)
                                {
                                    if (tb.Text == "" | tb.Text == null)
                                    {
                                        new MessagesBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                        //Interaction.MsgBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", Constants.vbExclamation, grd.AppName);
                                        return false;
                                    }
                                }

                            }

                            break;
                        }

                    case var case8 when case8 == "Multiline".ToUpper():
                        {

                            System.Windows.Controls.RichTextBox tb = (RichTextBox)this.FindName("objRF" + Strings.Format(Operators.AddObject(i, 1), "00"));
                            if (!(tb == null))
                            {
                                if (grd.grdData.CurrentActivity.Activity.ConfigInfo[i].IsRequired == true)
                                {

                                    string richText = null;

                                    try
                                    {
                                        richText = new TextRange(tb.Document.ContentStart, tb.Document.ContentEnd).Text;
                                    }
                                    catch (Exception ex)
                                    {

                                    }


                                    if (string.IsNullOrEmpty(richText) | richText == null)
                                    {
                                        new MessagesBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                        //Interaction.MsgBox(grd.grdData.CurrentActivity.Activity.ConfigInfo[i].FieldName + " is required.", Constants.vbExclamation, grd.AppName);
                                        return false;
                                    }
                                }

                            }

                            break;
                        }

                }


            }



            return true;

        }

        private void PassValueToRF(int ctr, string _Value)
        {

            switch (ctr + 1)
            {

                case 1:
                    {
                        grd.grdData.CurrentPerfInfo.RF01 = _Value;
                        break;
                    }
                case 2:
                    {
                        grd.grdData.CurrentPerfInfo.RF02 = _Value;
                        break;
                    }
                case 3:
                    {
                        grd.grdData.CurrentPerfInfo.RF03 = _Value;
                        break;
                    }
                case 4:
                    {
                        grd.grdData.CurrentPerfInfo.RF04 = _Value;
                        break;
                    }
                case 5:
                    {
                        grd.grdData.CurrentPerfInfo.RF05 = _Value;
                        break;
                    }
                case 6:
                    {
                        grd.grdData.CurrentPerfInfo.RF06 = _Value;
                        break;
                    }
                case 7:
                    {
                        grd.grdData.CurrentPerfInfo.RF07 = _Value;
                        break;
                    }
                case 8:
                    {
                        grd.grdData.CurrentPerfInfo.RF08 = _Value;
                        break;
                    }
                case 9:
                    {
                        grd.grdData.CurrentPerfInfo.RF09 = _Value;
                        break;
                    }
                case 10:
                    {
                        grd.grdData.CurrentPerfInfo.RF10 = _Value;
                        break;
                    }

                case 11:
                    {
                        grd.grdData.CurrentPerfInfo.RF11 = _Value;
                        break;
                    }
                case 12:
                    {
                        grd.grdData.CurrentPerfInfo.RF12 = _Value;
                        break;
                    }
                case 13:
                    {
                        grd.grdData.CurrentPerfInfo.RF13 = _Value;
                        break;
                    }
                case 14:
                    {
                        grd.grdData.CurrentPerfInfo.RF14 = _Value;
                        break;
                    }
                case 15:
                    {
                        grd.grdData.CurrentPerfInfo.RF15 = _Value;
                        break;
                    }
                case 16:
                    {
                        grd.grdData.CurrentPerfInfo.RF16 = _Value;
                        break;
                    }
                case 17:
                    {
                        grd.grdData.CurrentPerfInfo.RF17 = _Value;
                        break;
                    }
                case 18:
                    {
                        grd.grdData.CurrentPerfInfo.RF18 = _Value;
                        break;
                    }
                case 19:
                    {
                        grd.grdData.CurrentPerfInfo.RF19 = _Value;
                        break;
                    }
                case 20:
                    {
                        grd.grdData.CurrentPerfInfo.RF20 = _Value;
                        break;
                    }

                case 21:
                    {
                        grd.grdData.CurrentPerfInfo.RF21 = _Value;
                        break;
                    }
                case 22:
                    {
                        grd.grdData.CurrentPerfInfo.RF22 = _Value;
                        break;
                    }
                case 23:
                    {
                        grd.grdData.CurrentPerfInfo.RF23 = _Value;
                        break;
                    }
                case 24:
                    {
                        grd.grdData.CurrentPerfInfo.RF24 = _Value;
                        break;
                    }
                case 25:
                    {
                        grd.grdData.CurrentPerfInfo.RF25 = _Value;
                        break;
                    }
                case 26:
                    {
                        grd.grdData.CurrentPerfInfo.RF26 = _Value;
                        break;
                    }
                case 27:
                    {
                        grd.grdData.CurrentPerfInfo.RF27 = _Value;
                        break;
                    }
                case 28:
                    {
                        grd.grdData.CurrentPerfInfo.RF28 = _Value;
                        break;
                    }
                case 29:
                    {
                        grd.grdData.CurrentPerfInfo.RF29 = _Value;
                        break;
                    }
                case 30:
                    {
                        grd.grdData.CurrentPerfInfo.RF30 = _Value;
                        break;
                    }

                case 31:
                    {
                        grd.grdData.CurrentPerfInfo.RF31 = _Value;
                        break;
                    }
                case 32:
                    {
                        grd.grdData.CurrentPerfInfo.RF32 = _Value;
                        break;
                    }
                case 33:
                    {
                        grd.grdData.CurrentPerfInfo.RF33 = _Value;
                        break;
                    }
                case 34:
                    {
                        grd.grdData.CurrentPerfInfo.RF34 = _Value;
                        break;
                    }
                case 35:
                    {
                        grd.grdData.CurrentPerfInfo.RF35 = _Value;
                        break;
                    }
                case 36:
                    {
                        grd.grdData.CurrentPerfInfo.RF36 = _Value;
                        break;
                    }
                case 37:
                    {
                        grd.grdData.CurrentPerfInfo.RF37 = _Value;
                        break;
                    }
                case 38:
                    {
                        grd.grdData.CurrentPerfInfo.RF38 = _Value;
                        break;
                    }
                case 39:
                    {
                        grd.grdData.CurrentPerfInfo.RF39 = _Value;
                        break;
                    }
                case 40:
                    {
                        grd.grdData.CurrentPerfInfo.RF40 = _Value;
                        break;
                    }

                case 41:
                    {
                        grd.grdData.CurrentPerfInfo.RF41 = _Value;
                        break;
                    }
                case 42:
                    {
                        grd.grdData.CurrentPerfInfo.RF42 = _Value;
                        break;
                    }
                case 43:
                    {
                        grd.grdData.CurrentPerfInfo.RF43 = _Value;
                        break;
                    }
                case 44:
                    {
                        grd.grdData.CurrentPerfInfo.RF44 = _Value;
                        break;
                    }
                case 45:
                    {
                        grd.grdData.CurrentPerfInfo.RF45 = _Value;
                        break;
                    }
                case 46:
                    {
                        grd.grdData.CurrentPerfInfo.RF46 = _Value;
                        break;
                    }
                case 47:
                    {
                        grd.grdData.CurrentPerfInfo.RF47 = _Value;
                        break;
                    }
                case 48:
                    {
                        grd.grdData.CurrentPerfInfo.RF48 = _Value;
                        break;
                    }
                case 49:
                    {
                        grd.grdData.CurrentPerfInfo.RF49 = _Value;
                        break;
                    }
                case 50:
                    {
                        grd.grdData.CurrentPerfInfo.RF50 = _Value;
                        break;
                    }

            }

        }

        private void RichTexbox_LostFocus(object sender, RoutedEventArgs e)
        {


            //string str = new TextRange(sender.Document.ContentStart, sender.Document.ContentEnd).Text;

            //if (Strings.Len(str) > 1000)
            //{

            //    Interaction.MsgBox("You have exceeded the maximum limit of 1000 char.", Constants.vbCritical, grd.AppName);

            //    sender.Document.Blocks.Clear();
            //    str = Strings.Trim(str);
            //    str = Strings.Mid(str, 1, 950);

            //    sender.Document.Blocks.Add(new Paragraph(new Run(str)));

            //}



        }

        private string GetObjectName(int ctr)
        {
            return "objRF" + Strings.Format(ctr + 1, "00");
        }


        #endregion

        //#region "OpenCompletedActivities"
        //private void LoadOpenActivity()
        //{

        //    MainScrn.lvOpenActivities.ItemsSource = null;

        //    var AddRF = new List<string>();
        //    int ActId = 0;

        //    if (MainScrn.cmbOpenActName.SelectedIndex == 0)
        //    {
        //        AddRF = grd.GetConfigFieldName(0);
        //    }
        //    else
        //    {
        //        try
        //        {
        //            gridActivity act = (gridActivity)MainScrn.cmbOpenActName.SelectedItem;
        //            ActId = act.Id;
        //            AddRF = grd.GetConfigFieldName(act.Id);
        //        }

        //        catch (Exception ex)
        //        {

        //        }

        //    }

        //    var gView = new GridView();

        //    //gView.AllowsColumnReorder = true;
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "ID", DisplayMemberBinding = new Binding("Id"), Width = 0 });
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "ActId", DisplayMemberBinding = new Binding("Activity.Id"), Width = 0 });
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "Activity Name", DisplayMemberBinding = new Binding("Activity.ActName") });
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "Time Started", DisplayMemberBinding = new Binding("TimeStart") });
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "Duration", DisplayMemberBinding = new Binding("TimeElapsed") });
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "Type", DisplayMemberBinding = new Binding("Activity.Type") });
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "Process", DisplayMemberBinding = new Binding("Activity.Process") });
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "Ref No.", DisplayMemberBinding = new Binding("ReferenceId") });
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "Item Id", DisplayMemberBinding = new Binding("LOBItemId"), Width = 0 });


        //    if (!(AddRF == null))
        //    {
        //        if (AddRF.Count > 0)
        //        {

        //            for (int i = 0, loopTo = AddRF.Count - 1; i <= loopTo; i++)
        //                gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = AddRF[i].ToString(), DisplayMemberBinding = new Binding("PerfConfigData.RF" + Strings.Format(i + 1, "00")), Width = 100 });

        //        }
        //    }

        //    MainScrn.lvOpenActivities.View = gView;

        //    MainScrn.lvOpenActivities.ItemsSource = grd.GetPerformances(ActId, 1);



        //    MainScrn.TabItem1.Header = "(" + MainScrn.lvOpenActivities.Items.Count + ") " + " Open";

        //}

        //private void LoadCompletedActivity()
        //{


        //    dtCompleted = new DataTable();

        //    MainScrn.lvClosedActivities.ItemsSource = null;

        //    //lblTotalAudit.Visibility = Visibility.Hidden;
        //    //btnExportCompleted.Visibility = Visibility.Hidden;
        //    var AddRF = new List<string>();

        //    int ActId = 0;
        //    if (MainScrn.cmbCompletedActName.SelectedIndex == 0)
        //    {
        //        AddRF = grd.GetConfigFieldName(0);
        //    }
        //    else
        //    {
        //        try
        //        {
        //            gridActivity act = (gridActivity)MainScrn.cmbCompletedActName.SelectedItem;
        //            ActId = act.Id;
        //            AddRF = grd.GetConfigFieldName(act.Id);
        //        }
        //        catch (Exception ex)
        //        {

        //        }

        //    }

        //    var gView = new GridView();

        //    dtCompleted.Columns.Add("ID");
        //    dtCompleted.Columns.Add("Activity.ActName");
        //    dtCompleted.Columns.Add("Activity.Type");
        //    dtCompleted.Columns.Add("Activity.Process");
        //    dtCompleted.Columns.Add("TimeStart");
        //    dtCompleted.Columns.Add("TimeEnd");
        //    dtCompleted.Columns.Add("TimeElapsed");
        //    dtCompleted.Columns.Add("ReferenceId");


        //    gView.AllowsColumnReorder = true;
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "ID", DisplayMemberBinding = new Binding("Id"), Width = 0 });
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "Activity Name", DisplayMemberBinding = new Binding("Activity.ActName") });
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "Type", DisplayMemberBinding = new Binding("Activity.Type") });
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "Process", DisplayMemberBinding = new Binding("Activity.Process") });
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "Time Started", DisplayMemberBinding = new Binding("TimeStart") });
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "Time Ended", DisplayMemberBinding = new Binding("TimeEnd") });
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "Duration", DisplayMemberBinding = new Binding("TimeElapsed") });
        //    gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = "Ref No.", DisplayMemberBinding = new Binding("ReferenceId") });



        //    if (!(AddRF == null))
        //    {
        //        if (AddRF.Count > 0)
        //        {

        //            for (int i = 0, loopTo = AddRF.Count - 1; i <= loopTo; i++)
        //            {
        //                try
        //                {
        //                    dtCompleted.Columns.Add(AddRF[i].ToString());
        //                }
        //                catch (Exception ex)
        //                {

        //                }


        //                gView.Columns.Add(new System.Windows.Controls.GridViewColumn() { Header = AddRF[i].ToString(), DisplayMemberBinding = new Binding("PerfConfigData.RF" + Strings.Format(i + 1, "00")), Width = 100 });
        //            }

        //        }
        //    }



        //    MainScrn.lvClosedActivities.View = gView;

        //    MainScrn.lvClosedActivities.ItemsSource = grd.GetPerformances(ActId, 2);

        //    MainScrn.tabItem2.Header = "(" + MainScrn.lvClosedActivities.Items.Count + ")" + " Completed";
        //}

        //#endregion



        private void ElapsedTimer_Tick(object sender, EventArgs e)
        {

            DateTime timeEndNow = Conversions.ToDate(Strings.Format(grd.ConvertTimeZone(grd.grdData.TeamInfo.OffSet), "MM/dd/yyyy h:mm:ss tt"));

            //lblCurrentDateTime.Content = Strings.Format(timeEndNow, "MMM. dd - h:mm tt");

            //if (grd.grdData.CurrentUser.ActualTagging | Conversion.Val(lblCompletedActivity.Content) == 0 & Conversion.Val(lblOpenActivity.Content) == 0)
            //{

            if (grd.grdData.CurrentActivity.Id <= 0)
            {

                this.MainScrn.lblTimeElapsed.Content = "00:00:00";


                grd.grdData.CurrentActivity.TimeStart = grd.ConvertTimeZone(grd.grdData.TeamInfo.OffSet).ToString();
                grd.grdData.CurrentActivity.TimeEnd = grd.ConvertTimeZone(grd.grdData.TeamInfo.OffSet).ToString();
                grd.grdData.CurrentActivity.TimeStart2 = grd.ConvertTimeZone(grd.grdData.TeamInfo.OffSet);
                grd.grdData.CurrentActivity.TimeEnd2 = Convert.ToDateTime(grd.ConvertTimeZone(grd.grdData.TeamInfo.OffSet));

                this.MainScrn.BorderTotalTime.Background = System.Windows.Media.Brushes.Transparent;

                this.MainScrn.lblStartTime.Content = "00:00:00";


                return;
            }

            else
            {
                this.MainScrn.lblStartTime.Content = grd.grdData.CurrentActivity.TimeStart2.ToLongTimeString();
            }

            //}




            if (grd.grdData.CurrentActivity.Started == true)
            {

                var curElapsedTime = Conversions.ToDate(this.MainScrn.lblTimeElapsed.Content).AddSeconds(1d);

                this.MainScrn.lblTimeElapsed.Content = curElapsedTime.ToString("HH:mm:ss");

                if (grd.grdData.CurrentActivity.IsPaused == false)
                {
                    this.MainScrn.lblTimeElapsed.Content = grd.MGetTimeElapsed(grd.grdData.CurrentActivity.TimeStart2, timeEndNow, this.MainScrn.lblTimeElapsed.Content.ToString());
                }
                else
                {
                    this.MainScrn.lblTimeElapsed.Content = grd.MGetTimeElapsedPaused(grd.grdData.CurrentActivity.TimeEnd2, timeEndNow, this.MainScrn.lblTimeElapsed.Content.ToString());
                }

                grd.grdData.CurrentActivity.TimeEnd = Strings.Format(timeEndNow, "MM/dd/yyyy h:mm:ss tt");
                grd.grdData.CurrentActivity.TimeEnd2 = timeEndNow;

                //CheckIfOverAHT();
            }

            else
            {
                this.MainScrn.lblTimeElapsed.Content = "00:00:00";
            }



        }


        private void _eMsgBox(params string[] errorMsg)
        {
            string msg = string.Format("Unhandled error has been found. " + "Please contact your system administrator. Error:{0}{0}{1}", Constants.vbNewLine, Strings.Join(errorMsg, ""));
            Interaction.MsgBox(msg, Constants.vbCritical, grd.AppName);
        }

        public void ResetValues()
        {
            grd.grdData.CurrentActivity.Reset();
            MainScrn.lblAHT.Content = "00:00:00";
            MainScrn.lblStartTime.Content = "00:00:00";
            MainScrn.lblActivityName.Content = "";
            MainScrn.lblProcessName.Content = "";
        }

        public void changelblbg()
        {
            MainScrn.BorderTotalTime.Background = System.Windows.Media.Brushes.Transparent;
            //txtActName.Foreground = Brushes.White;
            //lblActName.Foreground = Brushes.White;
            //txtCurrentTime.Foreground = Brushes.White;
            //lblCurrentTime.Foreground = Brushes.White;
            //txtProcess.Foreground = Brushes.White;
            //lblProcess.Foreground = Brushes.White;
            //txtActType.Foreground = Brushes.White;
            //lblActType.Foreground = Brushes.White;
            //txtAHT.Foreground = Brushes.White;
            //lblAHT.Foreground = Brushes.White;
            //txtTimeStart.Foreground = Brushes.White;
            //lblTimeStart.Foreground = Brushes.White;
            //// txtTimeElapsed.Foreground = Brushes.White
            //// lblTimeElapsed.Foreground = Brushes.White



            //switch (txtActType.Text.ToUpper)
            //{


            //    case "PRODUCTIVE":
            //        {

            //            radButtonActName.Background = Brushes.Green; // G_Green
            //            radButtonActType.Background = Brushes.Green; // G_Green
            //            radButtonAHT.Background = Brushes.Green; // G_Green
            //            radButtonTimeStart.Background = Brushes.Green; // G_Green
            //            radButtonProcess.Background = Brushes.Green; // G_Green
            //            btnCurrentTime.Background = Brushes.Green; // G_Green
            //            break;
            //        }

            //    case "SHRINKAGE":
            //        {

            //            radButtonActName.Background = Brushes.Orange; // G_Orange
            //            radButtonActType.Background = Brushes.Orange; // G_Orange
            //            radButtonAHT.Background = Brushes.Orange; // G_Orange
            //            radButtonTimeStart.Background = Brushes.Orange; // G_Orange
            //            radButtonProcess.Background = Brushes.Orange; // G_Orange
            //            btnCurrentTime.Background = Brushes.Orange; // G_Orange

            //            txtActName.Foreground = Brushes.Black;
            //            lblActName.Foreground = Brushes.Black;
            //            txtCurrentTime.Foreground = Brushes.Black;
            //            lblCurrentTime.Foreground = Brushes.Black;
            //            txtProcess.Foreground = Brushes.Black;
            //            lblProcess.Foreground = Brushes.Black;
            //            txtActType.Foreground = Brushes.Black;
            //            lblActType.Foreground = Brushes.Black;
            //            txtAHT.Foreground = Brushes.Black;
            //            lblAHT.Foreground = Brushes.Black;
            //            txtTimeStart.Foreground = Brushes.Black;
            //            lblTimeStart.Foreground = Brushes.Black;
            //            break;
            //        }

            //    case "BREAK":
            //        {
            //            radButtonActName.Background = Brushes.Orange; // G_Orange
            //            radButtonActType.Background = Brushes.Orange; // G_Orange
            //            radButtonAHT.Background = Brushes.Orange; // G_Orange
            //            radButtonTimeStart.Background = Brushes.Orange; // G_Orange
            //            radButtonProcess.Background = Brushes.Orange; // G_Orange
            //            btnCurrentTime.Background = Brushes.Orange; // G_Orange

            //            txtActName.Foreground = Brushes.Black;
            //            lblActName.Foreground = Brushes.Black;
            //            txtCurrentTime.Foreground = Brushes.Black;
            //            lblCurrentTime.Foreground = Brushes.Black;
            //            txtProcess.Foreground = Brushes.Black;
            //            lblProcess.Foreground = Brushes.Black;
            //            txtActType.Foreground = Brushes.Black;
            //            lblActType.Foreground = Brushes.Black;
            //            txtAHT.Foreground = Brushes.Black;
            //            lblAHT.Foreground = Brushes.Black;
            //            txtTimeStart.Foreground = Brushes.Black;
            //            lblTimeStart.Foreground = Brushes.Black;
            //            break;
            //        }


            //    case "ADMIN":
            //        {

            //            radButtonActName.Background = Brushes.Blue; // G_Purple
            //            radButtonActType.Background = Brushes.Blue; // G_Purple
            //            radButtonAHT.Background = Brushes.Blue; // G_Purple
            //            radButtonTimeStart.Background = Brushes.Blue; // G_Purple
            //            radButtonProcess.Background = Brushes.Blue; // G_Purple
            //            btnCurrentTime.Background = Brushes.Blue; // G_Purple
            //            break;
            //        }

            //    case "PILOT":
            //        {

            //            radButtonActName.Background = Brushes.Green; // G_Green
            //            radButtonActType.Background = Brushes.Green; // G_Green
            //            radButtonAHT.Background = Brushes.Green; // G_Green
            //            radButtonTimeStart.Background = Brushes.Green; // G_Green
            //            radButtonProcess.Background = Brushes.Green; // G_Green
            //            btnCurrentTime.Background = Brushes.Green; // G_Green
            //            break;
            //        }

            //    default:
            //        {
            //            radButtonActName.Background = Brushes.Transparent;
            //            radButtonActType.Background = Brushes.Transparent;
            //            radButtonAHT.Background = Brushes.Transparent;
            //            radButtonTimeStart.Background = Brushes.Transparent;
            //            radButtonProcess.Background = Brushes.Transparent;
            //            btnCurrentTime.Background = Brushes.Transparent;
            //            break;
            //        }

            //}

        }
        public void ClearWrapPanel()
        {
            this.WrapPanelMain.Visibility = Visibility.Collapsed;
            this.WrapPanelMain2.Visibility = Visibility.Collapsed;
            this.WrapPanelMain.Children.Clear();
            this.WrapPanelMain2.Children.Clear();
        }

        private void StartActivity(int _ctr)
        {
            bool IsActivityStarted = false;
            int LOBItemId = 0;

            if (_ctr == 1)
            {


                if (lvMyActivities.SelectedItem is not null)
                {
                    curAct = (gridActivity)lvMyActivities.SelectedItem;


                    var q = from p in grd.grdData.ActivityList
                            where p.Id == curAct.Id
                            select p.ConfigInfo;

                    if (!(q == null))
                    {
                        if (q.Count() > 0)
                        {
                            curAct.ConfigInfo = q.FirstOrDefault();
                        }
                    }

                    if (curAct.ConfigInfo == null)
                    {
                        //curAct.ConfigInfo = grd.GetPerfConfigListLocal(curAct.LOBId == 79 ? 1594 : curAct.Id);
                    }
                }
            }
            else if (_ctr == 2)
            {

                if (GvProductivity.SelectedItem is not null)
                {
                    curAct = (gridActivity)GvProductivity.SelectedItem;

                    if (curAct.LOBId == 79)
                    {
                        var q = from p in grd.grdData.ActivityList
                                where p.Id == 1594
                                select p.ConfigInfo;
                        if (!(q == null))
                        {
                            if (q.Count() > 0)
                            {
                                curAct.ConfigInfo = q.FirstOrDefault();
                            }
                        }
                    }

                    if (curAct.ConfigInfo == null)
                    {
                        //curAct.ConfigInfo = grd.GetPerfConfigListLocal(curAct.LOBId == 79 ? 1594 : curAct.Id);
                    }

                }
            }
            else if (_ctr == 3)
            {
            }


            if (!(curAct == null))
            {
                grd.grdData.CurrentActivity.Started = true;
                if (grd.grdData.CurrentActivity.Id > 0)
                {
                    grd.grdData.MainWindowAction = "CHANGE";
                }

                else
                {
                    grd.grdData.MainWindowAction = "START";
                    IsActivityStarted = true;
                }

                var withBlock = grd.grdData.CurrentActivity;

                withBlock.Activity = curAct;
                withBlock.ActivityId = curAct.Id;
                withBlock.LOBId = curAct.LOBId;
                withBlock.LOBItemId = 0;

                if (IsActivityStarted)
                    this.MainWindowActivated();

            }

        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            {
                if (!(grd.grdData.CurrentActivity.Id > 0))
                {
                    new MessagesBox("There is no current activity to Pause.", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                    return;
                }

                try
                {
                    this.PauseActivity(false);

                    MainScrn.cmbCompletedActName.ItemsSource = grd.GetDistinctPerfActivity(grd.grdData.CurrentUser.EmpNo, grd.grdData.CurrentUser.TransactionDate2, 2);
                    MainScrn.cmbCompletedActName.SelectedIndex = 0;
                    MainScrn.cmbOpenActName.ItemsSource = grd.GetDistinctPerfActivity(grd.grdData.CurrentUser.EmpNo, grd.grdData.CurrentUser.TransactionDate2, 1);
                    MainScrn.cmbOpenActName.SelectedIndex = 0;

                    //this.LoadOpenActivity();
                    //this.LoadCompletedActivity();

                    grd.grdData.CurrentActivity.Started = false;

                    this.btnCloseMyAct.Visibility = Visibility.Visible;
                    this.GridDynamicObjects.Visibility = Visibility.Collapsed;
                    this.WrapPanelMain.Visibility = Visibility.Collapsed;
                    this.WrapPanelMain2.Visibility = Visibility.Collapsed;

                    WrapActivityList.Visibility = Visibility.Visible;

                    btnStop.Visibility = Visibility.Collapsed;
                    btnPause.Visibility = Visibility.Collapsed;
                    btnChange.Visibility = Visibility.Collapsed;

                    grd.grdData.ScrContent.IsActivityRunning = false;

                    if (grd.grdData.ScrContent.IsBreakStarted)
                        grd.grdData.ScrContent.IsBreakStarted = false;

                

                    notifier.ShowWarning("Current activity paused.");

                    new MessagesBox("Current Activity Paused.", MessageType.Info, MessageButtons.Ok).ShowDialog();
                }

                catch (Exception ex)
                {
                    _eMsgBox("clickPauseActivity", ex.Message);
                }
            }
        }

        private void clickStopActivity(object sender, RoutedEventArgs e)
        {
            try
            {
                StopActivity();

                MainScrn.cmbCompletedActName.ItemsSource = grd.GetDistinctPerfActivity(grd.grdData.CurrentUser.EmpNo, grd.grdData.CurrentUser.TransactionDate2, 2);
                MainScrn.cmbCompletedActName.SelectedIndex = 0;
                MainScrn.cmbOpenActName.ItemsSource = grd.GetDistinctPerfActivity(grd.grdData.CurrentUser.EmpNo, grd.grdData.CurrentUser.TransactionDate2, 1);
                MainScrn.cmbOpenActName.SelectedIndex = 0;

                //this.LoadOpenActivity();
                //this.LoadCompletedActivity();
                grd.grdData.ScrContent.IsActivityRunning = false;

                if (grd.grdData.ScrContent.IsBreakStarted)
                    grd.grdData.ScrContent.IsBreakStarted = false;
            }

            catch (Exception ex)
            {
                _eMsgBox("clickStopActivity", ex.Message);
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            WrapActivityList.Visibility = Visibility.Visible;

            if (grd.grdData.ScrContent.IsBreakClicked)
            {
                tabMyActivities.Visibility = Visibility.Collapsed;
                tabProductive.Visibility = Visibility.Collapsed;
                tabMyActivities.Width = 0;
                tabProductive.Width = 0;
            }
            else
            {
                tabMyActivities.Visibility = Visibility.Visible;
                tabProductive.Visibility = Visibility.Visible;
                //tabMyActivities.Width = 100;
                //tabProductive.Width = 100;
                tabMyActivities.IsSelected = true;
                tabMyActivities.Focus();
            }

            btnStart.Visibility = Visibility.Collapsed;
            btnStop.Visibility = Visibility.Collapsed;
            btnPause.Visibility = Visibility.Collapsed;
            btnChange.Visibility = Visibility.Collapsed;
            btnCloseMyAct.Visibility = Visibility.Visible;
        }

        private void GvProductivity_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.StartActivity(2);
            btnStop.Visibility = Visibility.Visible;
            btnPause.Visibility = Visibility.Visible;
            btnChange.Visibility = Visibility.Visible;
            grd.grdData.ScrContent.IsActivityRunning = true;
            grd.grdData.ScrContent.IsMyDataChanged = true;
        }

        private void lvMyActivities_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.StartActivity(1);
            btnStop.Visibility = Visibility.Visible;
            btnPause.Visibility = Visibility.Visible;
            btnChange.Visibility = Visibility.Visible;
            grd.grdData.ScrContent.IsActivityRunning = true;
            grd.grdData.ScrContent.IsMyDataChanged = true;
        }

        private void cmdMyFilter_Click(object sender, RoutedEventArgs e)
        {
            lvMyActivities.ItemsSource = grd.grdData._lstMyActivitiesOrig;
            cmbProcess.SelectedItem = null;
        }

        private void btnDeleteFromFav_Click(object sender, RoutedEventArgs e)
        {
            int x = 0;

            foreach (gridActivity hAct in lvMyActivities.SelectedItems)
            {

                if (grd.DeleteActivityFromFavorites(hAct, Convert.ToInt32(grd.grdData.CurrentUser.EmpNo)))
                {
                    x += 1;
                }

            }
            new MessagesBox(string.Format("{0} Activities Deleted Successfully!", (object)x), MessageType.Info, MessageButtons.Ok).ShowDialog();

            //Interaction.MsgBox(string.Format("{0} activities deleted successfully!", (object)x), Constants.vbInformation, grd.AppName);

            this.PopulateFavoriteList();
        }

        private void PopulateFavoriteList() // 11-13
        {
            lvMyActivities.ItemsSource = null;
            grd.grdData._lstMyActivitiesOrig = grd.GetFavoriteActivities();
            lvMyActivities.ItemsSource = grd.grdData._lstMyActivitiesOrig;

            this.PopulateProcessAndSubProcessMyActivities();
        }

        private void cmbProcessProd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProcessProd.SelectedValue is not null)
            {

                var q = from p in grd.grdData._lstProductivityOrig
                        where p.Process.Equals(cmbProcessProd.SelectedValue.ToString())
                        select p;

                GvProductivity.ItemsSource = q;

            }
        }

        private void btnClearFilterProd_Click(object sender, RoutedEventArgs e)
        {
            GvProductivity.ItemsSource = grd.grdData._lstProductivityOrig;
            cmbProcessProd.SelectedItem = null;
        }

        private void btnProductivityAddtoFav_Click(object sender, RoutedEventArgs e)
        {
            int x = 0;
            foreach (gridActivity hAct in GvProductivity.SelectedItems)
            {

                hAct.UserId = grd.grdData.CurrentUser.EmpNo;

                if (grd.AddActivityToFavorites(hAct))
                {
                    x += 1;
                }
            }

            if (x == 0)
            {
                new MessagesBox("No activity added. Please check if it's already in the My Favorites.", MessageType.Info, MessageButtons.Ok).ShowDialog();
            }
            else
            {
                new MessagesBox(string.Format("{0} Activities Added Successfully!", (object)x), MessageType.Info, MessageButtons.Ok).ShowDialog();

            }

            this.PopulateFavoriteList();
        }

        private void btnCloseMyAct_Click(object sender, RoutedEventArgs e)
        {
            this.WrapActivityList.Visibility = Visibility.Collapsed;

            btnStart.Visibility = Visibility.Visible;
            btnStop.Visibility = Visibility.Collapsed;
            btnPause.Visibility = Visibility.Collapsed;
            btnChange.Visibility = Visibility.Collapsed;

            btnCloseMyAct.Visibility = Visibility.Collapsed;
            grd.grdData.ScrContent.IsStartClicked = false;
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            notifier.ShowError("Error");
        }

        private void cmbProcess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProcess.SelectedValue is not null)
            {

                var q = from p in grd.grdData._lstMyActivitiesOrig
                        where p.Process.Equals(cmbProcess.SelectedValue.ToString())
                        select p;

                lvMyActivities.ItemsSource = q;

            }
        }
    }
}