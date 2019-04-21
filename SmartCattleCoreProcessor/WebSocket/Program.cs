using System;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;
using System.Net;
using TableDependency;
using TableDependency.EventArgs;
using TableDependency.SqlClient;
using WebSocket.Helper;
using WebSocket.Models;
using NHibernate;
using WebSocket.Domain;
using System.Collections.Generic;
using NHibernate.Criterion;

namespace WebSocket
{
    public class Customer
    {
        public int Id { get; set; }
        public string ValueName { get; set; }
        public float Value { get; set; }
    }

    class Program
    {
        private static String Address = "http://79.175.133.194:8080/";

        static void Main(string[] args)
        {
            InitialServer();
            InitialSqlDependency();
        }

        private static void InitialPort()
        {
            var prefix = Address;
            var username = Environment.GetEnvironmentVariable("USERNAME");
            var userdomain = Environment.GetEnvironmentVariable("USERDOMAIN");

            HttpListener _server = new HttpListener();
            _server.Prefixes.Add(prefix);

            try
            {
                _server.Start();
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 5)
                {
                    Console.WriteLine("You need to run the following command:");
                    Console.WriteLine("netsh http add urlacl url={0} user={1}\\{2} listen=yes",
                        prefix, userdomain, username);
                }
                else
                {
                    throw;
                }
            }
            Console.ReadKey();
        }

        private static void InitialServer()
        {
            var username = Environment.GetEnvironmentVariable("USERNAME");
            var userdomain = Environment.GetEnvironmentVariable("USERDOMAIN");

            try
            {
                WebApp.Start(Address);
            }
            catch (Exception)
            {
                Console.WriteLine(username);
                Console.WriteLine(userdomain);
            }

            Console.WriteLine("Server running on {0}", Address);
        }

        public static void InitialSqlDependency()
        {
            SmartCattleContext mContext = new SmartCattleContext();
            var TrigOnCurrentValue = mContext.CurrentValue.SetListener(x => x.Value).SetOnChange();
            TrigOnCurrentValue.OnChanged += TrigOnCurrentValue_OnChanged;
            TrigOnCurrentValue.Start();

            Console.ReadKey();

            TrigOnCurrentValue.Stop();
        }

        private static void TrigOnCurrentValue_OnChanged(object sender, RecordChangedEventArgs<SmartCattleContext.CurrentValueTbl.X_Model> e)
        {
            SmartCattleContext mContext = new SmartCattleContext();
            String tag = "";
            String topic = e.Entity.ValueName.Contains("Cattle_") ? "Cattle" : "FreeStall";
            switch (topic)
            {
                case "Cattle":
                    String[] CattleIdSplited = e.Entity.ValueName.Split('_');
                    String CattleId = CattleIdSplited[CattleIdSplited.Length - 1];
                    tag = "#Tag_" + topic + "_Avg_" + (e.Entity.ValueName.Contains("Temp_") ? "Temp" : "Activity");
                    CheckForCattleNotification(CattleId, tag);
                    break;

                case "FreeStall":
                    break;
            }
        }

