using Nskd;
using System;
using System.Data;
using System.Web.Mvc;

namespace ImEx.Controllers
{
    public class F0Controller : Controller
    {
        public Object Index()
        {
            Object r = "FsImEx_ImEx_HomeController_Index()";
            DataSet ds = new DataSet();

            RequestPackage rqp = new RequestPackage { Command = "ПолучитьСписокРасходныхНакладных" };
            ResponsePackage rsp = rqp.GetResponse("http://127.0.0.1:11004/"); // 1С Фарм-Сиб
            if (rsp != null && rsp.Data != null && rsp.Data.Tables.Count > 0)
            {
                ds.Tables.Add(rsp.Data.Tables[0].Copy());
            }

            rqp.Command = "ПолучитьСписокПриходныхНакладных";
            rsp = rqp.GetResponse("http://127.0.0.1:11014/"); // 1С ФК Гарза
            if (rsp != null && rsp.Data != null && rsp.Data.Tables.Count > 0)
            {
                ds.Tables.Add(rsp.Data.Tables[0].Copy());
            }

            r = PartialView("~/Views/F0/Index.cshtml", ds);
            return r;
        }

        public Object WaitingPage()
        {
            Object v = null;
            if (ControllerContext.HttpContext.IsDebuggingEnabled)
                v = View("~/Views/F0/WaitingPage.cshtml"); // _ViewStart.cshtml
            else
                v = PartialView("~/Views/F0/WaitingPage.cshtml");
            return v;
        }

        public Object SendBill(String fsN)
        {
            String status = String.Format($"FsImEx.ImEx.Controllers.F0Controller.SendBill({fsN})");

            // получить расходную накладную из 1С "Фарм-Сиб" по номеру
            String РасходнаяНакладная = ПолучитьИз1СФармСибРасходнуюНакладную(fsN);
            if (РасходнаяНакладная != null)
            {
                status += String.Format($"\nПолучена расходная накладная №{fsN}");

                ДобавитьВ1СФКГарзаПриходнуюНакладную(РасходнаяНакладная);
            }
            return status;
        }

        private String ПолучитьИз1СФармСибРасходнуюНакладную(String fsN)
        {
            String РасходнаяНакладная = null;
            RequestPackage rqp = new RequestPackage
            {
                Command = "ПолучитьРасходнуюНакладную",
                Parameters = new RequestParameter[]
                {
                    new RequestParameter { Name = "fsN", Value = fsN }
                }
            };
            ResponsePackage rsp = rqp.GetResponse("http://127.0.0.1:11004/");
            if (rsp != null && rsp.Data != null && rsp.Data.Tables.Count > 0 && rsp.Data.Tables[0].Rows.Count > 0)
            {
                РасходнаяНакладная = rsp.Data.Tables[0].Rows[0][0] as String;
            }
            return РасходнаяНакладная;
        }

        private void ДобавитьВ1СФКГарзаПриходнуюНакладную(String РасходнаяНакладная)
        {
            Double скидкаПоставщика = 0.05; // 5%
            RequestPackage rqp = new RequestPackage
            {
                Command = "ДобавитьПриходнуюНакладную",
                Parameters = new RequestParameter[]
                {
                    new RequestParameter { Name = "РасходнаяНакладная", Value = РасходнаяНакладная },
                    new RequestParameter { Name = "СкидкаПоставщика", Value = скидкаПоставщика }
                }
            };
            ResponsePackage rsp = rqp.GetResponse("http://127.0.0.1:11014/");
            if (rsp != null)
            {

            }
        }
    }
}