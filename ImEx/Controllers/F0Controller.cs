using Nskd;
using System;
using System.Globalization;
using System.Data;
using System.Web.Mvc;

namespace ImEx.Controllers
{
    //public Double Session["discount"] // СкидкаПоставщикаВПроцентах

    public class F0Controller : Controller
    {
        public Object Index(String sessionId)
        {
            Object r = "FsImEx_ImEx_HomeController_Index()";
            DateTime преиодС = DateTime.Now.AddMonths(-2);
            Guid.TryParse(sessionId, out Guid sid);

            DataSet ds = new DataSet();

            RequestPackage rqp = new RequestPackage
            {
                SessionId = sid,
                Command = "ПолучитьСписокРасходныхНакладных",
                Parameters = new RequestParameter[] {
                    new RequestParameter { Name = "период_с", Value = преиодС },
                    new RequestParameter { Name = "клиент", Value = "ФК ГАРЗА" }
                }
            };
            ResponsePackage rsp = rqp.GetResponse("http://127.0.0.1:11004/"); // 1С Фарм-Сиб
            if (rsp != null && rsp.Data != null && rsp.Data.Tables.Count > 0)
            {
                ds.Tables.Add(rsp.Data.Tables[0].Copy());
            }

            rqp.Command = "ПолучитьСписокПриходныхНакладных";
            rqp.Parameters = new RequestParameter[] {
                    new RequestParameter { Name = "период_с", Value = преиодС },
                    new RequestParameter { Name = "клиент", Value = "Фарм-Сиб" }
                };
            rsp = rqp.GetResponse("http://127.0.0.1:11014/"); // 1С ФК Гарза
            if (rsp != null && rsp.Data != null && rsp.Data.Tables.Count > 0)
            {
                ds.Tables.Add(rsp.Data.Tables[0].Copy());
            }

            rqp.Command = "[Pharm-Sib].[dbo].[settings_get]";
            rqp.Parameters = new RequestParameter[] { };
            rqp.AddSessionIdToParameters();
            rsp = rqp.GetResponse("http://127.0.0.1:11012/"); // SQL Server
            if (rsp != null && rsp.Data != null && rsp.Data.Tables.Count > 0)
            {
                DataTable dt = rsp.Data.Tables[0];
                if (dt.Columns.Count > 0 && dt.Rows.Count > 3)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        // Я или Гриша
                        if ((((Int32)dr[1]) == 2 || ((Int32)dr[1]) == 3) && dr[2] as String == "Скидка при оформлении передачи из Фарм-Сиб в Гарза")
                        {
                            Double.TryParse(dr[3] as String, NumberStyles.Any, CultureInfo.InvariantCulture , out Double discount);
                            Session["discount"] = discount;
                        }
                    }
                }
            }

            r = PartialView("~/Views/F0/Index.cshtml", ds);
            return r;
        }

        public Object WaitingPage(String sessionId)
        {
            Guid.TryParse(sessionId, out Guid sid);
            Object v = PartialView("~/Views/F0/WaitingPage.cshtml", sid);
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
                Command = "ПолучитьИз1СФармСибРасходнуюНакладную",
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
            RequestPackage rqp = new RequestPackage
            {
                Command = "ДобавитьВ1СФКГарзаПриходнуюНакладную",
                Parameters = new RequestParameter[]
                {
                    new RequestParameter { Name = "РасходнаяНакладная", Value = РасходнаяНакладная },
                    new RequestParameter { Name = "СкидкаПоставщикаВПроцентах", Value = (Double)Session["discount"] }
                }
            };
            ResponsePackage rsp = rqp.GetResponse("http://127.0.0.1:11014/");
            if (rsp != null)
            {

            }
        }
    }
}