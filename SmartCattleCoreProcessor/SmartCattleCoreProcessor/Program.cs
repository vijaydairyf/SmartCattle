using NHibernate;
using SmartCattleCoreProcessor.Domain;
using SmartCattleCoreProcessor.Handler;
using SmartCattleCoreProcessor.Helper;
using SmartCattleCoreProcessor.Models;
using SmartCattleCoreProcessor.Utility;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Threading;
using TableDependency;
using TableDependency.EventArgs;

namespace SmartCattleCoreProcessor
{
    class Program
    {
        static void Main()
        {
            SmartCattleContext mContext = new SmartCattleContext();

            #region Listener
            var TrigOnCattle_Tempreture = mContext.Tempreture.SetListener(x => x.value).SetOnChange();
            TrigOnCattle_Tempreture.OnChanged += CattleTempreture_OnChanged;
            TrigOnCattle_Tempreture.Start();

            var TrigOnCattle_Activity = mContext.Activity.SetListener(x => x.cattleId).SetOnChange();
            TrigOnCattle_Activity.OnChanged += TrigOnCattle_Activity_OnChanged;
            TrigOnCattle_Activity.Start();

            #endregion

            #region Time Interval
            //CurrentValueHandler _currentValue = new CurrentValueHandler();
            //DateTime _datetime = DateTime.ParseExact("2018-02-24 00:59:52", "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

            //var startTimeSpan = TimeSpan.Zero;
            //var periodTimeSpan = TimeSpan.FromMinutes(2);
            //var timer = new System.Threading.Timer((e) =>
            //{
            //}, null, startTimeSpan, periodTimeSpan);
            #endregion

            Console.ReadKey();
            TrigOnCattle_Tempreture.Stop();
            TrigOnCattle_Activity.Stop();
        }

        private static void TrigOnCattle_Activity_OnChanged(object sender, RecordChangedEventArgs<SmartCattleContext.ActivityStateTbl.X_Model> e)
        {
            ComputeActivity_Sitting((double)e.Entity.Sitting, e);
            ComputeActivity_Standing((double)e.Entity.Standing, e);
            ComputeActivity_Walking((double)e.Entity.Walking, e);
            ComputeActivity_Eating((double)e.Entity.Eating, e);
            ComputeActivity_Rumination((double)e.Entity.Rumination, e);
            ComputeActivity_Drinking((double)e.Entity.Drinking, e);
        }

        private static void ComputeActivity_Sitting(double sitting, RecordChangedEventArgs<SmartCattleContext.ActivityStateTbl.X_Model> e)
        {
            ISession mContext = Context.Open();
            IList<CattleNotificationsSetting> _CattleNotificationsSetting = mContext.QueryOver<CattleNotificationsSetting>().List<CattleNotificationsSetting>();
            for (int i = 0; i < _CattleNotificationsSetting.Count; i++)
            {
                double WindowTime = _CattleNotificationsSetting[i].WindowTime;
                double PeroidTime = _CattleNotificationsSetting[i].PeroidTime;
                DateTime PastWindowTime = DateTime.Now.AddHours(WindowTime * (-1));
                IList<ActivityStateTbl> ActivityStateTblList = mContext.QueryOver<ActivityStateTbl>().Where(x => x.cattleId == e.Entity.cattleId).Where(x => x.date >= PastWindowTime).List();
                double AvgActivity_Sitting = ActivityStateTblList.Average(x => x.Sitting);
                String TagName = "#Tag_Cattle_Avg_Activity_Sitting_" + _CattleNotificationsSetting[i].ID + "_" + e.Entity.cattleId;
                int ID = mContext.QueryOver<CurrentValue>().Where(x => x.ValueName == TagName).Select(x => x.ID).SingleOrDefault<int>();
                CurrentValue newValue = new CurrentValue();

                if (ID != 0)
                {
                    mContext.Clear();
                    newValue = mContext.Load<CurrentValue>(ID);
                    if (newValue.LastComputationDate.AddHours(PeroidTime) <= DateTime.Now)
                    {
                        newValue.LastComputationDate = DateTime.Now;
                        newValue.Value = AvgActivity_Sitting;
                        newValue.FarmId = 3;
                        mContext.Update(newValue);
                        try
                        {
                            mContext.Flush();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                            Console.WriteLine(ex.Message);
                            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                        }
                    }
                }
                else
                {
                    newValue.ValueName = TagName;
                    newValue.Value = AvgActivity_Sitting;
                    newValue.LastComputationDate = DateTime.Now;
                    newValue.FarmId = 3;
                    mContext.Save(newValue);
                }
                Console.WriteLine("------------------------------------------------------------------");
            }

            Context.Close(mContext);
        }

