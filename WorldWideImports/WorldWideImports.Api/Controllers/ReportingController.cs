using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldWideImports.Data;

namespace WorldWideImports.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportingController : ControllerBase
    {
        WideWorldImportersContext _context;

        public ReportingController(WideWorldImportersContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetSalesByWeekGroupedByYearAndQuarter")]
        public async Task<ActionResult> GetSalesByWeekGroupedByYearAndQuarter(string salesTerritory = "%")
        {
            try
            {
                var query = _context.InvoiceLines.AsQueryable();

                if (salesTerritory.Equals("%"))
                    query = query.Where(il => EF.Functions.Like(il.Invoice.Customer.DeliveryCity.StateProvince.SalesTerritory, salesTerritory));
                else
                    query = query.Where(il => il.Invoice.Customer.DeliveryCity.StateProvince.SalesTerritory == salesTerritory);

                return Ok(
                     await query
                     .GroupBy(
                     il => new
                     {
                         Year    = _context.DatePart("Year", il.Invoice.InvoiceDate),
                         Quarter = _context.DatePart("Quarter", il.Invoice.InvoiceDate),
                         Week    = _context.DatePart("Week", il.Invoice.InvoiceDate)
                     },
                     il => il,
                     (yearQuarterWeek, invoiceLines) => new
                     {
                         Year        = yearQuarterWeek.Year,
                         Quarter     = yearQuarterWeek.Quarter,
                         Week        = yearQuarterWeek.Week,
                         SalesAmount = invoiceLines.Sum(il => il.ExtendedPrice)
                     })
                     .ToListAsync()
                );
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet]
        [Route("GetLast5CustomerTransactions")]
        public async Task<ActionResult> GetLast5CustomerTransactions(string salesTerritory = "%")
        {
            try
            {
                var query = _context.CustomerTransactions.AsQueryable();

                if (salesTerritory.Equals("%"))
                    query = query.Where(ct => EF.Functions.Like(ct.Customer.DeliveryCity.StateProvince.SalesTerritory, salesTerritory) && ct.TransactionTypeId == 1);
                else
                    query = query.Where(ct => ct.Customer.DeliveryCity.StateProvince.SalesTerritory == salesTerritory && ct.TransactionTypeId == 1); 

                return Ok(await query
                    .OrderByDescending(ct => ct.TransactionDate)
                    .Select(ct => new { ct.Customer.CustomerName, ct.TransactionAmount})
                    .Take(5) // take causes a subquery to happen. 
                    .ToListAsync());
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet]
        [Route("GetSalesGroupedByYearAndQuarter")]
        public async Task<ActionResult> GetSalesGroupedByYearAndQuarter(string salesTerritory = "%")
        {
            try
            {
                var query = _context.InvoiceLines.AsQueryable();

                if (salesTerritory.Equals("%"))
                    query = query.Where(il => EF.Functions.Like(il.Invoice.Customer.DeliveryCity.StateProvince.SalesTerritory, salesTerritory));
                else
                    query = query.Where(il => il.Invoice.Customer.DeliveryCity.StateProvince.SalesTerritory == salesTerritory); 

                return Ok(await query
                    .GroupBy(
                    il => new
                    {
                        Year    = _context.DatePart("Year", il.Invoice.InvoiceDate),
                        Quarter = _context.DatePart("Quarter", il.Invoice.InvoiceDate)
                    },
                    il => il,
                    (yearQuarter, invoiceLines) => new
                    {
                        Year        = yearQuarter.Year,
                        Quarter     = yearQuarter.Quarter,
                        SalesAmount = invoiceLines.Sum(il => il.ExtendedPrice)
                    })
                    .ToListAsync());
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet]
        [Route("GetOrdersGroupedByYearAndQuarter")]
        public async Task<ActionResult> GetOrdersGroupedByYearAndQuarter(string salesTerritory = "%")
        {
            try
            {
                var query = _context.OrderLines.AsQueryable();

                if (salesTerritory.Equals("%"))
                    query = query.Where(ol => EF.Functions.Like(ol.Order.Customer.DeliveryCity.StateProvince.SalesTerritory, salesTerritory));
                else
                    query = query.Where(ol => ol.Order.Customer.DeliveryCity.StateProvince.SalesTerritory == salesTerritory);
                

                return Ok(await query
                    .GroupBy(
                    ol => new 
                    { 
                        Year    = _context.DatePart("Year", ol.Order.OrderDate),
                        Quarter = _context.DatePart("Quarter", ol.Order.OrderDate)
                    },
                    ol => ol.OrderLineId,
                    (yearQuarter, orderLines) => new
                    {
                        Year       = yearQuarter.Year,
                        Quarter    = yearQuarter.Quarter,
                        OrderCount = orderLines.Count()
                    })
                    .ToListAsync());
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
    }
}
