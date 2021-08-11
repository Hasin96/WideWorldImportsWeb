using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using WorldWideImports.Data;
using WorldWideImports.Domain;

namespace ded
{
    class Program
    {
        static WideWorldImportersContext _context = new WideWorldImportersContext();
        public  static void Main(string[] args)
        {
            Console.WriteLine("yo");
            //var p = _context.InvoiceLines
            //     .Include(il => il.Invoice.Customer.DeliveryCity.StateProvince)
            //     .AsNoTracking()
            //     .GroupBy(
            //     il => new
            //     {
            //         Year = _context.DatePart("Year", il.Invoice.InvoiceDate),
            //         Quarter = _context.DatePart("Quarter", il.Invoice.InvoiceDate),
            //         Week = _context.DatePart("Week", il.Invoice.InvoiceDate)
            //     },
            //     il => il,
            //     (yearQuarterWeek, invoiceLines) => new
            //     {
            //         Year = yearQuarterWeek.Year,
            //         Quarter = yearQuarterWeek.Quarter,
            //         Week = yearQuarterWeek.Week,
            //         SalesAmount = invoiceLines.Sum(il => il.ExtendedPrice)
            //     })
            //     .ToList();

            var cts = _context.CustomerTransactions
                .AsNoTracking()
                .Where(ct => ct.TransactionTypeId == 1)
                .Take(5) // subquery
                .OrderByDescending(ct => ct.TransactionDate)
                .Select(ct => new { ct.Customer.CustomerName, ct.TransactionAmount })
                .ToList();


            Console.WriteLine("Hello World!");
        }
    }
}
