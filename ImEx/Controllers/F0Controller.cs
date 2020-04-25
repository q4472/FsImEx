using Nskd;
using System;
using System.Globalization;
using System.Data;
using System.Web.Mvc;

namespace ImEx.Controllers
{
    public class F0Controller : Controller
    {
        public Object Index(Int32 src, Int32 period)
        {
            Object r = "ImEx.Controllers.F0Controller.Index()";
            Env env = (Env)Session["env"];
            env.Src = src;
            env.Period = period;
            switch (src)
            {
                case 1:
                    env.SrcUri = "http://127.0.0.1:11014/"; // 1С ФК ГАРЗА
                    env.SrcFirm = "ООО \"ФК ГАРЗА\"";
                    env.SrcClient = "ООО \"Фарм-Сиб\"";
                    env.DestUri = "http://127.0.0.1:11004/"; // 1С Фарм-Сиб
                    env.DestFirm = "ООО \"Фарм-Сиб\"";
                    env.DestClient = "ООО \"ФК ГАРЗА\"";
                    env.DiscountKey = "Скидка при оформлении передачи из Гарза в Фарм-Сиб";
                    break;
                case 0:
                default:
                    env.SrcUri = "http://127.0.0.1:11004/"; // 1С Фарм-Сиб
                    env.SrcFirm = "ООО \"Фарм-Сиб\"";
                    env.SrcClient = "ООО \"ФК ГАРЗА\"";
                    env.DestUri = "http://127.0.0.1:11014/"; // 1С ФК ГАРЗА
                    env.DestFirm = "ООО \"ФК ГАРЗА\"";
                    env.DestClient = "ООО \"Фарм-Сиб\"";
                    env.DiscountKey = "Скидка при оформлении передачи из Фарм-Сиб в Гарза";
                    break;
            }

            DataSet ds = new DataSet();

            RequestPackage rqp = new RequestPackage
            {
                SessionId = env.SessionId,
                Command = "ПолучитьСписокРасходныхНакладных",
                Parameters = new RequestParameter[] {
                    new RequestParameter { Name = "период_с", Value = DateTime.Now.AddMonths(-env.Period) },
                    new RequestParameter { Name = "клиент", Value = env.SrcClient }
                }
            };
            ResponsePackage rsp = rqp.GetResponse(env.SrcUri);
            if (rsp != null && rsp.Data != null && rsp.Data.Tables.Count > 0)
            {
                ds.Tables.Add(rsp.Data.Tables[0].Copy());
            }

            rqp.Command = "ПолучитьСписокПриходныхНакладных";
            rqp.Parameters = new RequestParameter[] {
                    new RequestParameter { Name = "период_с", Value = DateTime.Now.AddMonths(-env.Period) },
                    new RequestParameter { Name = "клиент", Value = env.DestClient }
                };
            rsp = rqp.GetResponse(env.DestUri);
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
                        if (dr[1] != DBNull.Value && ((Int32)dr[1]) == 3 &&
                            dr[2] != DBNull.Value && ((String)dr[2]) == env.DiscountKey)
                        {
                            Double.TryParse(dr[3] as String, NumberStyles.Any, CultureInfo.InvariantCulture, out env.Discount);
                        }
                    }
                }
            }
            
            Session["env"] = env;
            r = PartialView("~/Views/F0/Index.cshtml", ds);
            return r;
        }

        public Object WaitingPage(String sessionId)
        {
            Session["env"] = new Env(sessionId);
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
                status += $"<br>\nПолучена расходная накладная №{num}";
                //status += $"<br>\n{РасходнаяНакладная}";
                status += $"<br>\n{ДобавитьПриходнуюНакладную(РасходнаяНакладная)}";
            }
            return status;
        }

        private String ПолучитьРасходнуюНакладную(String num)
        {
            String РасходнаяНакладная = null;
            Env env = (Env)Session["env"];
            RequestPackage rqp = new RequestPackage
            {
                SessionId = env.SessionId,
                Command = "ПолучитьРасходнуюНакладную",
                Parameters = new RequestParameter[]
                {
                    new RequestParameter { Name = "период_с", Value = DateTime.Now.AddMonths(-env.Period) },
                    new RequestParameter { Name = "num", Value = num }
                }
            };
            ResponsePackage rsp = rqp.GetResponse(env.SrcUri);
            if (rsp != null && rsp.Data != null && rsp.Data.Tables.Count > 0 && rsp.Data.Tables[0].Rows.Count > 0)
            {
                РасходнаяНакладная = rsp.Data.Tables[0].Rows[0][0] as String;
            }
            return РасходнаяНакладная;
        }

        private String ДобавитьПриходнуюНакладную(String РасходнаяНакладная)
        {
            String status = "ДобавитьПриходнуюНакладную()\n";
            Env env = (Env)Session["env"];
            try
            {

                RequestPackage rqp = new RequestPackage();
                rqp.SessionId = env.SessionId;
                rqp.Command = "ДобавитьПриходнуюНакладную";
                rqp.Parameters = new RequestParameter[]
                {
                        new RequestParameter { Name = "Фирма", Value = env.DestFirm },
                        new RequestParameter { Name = "Клиент", Value = env.DestClient },
                        new RequestParameter { Name = "РасходнаяНакладная", Value = РасходнаяНакладная },
                        new RequestParameter { Name = "СкидкаПоставщикаВПроцентах", Value = env.Discount }
                };
                rqp.GetResponse(env.DestUri);
                status += "<br>\nOK.";
            }
            catch (Exception e) { status += "\n<br>" + e.ToString(); }
            return status;
        }
    }
    public class Env
    {
        public Guid SessionId;
        public Int32 Src;
        public Int32 Period;
        public String SrcUri;
        public String SrcFirm;
        public String SrcClient;
        public String DestUri;
        public String DestFirm;
        public String DestClient;
        public String DiscountKey;
        public Double Discount;
        public Env(String sessionId)
        {
            Guid.TryParse(sessionId, out SessionId);
        }
    }
}