        private static void CheckForCattleNotification(String CattleId, String tag)
        {
            ISession mContext = Context.Open();

            IList<CattleNotificationsSetting> CattleNotificationSetting = mContext.QueryOver<CattleNotificationsSetting>().List<CattleNotificationsSetting>();
            if (CattleNotificationSetting.Count != 0)
            {
                for (int i = 0; i < CattleNotificationSetting.Count; i++)
                {
                    if (CattleNotificationSetting[i].ActivationState.Equals("Active"))
                    {
                        String tmpCattleTemp = "#Tag_Cattle_Avg_Temp_"   +                   CattleNotificationSetting[i].ID.ToString() + "_" + CattleId;
                        String tmpSitting    = "#Tag_Cattle_Avg_Activity" + "_Sitting_"    + CattleNotificationSetting[i].ID.ToString() + "_" + CattleId;
                        String tmpWalking    = "#Tag_Cattle_Avg_Activity" + "_Walking_"    + CattleNotificationSetting[i].ID.ToString() + "_" + CattleId;
                        String tmpRumination = "#Tag_Cattle_Avg_Activity" + "_Rumination_" + CattleNotificationSetting[i].ID.ToString() + "_" + CattleId;
                        String tmpDrinking   = "#Tag_Cattle_Avg_Activity" + "_Drinking_"   + CattleNotificationSetting[i].ID.ToString() + "_" + CattleId;
                        String tmpEating     = "#Tag_Cattle_Avg_Activity" + "_Eating_"     + CattleNotificationSetting[i].ID.ToString() + "_" + CattleId;
                        String tmpStanding   = "#Tag_Cattle_Avg_Activity" + "_Standing_"   + CattleNotificationSetting[i].ID.ToString() + "_" + CattleId;

                        var tmpCattleTempList = mContext.QueryOver<CurrentValue>().Where(x => x.ValueName == tmpCattleTemp).List<CurrentValue>();
                        var tmpSittingList = mContext.QueryOver<CurrentValue>().Where(x => x.ValueName == tmpSitting).List<CurrentValue>();
                        var tmpWalkingList = mContext.QueryOver<CurrentValue>().Where(x => x.ValueName == tmpWalking).List<CurrentValue>();
                        var tmpRuminationList = mContext.QueryOver<CurrentValue>().Where(x => x.ValueName == tmpRumination).List<CurrentValue>();
                        var tmpDrinkingList = mContext.QueryOver<CurrentValue>().Where(x => x.ValueName == tmpDrinking).List<CurrentValue>();
                        var tmpEatingList = mContext.QueryOver<CurrentValue>().Where(x => x.ValueName == tmpEating).List<CurrentValue>();
                        var tmpStandingList = mContext.QueryOver<CurrentValue>().Where(x => x.ValueName == tmpStanding).List<CurrentValue>();

                        double CattleTempMin = (double)CattleNotificationSetting[i].CattleTempMin;
                        double CattleTempMax = (double)CattleNotificationSetting[i].CattleTempMax;
                        double SittingMin = (double)CattleNotificationSetting[i].SittingMin;
                        double SittingMax = (double)CattleNotificationSetting[i].SittingMax;
                        double WalkingMin = (double)CattleNotificationSetting[i].WalkingMin;
                        double WalkingMax = (double)CattleNotificationSetting[i].WalkingMax;
                        double RuminationMin = (double)CattleNotificationSetting[i].RuminationMin;
                        double RuminationMax = (double)CattleNotificationSetting[i].RuminationMax;
                        double DrinkingMin = (double)CattleNotificationSetting[i].DrinkingMin;
                        double DrinkingMax = (double)CattleNotificationSetting[i].DrinkingMax;
                        double EatingMin = (double)CattleNotificationSetting[i].EatingMin;
                        double EatingMax = (double)CattleNotificationSetting[i].EatingMax;
                        double StandingMin = (double)CattleNotificationSetting[i].StandingMin;
                        double StandingMax = (double)CattleNotificationSetting[i].StandingMax;

                        double CattleTemp = tmpCattleTempList.Count != 0 ? tmpCattleTempList[0].Value : 0;
                        double Sitting = tmpSittingList.Count != 0 ? tmpSittingList[0].Value : 0;
                        double Walking = tmpWalkingList.Count != 0 ? tmpWalkingList[0].Value : 0;
                        double Rumination = tmpRuminationList.Count != 0 ? tmpRuminationList[0].Value : 0;
                        double Drinking = tmpDrinkingList.Count != 0 ? tmpDrinkingList[0].Value : 0;
                        double Eating = tmpEatingList.Count != 0 ? tmpEatingList[0].Value : 0;
                        double Standing = tmpStandingList.Count != 0 ? tmpStandingList[0].Value : 0;
                        int tmpCattleId = Convert.ToInt32(CattleId);
                        int AnimalNumber = mContext.QueryOver<CattleTbl>().Where(x => x.ID == tmpCattleId).Select(Projections.Property<CattleTbl>(row => row.animalNumber)).SingleOrDefault<int>();

                        bool _CattleTemp = (CattleTempMin == -5 && CattleTempMax == 40)  ? true : false;
                        bool _Sitting    = (SittingMin    == 0  && SittingMax == 100)    ? true : false;
                        bool _Walking    = (WalkingMin    == 0  && WalkingMax == 100)    ? true : false;
                        bool _Rumination = (RuminationMin == 0  && RuminationMax == 100) ? true : false;
                        bool _Drinking   = (DrinkingMin   == 0  && DrinkingMax == 100)   ? true : false;
                        bool _Eating     = (EatingMin     == 0  && EatingMax == 100)     ? true : false;
                        bool _Standing   = (StandingMin   == 0  && StandingMax == 100)   ? true : false;

                        bool f_ON_or_Off = false;
                        String tmpNotificationTag = "#Tag_CattleNotificationSettingID_" + CattleNotificationSetting[i].ID + "_" + CattleId;
                        var taged_notification = mContext.QueryOver<NotificationsTable>().Where(x => x.TagName == tmpNotificationTag).OrderBy(x => x.ID).Asc.List<NotificationsTable>();

                        if ((_CattleTemp || (CattleTemp <= CattleTempMin || CattleTemp >= CattleTempMax)))
                        {
                            if ((_Sitting || (Sitting <= SittingMin || Sitting >= SittingMax)))
                            {
                                if ((_Walking || (Walking <= WalkingMin || Walking >= WalkingMax)))
                                {
                                    if ((_Rumination || (Rumination <= RuminationMin || Rumination >= RuminationMax)))
                                    {
                                        if ((_Drinking || (Drinking <= DrinkingMin || Drinking >= DrinkingMax)))
                                        {
                                            if ((_Eating || (Eating <= EatingMin || Eating >= EatingMax)))
                                            {
                                                if ((_Standing || (Standing <= StandingMin || Standing >= StandingMax)))
                                                {
                                                    f_ON_or_Off = true;
                                                    String strSnooze = CattleNotificationSetting[i].SnoozeTime;
                                                    double Snooze = Convert.ToDouble(strSnooze.Replace("h", ""));
                                                    String tmpTopic = CattleNotificationSetting[i].Topic;
                                                    String tmpComment = CattleNotificationSetting[i].Comment;
                                                    String tmpRoles = CattleNotificationSetting[i].Roles;
                                                    String tmpCreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                                                    String tmpNotificationType = CattleNotificationSetting[i].GroupName;
                                                    
                                                    if (taged_notification.Count == 0)
                                                    {
                                                        mContext.Clear();
                                                        NotificationsTable newNotification = new NotificationsTable();
                                                        newNotification.uId = Encryption.GenerateAlarmUId();
                                                        newNotification.Topic = CattleNotificationSetting[i].Topic;
                                                        newNotification.Comment = CattleNotificationSetting[i].Comment;
                                                        newNotification.FarmID = 3;
                                                        newNotification.RoleName = CattleNotificationSetting[i].Roles;
                                                        newNotification.CreatedDate = DateTime.Now;
                                                        newNotification.Status = "ACTIVE";
                                                        newNotification.NotificationType = "CATTLE";
                                                        newNotification.Snooze = Convert.ToDouble(CattleNotificationSetting[i].SnoozeTime.Replace("h", ""));
                                                        newNotification.TagName = tmpNotificationTag;
                                                        newNotification.Cattle_Freestall_Id = AnimalNumber;
                                                        newNotification.NotificationGroup = CattleNotificationSetting[i].GroupName;
                                                        newNotification.Act = "NO";

                                                        mContext.Save(newNotification);

                                                        //SendNotification(CattleNotificationSetting[i].Comment + ". CattleId: " + CattleId);
                                                    }
                                                    else
                                                    {
                                                        DateTime Now = DateTime.Now;
                                                        DateTime CreatedDate = taged_notification[taged_notification.Count - 1].CreatedDate;
                                                        DateTime CreatedDateAddSnooze = CreatedDate.AddHours(Snooze);
                                                        String uId = taged_notification[0].uId;
                                                        if (Now > CreatedDateAddSnooze)
                                                        {
                                                            if(taged_notification[taged_notification.Count - 1].Act.Equals("YES"))
                                                            {
                                                                mContext.Clear();
                                                                NotificationsTable newNotification = new NotificationsTable();
                                                                newNotification.uId = Encryption.GenerateAlarmUId();
                                                                newNotification.Topic = CattleNotificationSetting[i].Topic;
                                                                newNotification.Comment = CattleNotificationSetting[i].Comment;
                                                                newNotification.FarmID = 3;
                                                                newNotification.RoleName = CattleNotificationSetting[i].Roles;
                                                                newNotification.CreatedDate = DateTime.Now;
                                                                newNotification.Status = "ACTIVE";
                                                                newNotification.NotificationType = "CATTLE";
                                                                newNotification.Snooze = Convert.ToDouble(CattleNotificationSetting[i].SnoozeTime.Replace("h", ""));
                                                                newNotification.TagName = tmpNotificationTag;
                                                                newNotification.Cattle_Freestall_Id = AnimalNumber;
                                                                newNotification.NotificationGroup = CattleNotificationSetting[i].GroupName;
                                                                newNotification.Act = "NO";

                                                                mContext.Save(newNotification);
                                                            }
                                                            else if(taged_notification[taged_notification.Count - 1].Deactive == null)
                                                            {
                                                                if (taged_notification[taged_notification.Count - 1].Status.Equals("DEACTIVE"))
                                                                {
                                                                    mContext.Clear();
                                                                    NotificationsTable UpdateNotification = mContext.Get<NotificationsTable>(taged_notification[taged_notification.Count - 1].ID);
                                                                    UpdateNotification.Status = "ACTIVE";
                                                                    mContext.Update(UpdateNotification);
                                                                    mContext.Flush();
                                                                }
                                                            }
                                                            else if (taged_notification[taged_notification.Count - 1].Deactive != null)
                                                            {
                                                                if (!taged_notification[taged_notification.Count - 1].Deactive.Equals("DEACTIVE"))
                                                                {
                                                                    if (taged_notification[taged_notification.Count - 1].Status.Equals("DEACTIVE"))
                                                                    {
                                                                        mContext.Clear();
                                                                        NotificationsTable UpdateNotification = mContext.Load<NotificationsTable>(taged_notification[taged_notification.Count - 1].ID);
                                                                        UpdateNotification.Status = "ACTIVE";
                                                                        mContext.Update(UpdateNotification);
                                                                    }
                                                                }
                                                            }
                                                            

                                                            //SendNotification(CattleNotificationSetting[i].Comment + ". CattleId: " + CattleId);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (taged_notification.Count != 0 && !f_ON_or_Off)
                        {
                            double Snooze = taged_notification[0].Snooze;
                            DateTime Now = DateTime.Now;
                            DateTime CreatedDate = taged_notification[taged_notification.Count - 1].CreatedDate;
                            DateTime CreatedDateAddSnooze = CreatedDate.AddHours(Snooze);
                            String uId = taged_notification[0].uId;
                            if (Now > CreatedDateAddSnooze && taged_notification[taged_notification.Count - 1].Act.Equals("NO"))
                            {
                                if (taged_notification[taged_notification.Count - 1].Deactive == null || taged_notification[taged_notification.Count - 1].Deactive == "")
                                {
                                    mContext.Clear();
                                    NotificationsTable newNotification = new NotificationsTable();
                                    newNotification.uId = Encryption.GenerateAlarmUId();
                                    newNotification.Topic = CattleNotificationSetting[i].Topic;
                                    newNotification.Comment = CattleNotificationSetting[i].Comment;
                                    newNotification.FarmID = 3;
                                    newNotification.RoleName = CattleNotificationSetting[i].Roles;
                                    newNotification.CreatedDate = DateTime.Now;
                                    newNotification.Status = "DEACTIVE";
                                    newNotification.NotificationType = "CATTLE";
                                    newNotification.Snooze = Convert.ToDouble(CattleNotificationSetting[i].SnoozeTime.Replace("h", ""));
                                    newNotification.TagName = tmpNotificationTag;
                                    newNotification.Cattle_Freestall_Id = AnimalNumber;
                                    newNotification.NotificationGroup = CattleNotificationSetting[i].GroupName;
                                    newNotification.Act = "NO";

                                    mContext.Save(newNotification);
                                }
                            }
                        }
                    }
                }
            }
            Context.Close(mContext);
        }

        private static void SendNotification(String msg)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<SmartCattleHub>();
            context.Clients.All.addMessage("SmartCattleClient", msg);
        }
    }


    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }

    public class SmartCattleHub : Hub
    {
        public void Send(string name, string message)
        {
            //Clients.All.addMessage(name, "You enter this message: " + message);
            Console.WriteLine(name + ": " + message);
        }
    }
}
