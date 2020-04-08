using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Xml;
using Baidu.Aip.Ocr;
using MySql.Data.MySqlClient;
namespace NPWEB
{
    /// <summary>
    /// JGNP 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class JGNP : System.Web.Services.WebService
    {
        #region 百度云车号识别
        [WebMethod(Description = "百度云车号识别")]
        public string carNumIdentification(byte[] image)
        {
            //string url = "https://aip.baidubce.com/rest/2.0/ocr/v1/license_plate";
            string carNo = "";
            string Color = "";
            //JsonResult ajxares = new JsonResult();
            //ajxares.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            //二进制流转byte[]
           // byte[] image = Encoding.UTF8.GetBytes(images);
            ArrayList list = new ArrayList();
            string clientId = "yf3TW6DbsxsGt2aRxpFMFxGf";
            // 百度云中开通对应服务应用的 Secret Key
            string clientSecret = "aIQPt204EaKp7vswVDppiQhouUHQ0kWk";
            //var image = System.IO.File.ReadAllBytes("图片文件路径");
            ////// 调用车牌识别，可能会抛出网络等异常，请使用try/catch捕获
            ////var result = client.LicensePlate(image);
            //Byte[] image = System.IO.File.ReadAllBytes("F://1.jpg");
            var client = new Ocr(clientId, clientSecret);
            string result = client.PlateLicense(image).ToString();
            return result;
        }
        #endregion
        #region 保存派车单信息到排队数据表
        [WebMethod(Description = "保存派车单信息到排队数据表")]
        public int saveCarInfo(string carCode,string gateCode)
        {
            GTHYDB dbopGh = new GTHYDB();
            int result = 0;
            string web_time = webTime();
            //查询派车单信息
            DataTable dt = dispathIndo(carCode);
            if (dt.Rows.Count > 0 )
            {
                string insertSql = "insert into CM_NumInfoTemp(C_CarryNo,pk_ClassType,C_MateriID,C_FactoryID,C_DlgtUser,C_TelePhone,GateCode,I_Store_id,ts) " +
                              "values ('" + dt.Rows[0]["cCarNum"] + "','" + dt.Rows[0]["C_ClassType"] + "', " +
                              "'" + dt.Rows[0]["pk_invbasdoc"] + "','" + dt.Rows[0]["pk_cubasdoc"] + "', "+
                              "'" + dt.Rows[0]["cDriver"] + "','"+ dt.Rows[0]["cDriverPhone"] + "','"+ gateCode + "',1,'"+ web_time + "')";

                result = dbopGh.getsqlcom(insertSql);
            }
            else
            {
                result = 0;
            }
           
            return result;
        }
        #endregion
        #region 查询排队信息
        [WebMethod(Description = "查询排队信息")]
        public string lineUpInfo(string doorCode)
        {
            string returnXml = "";
            GTHYDB hydbop = new GTHYDB();
            CMoperation cmop = new CMoperation();
            string web_time = webTime().Substring(0,10);
            string selectSql = "select pk_NumInfo,C_CarryNo,ts from CM_NumInfoTemp "+
                               "where CONVERT(varchar(10),ts,120) like '%" + web_time + "%' and GateCode = '"+doorCode+"'" +
                               "order by pk_NumInfo desc";
            DataTable dt = hydbop.GetTable(selectSql);
            if (dt.Rows.Count > 0)
            {
                //table 转xml
                returnXml = cmop.ConvertDataTableToXML(dt, "");
            }
            else
            {
                returnXml = "null";
            }
            return returnXml;
        }
        #endregion
        #region 查询金马派车单信息
        private DataTable dispathIndo(string carCode)
        {
            GTHYDB dbopGh = new GTHYDB();
            DataTable dt = null;
           string web_time = webTime().Substring(0,10);
            try
            {
                string selectSql = "select cCarNum,C_ClassType,pk_invbasdoc,pk_cubasdoc,C_MeasID,cDriver,cDriverPhone from CM_MeasTemp where  cCarNum  = '" + carCode + "'  and creatTm like '%" + web_time + "%' " +
                                "and (bRead = 0 and  bFinish = 0) order by autoid desc";
                dt = dbopGh.GetTable(selectSql);
            }
            catch (Exception ex)
            {
                dt = null;
            }
            
            return dt;
        }
        #endregion
        #region 查询手持用户信息
        [WebMethod(Description ="查询用户信息手持")]
        public string  selectAppUserJM(string  userCode,string passWord)
        {
            string doorCode = "";
            GTHYDB dbopGh = new GTHYDB();
            string selectSql = "select doorCode,cUserName from CB_AppUser where cUserCode = '"+userCode+"' and cPassWord = '"+passWord+"'";
            DataTable dt = dbopGh.GetTable(selectSql);
            if (dt.Rows.Count > 0)
            {
                doorCode = dt.Rows[0]["doorCode"].ToString(); ;
            }
           
            return doorCode;
        }
        #endregion
        #region post数据
        private static string Post(string Url, string Data)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            //请求内容为json格式
            request.ContentType = "application/x-www-form-urlencoded";
            byte[] bytes = Encoding.UTF8.GetBytes(Data);
            //请求内容为form表单
            // request.ContentType = "application/x-www-form-urlencoded";//‘
            request.ContentLength = bytes.Length;
            //字符流 发送请求数据
            Stream myResponseStream = request.GetRequestStream();
            myResponseStream.Write(bytes, 0, bytes.Length);
            //接收返回数据
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();

            myStreamReader.Close();
            myResponseStream.Close();

            if (response != null)
            {
                response.Close();
            }
            if (request != null)
            {
                request.Abort();
            }
            return retString;
        }
        #endregion

        #region 函数的递归调用
        [WebMethod(Description ="递归函数测试")]
        public int fac(int n)
        {
            int y = 0;
            if (n<0)
            {
                return y;
            }
            else
            {
                if (n == 0 || n == 1)
                {
                    y = 1;
                }
                else
                {
                    y = fac(n - 1) * n;

                    int x = 1;
                }
            }
            return y;
        }
        #endregion


        #region 济钢车辆出厂程序
        [WebMethod(Description ="查询济钢app用户")]
        public string selectUser(string UserCode)
        {
            string password = "";
            string username = "";
            string ipAdress = "";
            string pubInfo = "";
            string userCode = "";
            //string xml_str = "";
            JGDBOP dbopGh = new JGDBOP();
    
            string sql = "select cUserCode,cUserName,cPassWord,cIpAdress from CB_AppUser where cUserCode = '" + UserCode+"'";
            DataTable dt_user = dbopGh.GetTable(sql);
            if (dt_user.Rows.Count > 0)
            {
                password = dt_user.Rows[0]["cPassWord"].ToString();
                username = dt_user.Rows[0]["cUserName"].ToString();
                ipAdress = dt_user.Rows[0]["cIpAdress"].ToString();
                userCode = dt_user.Rows[0]["cUserCode"].ToString();
                
                pubInfo = password + ',' + username + ',' + ipAdress + ',' + userCode;
            }
            return pubInfo;
        }

