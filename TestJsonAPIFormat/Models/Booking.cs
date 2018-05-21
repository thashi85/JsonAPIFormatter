using JsonAPIFormatSerializer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestJsonAPIFormat.Model
{
    [Resource(new string[] { "self:bookings/#{booking.id}#/","test:dummy" })]
    public class Booking 
    {
        public Booking()
        {
           
        }

        public int? Id { get; set; }
        public DateTime? BookingDate { get; set; }
        public int? BookingStatusId { get; set; }
        private string bookingStatusName;
        public string BookingStatusName { get { return bookingStatusName; } }
        public string Note { get; set; }

        public string Etag { get; set; }
        [Resource(new string[] { "self:bookings/#{booking.id}#/relationships/clients/#{client.id}#", "related:clients/#{client.id}#/" },IncludeName ="client")]
        public Client Customer { get; set; }

        [Resource(new string[] { "self:bookings/#{booking.id}#/relationships/bookingdetails", "related:bookings/#{booking.id}#/bookingdetails" },IncludeName ="booking.details")]
        public List<BookingDetail> BookingDetails { get; set; }
        [Resource(IsResource = false)]
        public List<SalesCategory> SalesCategories { get; set; }
        [Resource(IsResource = false)]
        public SalesChannel SalesChannel { get; set; }

    }
    [Resource(new string[] { "self:bookings/#{booking.id}#/bookingdetails/#{bookingdetail.id}#" })]
    public class BookingDetail 
    {
        public int? Id { get; set; }
        public int? BookingDetailId { get; set; }
        public int? PackageId { get; set; }
        public string PackageName { get; set; }
        private int? eventId;
        public int? EventId { get { return eventId; } }
        private string eventName;
        public string EventName { get { return eventName; } }
        public float? UnitNetPrice { get; set; }
        public float? UnitTax { get; set; }
        public float? UnitGrossPrice { get; set; }
        public float? UnitNetPriceExcludingExtras { get; set; }
        public float? UnitTaxExcludingExtras { get; set; }
        public float? UnitGrossPriceExcludingExtras { get; set; }

        public float? TotalNetPrice { get; set; }
        public float? TotalTax { get; set; }
        public float? TotalGrossPrice { get; set; }
        [Resource(new string[] { "self:bookings/#{booking.id}#/bookingdetails/#{bookingdetail.id}#/relationships/bookingitems",
            "related:bookings/#{booking.id}#/bookingdetails/#{bookingdetail.id}#/bookingitems"
        })]
        public List<BookingItem> BookingItems { get; set; }
    }
    [Resource(new string[] { "self:bookings/#{booking.id}#/bookingdetails/#{bookingdetail.id}#/bookingitems/#{bookingitem.id}#"
        })]
    public class BookingItem 
    {
        public int? Id { get; set; }
        public int? BookingItemId { get; set; }

        public int? ItemId { get; set; }

        public string Name { get; set; }

        public int? ItemTypeId { get; set; }

    }
    [Resource(new string[] { "self:contacts/#{contact.id}#/" })]
    public class Contact 
    {
        public int? Id { get; set; }
        public string ContactRef { get; set; }
        public string FirstName { get; set; }
        public string Initial { get; set; }
        public string LastName { get; set; }
        public int? TitleId { get; set; }
        public string TitleName { get; set; }      
        public Client Client { get; set; }

    }

    public class Client 
    {
        public int? Id { get; set; }
        public string ClientRef { get; set; }       
        public List<Contact> Contacts { get; set; }
    }

    public class CooperateClient:Client
    {
        public string CompanyName { get; set; }
    }
    public class IndividualClient : Client
    {
        public string PhoneNo { get; set; }
    }

    [Resource(IsResource = false)]
    public class SalesChannel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Resource(IsResource = false)]
    public class SalesCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}