        private static void ComputeActivity_Standing(double standing, RecordChangedEventArgs<SmartCattleContext.ActivityStateTbl.X_Model> e)
        {
            ISession mContext = Context.Open();
            IList<CattleNotificationsSetting> _CattleNotificationsSetting = mContext.QueryOver<CattleNotificationsSetting>().List<CattleNotificationsSetting>();

            for (int i = 0; i < _CattleNotificationsSetting.Count; i++)
            {
                double WindowTime = _CattleNotificationsSetting[i].WindowTime;
                double PeroidTime = _CattleNotificationsSetting[i].PeroidTime;
                DateTime PastWindowTime = DateTime.Now.AddHours(WindowTime * (-1));
                IList<ActivityStateTbl> ActivityStateTblList = mContext.QueryOver<ActivityStateTbl>().Where(x => x.cattleId == e.Entity.cattleId).Where(x => x.date >= PastWindowTime).List();
                double Activity_Standing = ActivityStateTblList.Average(x => x.Standing);
                String TagName = "#Tag_Cattle_Avg_Activity_Standing_" + _CattleNotificationsSetting[i].ID + "_" + e.Entity.cattleId;
                int ID = mContext.QueryOver<CurrentValue>().Where(x => x.ValueName == TagName).Select(x => x.ID).SingleOrDefault<int>();
                CurrentValue newValue = new CurrentValue();

                if (ID != 0)
                {
                    mContext.Clear();
                    newValue = mContext.Load<CurrentValue>(ID);
                    if(newValue.LastComputationDate.AddHours(PeroidTime) <= DateTime.Now)
                    {
                        newValue.LastComputationDate = DateTime.Now;
                        newValue.Value = Activity_Standing;
                        newValue.FarmId = 3;
                        mContext.Update(newValue);
                        try
                        {
                            mContext.Flush();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                            Console.WriteLine(ex.Message);
                            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                        }
                    }
                }
                else
                {
                    newValue.ValueName = TagName;
                    newValue.Value = Activity_Standing;
                    newValue.LastComputationDate = DateTime.Now;
                    newValue.FarmId = 3;
                    mContext.Save(newValue);
                }
                Console.WriteLine("------------------------------------------------------------------");
            }

            Context.Close(mContext);
        }

        private static void ComputeActivity_Walking(double walking, RecordChangedEventArgs<SmartCattleContext.ActivityStateTbl.X_Model> e)
        {
            ISession mContext = Context.Open();
            IList<CattleNotificationsSetting> _CattleNotificationsSetting = mContext.QueryOver<CattleNotificationsSetting>().List<CattleNotificationsSetting>();

            for (int i = 0; i < _CattleNotificationsSetting.Count; i++)
            {
                double WindowTime = _CattleNotificationsSetting[i].WindowTime;
                double PeroidTime = _CattleNotificationsSetting[i].PeroidTime;
                DateTime PastWindowTime = DateTime.Now.AddHours(WindowTime * (-1));
                IList<ActivityStateTbl> ActivityStateTblList = mContext.QueryOver<ActivityStateTbl>().Where(x => x.cattleId == e.Entity.cattleId).Where(x => x.date >= PastWindowTime).List();
                double Activity_Walking = ActivityStateTblList.Average(x => x.Walking);
                String TagName = "#Tag_Cattle_Avg_Activity_Walking_" + _CattleNotificationsSetting[i].ID + "_" + e.Entity.cattleId;
                int ID = mContext.QueryOver<CurrentValue>().Where(x => x.ValueName == TagName).Select(x => x.ID).SingleOrDefault<int>();
                CurrentValue newValue = new CurrentValue();

                if (ID != 0)
                {
                    mContext.Clear();
                    newValue = mContext.Load<CurrentValue>(ID);
                    if (newValue.LastComputationDate.AddHours(PeroidTime) <= DateTime.Now)
                    {
                        newValue.LastComputationDate = DateTime.Now;
                        newValue.Value = Activity_Walking;
                        newValue.FarmId = 3;
                        mContext.Update(newValue);
                        try
                        {
                            mContext.Flush();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                            Console.WriteLine(ex.Message);
                            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                        }
                    }
                }
                else
                {
                    newValue.ValueName = TagName;
                    newValue.Value = Activity_Walking;
                    newValue.LastComputationDate = DateTime.Now;
                    newValue.FarmId = 3;
                    mContext.Save(newValue);
                }
                Console.WriteLine("------------------------------------------------------------------");
            }

            Context.Close(mContext);
        }