        [WebMethod(Description = "查询济钢计量信息")]
        public string CmeasureInfoJL99(string cmCode)
        {
            //and C_CarryNo = '" + carCode + "'
            string xml_str = "";
            CMoperation cmop = new CMoperation();
            JGDBOP dbop99 = new JGDBOP();
            string select_sql = "select AutoID,I_FinishFlag,C_MeasureDocID,MoreID,C_CarryNo,C_MeasureType,C_PurchaseOrderID," +
                                "C_SendFactoryDes,C_MaterielDes,C_Standard,C_FactoryDes,C_GrossTime,N_GrossWeight," +
                                "N_TareWeight,C_TareTime,C_RefenceTime,N_RefenceWeight,C_ClassType,C_QuanlityType,C_DlgtUser " +
                                "from CM_MeasureInfo where C_MeasureDocID ='" + cmCode + "' or MoreID = '" + cmCode + "' order by AutoID";

            DataTable dt_cm = dbop99.GetTable(select_sql);
            if (dt_cm.Rows.Count > 0)
            {
                xml_str = cmop.ConvertDataTableToXML(dt_cm, "");
            }
            return xml_str;
        }
        [WebMethod(Description = "车辆出厂确认")]
        public bool exitSure(string measType, string cmCode, string moreCode, string userName, int flag)
        {
            JGDBOP dbop99 = new JGDBOP();
            bool resultCmd = false;
            List<string> cmCode_list = new List<string>();
            string carCode = "";
            if (measType == "001")
            {
                resultCmd = updataTableExit(userName,flag,cmCode);  
            }
            if (measType == "002")
            {
                //根据morCode 查询 cmCode
                string selectSql = "select C_MeasureDocID,C_CarryNo from CM_MeasureInfo where MoreID = '" + moreCode + "'";
                DataTable dt = dbop99.GetTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        cmCode_list.Add(dt.Rows[i]["C_MeasureDocID"].ToString());
                        carCode = dt.Rows[0]["C_CarryNo"].ToString();
                    }
                    resultCmd = updataTableExitMore(userName, flag, cmCode_list,carCode);
                }       
            }
            return resultCmd;
        }
        [WebMethod(Description = "向大屏中间表中插入数据")]
        public bool insertOutDoor(string meaType, string cmCode, string moreCode, string carCode)
        {
            JGDBOP dbop99 = new JGDBOP();
            bool result = false;
            string webtm = webTime();
            string insertSql = "";

            //判断表中是否有该计量
            string selectSql = "select C_MeasureDocID from CM_OutScreen where C_MeasureDocID = '" + cmCode + "'";
            DataTable dt = dbop99.GetTable(selectSql);
            if (dt.Rows.Count == 0)
            {
                if (meaType == "001")
                {
                    insertSql = "insert into CM_OutScreen(C_MeasureDocID,C_CarryNo,C_TimeStamp) values " +
                                "('" + cmCode + "','" + carCode + "','" + webtm + "') ";

                }
                if (meaType == "002")
                {
                    insertSql = "insert into CM_OutScreen(C_MeasureDocID,C_CarryNo,C_TimeStamp) values " +
                               "('" + moreCode + "','" + carCode + "','" + webtm + "') ";
                }
                if (dbop99.getsqlcom(insertSql) == 1)
                {
                    result = true;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }
        [WebMethod(Description = "查询济钢内盘车辆路线信息")]
        public string selectNeiPanInfo(string carCode)
        {
            JGDBOP dbop99 = new JGDBOP();
            CMoperation cmop = new CMoperation();
            string xml_str = "";
            string selectSql = "select C_CarryNo 车号,C_SedFactoryDes 发货单位,C_MaterielDes 货物名称,C_RecFactoryDes 收货单位 from CM_OtherInfo where C_CarryNo  = '" + carCode + "' ";
            DataTable dtNp = dbop99.GetTable(selectSql);
            if (dtNp.Rows.Count > 0)
            {
                xml_str = cmop.ConvertDataTableToXML(dtNp, "");
            }
            return xml_str;
        }
        [WebMethod(Description = "装/卸货确认")]
        public bool storeLoad(string loadFlag, string loadUser, string loadTime, int loadId)
        {
            JGDBOP dbop = new JGDBOP();
            bool result = false;
            string update_sql = "update CM_MeasureInfo set C_AffirmFlag = '" + loadFlag + "'," +
                               "C_AffirmUser = '" + loadUser + "'," +
                               "C_AffirmTime = '" + loadTime + "' where AutoID = '" + loadId + "'";
            if (dbop.getsqlcom(update_sql) > 0)
            {
                result = true;
            }
            return result;
        }
        [WebMethod(Description = "取样确认")]
        public bool storeQua(string quaFlag, string quaUser, string quaTime, int autoId)
        {
            JGDBOP dbop = new JGDBOP();
            bool result = false;
            string update_sql = "update CM_MeasureInfo set C_AffirmFlag = '" + quaFlag + "'," +
                               "C_AffirmUser = '" + quaUser + "'," +
                               "C_AffirmTime = '" + quaTime + "' where AutoID = '" + autoId + "'";
            if (dbop.getsqlcom(update_sql) == 1)
            {
                result = true;
            }
            return result;
        }
        [WebMethod(Description = "修改密码")]
        public bool modifyPass(string userCode, string userName, string passWord)
        {
            JGDBOP dbop = new JGDBOP();
            bool result = false;
            string update_sql = "update CB_AppUser set cPassWord = '" + passWord + "' where cUserCode = '" + userCode + "' and cUserName = '" + userName + "'";
            if (dbop.getsqlcom(update_sql) == 1)
            {
                result = true;
            }
            return result;
        }
        [WebMethod(Description = "超时判断")]
        public int overTimm(string carCode)
        {
            //判断车辆是否超时
            int IsLock = OverTime.OverTimeContrl(carCode);
            return IsLock;
        }
        #endregion

        #region 风险管控项目

        #region xml数据转table
        public DataTable ConvertXMLToDataTable(string xmlData)
        {
            TextReader sr = null;
            try
            {
                DataTable dt = new DataTable();
                sr = new StringReader(xmlData);
                dt.ReadXml(sr);
                return dt;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (sr != null) sr.Close();
            }
        }
        #endregion

        #region 查询服务器时间mysql
        [WebMethod(Description ="查询服务器时间")]
        public string webTimeMysql()
        {
            SafeDBClasscs jgdbop = new SafeDBClasscs();
            string web_time = "";
            string select_sql = "SELECT NOW()";
            System.Data.DataTable dt_time = jgdbop.GetDataTable(select_sql);
            if (dt_time.Rows.Count > 0)
            {
                web_time = dt_time.Rows[0][0].ToString();
                DateTime webtm = Convert.ToDateTime(web_time);
                web_time = webtm.ToString("yyyy-MM-dd");
            }
            return web_time;
        }
        #endregion

        #region 递归方法查询登录用户id所包含的部门id
        [WebMethod(Description = "递归方法查询登录用户id的上级部门id   ------- 上级部门" )]
        public string selectDepartID(int deptID,string parengIDStr)
        {
            int parentID = 0;
            int autoID = deptID;
            SafeDBClasscs safeDb = new SafeDBClasscs();
            List<int> parentIDList = new List<int>();
            do
            {
                autoID = wtf(autoID);
                if (autoID > 0)
                {
                    parengIDStr += autoID.ToString() + ",";
                }
            }
            while (autoID > 0);
            return parengIDStr;
        }
        private int wtf(int deptID)
        {
            SafeDBClasscs safeDb = new SafeDBClasscs();
            string selectSql = "select id,parent_id from cmf_department where id = " + deptID + "";
            try
            {
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    return Convert.ToInt32(dt.Rows[0]["parent_id"].ToString());
                }
                else
                    return 0;
            }
            catch (Exception ex)
            {
             
                WriteYCLog(ex,"查询上级部门报错");
                return 0;
            }
        }

        //----- 查询当前用户的上级所有部门id-------------//
        private string selectUpDepartID(int deptID, string parengIDStr)
        {
            int parentID = 0;
            int autoID = deptID;
            SafeDBClasscs safeDb = new SafeDBClasscs();
            List<int> parentIDList = new List<int>();
            do
            {
                autoID = wtf(autoID);
                if (autoID > 0)
                {
                    parengIDStr += autoID.ToString() + ",";
                }
            }
            while (autoID > 0);
            return parengIDStr;
        }
        #endregion

        #region 查询审核部门信息
        [WebMethod(Description ="查询审核部门信息")]
        public string SelectaritDepartData(int departID)
        {
            string returnXML = "";
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();
            //查询当前用户的上级所有部门
            string auitDeptStr = selectUpDepartID(departID,"");
            if (auitDeptStr != "")
            {
                auitDeptStr = auitDeptStr.TrimEnd(',');
                string selectSql = "select name from cmf_department where id in(" + auitDeptStr + ")";
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    //组装xml数据
                    returnXML = xmlDB.ConvertDataTableToXML(dt, "");
                }
            }
            return returnXML;
        }
        #endregion

        #region 隐患审核提交
        [WebMethod(Description = "隐患审核提交")]
        public  int updateAuitInfo(int autoID,string auitName)
        {
            int result = 0;
            string webTm = webTimeMysql();
            SafeDBClasscs safeDb = new SafeDBClasscs();
            string selectTime = "SELECT NOW()";
            DataTable dt = safeDb.GetDataTable(selectTime);
            string auitTime = dt.Rows[0][0].ToString();
            string updateSql = "update cmf_danger_hiden set bCheck = 1,cauitPerson = '"+auitName+ "',dAuitTime = '"+auitTime+"' where hidenID = " + autoID+" ";
            try
            {
                result = safeDb.getsqlCom(updateSql);
                if (result > 0)
                {
                    result = 1;//审核成功
                }
                else
                {
                    result = 22;//审核失败
                }
            }
            catch (Exception ex)
            {}
            return result;
        }
        #endregion

        #region 隐患弃审提交
        [WebMethod(Description = "隐患弃审提交")]
        public int updateAuitInfoNO(string memos,int autoID,string auitName)
        {
            int result = 0;
            SafeDBClasscs safeDb = new SafeDBClasscs();
            string selectTime = "SELECT NOW()";
            DataTable dt = safeDb.GetDataTable(selectTime);
            string auitTime = dt.Rows[0][0].ToString();
            string updateSql = "update cmf_danger_hiden set cMemos = '"+ memos + "',bCheck = 2,cauitPerson = '" + auitName + "',dAuitTime = '" + auitTime + "' where hidenID = " + autoID + " ";
            try
            {
                result = safeDb.getsqlCom(updateSql);
                if (result > 0)
                {
                    result = 1;//弃审成功
                }
                else
                {
                    result = 22;//弃审失败
                }
            }
            catch (Exception ex)
            { }
            return result;
        }
        #endregion

        #region 密码修改
        [WebMethod(Description = "密码修改")]
        public int modifyPassMySQL(int accID, int userID, string passNew, string passOld)
        {
            int result = 0;
            SafeDBClasscs safeDb = new SafeDBClasscs();
            string selectSql = "select user_app_pass from cmf_user " +
                               "where accID = " + accID + " and id = " + userID + " and  user_app_pass = '" + passOld + "' " +
                              "order by id desc";
            DataTable dt = safeDb.GetDataTable(selectSql);
            if (dt.Rows.Count > 0)
            {
               
                string updateSql = "update cmf_user set user_app_pass = '" + passNew + "' where id = " + userID + " and accID = " + accID + "";
                result = safeDb.getsqlCom(updateSql);
                if (result > 0)
                {
                    result = 1;//密码修改成功
                }
                else
                {
                    result = 22;//密码修改失败 请检查网络是否连接
                }
            }
            else
            {
                result = 33;//原密码输入错误
            }
            return result;
        }
        #endregion

        #region 查询登录用户的所有下级部门
        private string selectDepartIDChild(int deptID)
        {
            string parengIDStr = "";
            SafeDBClasscs safeDb = new SafeDBClasscs();
            
            List<int> childIDList = new List<int>();
            string sql = "SELECT queryChildrenAreaInfo("+deptID+") as childStr";
            DataTable dt = safeDb.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                parengIDStr = dt.Rows[0]["childStr"].ToString();
            }
            return parengIDStr;
        }
        #endregion
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="content">图片字符流</param>
        /// <param name="pathandname">图片名称</param>
        /// <returns></returns>
        [WebMethod(Description = "保存上报的隐患图片")]
        public bool UpdateFile(string content, string pathandname)
        {
            string partolTm = webTime().Substring(0,10);
           // string imagePath = "/j/SAFEJG/public/upload" + "/" + partolTm;
            string imagePath = "/" + "炼铁厂";
            //保存图片路径
            string FilePath = Server.MapPath(imagePath);
            //判断路径是否存在
            if (!Directory.Exists(FilePath))
            {
                //创建路径
                Directory.CreateDirectory(FilePath);
            }

            string SaveFilePath = Path.Combine(FilePath, pathandname);
            byte[] fileBytes;
            try
            {
                fileBytes = Convert.FromBase64String(content);
                MemoryStream memoryStream = new MemoryStream(fileBytes); //1.定义并实例化一个内存流，以存放提交上来的字节数组。  
                FileStream fileUpload = new FileStream(SaveFilePath, FileMode.Create); ///2.定义实际文件对象，保存上载的文件。  
                memoryStream.WriteTo(fileUpload); ///3.把内存流里的数据写入物理文件  
                memoryStream.Close();
                fileUpload.Close();
                fileUpload = null;
                memoryStream = null;

                //string imgAdres = FilePath + "\\炼铁厂.jpg";
                //imgAdres = imgAdres.Replace("\\","\\\\");


                //string sql = "update cmf_danger_hiden set cImg = '" + imgAdres + "' where AutoID = " + autoID + "";
                //SafeDBClasscs safeDB = new SafeDBClasscs();
                //int n = safeDB.getsqlCom(sql);
                return true;
            }
            catch
            {
                return false;
            }
        }

        [WebMethod(Description ="查询所有的部门")]
        public string selectAllDepart(int accID)
        {
            string returnXML = "";
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();
            try
            {
                string selectSql = "select name from cmf_department where accID = '" + accID + "'";
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    //组装xml数据
                    returnXML = xmlDB.ConvertDataTableToXML(dt, "");
                }
            }
            catch (Exception ex){}
          
            return returnXML;
        }

        [WebMethod(Description ="隐患转上一级")]
        public int updateHidenUpDept(int hidenID,int deptID)
        {
            int result = 0;
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();
            try
            {
                //查询当前部门的父id
                string selectSql = "select parent_id,name from cmf_department where  id = "+deptID+"";
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    if (Convert.ToInt32(dt.Rows[0]["parent_id"].ToString()) != 0)
                    {
                        //更新deptID deptIDOld
                        string updateSql = "update cmf_danger_hiden set deptIDOld = " + deptID + " ," +
                                           "departID = " + dt.Rows[0]["parent_id"].ToString() + ",iUpHiden = 1 where hidenID = " + hidenID + "";
                        if (safeDb.getsqlCom(updateSql) > 0)
                        {
                            result = 1;
                        }
                        else
                        {
                            result = 0;
                        }
                    }
                    else
                    {
                        result = 2;//已经是最高部门 不能转上一级
                    }
                }
            }
            catch (Exception ex) { }
            return result;
        }
        [WebMethod(Description ="隐患跨部门下发整改通知单")]
        public bool updateHidenChangeDept(int autoID, string revXML)
        {
            bool result = false;
            string updateSql = "";
            SafeDBClasscs safeDB = new SafeDBClasscs();
            try
            {
                //解析XML数据
                //xml转table
                StringReader StrStream = null;
                XmlTextReader Xmlrdr = null;
                DataSet ds = new DataSet();
                StrStream = new StringReader(revXML);
                //获取StrStream中的数据  
                Xmlrdr = new XmlTextReader(StrStream);
                //ds获取Xmlrdr中的数据                 
                ds.ReadXml(Xmlrdr);
                DataTable dt = ds.Tables[1];
                if (dt.Rows.Count > 0)
                {
                    updateSql = "update cmf_danger_hiden set abarDepartID = " + Convert.ToInt32(dt.Rows[0]["abarDepartID"].ToString()) + ",abarStyle = '" + dt.Rows[0]["abarStyle"].ToString() + "'," +
                             "abarbeitungTime = '" + dt.Rows[0]["abarbeitungTime"].ToString() + "',abarRequest = '" + dt.Rows[0]["abarRequest"].ToString() + "',hidenFlag = 1,iChangeHide = 1 ," +
                             "iAuitDepartID = " + Convert.ToInt32(dt.Rows[0]["iAuitDepartID"].ToString()) + ",abarbeitungPerson = '" + dt.Rows[0]["abarbeitungPerson"].ToString() + "' " +
                             "where hidenID = '" + autoID + "'";
                    if (safeDB.getsqlCom(updateSql) > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteYCLog(ex, "下发整改通知单失败" + updateSql);
            }
            return result;
        }
        [WebMethod(Description = "隐患审核列表信息查询")]
        public string selectHidenAuitDataList(int accID, int departID)
        {
            string returnXML = "";
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();
            string selectSql = "select a.hidenID,d.name,b.user_nickname,a.hidenTime,a.hidenPersonName,d.name as 上报部门,e.dangerTypeName,f.partolStyleName,a.hidenImg,a.abarImg," +
                                "a.hidenLevel,a.hidenInfo,a.abarDepartID,g.name as 隐患整改部门,a.abarStyle,a.abarbeitungTime,h.name 审核部门," +
                                "a.abarRequest,a.abarbeitungPerson,a.abarbeitungPerson_2,a.abarTime,a.abarResult,a.reviewPerson,a.reviewResult,a.reviewTime," +
                                "(case hidenFlag when 0 then '已上报' when 1 then '待整改' when 2 then '待复查' when 3 then '已完成' end) as hidenFlag " +
                                "from cmf_danger_hiden a left join cmf_user b on a.hidenPersonID = b.id " +
                                "left join cmf_department d on b.departID = d.id " +
                                "left join cmf_danger_type e on a.hidenTypeID = e.dangerTypeID " +
                                "left join cmf_partol_style f on a.hidenPtrStyleID = f.partolStyleID " +
                                "left join cmf_department g on a.abarDepartID = g.id " +
                                "left join cmf_department h on a.iAuitDepartID = h.id " +
                                "where  a.hidenFlag = 1 and a.accID = " + accID + " and a.iAuitDepartID = " + departID + " and  bCheck = 0";

            try
            {
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    //table转xml
                    returnXML = xmlDB.ConvertDataTableToXML(dt, "");
                }
            }
            catch (Exception ex) { }
            return returnXML;
        }

        [WebMethod(Description ="隐患审核信息查询")]
        public string selectHidenAuitData(int accID, int departID,int autoID)
        {
            string returnXML = "";
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();
           string selectSql = "select a.hidenID,d.name as 隐患上报部门,b.user_nickname,a.hidenTime, e.dangerTypeName,f.partolStyleName,a.hidenImg,a.abarImg," +
                               "a.hidenLevel,a.hidenInfo,a.abarDepartID,g.name as 隐患整改部门,a.abarStyle,a.abarbeitungTime,h.name 审核部门," +
                               "a.abarRequest,a.abarbeitungPerson,a.abarbeitungPerson_2,a.abarTime,a.abarResult,a.reviewPerson,a.reviewResult,a.reviewTime," +
                               "(case hidenFlag when 0 then '已上报' when 1 then '待整改' when 2 then '待复查' when 3 then '已完成' end) as hidenFlag " +
                               "from cmf_danger_hiden a left join cmf_user b on a.hidenPersonID = b.id " +
                               "left join cmf_department d on b.departID = d.id " +
                               "left join cmf_danger_type e on a.hidenTypeID = e.dangerTypeID " +
                               "left join cmf_partol_style f on a.hidenPtrStyleID = f.partolStyleID " +
                               "left join cmf_department g on a.abarDepartID = g.id " +
                               "left join cmf_department h on a.iAuitDepartID = h.id "+
                               "where a.hidenID = " + autoID + "  and a.hidenFlag = 1 and a.accID = " + accID + " and a.iAuitDepartID = " + departID + " ";

            try
            {
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    //table转xml
                    returnXML = xmlDB.ConvertDataTableToXML(dt, "");
                }
            }
            catch (Exception ex){ }
            return returnXML;
        }
        
        [WebMethod(Description ="版本号 查询")]
        public string searchVersion(int accID)
        {
            string returnXML = "";
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();
            string selectSql = "select apkName,versionCode,versionName from cmf_version where accID = '"+accID+"'";
            DataTable dt = safeDb.GetDataTable(selectSql);
            if (dt.Rows.Count > 0)
            {
                //组装xml数据
                returnXML = xmlDB.ConvertDataTableToXML(dt, "");
            }
            return returnXML;
        }

        [WebMethod(Description = "未巡查查询")]
        public int searchMessagePartol(int accID,int departID)
        {
            int returnID = 0;
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();
            string selectSqls = "select NOW()";
        
            DataTable dts = safeDb.GetDataTable(selectSqls);
            string partolTime = dts.Rows[0][0].ToString().Substring(0,10);
            DateTime tm = Convert.ToDateTime(partolTime);
            partolTime = tm.ToString("yyyy-MM-dd");
        string selectSql = "select count(partolID) as counts from cmf_danger_partol where partolFlag = 1 and accID = "+accID+" and departID = "+departID+ " and partolTime like '%"+ partolTime + "%' ";
            DataTable dt = safeDb.GetDataTable(selectSql);
            if (dt.Rows.Count > 0)
            {
                //组装xml数据
                returnID = Convert.ToInt32(dt.Rows[0]["counts"].ToString());
            }
            return returnID;
        }
        [WebMethod(Description = "未整改、未复查隐患查询")] 
        public string searchMessageHiden(int accID, int departID)
        {
            string returnXML = "";
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();
            string selectSql = "select count(hidenID)as counts,hidenFlag from cmf_danger_hiden " +
                       " where accID = " + accID + " and hidenFlag = 1 and departID = " + departID + " " +
                       " union ALL " +
                       " select count(hidenID) as counts,hidenFlag from cmf_danger_hiden " +
                       "  where  accID = " + accID + " " +
                       "and hidenFlag = 2 " +
                       " and departID = " + departID + " ";
            DataTable dt = safeDb.GetDataTable(selectSql);
            if (dt.Rows.Count > 0)
            {
                //组装xml数据
                returnXML = xmlDB.ConvertDataTableToXML(dt, "");
            }
            return returnXML;
        }

        [WebMethod(Description ="查询岗位风险巡查周期")]
        public string searchPartolStand(int departID)
        {
            string returnXML = "";
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();
            string selectSql = "select a.`name`,b.partolPeriod,b.partolCount from cmf_department a " +
                               "left join cmf_partol_standard b on a.partolSDID = b.satandartID " + 
                               "where a.id = " + departID + "";
            try
            {
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    returnXML = dt.Rows[0]["name"].ToString() + dt.Rows[0]["partolPeriod"].ToString() + "巡查" + dt.Rows[0]["partolCount"].ToString() + "次";
                }
            }
            catch (Exception ex)
            {

            }
            return returnXML;
        }
        #region 查询岗位隐患排查清单
        [WebMethod(Description = "岗位隐患排查清单查询")]
        public string select_hidenListInfo(int accID,int deptID)
        {
            string returnXML = "";
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();
            //查询当前部门id 的所有子节点部门id
            string childStr = selectDepartIDChild(deptID).Substring(2);
           
            string selectSql = "SELECT hidenListID,checkArea,checkObj,dangerYS,dangerType,dangerStandard,dangerLevel from cmf_hiden_list where accID = " + accID+ " and departID in ( " + childStr + ") ";
            DataTable dt = safeDb.GetDataTable(selectSql);
            if (dt.Rows.Count > 0)
            {
                //组装xml数据
                returnXML = xmlDB.ConvertDataTableToXML(dt, "");
            }
            return returnXML;
        }
        #endregion

        #region 查询岗位应急处置卡
        [WebMethod(Description = "岗位应急处置卡查询")]
        public string select_dangerControlInfo(int accID,int deptID)
        {
            string returnXML = "";
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();
            //查询当前部门id 的所有子节点部门id
            string childStr = selectDepartIDChild(deptID).Substring(2);
            string selectSql = "select contolID,dangerType,dangerMain,facIphone,deptIphone,emergencyIphone,fireIphone from cmf_danger_control where accID = " + accID+ " and departID in ("+ childStr + ") ";
            DataTable dt = safeDb.GetDataTable(selectSql);
            if (dt.Rows.Count > 0)
            {
                //组装xml数据
                returnXML = xmlDB.ConvertDataTableToXML(dt, "");
            }
            return returnXML;
        }
        #endregion
        #region 查询岗位风险告知卡
        [WebMethod(Description ="岗位风险告知卡查询")]
        public string select_dangerInfo(int accID,int deptID)
        {
            string returnXML = "";
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();
            //查询当前部门id 的所有子节点部门id
            string childStr = selectDepartIDChild(deptID).Substring(2);
            string selectSql = "select dangerID,workArea,dangerName,dangerInfo,accidentStand,accidentMeasures,dangerLevel from cmf_station_dangerinfo where accID = " + accID+" and departID in (" + childStr + ")";
            DataTable dt = safeDb.GetDataTable(selectSql);
            if (dt.Rows.Count > 0)
            {
                //组装xml数据
                returnXML = xmlDB.ConvertDataTableToXML(dt, "");
            }
            return returnXML;
        }
        #endregion
        [WebMethod(Description = "用户登录")]
        public string loginSafe(string loginCode, string passWord)
        {
            string returnXML = "";
            //mysql 数据表中的字段 设置为 utf-8  否则 查询会报错，
            //登录名为手机号
            CMoperation xmlDB = new CMoperation();
            SafeDBClasscs safeDb = new SafeDBClasscs();
            /**
             * user_status = 1 已审核
             * user_status = 0 未审核
             **/
            //string selectSql = "select a.id,a.user_nickname,a.user_login,b.workArea,a.mobile,b.stationCode,a.stationID,a.user_status,user_app_pass from cmf_user a " +
            //                   "left join cmf_station b on a.stationID = b.stationID " +
            //                   "where a.mobile = '" + loginCode + "' and a.user_pass = '"+passWord+"'";
            string selectSql = "select a.id as userID, a.user_app_pass,a.user_nickname,a.mobile,a.accID," +
                               "a.user_status,a.departID as departID,b.departCode,b.name,b.workArea,c.cCompanyName from cmf_user a " +
                               "left join cmf_department b on a.departID = b.id " +
                               "left join cmf_acc_info c on a.accID = c.accID " +
                               "where  a.mobile = '" + loginCode + "' and a.user_app_pass = '" + passWord + "'";
            try
            {
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    int userStatus = Convert.ToInt32(dt.Rows[0]["user_status"].ToString());
                    if (dt.Rows[0]["user_app_pass"].ToString() != passWord)
                    {
                        returnXML = "2";
                        return returnXML;
                    }
                    //判断是否审核
                    if (userStatus == 0)
                    {
                        returnXML = "1";
                        return returnXML;
                    }
                    else
                    {
                        //组装xml数据
                        returnXML = xmlDB.ConvertDataTableToXML(dt, "");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteYCLog(ex, "查询用户信息报错");
            }

            return returnXML;
        }
        [WebMethod(Description ="判断是否巡查")]
        public string partolStation(string stationCode)
        {
            string returnStr = "";
            SafeDBClasscs safeDb = new SafeDBClasscs();
            string partolTime = webTime();
            string selectSql = "select partolDepartment from cmf_danger_partol  where partolDpCode ='" + stationCode + "' and partolTime like '%"+ partolTime + "%' ";
            DataTable dt = safeDb.GetDataTable(selectSql);
            if (dt.Rows.Count > 0)
            {
                returnStr = dt.Rows[0]["partolDepartment"].ToString();
            }
            return returnStr;
        }
        [WebMethod(Description ="隐患统计")]
        public string select_hidenStatis(int accID,int  departID)
        {
            string xmlReturn = "";
            //定义隐患状态数组
            //int[] arrHidenFlag = new int[] { 0, 1, 2, 3 };
            List<string> hidenFlagNowList = new List<string>();
            List<string> hidenFlagList = new List<string>() { "0","1","2","3"};//初始化定义liat

            CMoperation cmop = new CMoperation();
            List<string> hidenFlagListUn = new List<string>();
            List<string> hidenFlagListUnion= new List<string>();
            SafeDBClasscs safeDB = new SafeDBClasscs();
            //查询子部门id
            string childStr = selectDepartIDChild(departID).Substring(2);
            //string selectSql = "select count(hidenFlag) AS counts,hidenFlag from cmf_danger_hiden " +
            //                    "where (departID in( " + childStr + " ) or abarDepartID in (" + childStr + ")) and accID = " + accID + " " +
            //                    "and iChangeHide = 0 group by hidenFlag  union all " +
            //                    "select count(hidenFlag) AS counts,hidenFlag from cmf_danger_hiden " +
            //                    "where (departID in( " + childStr + " ) or abarDepartID in (" + childStr + ")) and accID = " + accID + " " +
            //                    "and iChangeHide = 1 and bCheck = 1 group by hidenFlag ";
            string selectSql = "select count(hidenFlag) AS counts,hidenFlag from cmf_danger_hiden "+
                               "where(departID in (" + childStr + " ) or abarDepartID in (" + childStr + " )) and accID = "+accID+" " +
                               "and (iChangeHide = 0 or (iChangeHide = 1 and bCheck = 1)) " +
                               "group by hidenFlag";
            DataTable dt = safeDB.GetDataTable(selectSql);
            if (dt.Rows.Count > 0)
            {
                List<string> listName = new List<string>();

                for (int i=0;i<dt.Rows.Count;i++)
                {
                   //hidenDict.Add(dt.Rows[i]["hidenFlag"].ToString(), dt.Rows[i]["counts"].ToString());
                    hidenFlagNowList.Add(dt.Rows[i]["hidenFlag"].ToString());
                }
              
                //取两个list 没有的元素
                hidenFlagListUn = hidenFlagList.Except(hidenFlagNowList).ToList();
                DataRow newRow;
                for (int i=0;i<hidenFlagListUn.Count; i++)
                {
                    newRow = dt.NewRow();
                    newRow["counts"] = 0;
                    newRow["hidenFlag"] = hidenFlagListUn[i].ToString();
                    dt.Rows.Add(newRow);
                }
                //取两个list 共有的元素
                //hidenFlagListUn = hidenFlagList.Intersect(hidenFlagNowList).ToList();
                //取两个list 合并后的元素(保留重复项)
                //hidenFlagListUn = hidenFlagList.Concat(hidenFlagNowList).ToList();
                //取两个list 共有的元素(不保留重复项)
                // hidenFlagListUnion = hidenFlagList.Union(hidenFlagNowList).ToList();
            }
            else
            {
                DataRow newRow;
                for (int i = 0; i < hidenFlagList.Count; i++)
                {
                    newRow = dt.NewRow();
                    newRow["counts"] = 0;
                    newRow["hidenFlag"] = hidenFlagList[i].ToString();
                    dt.Rows.Add(newRow);
                }
            }
            //将table转为xml
            xmlReturn = cmop.ConvertDataTableToXML(dt, "");
            return xmlReturn;
        }
        [WebMethod(Description ="待审核隐患个数查询")]
        public int select_unCheckHiden(int accID, int departID)
        {
            int counts = 0;
            SafeDBClasscs safeDB = new SafeDBClasscs();
            //查询子部门id
            string selectSql = "select count(hidenID) as 个数 from cmf_danger_hiden where accID = "+accID+ " and iAuitDepartID = " + departID+ " and bCheck = 0 and iChangeHide = 1 and hidenFlag = 1";
            DataTable dt = safeDB.GetDataTable(selectSql);
            if (dt.Rows.Count > 0)
            {
                //将table转为xml
                counts = Convert.ToInt32(dt.Rows[0]["个数"].ToString());
            }
            
            return counts;
        }
        [WebMethod(Description ="保存巡查信息")]
        public int insertPartolInfo(string revXML)
        {
            int result = 0;
            SafeDBClasscs safeDB = new SafeDBClasscs();
            try
            {
                //xml转table
                StringReader StrStream = null;
                XmlTextReader Xmlrdr = null;
                DataSet ds = new DataSet();
                StrStream = new StringReader(revXML);
                //获取StrStream中的数据  
                Xmlrdr = new XmlTextReader(StrStream);
                //ds获取Xmlrdr中的数据                 
                ds.ReadXml(Xmlrdr);
                DataTable dt = ds.Tables[1];

                //判断当天是否已经巡查
                string partolTm = webTime();
                string selectSql = "select partolID from cmf_danger_partol where userID = " + Convert.ToInt32(dt.Rows[0]["userID"].ToString())+" " +
                                    "and partolTime like '%" + partolTm.Substring(0, 10) + "%' ";

                try
                {
                    DataTable dts = safeDB.GetDataTable(selectSql);
                    if (dts.Rows.Count == 0)
                    {
                        //未巡查 插入数据0
                        string insertSql = "insert into cmf_danger_partol(userID,partolPersonName,partolTime,partolFlag,partolStyleID,ts,accID) " +
                                           "values (" + Convert.ToInt32(dt.Rows[0]["userID"].ToString()) + ",'" + dt.Rows[0]["partolPersonName"].ToString() + "'," +
                                           "'" + dt.Rows[0]["partolTime"].ToString() + "','" + dt.Rows[0]["partolFlag"].ToString() + "'," +
                                           "'" + dt.Rows[0]["partolStyleID"].ToString() + "','" + dt.Rows[0]["partolTime"].ToString() + "','" + dt.Rows[0]["accID"].ToString() + "')";
                        if (safeDB.getsqlCom(insertSql) > 0)
                        {
                            result = 1;
                        }
                    }
                    else
                    {
                        //已巡查
                        result = 0;
                    }
                }
                catch (Exception ex)
                {
                    result = 2;
                    WriteYCLog(ex, "风险排查信息录入报错");
                }
            }
            catch (Exception ex)
            {
                result = 2;
                WriteYCLog(ex, "风险排查信息录入报错");
            }
            return result;
        }

        [WebMethod(Description = "保存巡查信息(不判断当天是否巡查)")]
        public int insertPartolInfoDay(string revXML)
        {
            int result = 0;
            SafeDBClasscs safeDB = new SafeDBClasscs();
            try
            {
                //xml转table
                StringReader StrStream = null;
                XmlTextReader Xmlrdr = null;
                DataSet ds = new DataSet();
                StrStream = new StringReader(revXML);
                //获取StrStream中的数据  
                Xmlrdr = new XmlTextReader(StrStream);
                //ds获取Xmlrdr中的数据                 
                ds.ReadXml(Xmlrdr);
                DataTable dt = ds.Tables[1];
                try
                {
                    //未巡查 插入数据0
                    string insertSql = "insert into cmf_danger_partol(userID,partolPersonName,partolTime,partolFlag,partolStyleID,ts,accID,departID) " +
                                       "values (" + Convert.ToInt32(dt.Rows[0]["userID"].ToString()) + ",'" + dt.Rows[0]["partolPersonName"].ToString() + "'," +
                                       "'" + dt.Rows[0]["partolTime"].ToString() + "','" + dt.Rows[0]["partolFlag"].ToString() + "'," +
                                       "'" + dt.Rows[0]["partolStyleID"].ToString() + "','" + dt.Rows[0]["partolTime"].ToString() + "','" + dt.Rows[0]["accID"].ToString() + "','" + dt.Rows[0]["departID"].ToString() + "')";
                    if (safeDB.getsqlCom(insertSql) > 0)
                    {
                        result = 1;
                    }
                }
                catch (Exception ex)
                {
                    result = 2;
                    WriteYCLog(ex, "风险排查信息录入报错");
                }
            }
            catch (Exception ex)
            {
                result = 2;
                WriteYCLog(ex, "风险排查信息录入报错");
            }
            return result;
        }
        [WebMethod(Description ="查询当前用户部门对应的下级部门")]
        public string selectChileDepartInfo(int departID)
        {
            string returnXML = "";
            CMoperation xmlDB = new CMoperation();
            SafeDBClasscs safeDb = new SafeDBClasscs();
            //
            string selectSql = "select departCode,name from cmf_department where parent_id = " + departID + "  order by id";
            try
            {
                //存在记录，不是末级部门；不存在记录，是末级部门，没有指定整改单位权限
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    returnXML = xmlDB.ConvertDataTableToXML(dt,"");
                }
                else
                {
                    returnXML = "false";
                }
            }
            catch(Exception ex)
            {
                WriteYCLog(ex,"查询下级部门报错");
            }
            return returnXML; 
        }
        [WebMethod(Description = "查询整改单位ID、整改人")]
        public string selectAbarDepartID(string departName,int accID)
        {
            string returnXml = "";
            CMoperation xmlDB = new CMoperation();
            SafeDBClasscs safeDb = new SafeDBClasscs();
            int departID = 0;
            //string selectSql = "select id from cmf_department where name = '" + departName + "'";
            string selectSql = "select a.id,b.user_nickname from cmf_department a " +
                               "left join cmf_user b on a.id = b.departID " +
                               "where a.name = '"+departName+"' and b.accID = "+accID+"";
            try
            {
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    returnXml = xmlDB.ConvertDataTableToXML(dt,"");
                   // departID = Convert.ToInt32(dt.Rows[0]["id"].ToString());
                }
            }
            catch (Exception ex)
            {
                WriteYCLog(ex,"查询部门id报错");
            }
            return returnXml;
        }

        [WebMethod(Description = "查询审核单位ID")]
        public int selectAuitDepartID(string departName)
        {
            CMoperation xmlDB = new CMoperation();
            SafeDBClasscs safeDb = new SafeDBClasscs();
            int departID = 0;
            string selectSql = "select id from cmf_department where name = '" + departName + "'";
            try
            {
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    departID = Convert.ToInt32(dt.Rows[0]["id"].ToString());
                }
            }
            catch (Exception ex)
            {
                WriteYCLog(ex, "查询部门id报错");
            }
            return departID;
        }

        [WebMethod(Description = "查询分厂 车间 岗位")]      
        public string[] deptSearch(string role,string userCode)
        {
            string selectSql = "";
            List<string> deptList = new List<string>();
            SafeDBClasscs safeDb = new SafeDBClasscs();
         
            if (role == "岗位管理员")
            {
                 selectSql = "select b.facName,c.deptName,a.stationName from cmf_station a " +
                                   "left join cmf_factory b on a.facID = b.facID " +
                                   "left join cmf_dept c on a.deptID = c.deptID " +
                                   "where a.stationCode = '"+userCode+"'";
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    deptList.Add( dt.Rows[0]["facName"].ToString());
                    deptList.Add(dt.Rows[0]["deptName"].ToString());
                    deptList.Add(dt.Rows[0]["stationName"].ToString());
                    deptList.Add(dt.Rows[0]["facName"].ToString() + dt.Rows[0]["deptName"].ToString() + dt.Rows[0]["stationName"].ToString());
                }
                else
                {
                    deptList.Add("");
                }
            }
            else if (role == "车间管理员")
            {
                selectSql = "select b.facName,a.deptName from cmf_dept a " +
                            "left join cmf_factory b on a.iFacID = b.facID " +
                            "where a.deptCode = '" + userCode + "'";
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    deptList.Add(dt.Rows[0]["facName"].ToString());
                    deptList.Add(dt.Rows[0]["deptName"].ToString());
                    deptList.Add("");

                    deptList.Add(dt.Rows[0]["facName"].ToString() + dt.Rows[0]["deptName"].ToString());
                }
                else
                {
                    deptList.Add("");
                }
            }
            else if (role == "分厂管理员")
            {
                selectSql = "select facName from cmf_factory where facCode = '" + userCode + "'";
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    deptList.Add(dt.Rows[0]["facName"].ToString());
                    deptList.Add("");
                    deptList.Add("");

                    deptList.Add(dt.Rows[0]["facName"].ToString());
                }
                else
                {
                    deptList.Add("");
                }
            }
            string[] arrDept = deptList.ToArray();
            return arrDept;
        }
        [WebMethod(Description = "查询用户角色")]
        public string roleSafe(int userID)
        {
            string roleName = "";
            SafeDBClasscs safeDb = new SafeDBClasscs();
            string selectSql = "select a.name from cmf_role a left join cmf_role_user b on a.id = b.role_id where b.user_id = " + userID + " ";
            DataTable dt = safeDb.GetDataTable(selectSql);
            if (dt.Rows.Count > 0)
            {
                roleName = dt.Rows[0]["name"].ToString();
            }
            return roleName;
        }
        [WebMethod(Description ="查询隐患排查方式")]
        public string selectHidenPartolType()
        {
            string returnXml = "";
            CMoperation xmlDB = new CMoperation();
            SafeDBClasscs safeDB = new SafeDBClasscs();
            string selectSql = "select partolStyleID,partolStyleName from cmf_partol_style";
            try
            {
                DataTable dt = safeDB.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    returnXml = xmlDB.ConvertDataTableToXML(dt, "");
                }
            }
            catch (Exception ex )
            {
                WriteYCLog(ex,"查询排查方式报错");
            }
            return returnXml;
        }
        [WebMethod(Description = "查询隐患排查方式ID")]
        public int selectHidenPartolTypeID(string partolStyleName)
        {
            int partolStyleID = 0;
            CMoperation xmlDB = new CMoperation();
            SafeDBClasscs safeDB = new SafeDBClasscs();
            string selectSql = "select partolStyleID  from cmf_partol_style where partolStyleName ='"+ partolStyleName + "'";
            
            DataTable dt = safeDB.GetDataTable(selectSql);
            if (dt.Rows.Count > 0)
            {
                partolStyleID = Convert.ToInt32(dt.Rows[0]["partolStyleID"].ToString());
            }
            return partolStyleID;
        }
        [WebMethod(Description = "查询隐患类别")]
        public string selectHidenType()
        {
            string returnXml = "";
            CMoperation xmlDB = new CMoperation();
            SafeDBClasscs safeDB = new SafeDBClasscs();
            string selectSql = "select dangerTypeID,dangerTypeName from cmf_danger_type";
            try
            {
                DataTable dt = safeDB.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    returnXml = xmlDB.ConvertDataTableToXML(dt, "");
                }
            }
            catch (Exception ex)
            {
                WriteYCLog(ex,"查询隐患类别报错");
            }
           
            return returnXml;
        }
        [WebMethod(Description = "查询隐患类别ID")]
        public int selectHidenTypeID(string hidenType)
        {
            int hidenTypeID = 0;
            CMoperation xmlDB = new CMoperation();
            SafeDBClasscs safeDB = new SafeDBClasscs();
            string selectSql = "select dangerTypeID from cmf_danger_type where dangerTypeName = '"+hidenType+"'";
            DataTable dt = safeDB.GetDataTable(selectSql);
            if (dt.Rows.Count > 0)
            {
                hidenTypeID = Convert.ToInt32(dt.Rows[0]["dangerTypeID"].ToString());
            }
            return hidenTypeID;
        }

        [WebMethod(Description = "隐患信息录入")]
        public bool insertHidenInfo( string revXML)
        {
            bool result = false;
            string insertSql = "";
            SafeDBClasscs safeDB = new SafeDBClasscs();
            string hidenTime = webTimeMysql();
            if (revXML != "")
            {
                //xml转table
                StringReader StrStream = null;
                XmlTextReader Xmlrdr = null;
                DataSet ds = new DataSet();
                StrStream = new StringReader(revXML);
                //获取StrStream中的数据  
                Xmlrdr = new XmlTextReader(StrStream);
                //ds获取Xmlrdr中的数据                 
                ds.ReadXml(Xmlrdr);
                DataTable dt = ds.Tables[1];
                //判断当天是否已经巡查
                try
                {
                    //上传图片到服务器 
                    //保存图片路径
                    //  string imagePath = "/" + arr[0];
                    string imagePath = "/" + dt.Rows[0]["cImgFile"].ToString();
                  //  string FilePath = Server.MapPath(imagePath);//创建文件夹
                    string FilePath = "D:/cloudsafe/public/static/hidenImage" + imagePath;
                    //判断路径是否存在
                    if (!Directory.Exists(FilePath))
                    {
                        //创建路径
                        Directory.CreateDirectory(FilePath);
                    }
                    //string SaveFilePath = Path.Combine(FilePath, arr[11]);//两个边路 文件夹地址 图片名称
                    string SaveFilePath = Path.Combine(FilePath, dt.Rows[0]["cImgName"].ToString());//两个边路 文件夹地址 图片名称
                    byte[] fileBytes;

                    //fileBytes = Convert.FromBase64String(arr[12]);//变量为图片字节流
                    fileBytes = Convert.FromBase64String(dt.Rows[0]["cImgBytes"].ToString());//变量为图片字节流
                    MemoryStream memoryStream = new MemoryStream(fileBytes); //1.定义并实例化一个内存流，以存放提交上来的字节数组。  
                    FileStream fileUpload = new FileStream(SaveFilePath, FileMode.Create); ///2.定义实际文件对象，保存上载的文件。  
                    memoryStream.WriteTo(fileUpload); ///3.把内存流里的数据写入物理文件  
                    memoryStream.Close();
                    fileUpload.Close();
                    fileUpload = null;
                    memoryStream = null;

                    string FilePathImg = FilePath + dt.Rows[0]["cImgName"].ToString();
                    FilePathImg = FilePathImg.Replace("\\", "\\\\");

                    insertSql = "insert into cmf_danger_hiden(hidenPersonID,hidenPersonName,hidenTime,hidenPtrStyleID,hidenTypeID,hidenLevel,hidenInfo,accID,hidenFlag,hidenImg,departID) values " +
                                   "(" + Convert.ToInt32(dt.Rows[0]["hidenPersonID"].ToString()) + ",'"+ dt.Rows[0]["hidenPersonName"].ToString() + "','" + dt.Rows[0]["hidenTime"].ToString() + "'," + Convert.ToInt32(dt.Rows[0]["hidenPtrStyleID"].ToString()) + "," +
                                   "" + Convert.ToInt32(dt.Rows[0]["hidenTypeID"].ToString()) + ",'" + dt.Rows[0]["hidenLevel"].ToString() + "'," +
                                   "'" + dt.Rows[0]["hidenInfo"].ToString() + "',"+Convert.ToInt32(dt.Rows[0]["accID"].ToString())+",0,'"+FilePathImg+"','"+ dt.Rows[0]["departID"].ToString() + "')";
                    WriteID(insertSql,"隐患录入");
                    if (safeDB.getsqlCom(insertSql) > 0)
                    {
                        result = true;
                    }
                }
                catch (Exception ex)
                {
                    result = false;
                    WriteYCLog(ex, "保存隐患上报信息sql语句" + insertSql);
                }
            }
            else
            {
                result =  false;
            }
            return result;
           
        }
        [WebMethod(Description = "下发整改通知单")]
        public bool updateReformInfo(int autoID, string revXML)
        {
            //arr 0分厂 1车间 2整改类型 3整改期限 4整改要求  5AutoID
            bool result = false;
            string updateSql = "";
            SafeDBClasscs safeDB = new SafeDBClasscs();
            try
            {
                //解析XML数据
                //xml转table
                StringReader StrStream = null;
                XmlTextReader Xmlrdr = null;
                DataSet ds = new DataSet();
                StrStream = new StringReader(revXML);
                //获取StrStream中的数据  
                Xmlrdr = new XmlTextReader(StrStream);
                //ds获取Xmlrdr中的数据                 
                ds.ReadXml(Xmlrdr);
                DataTable dt = ds.Tables[1];
                if (dt.Rows.Count > 0)
                {
                    updateSql = "update cmf_danger_hiden set abarDepartID = " + Convert.ToInt32(dt.Rows[0]["abarDepartID"].ToString()) + ",abarStyle = '" + dt.Rows[0]["abarStyle"].ToString() + "',abarbeitungPerson = '"+ dt.Rows[0]["abarbeitungPerson"].ToString() + "'," +
                             "abarbeitungTime = '" + dt.Rows[0]["abarbeitungTime"].ToString() + "',abarRequest = '" + dt.Rows[0]["abarRequest"].ToString() + "',hidenFlag = 1 " +
                             "where hidenID = '" + autoID + "'";
                    if (safeDB.getsqlCom(updateSql) > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteYCLog(ex,"录入整改信息失败" + updateSql);
            }
            return result;
        }
        [WebMethod(Description = "整改人信息录入")]
        public bool updateReformSureInfo(int autoID, string revXML)
        {
            //arr 0整改责任人  1整改日期 
            bool result = false;
            SafeDBClasscs safeDB = new SafeDBClasscs();
            try
            {
                //解析XML数据
                //xml转table
                StringReader StrStream = null;
                XmlTextReader Xmlrdr = null;
                DataSet ds = new DataSet();
                StrStream = new StringReader(revXML);
                //获取StrStream中的数据  
                Xmlrdr = new XmlTextReader(StrStream);
                //ds获取Xmlrdr中的数据                 
                ds.ReadXml(Xmlrdr);
                DataTable dt = ds.Tables[1];

                string updateSql = "update cmf_danger_hiden set abarbeitungPerson = '" + dt.Rows[0]["abarbeitungPerson"].ToString() +"'," +
                                   "abarbeitungPerson_2 = '" + dt.Rows[0]["abarbeitungPerson_2"].ToString() + "', "+
                                 
                                   "abarTime = '" + dt.Rows[0]["abarTime"].ToString() + "'," +
                                   "abarTS = '" + dt.Rows[0]["abarTS"].ToString() + "'," +
                                   "abarResult = '" + dt.Rows[0]["abarResult"].ToString() + "', " +
                                   "hidenFlag = 2  " +
                                   "where hidenID = '" + autoID + "'";
                if (safeDB.getsqlCom(updateSql) > 0)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                WriteYCLog(ex,"录入整改人报错");
            }
           
            return result;
        }
        [WebMethod(Description = "整改人信息录入2")]
        public bool updateReformSureInfoNew(int autoID, string revXML)
        {
            //arr 0整改责任人  1整改日期 
            bool result = false;
            SafeDBClasscs safeDB = new SafeDBClasscs();
            try
            {
                //解析XML数据
                //xml转table
                StringReader StrStream = null;
                XmlTextReader Xmlrdr = null;
                DataSet ds = new DataSet();
                StrStream = new StringReader(revXML);
                //获取StrStream中的数据  
                Xmlrdr = new XmlTextReader(StrStream);
                //ds获取Xmlrdr中的数据                 
                ds.ReadXml(Xmlrdr);
                DataTable dt = ds.Tables[1];

                //上传图片到服务器 
                //保存图片路径
                //  string imagePath = "/" + arr[0];
                string imagePath = "/" + dt.Rows[0]["cImgFile"].ToString();
                //  string FilePath = Server.MapPath(imagePath);//创建文件夹
                string FilePath = "D:/cloudsafe/public/static/abarImage" + imagePath;
                //判断路径是否存在
                if (!Directory.Exists(FilePath))
                {
                    //创建路径
                    Directory.CreateDirectory(FilePath);
                }
                //string SaveFilePath = Path.Combine(FilePath, arr[11]);//两个边路 文件夹地址 图片名称
                string SaveFilePath = Path.Combine(FilePath, dt.Rows[0]["cImgName"].ToString());//两个边路 文件夹地址 图片名称
                byte[] fileBytes;

                //fileBytes = Convert.FromBase64String(arr[12]);//变量为图片字节流
                fileBytes = Convert.FromBase64String(dt.Rows[0]["cImgBytes"].ToString());//变量为图片字节流
                MemoryStream memoryStream = new MemoryStream(fileBytes); //1.定义并实例化一个内存流，以存放提交上来的字节数组。  
                FileStream fileUpload = new FileStream(SaveFilePath, FileMode.Create); ///2.定义实际文件对象，保存上载的文件。  
                memoryStream.WriteTo(fileUpload); ///3.把内存流里的数据写入物理文件  
                memoryStream.Close();
                fileUpload.Close();
                fileUpload = null;
                memoryStream = null;

                string FilePathImg = FilePath + dt.Rows[0]["cImgName"].ToString();
                FilePathImg = FilePathImg.Replace("\\", "\\\\");

                string updateSql = "update cmf_danger_hiden set " +
                                   "abarbeitungPerson_2 = '" + dt.Rows[0]["abarbeitungPerson_2"].ToString() + "', " +
                                   "abarImg = '" + FilePathImg + "', " +
                                   "abarTime = '" + dt.Rows[0]["abarTime"].ToString() + "'," +
                                   "abarTS = '" + dt.Rows[0]["abarTS"].ToString() + "'," +
                                   "abarResult = '" + dt.Rows[0]["abarResult"].ToString() + "', " +
                                   "hidenFlag = 2  " +
                                   "where hidenID = '" + autoID + "'";
                if (safeDB.getsqlCom(updateSql) > 0)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                WriteYCLog(ex, "录入整改人报错");
            }

            return result;
        }

        [WebMethod(Description = "整改信息复查人录入")]
        public bool updateReformCheckInfo(int autoID, string revXML)
        {
            //arr 0整改责任人  1整改日期 
            bool result = false;
            SafeDBClasscs safeDB = new SafeDBClasscs();
            try
            {
                //解析XML数据
                //xml转table
                StringReader StrStream = null;
                XmlTextReader Xmlrdr = null;
                DataSet ds = new DataSet();
                StrStream = new StringReader(revXML);
                //获取StrStream中的数据  
                Xmlrdr = new XmlTextReader(StrStream);
                //ds获取Xmlrdr中的数据                 
                ds.ReadXml(Xmlrdr);
                DataTable dt = ds.Tables[1];

                string updateSql = "update cmf_danger_hiden set reviewPerson = '" +dt.Rows[0]["reviewPerson"].ToString() + "',"+
                                    "reviewResult = '" + dt.Rows[0]["reviewResult"].ToString() + "',"+
                                    "reviewTime = '" + dt.Rows[0]["reviewTime"].ToString() + "'," +
                                    "reviewTS = '" + dt.Rows[0]["reviewTS"].ToString() + "'," +
                                    "hidenFlag = 3 " +
                                    "where hidenID = '" + autoID + "'";
                if (safeDB.getsqlCom(updateSql) > 0)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                WriteYCLog(ex,"复查人信息更新报错");
            }
            return result;
        }
        [WebMethod(Description ="隐患信息查询")]
        public string hidenInfoList(int deptID,int hidenFlag,int accID)
        {
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDb = new CMoperation();
            string returnXml = "";
            string selectSql = "";
            //根据登录用户部门id查询上级部门id集合
            ArrayList childList = new ArrayList();
            string hidenTime = webTime();
         
            DateTime hiden_time = Convert.ToDateTime(hidenTime);
           
            hidenTime = hiden_time.ToString("yyyy-MM");
            //获取下级部门
            string childStr = selectDepartIDChild(deptID).Substring(2);
            //截取逗号 组成数组
            string[] childArr = childStr.Split(',');
            //去除第一个逗号和$符号
           
            try
            {
                if (hidenFlag != 4)
                {
                    if (hidenFlag == 1)
                    {
                        //待整改 审核后可见
                        selectSql = "(select a.hidenID,a.hidenPersonName,b.user_nickname,a.hidenTime,a.hidenInfo,d.name,a.hidenImg,a.abarImg," +
                              "(case hidenFlag when 0 then '已上报' when 1 then '待整改' when 2 then '待复查' when 3 then '已完成' end) as hidenFlag " +
                              "from cmf_danger_hiden a  left join cmf_user b on a.hidenPersonID = b.id " +
                              "left join cmf_department d on b.departID = d.id where  a.hidenFlag = " + hidenFlag + " and a.accID = " + accID + " and  iChangeHide =0  " +
                              "and (a.departID in (" + childStr + ") or abarDepartID in (" + childStr + "))) union all " +
                              "(select a.hidenID,a.hidenPersonName,b.user_nickname,a.hidenTime,a.hidenInfo,d.name,a.hidenImg,a.abarImg," +
                              "(case hidenFlag when 0 then '已上报' when 1 then '待整改' when 2 then '待复查' when 3 then '已完成' end) as hidenFlag " +
                              "from cmf_danger_hiden a  left join cmf_user b on a.hidenPersonID = b.id " +
                              "left join cmf_department d on b.departID = d.id where  a.hidenFlag = " + hidenFlag + " and a.accID = " + accID + " and iChangeHide =1 and bCheck = 1  " +
                              "and (a.departID in (" + childStr + ") or abarDepartID in (" + childStr + "))) order by hidenID desc";
                              
                    }
                    else
                    {
                        selectSql = "select a.hidenID,a.hidenPersonName,b.user_nickname,a.hidenTime,a.hidenInfo,d.name,a.hidenImg,a.abarImg," +
                              "(case hidenFlag when 0 then '已上报' when 1 then '待整改' when 2 then '待复查' when 3 then '已完成' end) as hidenFlag " +
                              "from cmf_danger_hiden a  left join cmf_user b on a.hidenPersonID = b.id " +
                              "left join cmf_department d on b.departID = d.id where  a.hidenFlag = " + hidenFlag + "   and a.accID = " + accID + " "+
                              "and (a.departID in (" + childStr + ") or abarDepartID in (" + childStr + ")) order by a.hidenID desc ";
                    }
                   
                }
                else
                {
                    selectSql = "select a.hidenID,a.hidenPersonName,b.user_nickname,a.hidenTime,a.hidenInfo,d.name,a.hidenImg,a.abarImg," +
                               "(case hidenFlag when 0 then '已上报' when 1 then '待整改' when 2 then '待复查' when 3 then '已完成' end) as hidenFlag " +
                               "from cmf_danger_hiden a  left join cmf_user b on a.hidenPersonID = b.id " +
                               "left join cmf_department d on b.departID = d.id where  a.accID = " + accID + " and hidenTime like  '%"+hidenTime+ "%' and a.departID in (" + childStr + ") " +
                               "order by a.hidenID desc";
                }
               
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    //table 转xml
                    returnXml = xmlDb.ConvertDataTableToXML(dt, "");
                }
            }
            catch (Exception ex)
            {
                WriteYCLog(ex,"查询隐患信息错误");
            }
          
            return returnXml;
        }

        [WebMethod(Description = "隐患信息指定查询")]
        public string selecthidenInfo(int autoID,int hidenFlag,int accID)
        {
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDb = new CMoperation();
            string returnXml = "";
            string hidenTm = webTime();
            DateTime hiden_time = Convert.ToDateTime(hidenTm);
            hidenTm = hiden_time.ToString("yyyy-MM");
            //selectSql = "select AutoID,hidenPerson,hidenTiem,hidenInfo,concat(hidenFac,hidenDept,hidenStation) as hidenDpt," +
            //            "(case hidenFlag  when 0 then '已上报' when 1 then '待整改' when 2 then '待复查' when 3 then '已完成' end)as hidentype, " +
            //            "abarbeitungFac,abarbeitungDept,abarStyle,abarbeitungTm,abrRequest,abarbeitungPerson,abarTime,reviewPerson "+
            //            "from cmf_danger_hiden where AutoID = " + autoID + " and hidenTiem like '%" + hidenTime + "%' and hidenFlag = " + hidenFlag + " order by AutoID desc";

            string selectSql = "select a.hidenID,a.departID,d.name as 隐患上报部门,b.user_nickname,a.hidenTime, e.dangerTypeName,f.partolStyleName,a.hidenImg,a.abarImg," +
                               "a.hidenLevel,a.hidenInfo,a.abarDepartID,g.name as 隐患整改部门,a.abarStyle,a.abarbeitungTime,"+
                               "a.abarRequest,a.abarbeitungPerson,a.abarbeitungPerson_2,a.abarTime,a.abarResult,a.reviewPerson,a.reviewResult,a.reviewTime," +                          
                               "(case hidenFlag when 0 then '已上报' when 1 then '待整改' when 2 then '待复查' when 3 then '已完成' end) as hidenFlag "+
                               "from cmf_danger_hiden a left join cmf_user b on a.hidenPersonID = b.id " +
                               "left join cmf_department d on b.departID = d.id " +
                               "left join cmf_danger_type e on a.hidenTypeID = e.dangerTypeID " +
                               "left join cmf_partol_style f on a.hidenPtrStyleID = f.partolStyleID " +
                               "left join cmf_department g on a.abarDepartID = g.id " +
                               "where a.hidenID = " + autoID + "  and a.hidenFlag = " + hidenFlag + " and a.accID = "+accID+ " ";

            try
            {
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    //table 转xml
                    returnXml = xmlDb.ConvertDataTableToXML(dt, "");
                } 
            }
            catch (Exception ex)
            {
                WriteYCLog(ex,"隐患信息查询失败");
            }
            return returnXml;
        }
        [WebMethod(Description = "安全确认查询")]
        public string searchSafeSureInfo( int departID)
        {
            string returnXML = "";
            string selectSql = "";

            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();

            selectSql = "select a.dangerID,a.dangerName,a.workArea,a.dangerName,a.dangerLevel,b.name as departName,c.equipmentName,d.workActiveName,e.workplaceName,f.dangerLevelName," +
                        "a.dangerObj,a.dangerInfo,a.accidentMeasures,a.accidentStand from  cmf_station_dangerinfo a " +
                        "left join cmf_department b on a.departID = b.id " +
                        "left join cmf_equipment c on a.equimentID = c.equipmentID " +
                        "left join cmf_workactive d on a.workactiveID = d.workActivID " +
                        "left join cmf_workplace e on a.workplaceID = e.workplaceID " +
                        "left join cmf_danger_level f on a.dangerLevelID = f.dangerLevelID where  b.id = " + departID + "";
            try
            {
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    //table 转xml
                    returnXML = xmlDB.ConvertDataTableToXML(dt, "");
                }
            }
            catch (Exception ex)
            {
                WriteYCLog(ex, "风险点信息查询报错");
            }

            return returnXML;

        }
        [WebMethod(Description ="安全确认提交")]
        public int subSafeRecord(string revXML)
        {
            int result = 0;
            SafeDBClasscs safeDB = new SafeDBClasscs();
            try
            {
                //xml转table
                StringReader StrStream = null;
                XmlTextReader Xmlrdr = null;
                DataSet ds = new DataSet();
                StrStream = new StringReader(revXML);
                //获取StrStream中的数据  
                Xmlrdr = new XmlTextReader(StrStream);
                //ds获取Xmlrdr中的数据                 
                ds.ReadXml(Xmlrdr);
                DataTable dt = ds.Tables[1];

                //判断当天是否已经巡查
                string partolTm = webTime();
                string selectSql = "select partolID from cmf_confirmation_record where userID = " + Convert.ToInt32(dt.Rows[0]["userID"].ToString()) + " " +
                                    "and partolTime like '%" + partolTm.Substring(0, 10) + "%' ";

                try
                {
                    DataTable dts = safeDB.GetDataTable(selectSql);
                    if (dts.Rows.Count == 0)
                    {
                        //未巡查 插入数据0
                        string insertSql = "insert into cmf_confirmation_record(userID,partolPersonName,partolTime,partolFlag,ts,accID,bHidenExit,bSubHiden,bLogHiden) " +
                                           "values (" + Convert.ToInt32(dt.Rows[0]["userID"].ToString()) + ",'" + dt.Rows[0]["partolPersonName"].ToString() + "'," +
                                           "'" + dt.Rows[0]["partolTime"].ToString() + "','" + dt.Rows[0]["partolFlag"].ToString() + "'," +
                                           "'" + dt.Rows[0]["partolTime"].ToString() + "','" + dt.Rows[0]["accID"].ToString() + "','" + dt.Rows[0]["bHidenExit"].ToString() + "','" + dt.Rows[0]["bSubHiden"].ToString() + "','" + dt.Rows[0]["bLogHiden"].ToString() + "')";
                        if (safeDB.getsqlCom(insertSql) > 0)
                        {
                            result = 1;
                        }
                    }
                    else
                    {
                        //已巡查
                        result = 0;
                    }
                }
                catch (Exception ex)
                {
                    result = 2;
                    WriteYCLog(ex, "安全确认添加记录");
                }
            }
            catch (Exception ex)
            {
                result = 2;
                WriteYCLog(ex, "风险排查信息录入报错");
            }
            return result;
        }
        [WebMethod(Description ="岗位风险信息查询")]
        public string searchDangerInfo(int departID)
        {
            string returnXML = "";
            string selectSql = "";

            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();

            selectSql = "select a.dangerID,a.dangerName,b.name as departName,c.equipmentName,d.workActiveName,e.workplaceName,f.dangerLevelName," +
                        "a.dangerObj,a.dangerInfo,a.accidentMeasures,a.accidentStand from  cmf_station_dangerinfo a " +
                        "left join cmf_department b on a.departID = b.id " +
                        "left join cmf_equipment c on a.equimentID = c.equipmentID " +
                        "left join cmf_workactive d on a.workactiveID = d.workActivID " +
                        "left join cmf_workplace e on a.workplaceID = e.workplaceID " +
                        "left join cmf_danger_level f on a.dangerLevelID = f.dangerLevelID where  b.id = "+ departID + "";
            try
            {
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    //table 转xml
                    returnXML = xmlDB.ConvertDataTableToXML(dt, "");
                }
            }
            catch (Exception ex)
            {
                WriteYCLog(ex,"风险点信息查询报错");
            }
           
            return returnXML;
        }
        [WebMethod(Description ="风险巡查记录查询")]
        public string searchPartolData(int userID)
        {
            string returnXML = "";
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();

            string partolTime = webTime();
            //string selectSql = "select partolID,partolPeople,partolFac,partolDept,partolStation,partolTime," +
            //                   "(case partolFlag  when 0 then '未巡查' ELSE '已巡查'end) as parflagg," +
            //                    "(case hidenPtrStyle  when '巡检检查' then '已上报隐患' ELSE '正常' end) as hidentype " +
            //                    "from cmf_danger_partol where partolDpCode = '" + userCode + "' and partolTime like '%"+ partolTime + "%' "+
            //                    "order  by AutoID,parflagg desc";
            string selectSql = "select a.partolID,a.partolPersonName,a.partolTime,c.name," +
                               "(case when partolFlag = 1 then '已巡查' else '未巡查' end) as partolFlag "+
                               " from cmf_danger_partol a " +
                               " left join cmf_user b on a.userID = b.id " +
                                "left join cmf_department c on b.departID = c.id " +
                                "where a.userID = "+ userID + "";
            try
            {
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    //table 转xml
                    returnXML = xmlDB.ConvertDataTableToXML(dt, "");
                }
            }
            catch (Exception ex)
            {
                WriteYCLog(ex,"巡查记录查询报错");
            }
           
            return returnXML;
        }
        [WebMethod(Description = "岗位设备信息")]
        public string searchEquipmentData(int departID)
        {
            string returnXML = "";
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();
            string selectSql = "select a.equipmentCode,a.equipmentName,c.stationName,a.equipmentID,a.equipmentControlName from cmf_equipment a  " +
                               "left join cmf_equipment_type b on a.equipmentType = b.equipmentTypeID " +
                               "left join cmf_station c on c.departID = a.departID " +
                               "where a.departID = " + departID + "";
            //组装xml
            DataTable dt = safeDb.GetDataTable(selectSql);
            if (dt.Rows.Count > 0)
            {
                returnXML = xmlDB.ConvertDataTableToXML(dt, "");
            }
            return returnXML;
        }
        [WebMethod(Description = "岗位设备控制点信息")]
        public string searchEquipmentControlData(string equipmentName)
        {
            string returnXML = "";
            SafeDBClasscs safeDb = new SafeDBClasscs();
            CMoperation xmlDB = new CMoperation();
            string selectSql = "select equipmentControlName from cmf_equipment where equipmentName = '"+ equipmentName + "'";
            //组装xml
            DataTable dt = safeDb.GetDataTable(selectSql);
            if (dt.Rows.Count > 0)
            {
                returnXML = xmlDB.ConvertDataTableToXML(dt, "");
            }
            return returnXML;
        }
        [WebMethod(Description = "插入设备点巡检记录")]
        public bool insertEquipmentPartolData(string[] arr)
        {
            bool result = false;
            string partolTime = webTime();
            SafeDBClasscs safeDb = new SafeDBClasscs();

            string[] arrStr = arr[3].Split(',');

            //服务器端创建文件夹
           // Directory.CreateDirectory(Server.MapPath("~/"));

            //上传图片到服务器  图片保存路径 图片名称 图片字节流
            string imgPath = partolTime.Substring(0, 10) + "/" + arrStr[0] + "/" + arrStr[2] + "/" + arr[1] + "/";

            string FilePathImg = saveImg( imgPath, arr[4],  arr[2]);
            if (FilePathImg != "")
            {
                //arr 0 巡查人名称  1 设备名称 2 图片 3 岗位名称
                string insertSql = "insert into cmf_equipment_partol(equipmentPartolName,equipmentPartolTime,equipmentName,imgAdress,satationName,equipmentCode) values " +
                                   "('" + arr[0] + "','" + partolTime + "','" + arr[1] + "','" + FilePathImg + "','" + arr[3] + "','"+arr[5]+"')";
                if (safeDb.getsqlCom(insertSql) > 0)
                {
                    result = true;
                }
            }
            return result;
        }
        [WebMethod(Description = "岗位信息查询")]
        public string searchStationData(int departID)
           {
                string stationStr = "";
                SafeDBClasscs safeDb = new SafeDBClasscs();
                string selectSql = "select a.stationName,b.facName,c.deptName from cmf_station a " +
                                   "left join cmf_factory b on a.facID = b.facID " +
                                   "left join cmf_dept c on a.deptID = c.deptID " +
                                   "where a.departID = " + departID + "";
                DataTable dt = safeDb.GetDataTable(selectSql);
                if (dt.Rows.Count > 0)
                {
                    stationStr = dt.Rows[0]["facName"].ToString() + "," + dt.Rows[0]["deptName"].ToString() + "," + dt.Rows[0]["stationName"].ToString();
                }
                return stationStr;
           }
        #endregion

        #region 保存图片到服务器指定文件夹
        private string saveImg(string imgPath,string imgName,string imgStr)
        {
            //imgPath ----- 图片的路径(文件夹名称 日期/分厂名称/岗位名称)
            //imgName ----- 图片的名称
            //imgStr ----- 图片字节流
            string FilePathImg = "";//图片地址
            //保存图片路径
            string imagePath = "/" + imgPath;
           // imagePath = imagePath.Replace("J:\\GTHYWEB\\GTHY\\","http:\\192.168.218.201:8057\\");
            string FilePath = Server.MapPath(imagePath);
            //判断路径是否存在
            if (!Directory.Exists(FilePath))
            {
                //创建路径
                Directory.CreateDirectory(FilePath);
            }
            string SaveFilePath = Path.Combine(FilePath, imgName);//两个边路 文件夹地址 图片名称
            byte[] fileBytes;

            fileBytes = Convert.FromBase64String(imgStr);//变量为图片字节流
            MemoryStream memoryStream = new MemoryStream(fileBytes); //1.定义并实例化一个内存流，以存放提交上来的字节数组。  
            FileStream fileUpload = new FileStream(SaveFilePath, FileMode.Create); ///2.定义实际文件对象，保存上载的文件。  
            memoryStream.WriteTo(fileUpload); ///3.把内存流里的数据写入物理文件  
            memoryStream.Close();
            fileUpload.Close();
            fileUpload = null;
            memoryStream = null;

            FilePathImg = FilePath + imgName;
            FilePathImg = FilePathImg.Replace("\\", "\\\\");

            return FilePathImg;
        }
        #endregion

        #region 查询服务器时间 mysql
        private string webTime()
        {
            //JGDBOP jgdbop = new JGDBOP();
            SafeDBClasscs jgdbop = new SafeDBClasscs();
            string web_time = "";
            string select_sql = "SELECT NOW()";
            System.Data.DataTable dt_time = jgdbop.GetDataTable(select_sql);
            if (dt_time.Rows.Count > 0)
            {
                web_time = dt_time.Rows[0][0].ToString();
                DateTime webtm = Convert.ToDateTime(web_time);
                web_time = webtm.ToString("yyyy-MM-dd");
            }
            return web_time;
        }
        #endregion

        #region 使用事务更新 计量表 和 门禁表 出厂时间 确认人(一车一货)
        private bool updataTableExit(string userName, int flag, string meaCode)
        {
            JGDBOP jgdbop = new JGDBOP();
            bool result = false;
            SqlTransaction SqlTran = null;
            SqlConnection mycon = jgdbop.GetCon();
            mycon.Open();
            SqlCommand SqlCmd = mycon.CreateCommand();
            try
            {
                SqlCmd.Connection = mycon;
                SqlTran = mycon.BeginTransaction();
                SqlCmd.Transaction = SqlTran;

                string update_sql_cm = "update CM_MeasureInfo set cExitUser ='" + userName + "'," +
                                        "cExitTm = convert(varchar,getdate(),120)," +
                                        "iExitFlag = " + flag + ", " +
                                        "iPrintCount = 1 ," +
                                        "dPrintDate = convert(varchar,getdate(),120) " +
                                        "where C_MeasureDocID = '" + meaCode + "'";
                SqlCmd.CommandText = update_sql_cm;
                SqlCmd.CommandType = CommandType.Text;
                SqlCmd.Connection = mycon;
                SqlCmd.ExecuteNonQuery();

                string update_sql_exit = "update CM_DoorControlMain set LeaveTmNew = convert(varchar,getdate(),120) "+
                                         "where MeasureDocID = '" + meaCode + "'";

                SqlCmd.CommandText = update_sql_exit;
                SqlCmd.CommandType = CommandType.Text;
                SqlCmd.Connection = mycon;
                SqlCmd.ExecuteNonQuery();

                SqlTran.Commit();
                result = true;
            }
            catch (Exception expt)
            {
                SqlTran.Rollback();
            }
            finally
            {
                mycon.Close();
                SqlTran.Dispose();
                mycon.Dispose();
            }
            return result;
        }
        #endregion

        #region 使用事务更新 计量表 和 门禁表 出厂时间 确认人(一车duo货)
        private bool updataTableExitMore(string userName, int flag, List<string> meaCodeList,string carCode)
        {
            JGDBOP jgdbop = new JGDBOP();
            bool result = false;
            SqlTransaction SqlTran = null;
            SqlConnection mycon = jgdbop.GetCon();
            mycon.Open();
            SqlCommand SqlCmd = mycon.CreateCommand();
            try
            {
                SqlCmd.Connection = mycon;
                SqlTran = mycon.BeginTransaction();
                SqlCmd.Transaction = SqlTran;

                for (int i = 0; i < meaCodeList.Count; i++)
                {
                     string update_sql_cm = "update CM_MeasureInfo set cExitUser ='" + userName + "'," +
                                        "cExitTm = convert(varchar,getdate(),120)," +
                                        "iExitFlag = " + flag + ", " +
                                        "iPrintCount = 1 ,"+
                                        "dPrintDate = convert(varchar,getdate(),120) "+
                                        "where C_MeasureDocID = '" + meaCodeList[i].ToString() + "'";
                    SqlCmd.CommandText = update_sql_cm;
                    SqlCmd.CommandType = CommandType.Text;
                    SqlCmd.Connection = mycon;
                    SqlCmd.ExecuteNonQuery();

                    string update_sql_exit = "update CM_DoorControlMain set LeaveTmNew = convert(varchar,getdate(),120) where CarNO = '" + carCode + "' and LeaveTmNew is null";
                    SqlCmd.CommandText = update_sql_exit;
                    SqlCmd.CommandType = CommandType.Text;
                    SqlCmd.Connection = mycon;
                    SqlCmd.ExecuteNonQuery();
                }

                SqlTran.Commit();
                result = true;
            }
            catch (Exception expt)
            {
                SqlTran.Rollback();
            }
            finally
            {
                mycon.Close();
                SqlTran.Dispose();
                mycon.Dispose();
            }
            return result;
        }
        #endregion

        #region 记录异常日志
        public void WriteYCLog(Exception ex, string strType)
        {
            //如果日志文件为空，则默认在Debug目录下新建 YYYY-mm-dd_Log.log文件
            string strPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + strType + ".txt";

            //把异常信息输出到文件
            StreamWriter sw = new StreamWriter(strPath, true);
            sw.WriteLine("当前时间：" + DateTime.Now.ToString());
            sw.WriteLine("异常信息：" + ex.Message);
            sw.WriteLine("异常对象：" + ex.Source);
            sw.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
            sw.WriteLine("触发方法：" + ex.TargetSite);
            sw.WriteLine();
            sw.Close();
        }
        #endregion

        #region 记录程序执行日志
        public  void WriteID(string strID, string strType)
        {
            string strPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + strType + ".txt";
            StreamWriter sw = new StreamWriter(strPath, true);
            string strtime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            sw.WriteLine(strType + "|" + strID + "|" + "执行时间" + "|" + strtime);
            sw.WriteLine();
            sw.Close();
        }
        #endregion
    }
}
