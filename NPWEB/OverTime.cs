using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;

namespace NPWEB
{
    /// <summary>超时控制类说明
    ///  判断是否超时  0代表 正常 1 代表超时
    ///  updateLock(string MeasureDocID, string carNO, string c_RefenceTime, int strLimte)//更新超时锁定和原因 参数（计量单号，车号，净重时间，限制时间）
    ///  int OverTimeContrl(string CarNO, string MeasureDocID)//超时控制
    ///  GetIP()//获得当前站点IP
    ///  ISLock(string MeasureDocID)//查询是否解锁
    ///  GetInIP(string MeasureDocID)//查询进厂IP
    ///  GetInIP(string MeasureDocID)//查询进厂IP
    /// </summary>
    /// <param name="超时控制"></param>
    /// <returns></returns>

    public class OverTime
    {
        private static string cMinCha = "";
        private static string MeasureDocID = "";
        public static int OverTimeContrl(string CarNO)//超时控制
        {
            int cLimitTime = 30;
            string IPNow = GetIP();
            string IPJL = "";

            #region 计量表查询净重时间与当前时间的时间差
            JGDBOP su = new JGDBOP();
            string sql = " select top 1 datediff(MINUTE,CONVERT(datetime,C_RefenceTime),getdate()) 分钟差,C_MeasureDocID,c_RefenceTime from CM_MeasureInfo "
                 + " where C_CarryNo='" + CarNO + "' and C_RefenceTime is not null  order by C_RefenceTime desc ";
            DataTable dtTime = su.GetTable(sql);
            if (dtTime.Rows.Count > 0)
            {
                MeasureDocID = dtTime.Rows[0]["C_MeasureDocID"].ToString();
            }
            cMinCha = dtTime.Rows[0]["分钟差"].ToString();//净重到当前打票的时间差
            int TimeDiff = Convert.ToInt32(cMinCha);
            string c_RefenceTime = dtTime.Rows[0]["C_RefenceTime"].ToString();//净重时间
            if (cMinCha == "")
            {
                cMinCha = "0";
            }
            #endregion

            #region 查询进厂IP
            IPJL = GetInIP(MeasureDocID);
            if (IPJL == "空")
            {
                insertDoorMain(MeasureDocID, CarNO);
            }
            #endregion

            #region 查询设定的限制时间
            sql = " select LeaveLimit from CM_LimitTimeSet"
                + " where  GateIPB='" + IPNow + "'";
            DataTable dtLimit = su.GetTable(sql);
            #endregion

            if (dtLimit.Rows.Count > 0)
            {
                #region 查询到时间限制
                cLimitTime = Convert.ToInt32(dtLimit.Rows[0]["LeaveLimit"].ToString());//得到限制的时间
                if (TimeDiff <= cLimitTime)
                {
                    return 0;
                }
                else
                {
                    string LockStatus = ISLock(MeasureDocID);
                    if (LockStatus == "0")
                    {
                        return 0;
                    }
                    else
                    {
                        updateLock(MeasureDocID, CarNO, c_RefenceTime, cLimitTime);//锁定
                        return 1;
                    }
                }
                #endregion
            }
            else
            {
                #region 未查询到时间限制

                if (TimeDiff <= cLimitTime)
                {
                    return 0;
                }
                else
                {
                    string LockStatus = ISLock(MeasureDocID);
                    if (LockStatus == "0")
                    {
                        return 0;
                    }
                    else
                    {
                        updateLock(MeasureDocID, CarNO, c_RefenceTime, cLimitTime);//锁定
                        return 1;
                    }
                }
                #endregion
            }
        }
        public static string GetIP()//获得当前站点IP
        {
            string IP = "";
            string name = Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            foreach (IPAddress ipa in ipadrlist)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                {
                    IP = ipa.ToString();
                }
            }
            return IP;
        }
        public static string ISLock(string MeasureDocID)//查询是否解锁
        {
            #region 查询是否解锁
            string LockStatus = "";

            JGDBOP su = new JGDBOP();
            string sqlIP = "select top 1 PrintLock 锁定状态 from CM_DoorControlMain where MeasureDocID ='" + MeasureDocID + "' order by TaskID Desc ";
            DataTable dt = su.GetTable(sqlIP);
            if (dt.Rows.Count > 0)
            {
                LockStatus = dt.Rows[0]["锁定状态"].ToString();
            }
            return LockStatus;
            #endregion
        }
        public static string GetInIP(string MeasureDocID)//查询进厂IP
        {
            string inIP = "空";
            #region 查询进厂IP
            JGDBOP su = new JGDBOP();
            string sqlIP = "select top 1 AccessIP 进厂IP from CM_DoorControlMain where MeasureDocID ='" + MeasureDocID + "' ";
            DataTable dt = su.GetTable(sqlIP);
            if (dt.Rows.Count > 0)
            {
                inIP = dt.Rows[0]["进厂IP"].ToString();

            }
            return inIP;

            #endregion
        }
        public static void updateLock(string MeasureDocID, string carNO, string c_RefenceTime, int strLimte)//更新超时锁定和原因 参数（计量单号，车号，净重时间，限制时间）
        {
            string LockReason = "车号:" + carNO + " 净重时间:" + c_RefenceTime + ",当前时间:" + getdate() + ",允许时间:" + strLimte + "分钟,车辆从计量到打票用时超时!"; ;
            JGDBOP su = new JGDBOP();
            string upLock = "update CM_DoorControlMain set OverTimeFlag='2',OverTimeMinute='" + cMinCha + "',LockTime='" + getdate() + "'," +
                            " PrintLock='1',LockSite='" + GetIP() + "',LockReason='" + LockReason + "' where MeasureDocID ='" + MeasureDocID + "' ";
            su.getsqlcom(upLock);
        }

        public static void insertDoorMain(string MeasureDocID, string carNO)
        {
            JGDBOP su = new JGDBOP();
            string upLock = "insert into CM_DoorControlMain " +
                            " (MeasureDocID,MsgTime,ModifyTime,CarNO,BusinessType,AccessTime,AccessIP,pk_stordoc,I_SiteID) " +
                            " values ('" + MeasureDocID + "','" + getdate() + "','" + getdate() + "','" + carNO + "',null, '" + getdate() + "','" + GetIP() + "',null,null)";
            su.getsqlcom(upLock);
        }

        private static string getdate()//获取服务器时间
        {
            JGDBOP su = new JGDBOP();
            string sqlDate = "select CONVERT(varchar(100), GETDATE(), 20) 时间";
            DataTable dtDate = su.GetTable(sqlDate);
            string Printdate = dtDate.Rows[0]["时间"].ToString();
            return Printdate;
        }

        public static string ReturnMeaID()
        {
            return MeasureDocID;
        }
    }
}