﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YolGecenHani.Models;
using YolGecenHani.Models.EntityModel;
using YolGecenHani.Models.HomeModel;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using System.Net;
using System.Web.UI.WebControls;


namespace YolGecenHani.Controllers

{
    public class CartController : Controller
    {
        DataContext db = new DataContext();
        // GET: Cart
        public ActionResult Index()
        {
            return PartialView(GetCart());
        }

        [HttpPost]
        public ActionResult AddToCart(int id)
        {
            var product = db.Product.FirstOrDefault(X => X.Id == id);
            if (product != null)  //null değilse ürünü sepete ekeleee   ürün ekleme burdann
            {
                GetCart().AddProduct(product, 1);  //1 adet product gonder ana sayfada adet seçtirmiyomm sepete tıkadıgında 1 taen awçilcek detayda adet seç,cezzz
            }
            return Redirect(Request.UrlReferrer.ToString());  //bunu onyuze bağlaualuımmm
        }
        [HttpPost]
        public ActionResult RemoveFromCart(int id)   //fimnf veya by buy ürün var mı yokmu
        {
            var product = db.Product.FirstOrDefault(X => X.Id == id);
            if (product != null)
            {
                GetCart().DeleteProduct(product);
            }


            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult ClearCart()
        {
            if (GetCart().CartLines.Count > 0)
            {
                GetCart().Clear();
            }
            return Redirect(Request.UrlReferrer.ToString());
        }

        //ürünlerr sessionda ttuutazz sayfa değiştiğinde gitmesimnnnnn

        public Cart GetCart()  //sayfalar araınsda geçiş yapıldığında sepetin doluluguuu  sayfa açuık oldugu sürece kalırr
        {//hangi cla türünde  tutuoysan convertlee
            var cart = (Cart)Session["Cart"];   //cartın boş olup olmamsı öneml,
            if (cart == null)
            {
                cart = new Cart();   //istersen session türünede depere çevir istersen cart türündee
                Session["Cart"] = cart; //eğer üründe varsa yeni bir üürrn oluştuırmucakkk üstüne eklicekk


                //ilk boyle bi session var mı varsa aktar yoksa null
                //  nullsa yeni bir card oluştur ve nesne oluyştur yen i oluşturulanı sessiona aktarrr
            }
            return cart;  //şimdi kartın listesine ürün ekledikk
        }


        public ActionResult ViewCart()
        {
            return View(GetCart());
        }

        public ActionResult ProductMinus(int id)
        {
            var product = db.Product.FirstOrDefault(X => X.Id == id);
            if (product != null)  //null değilse ürünü sepete ekeleee   ürün ekleme burdann
            {
                GetCart().AddProduct(product, -1);  //1 adet product gonder ana sayfada adet seçtirmiyomm sepete tıkadıgında 1 taen awçilcek detayda adet seç,cezzz
            }
            return Redirect(Request.UrlReferrer.ToString());  //bunu onyuze bağlaualuımmm
        }

        public ActionResult ProductPlus(int id)
        {
            var product = db.Product.FirstOrDefault(X => X.Id == id);
            if (product != null)  //null değilse ürünü sepete ekeleee   ürün ekleme burdann
            {
                GetCart().AddProduct(product, +1);  //1 adet product gonder ana sayfada adet seçtirmiyomm sepete tıkadıgında 1 taen awçilcek detayda adet seç,cezzz
            }
            return Redirect(Request.UrlReferrer.ToString());  //bunu onyuze bağlaualuımmm
        }

        public ActionResult Checkout()
        {
            return View();
        }

        //7hem ürfüneri almam hem kullalnıcı adresi getirmme lazımm

        public PartialViewResult CheckoutPartial()
        {
            return PartialView(GetCart());
        }

        public PartialViewResult CheckoutShippingForm()
        {

            var user = db.User.FirstOrDefault(x => x.UserName == User.Identity.Name);
            ShippingAddressModel model = new ShippingAddressModel();
            if (user != null)
            {
                model.User = user;
                model.City = db.City.Where(x => x.IsDelete == false).ToList();
                model.UserAddress = db.UserAddress.FirstOrDefault(x => x.UserId == user.Id && x.Selected == true);
            }
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult CheckoutShippingForm(string Name, string Surname, string AddressTitle, string Email, string Phone, string Address, string City, string Notes)
        {
            OrderShippingAddress osa = new OrderShippingAddress();
            osa.OrderNumber = "YGH" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + Guid.NewGuid().ToString().Substring(0, 10);
            osa.Name = Name;
            osa.Surname = Surname;
            osa.Email = Email;
            osa.Phone = Phone;
            osa.AddressTitle = AddressTitle;
            osa.Address = Address;
            osa.City = City;
            osa.Notes = Notes;

            var shippingaddres = (OrderShippingAddress)Session["ShippingAddress"];
            shippingaddres = osa;
            Session["ShippingAddress"] = shippingaddres;
            return RedirectToAction("Payment");
        }
        public ActionResult Payment()
        {
            var cart = GetCart();
            string totalPriceConvert = Convert.ToString(Math.Round(cart.TotalPrice(), 2));
            string totalPrice = totalPriceConvert.Replace(",", ".");

            var shippingaddres = (OrderShippingAddress)Session["ShippingAddress"];

            Options options = new Options();
            options.ApiKey = "sandbox-1kYKKwWG9WdnrtS1c9zUNuRFftvtMQJM";
            options.SecretKey = "sandbox-YuANBFUNc9ITymKmMzeUpMAkkvryC6TF";
            options.BaseUrl = "https://sandbox-api.iyzipay.com";

            CreateCheckoutFormInitializeRequest request = new CreateCheckoutFormInitializeRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = "123456789";
            request.Price = totalPrice.ToString();
            request.PaidPrice = totalPrice.ToString();
            request.Currency = Currency.TRY.ToString();
            request.BasketId = shippingaddres.OrderNumber.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
            request.CallbackUrl = "http://localhost:50262/Cart/Complate";

            //List<int> enabledInstallments = new List<int>();
            //enabledInstallments.Add(2);
            //enabledInstallments.Add(3);
            //enabledInstallments.Add(6);
            //enabledInstallments.Add(9);
            //request.EnabledInstallments = enabledInstallments;

            Buyer buyer = new Buyer();
            buyer.Id = shippingaddres.OrderNumber;
            buyer.Name = shippingaddres.Name;
            buyer.Surname = shippingaddres.Surname;
            buyer.GsmNumber = shippingaddres.Phone;
            buyer.Email = shippingaddres.Email;
            buyer.IdentityNumber = "74300864791";
            buyer.LastLoginDate = "2015-10-05 12:43:35";
            buyer.RegistrationDate = "2013-04-21 15:12:09";
            buyer.RegistrationAddress = shippingaddres.Address;
            buyer.Ip = "85.34.78.112";
            buyer.City = shippingaddres.City;
            buyer.Country = "Turkey";
            buyer.ZipCode = "34732";
            request.Buyer = buyer;

            Address shippingAddress = new Address();
            shippingAddress.ContactName = shippingaddres.Name + " " + shippingaddres.Surname;
            shippingAddress.City = shippingaddres.City;
            shippingAddress.Country = "Turkey";
            shippingAddress.Description = shippingaddres.Address;
            shippingAddress.ZipCode = "34742";
            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address();
            billingAddress.ContactName = shippingaddres.Name + " " + shippingaddres.Surname;
            billingAddress.City = shippingaddres.City;
            billingAddress.Country = "Turkey";
            billingAddress.Description = shippingaddres.Address;
            billingAddress.ZipCode = "34742";
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();

            foreach (var product in GetCart().CartLines)
            {
                BasketItem basketproduct = new BasketItem();
                basketproduct.Id = product.Product.Id.ToString();
                basketproduct.Name = product.Product.Name;
                basketproduct.Category1 = (db.Category.Find(product.Product.CategoryId).Name).ToString();
                basketproduct.ItemType = BasketItemType.PHYSICAL.ToString();

                double price = product.Product.Price * product.Quantity;
                string editPrice = Convert.ToString(Math.Round(price, 2));
                string productPrice = editPrice.Replace(",", ".");

                basketproduct.Price = productPrice.ToString();
                basketItems.Add(basketproduct);

            }

            request.BasketItems = basketItems;

            CheckoutFormInitialize checkoutFormInitialize = CheckoutFormInitialize.Create(request, options);

            ViewBag.pay = checkoutFormInitialize.CheckoutFormContent;

            if (checkoutFormInitialize.Status == "Success")
            {
                return RedirectToAction("Complate");
            }
            return View();
        }
        public ActionResult Complate()
        {
            return View();
        }
    }


}