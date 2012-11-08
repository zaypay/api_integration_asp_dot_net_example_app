using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace MvcApplication2.Models
{
    public class SampleData : DropCreateDatabaseIfModelChanges<ZaypayDBContext>
    {
        protected override void Seed(ZaypayDBContext context)
        {
            
            new List<Product>
            {
                new Product{ ID = 1, Name = "40 Points Package", Description = "40 points package deal", PriceSettingId = 140494 },
                new Product{ ID = 2, Name = "20 Points Package", Description = "20 points package deal", PriceSettingId = 140494 },
                new Product{ ID = 3, Name = "Unlimited Points", Description = "Unlimited points deal", PriceSettingId = 140494 }
            }.ForEach(i => context.Products.Add(i));

            context.SaveChanges();

        }
    }
}