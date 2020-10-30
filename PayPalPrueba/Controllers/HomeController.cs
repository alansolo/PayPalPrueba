using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PayPalPrueba.Controllers
{
    public class HomeController : Controller
    {
        string ClientId = "AegmqpJTnoKCSGKcuqVXuK1dItDnHXMw2WJp4F0DJ4ifTKoTwe9iQzZiGlV8fuEgNDQ7aPLnJDDhW3cB";
        string ClientSecret = "ELkonrRH-RMBjFxvth-p7KnLJEFpKoUH9s1ZCsYa4DL_iO7RpZenGkZKe_9ZM7XTHP_aajeeSyCMQVBX";
        private Payment payment;

        public Dictionary<string, string> GetConfig()
        {
            return PayPal.Api.ConfigManager.Instance.GetProperties();
        }

        private string GetAccessToken()
        {
            string accessToken = new OAuthTokenCredential(ClientId, ClientSecret, GetConfig()).GetAccessToken();
            return accessToken;
        }

        public APIContext GetAPIContext()
        {
            var apiContext = new APIContext(GetAccessToken());
            apiContext.Config = GetConfig();

            return apiContext;
        }

        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            var item = new Item()
            {
                name = "articulo 1",
                currency = "MXN",
                price = "5.00",
                quantity = "2",
                sku = "sku"
            };

            var itemList = new ItemList();
            itemList.items = new List<Item>();
            itemList.items.Add(item);

            var payer = new Payer()
            {
                payment_method = "paypal"
            };

            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl, //"http://localhost:1187/", //"http://localhost:3000/cancel",
                return_url = redirectUrl //"http://localhost:1187/" //"http://localhost:3000/process"
            };

            var details = new Details()
            {
                tax = "1",
                shipping = "2",
                subtotal = "10.00",

            };

            var amount = new Amount()
            {
                currency = "MXN",
                total = "13.00",
                details = details
            };

            var transaction = new List<Transaction>();
            transaction.Add(new Transaction()
            {
                description = "Primer compra",
                invoice_number = Convert.ToString((new Random()).Next(100000)),
                amount = amount,
                item_list = itemList
            });

            var payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transaction,
                redirect_urls = redirUrls
            };

            return payment.Create(apiContext);
        }

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            //EXECUTE PAYMENT
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            payment = new Payment()
            {
                id = paymentId
            };

            return payment.Execute(apiContext, paymentExecution);
        }

        public ActionResult Index()
        {
            string uno = Request.RequestType;

            return View();
        }

        public ActionResult SendPay()
        {
            string mensaje = string.Empty;

            RedirectToAction("Index", "Home");

            try
            {
                APIContext apiContext = GetAPIContext();

                string payerId = Request.Params["PayerId"];
                var paymentId = Request.Params["paymentId"];

                if (string.IsNullOrEmpty(payerId))
                {
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Home/Index?";

                    var guid = Convert.ToString((new Random()).Next(100000));

                    var createdPayment = CreatePayment(apiContext, baseURI + "guid=" + guid);

                    var links = createdPayment.links.GetEnumerator();

                    string paypalRedirectUrl = string.Empty;

                    while (links.MoveNext())
                    {
                        Links link = links.Current;
                        if (link.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = link.href;
                        }
                    }

                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    var executedPayment = ExecutePayment(apiContext, payerId, paymentId);

                    if (executedPayment.state.ToLower() != "approved")
                    {
                        mensaje = "Error";
                    }

                    mensaje = "Correcto";
                }
            }
            catch (Exception ex)
            {

            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}