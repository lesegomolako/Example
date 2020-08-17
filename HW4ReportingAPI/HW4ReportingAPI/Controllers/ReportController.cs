using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Dynamic;
using System.Data.Entity;
using HW4ReportingAPI.Models;
using System.Web.Http.Cors;
using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using System.Web.Hosting;
using System.Net.Http.Headers;
using System.Data;

namespace HW4ReportingAPI.Controllers
{ 
    //[EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ReportController : ApiController
    {
        [Route("api/Reports/getReportData/")]
        [HttpGet]
        public dynamic getReportData([FromUri]int productSelection)
        {
            
            NorthwindEntities db = new NorthwindEntities();
            db.Configuration.ProxyCreationEnabled = false;
            List<Order_Detail> quantitys;

            if(productSelection == 1)
            {


                quantitys = db.Order_Details.Include(gg => gg.Order).Include(gg => gg.Product).Include(gg => gg.Product.Category).ToList();



            }
            else if(productSelection == 2)
            {
                quantitys = db.Order_Details.Include(gg => gg.Order).Include(gg => gg.Product).Include(gg => gg.Product.Category).
                    Where(gr => db.Products.Any(cc => cc.ProductID == gr.ProductID)).ToList();
            }
            else
            {
                quantitys = db.Order_Details.Include(gg => gg.Order).Include(gg => gg.Product).Include(gg => gg.Product.Category).ToList();
            }
            return getExpandoReport(quantitys);
            //try
            //{
            //    return getExpandoReport(quantity);
            //}
            //catch(Exception ex)
            //{
            //    return ex.ToString();
            //}
        }
        private dynamic getExpandoReport(List<Order_Detail> quantitys)
        {
            dynamic outObject = new ExpandoObject();
            outObject.ChartData = null;
            outObject.TableData = null;
            var catlist = quantitys.GroupBy(gg => gg.Product.Category.CategoryName);
            List<dynamic> cats = new List<dynamic>();
            foreach(var group in catlist)
            {
                dynamic catrgories = new ExpandoObject();
                catrgories.CategoryName = group.Key;
                catrgories.average = group.Average(gg => gg.Quantity);
                cats.Add(catrgories);
            }
            outObject.Categories = cats;

            var productlist = quantitys.GroupBy(gg => gg.Product.ProductName);
            List<dynamic> productGroups = new List<dynamic>();
            foreach(var group in productlist)
            {
                dynamic product = new ExpandoObject();
                product.ProductName = group.Key;
                product.average = group.Average(gg => gg.Quantity);
                List<dynamic> flexiquantitys = new List<dynamic>();
                foreach(var item in group )
                {
                    dynamic quantityObj = new ExpandoObject();
                    quantityObj.ProductName = item.Product.ProductName;
                    quantityObj.Customer = item.Order.CustomerID;
                    quantityObj.Quantity = item.Quantity;
                    flexiquantitys.Add(quantityObj);
                }
                product.Order_Detail = flexiquantitys;
                productGroups.Add(product);
            }
            outObject.ChartData = cats;
            outObject.TableData = productGroups;
            return outObject;

        }
    }
}
