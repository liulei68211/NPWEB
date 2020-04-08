using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace NPWEB
{
    public class CreateXml
    {
        /// <summary>
        /// 生成xml头部
        /// </summary>
        /// strType 操作类型
        public string HeadXml(string strMain, string strType)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlNode node = xmldoc.CreateXmlDeclaration("1.0", "GBK", "");
            xmldoc.AppendChild(node);
            XmlNode root = xmldoc.CreateElement("xml_root");
            xmldoc.AppendChild(root);
            XmlNode node1 = xmldoc.CreateElement(strMain);
            root.AppendChild(node1);
            XmlElement xesub = xmldoc.CreateElement("operate_type");
            xesub.InnerText = strType;
            node1.AppendChild(xesub);
            string xml = xmldoc.OuterXml;
            return xml;

        }

        private void CreatElement(XmlDocument xmldoc, XmlNode node, string name, string value)
        {

            XmlElement element1 = xmldoc.CreateElement(name);
            element1.InnerText = value;
            node.AppendChild(element1);
        }

        /// <summary>
        /// 修改xml文件
        /// </summary>
        public string ModXml(string strXml, string Node, List<string> listName, List<string> listData)
        {
            string xml = "";
            try
            {
                XmlDocument xmldoc = new XmlDocument();

                xmldoc.LoadXml(strXml);

                XmlNode root = xmldoc.SelectSingleNode(Node);


                for (int i = 0; i < listName.Count; i++)
                {

                    XmlElement xesub = xmldoc.CreateElement(listName[i]);
                    xesub.InnerText = listData[i];
                    root.AppendChild(xesub);
                }

                xml = xmldoc.OuterXml;
            }
            catch (Exception e)
            {
                throw e;
            }

            return xml;

        }

        public string ModSTXml(string strXml, int count, string Node, List<string> listName, List<string> listData)
        {
            string xml = "";
            try
            {
                XmlDocument xmldoc = new XmlDocument();

                xmldoc.LoadXml(strXml);

                XmlNode root = xmldoc.SelectSingleNode("xml_root/lading/details");

                XmlNode node1 = xmldoc.CreateElement(Node);
                root.AppendChild(node1);
                for (int i = 0; i < listName.Count; i++)
                {

                    XmlElement xesub = xmldoc.CreateElement(listName[i]);
                    xesub.InnerText = listData[i];
                    node1.AppendChild(xesub);
                }

                xml = xmldoc.OuterXml;
            }
            catch (Exception e)
            {
                throw e;
            }

            return xml;

        }

        /// <summary>
        /// 生成xml头部
        /// </summary>
        public string SaleHeadXml(string strbilltype, string strfile, string strMain, string strHead, string strBody)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlNode node = xmldoc.CreateXmlDeclaration("1.0", "utf-8", "");
            xmldoc.AppendChild(node);
            XmlNode root = xmldoc.CreateElement("ufinterface");
            XmlAttribute account = xmldoc.CreateAttribute("account");
            account.Value = "0002";
            XmlAttribute billtype = xmldoc.CreateAttribute("billtype");
            billtype.Value = strbilltype;
            XmlAttribute filename = xmldoc.CreateAttribute("filename");
            filename.Value = strfile;
            XmlAttribute isexchange = xmldoc.CreateAttribute("isexchange");
            isexchange.Value = "Y";
            XmlAttribute proc = xmldoc.CreateAttribute("proc");
            proc.Value = "add";
            XmlAttribute receiver = xmldoc.CreateAttribute("receiver");
            receiver.Value = "1001";
            XmlAttribute replace = xmldoc.CreateAttribute("replace");
            replace.Value = "Y";
            XmlAttribute sender = xmldoc.CreateAttribute("sender");
            sender.Value = "0001";
            XmlAttribute subbilltype = xmldoc.CreateAttribute("subbilltype");
            subbilltype.Value = "run";
            root.Attributes.Append(account);
            root.Attributes.Append(billtype);
            root.Attributes.Append(filename);
            root.Attributes.Append(isexchange);
            root.Attributes.Append(proc);
            root.Attributes.Append(receiver);
            root.Attributes.Append(replace);
            root.Attributes.Append(sender);
            root.Attributes.Append(subbilltype);
            xmldoc.AppendChild(root);

            xmldoc.AppendChild(root);
            XmlNode node1 = xmldoc.CreateElement(strMain);

            root.AppendChild(node1);
            XmlNode node2 = xmldoc.CreateElement(strHead);
            node1.AppendChild(node2);


            XmlNode node3 = xmldoc.CreateElement(strBody);
            node1.AppendChild(node3);

            string xml = xmldoc.OuterXml;
            return xml;


        }

        /// <summary>
        /// 修改销售xml文件
        /// </summary>
        public string ModSaleXml(string strXml, string Node, List<string> listName, List<string> listData)
        {
            XmlDocument xmldoc = new XmlDocument();

            xmldoc.LoadXml(strXml);

            XmlNode root = xmldoc.SelectSingleNode(Node);
            for (int i = 0; i < listName.Count; i++)
            {

                XmlElement xesub = xmldoc.CreateElement(listName[i]);
                xesub.InnerText = listData[i];
                root.AppendChild(xesub);
            }

            string xml = xmldoc.OuterXml;
            return xml;

        }

        public string ModSaleBodyXml(string strXml, string Node, List<string> listName, List<string> listData)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(strXml);
            XmlNode root = xmldoc.SelectSingleNode(Node);
            XmlElement rel = xmldoc.CreateElement("entry");
            for (int i = 0; i < listName.Count; i++)
            {

                XmlElement xesub = xmldoc.CreateElement(listName[i]);
                xesub.InnerText = listData[i];
                rel.AppendChild(xesub);
            }

            root.AppendChild(rel);


            string xml = xmldoc.OuterXml;
            return xml;
        }
        public string CreatStockHead()
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlNode node = xmldoc.CreateXmlDeclaration("1.0", "GBK", "");
            xmldoc.AppendChild(node);
            XmlNode root = xmldoc.CreateElement("xml_root");
            xmldoc.AppendChild(root);
            //XmlNode node1 = xmldoc.CreateElement("res");
            //root.AppendChild(node1);
            string xml = xmldoc.OuterXml;
            return xml;

        }

        /// <summary>
        /// 生成库存xml文件
        /// </summary>
        /// <returns></returns>
        public string CreatStockXml(string strXml, List<string> listName, List<string> listData)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(strXml);
            XmlNode root = xmldoc.SelectSingleNode("xml_root");
            XmlElement rel = xmldoc.CreateElement("res");
            for (int i = 0; i < listName.Count; i++)
            {

                XmlElement xesub = xmldoc.CreateElement(listName[i]);
                xesub.InnerText = listData[i];
                rel.AppendChild(xesub);
            }
            root.AppendChild(rel);
            string xml = xmldoc.OuterXml;
            return xml;
        }


        public string CreatCKXml(string strXml, List<string> listName, List<string> listData)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(strXml);
            XmlNode root = xmldoc.SelectSingleNode("xml_root/out_infos");
            XmlElement rel = xmldoc.CreateElement("out_info");
            for (int i = 0; i < listName.Count; i++)
            {
                XmlElement xesub = xmldoc.CreateElement(listName[i]);
                xesub.InnerText = listData[i];
                rel.AppendChild(xesub);
            }
            root.AppendChild(rel);
            string xml = xmldoc.OuterXml;
            return xml;
        }

        public string CreatNCYHXml(string strXml, List<string> listName, List<string> listData)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(strXml);
            XmlNode root = xmldoc.SelectSingleNode("xml_root/cust_infos");
            XmlElement rel = xmldoc.CreateElement("cust_info");
            for (int i = 0; i < listName.Count; i++)
            {

                XmlElement xesub = xmldoc.CreateElement(listName[i]);
                xesub.InnerText = listData[i];
                rel.AppendChild(xesub);
            }
            root.AppendChild(rel);
            string xml = xmldoc.OuterXml;
            return xml;
        }
    }
}