        private static void ComputeActivity_Eating(double eating, RecordChangedEventArgs<SmartCattleContext.ActivityStateTbl.X_Model> e)
        {
            ISession mContext = Context.Open();
            IList<CattleNotificationsSetting> _CattleNotificationsSetting = mContext.QueryOver<CattleNotificationsSetting>().List<CattleNotificationsSetting>();

            for (int i = 0; i < _CattleNotificationsSetting.Count; i++)
            {
                double WindowTime = _CattleNotificationsSetting[i].WindowTime;
                double PeroidTime = _CattleNotificationsSetting[i].PeroidTime;
                DateTime PastWindowTime = DateTime.Now.AddHours(WindowTime * (-1));
                IList<ActivityStateTbl> ActivityStateTblList = mContext.QueryOver<ActivityStateTbl>().Where(x => x.cattleId == e.Entity.cattleId).Where(x => x.date >= PastWindowTime).List();
                double Activity_Eating = ActivityStateTblList.Average(x => x.Eating);
                String TagName = "#Tag_Cattle_Avg_Activity_Eating_" + _CattleNotificationsSetting[i].ID + "_" + e.Entity.cattleId;
                int ID = mContext.QueryOver<CurrentValue>().Where(x => x.ValueName == TagName).Select(x => x.ID).SingleOrDefault<int>();
                CurrentValue newValue = new CurrentValue();

                if (ID != 0)
                {
                    mContext.Clear();
                    newValue = mContext.Load<CurrentValue>(ID);
                    if (newValue.LastComputationDate.AddHours(PeroidTime) <= DateTime.Now)
                    {
                        newValue.LastComputationDate = DateTime.Now;
                        newValue.Value = Activity_Eating;
                        newValue.FarmId = 3;
                        mContext.Update(newValue);
                        try
                        {
                            mContext.Flush();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                            Console.WriteLine(ex.Message);
                            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                        }
                    }
                }
                else
                {
                    newValue.ValueName = TagName;
                    newValue.Value = Activity_Eating;
                    newValue.LastComputationDate = DateTime.Now;
                    newValue.FarmId = 3;
                    mContext.Save(newValue);
                }
                Console.WriteLine("------------------------------------------------------------------");
            }

            Context.Close(mContext);
        }

        private static void ComputeActivity_Rumination(double rumination, RecordChangedEventArgs<SmartCattleContext.ActivityStateTbl.X_Model> e)
        {
            ISession mContext = Context.Open();
            IList<CattleNotificationsSetting> _CattleNotificationsSetting = mContext.QueryOver<CattleNotificationsSetting>().List<CattleNotificationsSetting>();

            for (int i = 0; i < _CattleNotificationsSetting.Count; i++)
            {
                double WindowTime = _CattleNotificationsSetting[i].WindowTime;
                double PeroidTime = _CattleNotificationsSetting[i].PeroidTime;
                DateTime PastWindowTime = DateTime.Now.AddHours(WindowTime * (-1));
                IList<ActivityStateTbl> ActivityStateTblList = mContext.QueryOver<ActivityStateTbl>().Where(x => x.cattleId == e.Entity.cattleId).Where(x => x.date >= PastWindowTime).List();
                double Activity_Rumination = ActivityStateTblList.Average(x => x.Rumination);
                String TagName = "#Tag_Cattle_Avg_Activity_Rumination_" + _CattleNotificationsSetting[i].ID + "_" + e.Entity.cattleId;
                int ID = mContext.QueryOver<CurrentValue>().Where(x => x.ValueName == TagName).Select(x => x.ID).SingleOrDefault<int>();
                CurrentValue newValue = new CurrentValue();

                if (ID != 0)
                {
                    mContext.Clear();
                    newValue = mContext.Load<CurrentValue>(ID);
                    if (newValue.LastComputationDate.AddHours(PeroidTime) <= DateTime.Now)
                    {
                        newValue.LastComputationDate = DateTime.Now;
                        newValue.Value = Activity_Rumination;
                        newValue.FarmId = 3;
                        mContext.Update(newValue);
                        try
                        {
                            mContext.Flush();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                            Console.WriteLine(ex.Message);
                            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                        }
                    }
                }
                else
                {
                    newValue.ValueName = TagName;
                    newValue.Value = Activity_Rumination;
                    newValue.LastComputationDate = DateTime.Now;
                    newValue.FarmId = 3;
                    mContext.Save(newValue);
                }
                Console.WriteLine("------------------------------------------------------------------");
            }

            Context.Close(mContext);
        }

