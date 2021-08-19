using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorldWideImports.Data;

namespace WorldWideImports.Client.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        WideWorldImportersContext _context;

        public ReportsController(WideWorldImportersContext context)
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

                var queryResult = await query
                .Where(
                il => il.Invoice.InvoiceDate.Year >= DateTime.Now.AddYears(-5).Year
                && il.Invoice.InvoiceDate.Year <= DateTime.Now.Year)
                //.Where(il => il.Invoice.InvoiceDate.Year <= DateTime.Now.Year && il.Invoice.InvoiceDate.Year >= DateTime.Now.AddYears(-1).Year)
                .GroupBy(
                il => new
                {
                    Year = _context.DatePart("Year", il.Invoice.InvoiceDate),
                    Quarter = _context.DatePart("Quarter", il.Invoice.InvoiceDate),
                    Week = _context.DatePart("Week", il.Invoice.InvoiceDate)
                },
                (yearQuarterWeek, invoiceLines) => new
                {
                    Year = yearQuarterWeek.Year,
                    Quarter = yearQuarterWeek.Quarter,
                    Week = yearQuarterWeek.Week,
                    SalesAmount = invoiceLines.Sum(il => il.ExtendedPrice)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Quarter).ThenBy(x => x.Week)
                .ToListAsync();

                var lineChartData = queryResult.GroupBy(s => new
                {
                    Year = s.Year,
                    Quarter = s.Quarter
                })
                .Select(group => new
                {
                    Group = group,
                    Count = group.Count()
                })
                .SelectMany(groupWithCount => groupWithCount.Group.Select(b => b)
                        .Zip(
                            Enumerable.Range(1, groupWithCount.Count),
                            ((j, i) => new
                            {
                                j.Year,
                                j.Quarter,
                                j.SalesAmount,
                                weekNumber = i
                            })
                        )
                ) ;

                   var series = lineChartData
                    .GroupBy(x => new { x.Year, x.Quarter })
                    .Select(x =>
                    {
                        decimal salesAmount = 0;

                        return new
                        {
                            Name = GetName(x.Key.Year.Value, x.Key.Quarter.Value, x.Sum(il => il.SalesAmount)),
                            Color = GetColor(x.Key.Year.Value, x.Key.Quarter.Value),
                            LineWidth = GetLineWidth(x.Key.Year.Value, x.Key.Quarter.Value),
                            Values = x.Select(y =>
                            {
                                var SalesAmount = y.SalesAmount + salesAmount;
                                var WeekNumber = y.weekNumber;

                                salesAmount = SalesAmount;

                                return new 
                                { 
                                    SalesAmount, 
                                    WeekNumber 
                                };

                            }).ToList()
                        };
                    }); 


                var viewModel = new
                {
                   // MaxSalesAmount = series.Select(d => d.Values).Max(d => d.Sum(t => t.SalesAmount)),
                    Series = series,
                    MaxSalesAmount = series.Select(x => x.Values.Max(y => y.SalesAmount)).Max(),
                    WeekNumbers = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }
                };

                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        private int GetLineWidth(int year, int quarter)
        {
            if ((year == DateTime.Now.Year && quarter == ((DateTime.Now.Month + 2) / 3)))
            {
                return 5;
            }
            else if ((year == DateTime.Now.Year && quarter == (((DateTime.Now.Month + 2) / 3) - 1)))
            {
                return 3;
            }
            else if ((year == DateTime.Now.AddYears(-1).Year && quarter == ((DateTime.Now.Month + 2) / 3)))
            {
                return 3;
            }
            else
            {
                return 1;
            }
        }

        private string GetColor(int year, int quarter)
        {
                
            if (  (year == DateTime.Now.Year && quarter == ((DateTime.Now.Month + 2) / 3)))
            {
                return "dodgerblue";
            }
            else if ((year == DateTime.Now.Year && quarter == (((DateTime.Now.Month + 2) / 3) - 1)))
            {
                return "darkorange";
            }
            else if ((year == DateTime.Now.AddYears(-1).Year && quarter == ((DateTime.Now.Month + 2) / 3)))
            {
                return "green";
            }
            else
            {
                return "lightgrey";
            }
        }

        private string GetName(int year, int quarter, decimal salesAmount)
        {
            if ((year == DateTime.Now.Year && quarter == ((DateTime.Now.Month + 2) / 3)))
            {
                return string.Format(year + " Q" + quarter + " " + Format(Math.Ceiling(salesAmount)));
            } else
                return string.Empty;
        }

        private string Format(decimal num)
        {
            if (num < 1000000)
                return num.ToString("C").Substring(0, 3) + "k";

            if (num < 1000000000)
                return num.ToString("C").Substring(0, 2) + "m";

            return num.ToString("C").Substring(0, 2) + "b";
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
                    .Select(ct => new { ct.Customer.CustomerName, TransactionAmount = ct.TransactionAmount.ToString() })
                    .Take(5) // take causes a subquery to happen. 
                    .ToListAsync());
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet]
        [Route("GetTop5SalesBySalesTerritory")]
        public async Task<ActionResult> GetTop5SalesBySalesTerritoryThisQuarter()
        {
            try
            {
                var top5SalesBySalesTerritoryData = await _context.InvoiceLines
                    .Where(
                        il => _context.DatePart("Year", il.Invoice.InvoiceDate) == _context.DatePart("Year", DateTime.Now)
                        && _context.DatePart("Quarter", il.Invoice.InvoiceDate) == _context.DatePart("Quarter", DateTime.Now)
                    ).GroupBy(
                        il => new
                        {
                            SalesTerritory = il.Invoice.Customer.DeliveryCity.StateProvince.SalesTerritory,
                            Year = _context.DatePart("Year", il.Invoice.InvoiceDate),
                            Quarter = _context.DatePart("Quarter", il.Invoice.InvoiceDate)
                        },
                        il => il,
                        (yearQuarterSalesTerritory, invoiceLines) => new
                        {
                            SalesAmount    = invoiceLines.Sum(il => il.ExtendedPrice),
                            SalesTerritory = yearQuarterSalesTerritory.SalesTerritory,
                        }
                    )
                    .Take(5)
                    .OrderByDescending(il => il.SalesAmount)
                    .ToListAsync();

                return Ok(new
                {
                    MaxSalesAmount   = top5SalesBySalesTerritoryData.Max(s => s.SalesAmount),
                    SalesTerritories = top5SalesBySalesTerritoryData.Select(s => s.SalesTerritory),
                    top5SalesBySalesTerritoryData
                });
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet]
        [Route("GetTop5SalesByStateProvinceThisQuarter")]
        public async Task<ActionResult> GetTop5SalesByStateProvinceThisQuarter(string SalesTerritory)
        {
            try
            {

                if (string.IsNullOrEmpty(SalesTerritory))
                    return BadRequest("Sales territory cannot be null");

                var isGivenSalesTerritoryValid = _context.StateProvinces.Any(sp => sp.SalesTerritory == SalesTerritory);
                if (isGivenSalesTerritoryValid == false)
                    return BadRequest("Can't find sales territory");

                return Ok(await _context.InvoiceLines
                    .Where(
                        il => _context.DatePart("Year", il.Invoice.InvoiceDate) == _context.DatePart("Year", DateTime.Now)
                        && _context.DatePart("Quarter", il.Invoice.InvoiceDate) == _context.DatePart("Quarter", DateTime.Now)
                        && il.Invoice.Customer.DeliveryCity.StateProvince.SalesTerritory == SalesTerritory
                    ).GroupBy(
                        il => new
                        {
                            StateProvince = il.Invoice.Customer.DeliveryCity.StateProvince.StateProvinceName,
                            Year = _context.DatePart("Year", il.Invoice.InvoiceDate),
                            Quarter = _context.DatePart("Quarter", il.Invoice.InvoiceDate)
                        },
                        il => il,
                        (yearQuarterStateProvince, invoiceLines) => new
                        {
                            SalesAmount = invoiceLines.Sum(il => il.ExtendedPrice),
                            StateProvince = yearQuarterStateProvince.StateProvince
                        }
                    )
                    .Take(5)
                    .OrderByDescending(il => il.SalesAmount)
                    .ToListAsync());
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet]
        [Route("GetSales_Profit_ProfitMargin_OrderCount_GroupedByYearAndQuarter")]
        public async Task<ActionResult> GetSales_Profit_ProfitMargin_OrderCount_GroupedByYearAndQuarter(string salesTerritory = "%")
        {
            try
            {
                var query = _context.InvoiceLines.AsQueryable();

                if (salesTerritory.Equals("%"))
                    query = query.Where(il => EF.Functions.Like(il.Invoice.Customer.DeliveryCity.StateProvince.SalesTerritory, salesTerritory));
                else
                    query = query.Where(il => il.Invoice.Customer.DeliveryCity.StateProvince.SalesTerritory == salesTerritory);

                var queryResult = await query
                    .Where(
                    il => il.Invoice.InvoiceDate.Year >= DateTime.Now.AddYears(-5).Year
                    && il.Invoice.InvoiceDate.Year <= DateTime.Now.Year)
                    //.Where(il => il.Invoice.InvoiceDate.Year == DateTime.Now.AddYears(-3).Year)
                    .GroupBy(
                    il => new
                    {
                        Year = _context.DatePart("Year", il.Invoice.InvoiceDate),
                        Quarter = _context.DatePart("Quarter", il.Invoice.InvoiceDate)
                    },
                    il => new { InvoiceLines = il, Orders = il.Invoice.Order },
                    (yearQuarter, invoiceLinesAndOrders) => new
                    {
                        Year = yearQuarter.Year,
                        Quarter = yearQuarter.Quarter,
                        SalesAmount = invoiceLinesAndOrders.Sum(il => il.InvoiceLines.ExtendedPrice),
                        ProfitAmount = invoiceLinesAndOrders.Sum(il => il.InvoiceLines.LineProfit),
                        OrderCount = invoiceLinesAndOrders.Select(il => il.Orders.OrderId).Distinct().Count()
                    })
                    .ToListAsync();

                var barChartData = queryResult.Select(data => new
                {
                    Year         = data.Year,
                    Quarter      = data.Quarter,
                    SalesAmount  = data.SalesAmount,
                    ProfitAmount = data.ProfitAmount,
                    ProfitMargin = (data.ProfitAmount / data.SalesAmount),
                    OrderCount   = data.OrderCount
                });

                var viewModel = new
                {
                    Dates           = barChartData.OrderBy(b => b.Year).ThenBy(b => b.Quarter).Select(bcd => bcd.Year + " " + bcd.Quarter),
                    Years           = barChartData.Select(b => b.Year).OrderBy(b => b.Value),
                    MaxSalesAmont   = barChartData.Max(bcd => bcd.SalesAmount),
                    MaxProfitAmount = barChartData.Max(bcd => bcd.ProfitAmount),
                    MaxProfitMargin = barChartData.Max(bcd => bcd.ProfitMargin),
                    MaxOrderCount   = barChartData.Max(bcd => bcd.OrderCount),
                    BarChartData    = barChartData
                };


                return Ok(viewModel);

            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
    }
}
