using Nskd;
using System;
using System.Globalization;
using System.Data;
using System.Web.Mvc;

namespace ImEx.Controllers
{
    public class F0Controller : Controller
    {
        private DateTime периодС = DateTime.Now.AddMonths(-1);

        //Session["sessionId"]
        //Session["src"]
        //Session["sUri"]
        //Session["sClient"]
        //Session["dUri"]
        //Session["dClient"]
        //Session["discount"]

        public Object Index(Int32 src, Int32 period)
        {
            Object r = "ImEx.HomeController.Index()";

            String sUri, dUri;
            String sClient, dClient;
            String discounKey;
            switch (src)
            {
                case 1:
                    sUri = "http://127.0.0.1:11014/"; // 1С ФК Гарза
                    sClient = "Фарм-Сиб";
                    dUri = "http://127.0.0.1:11004/"; // 1С Фарм-Сиб
                    dClient = "ФК ГАРЗА";
                    discounKey = "Скидка при оформлении передачи из Гарза в Фарм-Сиб";
                    break;
                case 0:
                default:
                    sUri = "http://127.0.0.1:11004/"; // 1С Фарм-Сиб
                    sClient = "ФК ГАРЗА";
                    dUri = "http://127.0.0.1:11014/"; // 1С ФК Гарза
                    dClient = "Фарм-Сиб";
                    discounKey = "Скидка при оформлении передачи из Фарм-Сиб в Гарза";
                    break;
            }
            Session["src"] = src;
            Session["sUri"] = sUri;
            Session["sClient"] = sClient;
            Session["dUri"] = sUri;
            Session["dClient"] = dClient;

            периодС = DateTime.Now.AddMonths(-period);

            DataSet ds = new DataSet();

            RequestPackage rqp = new RequestPackage
            {
                SessionId = (Guid)Session["sessionId"],
                Command = "ПолучитьСписокРасходныхНакладных",
                Parameters = new RequestParameter[] {
                    new RequestParameter { Name = "период_с", Value = периодС },
                    new RequestParameter { Name = "клиент", Value = sClient }
                }
            };
            ResponsePackage rsp = rqp.GetResponse(sUri);
            if (rsp != null && rsp.Data != null && rsp.Data.Tables.Count > 0)
            {
                ds.Tables.Add(rsp.Data.Tables[0].Copy());
            }

            rqp.Command = "ПолучитьСписокПриходныхНакладных";
            rqp.Parameters = new RequestParameter[] {
                    new RequestParameter { Name = "период_с", Value = периодС },
                    new RequestParameter { Name = "клиент", Value = dClient }
                };
            rsp = rqp.GetResponse(dUri);
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
                        // Гриша
                        if (((Int32)dr[1]) == 3 && dr[2] as String == discounKey)
                        {
                            Double.TryParse(dr[3] as String, NumberStyles.Any, CultureInfo.InvariantCulture, out Double discount);
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
            Session["sessionId"] = sid;
            Object v = PartialView("~/Views/F0/WaitingPage.cshtml");
            return v;
        }

        public Object SendBill(String num)
        {
            String status = $"ImEx.Controllers.F0Controller.SendBill('{num}')";
            // получить расходную накладную из 1С "Фарм-Сиб" по номеру
            String РасходнаяНакладная = ПолучитьРасходнуюНакладную(num);
            if (РасходнаяНакладная != null)
            {
                status += $"\nПолучена расходная накладная №{num}";
                ДобавитьПриходнуюНакладную(РасходнаяНакладная);
            }
            return status;
        }

        private String ПолучитьРасходнуюНакладную(String num)
        {
            String РасходнаяНакладная = null;
            RequestPackage rqp = new RequestPackage
            {
                SessionId = (Guid)Session["sessionId"],
                Command = "ПолучитьРасходнуюНакладную",
                Parameters = new RequestParameter[]
                {
                    new RequestParameter { Name = "период_с", Value = периодС },
                    new RequestParameter { Name = "fsN", Value = num }
                }
            };
            ResponsePackage rsp = rqp.GetResponse((String)Session["sUri"]);
            if (rsp != null && rsp.Data != null && rsp.Data.Tables.Count > 0 && rsp.Data.Tables[0].Rows.Count > 0)
            {
                РасходнаяНакладная = rsp.Data.Tables[0].Rows[0][0] as String;
            }
            return РасходнаяНакладная;
        }

        private void ДобавитьПриходнуюНакладную(String РасходнаяНакладная)
        {
            RequestPackage rqp = new RequestPackage
            {
                SessionId = (Guid)Session["sessionId"],
                Command = "ДобавитьПриходнуюНакладную",
                Parameters = new RequestParameter[]
                {
                    new RequestParameter { Name = "Фирма", Value = Session["dClient"] },
                    new RequestParameter { Name = "Клиент", Value = Session["sClient"] },
                    new RequestParameter { Name = "РасходнаяНакладная", Value = РасходнаяНакладная },
                    new RequestParameter { Name = "СкидкаПоставщикаВПроцентах", Value = (Double)Session["discount"] }
                }
            };
            rqp.GetResponse((String)Session["sUri"]);
        }
    }
}