        private static void ComputeActivity_Drinking(double drinking, RecordChangedEventArgs<SmartCattleContext.ActivityStateTbl.X_Model> e)
        {
            ISession mContext = Context.Open();
            IList<CattleNotificationsSetting> _CattleNotificationsSetting = mContext.QueryOver<CattleNotificationsSetting>().List<CattleNotificationsSetting>();

            for (int i = 0; i < _CattleNotificationsSetting.Count; i++)
            {
                double WindowTime = _CattleNotificationsSetting[i].WindowTime;
                double PeroidTime = _CattleNotificationsSetting[i].PeroidTime;
                DateTime PastWindowTime = DateTime.Now.AddHours(WindowTime * (-1));
                IList<ActivityStateTbl> ActivityStateTblList = mContext.QueryOver<ActivityStateTbl>().Where(x => x.cattleId == e.Entity.cattleId).Where(x => x.date >= PastWindowTime).List();
                double Activity_Drinking = ActivityStateTblList.Average(x => x.Drinking);
                String TagName = "#Tag_Cattle_Avg_Activity_Drinking_" + _CattleNotificationsSetting[i].ID + "_" + e.Entity.cattleId;
                int ID = mContext.QueryOver<CurrentValue>().Where(x => x.ValueName == TagName).Select(x => x.ID).SingleOrDefault<int>();
                CurrentValue newValue = new CurrentValue();

                if (ID != 0)
                {
                    mContext.Clear();
                    newValue = mContext.Load<CurrentValue>(ID);
                    if (newValue.LastComputationDate.AddHours(PeroidTime) <= DateTime.Now)
                    {
                        newValue.LastComputationDate = DateTime.Now;
                        newValue.Value = Activity_Drinking;
                        newValue.FarmId = 3;
                        mContext.Update(newValue);
                        try
                        {
                            mContext.Flush();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                            Console.WriteLine(ex.Message);
                            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                        }
                    }
                }
                else
                {
                    newValue.ValueName = TagName;
                    newValue.Value = Activity_Drinking;
                    newValue.LastComputationDate = DateTime.Now;
                    newValue.FarmId = 3;
                    mContext.Save(newValue);
                }
                Console.WriteLine("------------------------------------------------------------------");
            }

            Context.Close(mContext);
        }

        private static void CattleTempreture_OnChanged(object sender, RecordChangedEventArgs<SmartCattleContext.TempretureTbl.X_Model> e)
        {
            ISession mContext = Context.Open();
            IList<CattleNotificationsSetting> _CattleNotificationsSetting = mContext.QueryOver<CattleNotificationsSetting>().List<CattleNotificationsSetting>();

            for (int i = 0; i < _CattleNotificationsSetting.Count; i++)
            {
                double WindowTime = _CattleNotificationsSetting[i].WindowTime;
                double PeroidTime = _CattleNotificationsSetting[i].PeroidTime;
                DateTime PastWindowTime = DateTime.Now.AddHours(WindowTime * (-1));
                IList<TempretureTbl> TempretureTblList = mContext.QueryOver<TempretureTbl>().Where(x => x.cattleId == e.Entity.cattleId).Where(x => x.date >= PastWindowTime).List();
                double AvgTemp = TempretureTblList.Average(x => x.value);
                String TagName = "#Tag_Cattle_Avg_Temp_" + _CattleNotificationsSetting[i].ID + "_" + e.Entity.cattleId;
                int ID = mContext.QueryOver<CurrentValue>().Where(x => x.ValueName == TagName).Select(x => x.ID).SingleOrDefault<int>();
                CurrentValue newValue = new CurrentValue();

                if (ID != 0)
                {
                    mContext.Clear();
                    newValue = mContext.Load<CurrentValue>(ID);
                    if (newValue.LastComputationDate.AddHours(PeroidTime) <= DateTime.Now)
                    {
                        newValue.LastComputationDate = DateTime.Now;
                        newValue.Value = AvgTemp;
                        newValue.FarmId = 3;
                        mContext.Update(newValue);
                        try
                        {
                            mContext.Flush();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                            Console.WriteLine(ex.Message);
                            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                        }
                    }
                }
                else
                {
                    newValue.ValueName = TagName;
                    newValue.Value = AvgTemp;
                    newValue.LastComputationDate = DateTime.Now;
                    newValue.FarmId = 3;
                    mContext.Save(newValue);
                }
                Console.WriteLine("##################################################################");
                Console.WriteLine("#Tag_Cattle_Temp => ID: " + e.Entity.cattleId + " - Value: " + ((double)e.Entity.value).ToString() + " - DateTime: " + DateTime.Now);
                Console.WriteLine("------------------------------------------------------------------");
            }

            Context.Close(mContext);
        }

        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }
    }
}
