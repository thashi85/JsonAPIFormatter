﻿using JsonAPISerializer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TestJsonAPIFormat.Model;

namespace TestJsonAPIFormat
{
    public class Program
    {
        static void Main(string[] args)
        {
            var booking = new Booking()
            {
                Id = 3,
                BookingDate = DateTime.Now,
                BookingStatusId = 1,
                Note = "test 19328794387439378943",
                Etag = "20398478383",
                SalesChannel=new SalesChannel() {
                    Id =1,
                    Name ="Cooperate"
                },
                SalesCategories=new List<SalesCategory>() {
                    new SalesCategory() {
                        Id =1,
                        Name ="Category 1"} ,
                    new SalesCategory() {
                        Id = 2,
                        Name = "Category 2" } }
            };

            {
                booking.BookingDetails = new List<BookingDetail>();
                var bookingDetail = new BookingDetail()
                {
                    Id = 1,
                    BookingDetailId = 1,
                    PackageId = 2,
                    UnitGrossPrice = 100,
                    UnitNetPrice = 80,
                    UnitTax = 20
                };

                {
                    var bookingItem = new BookingItem()
                    {
                        Id = 100,
                        BookingItemId = 100,
                        ItemId = 7,
                        Name = "Birthday Cake"
                    };
                    bookingDetail.BookingItems = new List<BookingItem>();
                    bookingDetail.BookingItems.Add(bookingItem);

                    bookingItem = new BookingItem()
                    {
                        Id = 101,
                        BookingItemId = 101,
                        ItemId = 9,
                        Name = "Coffee and Tea"
                    };
                    bookingDetail.BookingItems.Add(bookingItem);

                }
                booking.BookingDetails.Add(bookingDetail);
                bookingDetail = new BookingDetail()
                {
                    Id = 2,
                    BookingDetailId = 2,
                    PackageId = 3,
                    UnitGrossPrice = 200,
                    UnitNetPrice = 160,
                    UnitTax = 40
                };
                booking.BookingDetails.Add(bookingDetail);
            }


            {
                var client = new IndividualClient()
                {
                    Id = 1,
                    ClientRef="CL001",
                    PhoneNo="071258963"
                };
                //var client = new CooperateClient()
                //{
                //    Id = 1,
                //    ClientRef = "CL001",
                //    CompanyName = "Company ABC"
                //};
                booking.Customer = client;
            }

            var baseUrl="http://localhost/";
            //To serialize a POCO in json:api format
            //var setting = new JsonApiFormatSerializer(new string[] { "client", "bookingItems", "booking.details" },
            //     new string[] {  "Customer", "BookingDetails", "BookingDetail.bookingItems", "client.ClientRef" },
            //     new string[] { "self:bookings/#{booking.id}#" }               
            //);
            var setting = new JsonApiFormatSerializer();
            setting.BaseUrl = baseUrl;
            string json = JsonConvert.SerializeObject(booking,setting);
            Booking obj = JsonConvert.DeserializeObject<Booking>(json, new JsonApiFormatSerializer());


            var booking2 = new Booking()
            {
                Id = 2050,
                BookingDate = DateTime.Now,
                BookingStatusId = 1,
                Note = "test ",
                Etag = "789451"
            };

            {
                booking2.BookingDetails = new List<BookingDetail>();
                var bookingDetail = new BookingDetail()
                {
                    Id = 400,
                    BookingDetailId =400,
                    PackageId = 25,
                    UnitGrossPrice = 10,
                    UnitNetPrice = 5,
                    UnitTax = 7
                };
                booking2.BookingDetails.Add(bookingDetail);
            }
            List<Booking> bkList = new List<Booking>() { booking, booking2 };
            //To serialize a POCO in json:api format
            setting = new JsonApiFormatSerializer(null,null,new string[] { "self:bookings" });
            setting.BaseUrl = baseUrl;
            string json2 =( JsonConvert.SerializeObject(bkList, setting));
            List<Booking> objList = JsonConvert.DeserializeObject<List<Booking>>(json2, new JsonApiFormatSerializer());

        }
    }
}
