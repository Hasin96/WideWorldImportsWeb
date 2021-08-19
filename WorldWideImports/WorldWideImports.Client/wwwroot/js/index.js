//import { csv } from "https://cdn.skypack.dev/d3-fetch@3";
//import { min, max, extent, descending } from "https://cdn.skypack.dev/d3-array@3";
//import { axisLeft, axisTop } from "https://cdn.skypack.dev/d3-axis@3";
//import { scaleLinear, scaleBand } from "https://cdn.skypack.dev/d3-scale@4";
//import { selectAll, select } from "https://cdn.skypack.dev/d3-selection@3";
//import { timeFormat } from "https://cdn.skypack.dev/d3-time-format@4";
//import { format } from "https://cdn.skypack.dev/d3-format@3";

var index_module = (function () {
    let keyMetricsBarChartObj;
    let lineChartObj;
    let last5CustomerTransactionsObj;
    let top5SalesBySalesTerritoryData;

    function init() {
        
        fetch("/api/reports/GetSales_Profit_ProfitMargin_OrderCount_GroupedByYearAndQuarter")
            .then((response) => response.json())
            .then(data => {
                keyMetricsBarChartObj = data;

                createSalesBarChart();
                createProftBarChart();
                createProfitMarginBarChart();
                createOrderCountBarChart()

            });

        fetch("/api/reports/GetSalesByWeekGroupedByYearAndQuarter")
            .then((responsee) => responsee.json())
            .then(data => {
                lineChartObj = data;

                createSalesByQuarterLineChart()
            })

        fetch("/api/reports/GetLast5CustomerTransactions")
            .then((responsee) => responsee.json())
            .then(data => {
                last5CustomerTransactionsObj = data;

                var table = d3.select(".js-last-5-orders-container")
                    .append("table")
                    .attr("class", "table");

                var tbody = table.append("tbody");

                var tr = tbody
                    .selectAll("tr")
                    .data(last5CustomerTransactionsObj)
                    .enter()
                    .append("tr");

                tr
                    .append("td")
                    .text(d => d.customerName);

                tr
                    .append("td")
                    .text(d => d.transactionAmount);
            });

        fetch("/api/reports/GetTop5SalesBySalesTerritory")
            .then((responsee) => responsee.json())
            .then(data => {
                top5SalesBySalesTerritoryData = data;

                createTop5SalesByCompanyChart()
            })
    }

    function createTop5SalesByCompanyChart() {
        const margin = { top: 40, right: 100, bottom: 40, left: 100 };
        const width = 500 - margin.left - margin.right;
        const height = 200 - margin.top - margin.bottom;

        console.log(top5SalesBySalesTerritoryData)
        const xScale = d3.scaleLinear()
            .domain([0, top5SalesBySalesTerritoryData.maxSalesAmount])
            .range([0, width])
            //.padding(.4)

        // Scales
        const yScale = d3.scaleBand()
            .domain(top5SalesBySalesTerritoryData.salesTerritories)
            .range([0, height])
            .padding(;

        // Draw base
        const svg = d3.select(".top5SalesBySalesTerritory-container")
            .append("svg")
            .attr("width", width + margin.right + margin.left)
            .attr("height", height + margin.top + margin.bottom)
            .append("g")
            .attr("transform", `translate(${margin.left}, ${margin.top})`);

        // Draw Bars
        const bars = svg
            .selectAll(".bar")
            .data(top5SalesBySalesTerritoryData.top5SalesBySalesTerritoryData)
            .enter()
            .append("rect")
            .attr("class", "bar")
            //.attr("x", d => xScale(d.salesTerritory))
            .attr("y", d => yScale(d.salesTerritory))
            .attr("width", d => xScale(d.salesAmount))
            .attr("height", d => yScale.bandwidth() )
            .style("fill", "dodgerblue")

        const ticksAmount = 3;

        const yAxis = d3.axisLeft(yScale)
            //.tickFormat(formatTicks)
            .tickSizeInner(-height)
            .tickSize(0)
            .ticks(ticksAmount)

        const yAxisIsDraw = svg
            .append("g")
            .attr("class", "y axis")
            .call(yAxis);

        yAxisIsDraw.selectAll("text").attr("dx", "-.2rem");

        const xAxis = d3.axisBottom(xScale).tickFormat(formatTicks).tickSize(0);

        const xAxisIsDraw = svg.append("g")
            .attr("class", "x axis")
            .attr("transform", `translate(0, ${height})`)
            .call(xAxis);

        xAxisIsDraw.selectAll("text").attr("dy", "1rem");

    }

    function createSalesByQuarterLineChart() {
        const margin = { top: 80, right: 60, bottom: 40, left: 60 };
        const width = 800 - margin.right - margin.left;
        const height = 500 - margin.top - margin.bottom;

        const xScale = d3.scaleLinear()
            .domain(d3.extent(lineChartObj.weekNumbers))
            .range([0, width])

        // Scales
        const yScale = d3.scaleLinear()
            .domain([0, lineChartObj.maxSalesAmount])
            .range([height, 0]);

        // Draw base
        const svg = d3.select(".linechart-container")
            .append("svg")
            .attr("width", width + margin.right + margin.left)
            .attr("height", height + margin.top + margin.bottom)
            .append("g")
            .attr("transform", `translate(${margin.left}, ${margin.top})`);


        const lineGen = d3
            .line()
            .x(d => xScale(d.weekNumber))
            .y(d => yScale(d.salesAmount));

        const ticksAmount = 5;

        const yAxis = d3.axisLeft(yScale)
            .tickFormat(formatTicks)
            .tickSizeInner(-width)
            .tickSize(0)
            .ticks(ticksAmount)

        const yAxisIsDraw = svg
            .append("g")
            .attr("class", "y axis")
            .call(yAxis);

        yAxisIsDraw.selectAll("text").attr("dx", "-.2rem");

        const xAxis = d3.axisBottom(xScale);
            //.tickFormat(removeYear).tickSize(0);

        const xAxisIsDraw = svg.append("g")
            .attr("class", "x axis")
            .attr("transform", `translate(0, ${height})`)
            .call(xAxis);

        xAxisIsDraw.selectAll("text").attr("dy", "1rem");

        const chartGroup = svg.append("g").attr("class", "line-chart");

        chartGroup
            .selectAll(".line-series")
            .data(lineChartObj.series)
            .enter()
            .append("path")
            .attr("class", d => `line-series ${d.name.toLowerCase()}`)
            .attr('d', d => lineGen(d.values))
            .style("fill", "none")
            .style("stroke", d => d.color)
            .style("stroke-width", d => d.lineWidth);


        chartGroup
            .append("g")
            .attr("class", "series-labels")
            .selectAll(".series-label")
            .data(lineChartObj.series)
            .enter()
            .append("text")
            .attr("x", d => xScale(d.values[d.values.length - 1].weekNumber) + 5)
            .attr("y", d => yScale(d.values[d.values.length - 1].salesAmount))
            .text(d => d.name)
            .style("dominant-baseline", "central")
            .style("font-size", "0.7em")
            .style("font-weight", "bold")
            .style("fill", d => d.color);

       

    }

    function createOrderCountBarChart() {
        const margin = { top: 40, right: 100, bottom: 40, left: 40 };
        const width = 800 - margin.left - margin.right;
        const height = 200 - margin.top - margin.bottom;

        const xScale = d3.scaleBand()
            .domain(keyMetricsBarChartObj.dates)
            .range([0, width])
            .padding(.4)

        // Scales
        const yScale = d3.scaleLinear()
            .domain([0, keyMetricsBarChartObj.maxOrderCount])
            .range([height, 0]);

        // Draw base
        const svg = d3.select(".orderCountBarChart-container")
            .append("svg")
            .attr("width", width + margin.right + margin.left)
            .attr("height", height + margin.top + margin.bottom)
            .append("g")
            .attr("transform", `translate(${margin.left}, ${margin.top})`);

        // Draw Bars
        const bars = svg
            .selectAll(".bar")
            .data(keyMetricsBarChartObj.barChartData)
            .enter()
            .append("rect")
            .attr("class", "bar")
            .attr("x", d => xScale(d.year + " " + d.quarter))
            .attr("y", d => yScale(d.orderCount))
            .attr("width", d => xScale.bandwidth())
            .attr("height", d => height - yScale(d.orderCount))
            .style("fill", "dodgerblue")

        const ticksAmount = 3;

        const yAxis = d3.axisLeft(yScale)
            .tickFormat(formatTicks)
            .tickSizeInner(-height)
            .tickSize(0)
            .ticks(ticksAmount)

        const yAxisIsDraw = svg
            .append("g")
            .attr("class", "y axis")
            .call(yAxis);

        yAxisIsDraw.selectAll("text").attr("dx", "-.2rem");

        const xAxis = d3.axisBottom(xScale).tickFormat(removeYear).tickSize(0);

        const xAxisIsDraw = svg.append("g")
            .attr("class", "x axis")
            .attr("transform", `translate(0, ${height})`)
            .call(xAxis);

        xAxisIsDraw.selectAll("text").attr("dy", "1rem");

    }

    function createProfitMarginBarChart() {
        const margin = { top: 40, right: 100, bottom: 40, left: 40 };
        const width = 800 - margin.left - margin.right;
        const height = 200 - margin.top - margin.bottom;

        const xScale = d3.scaleBand()
            .domain(keyMetricsBarChartObj.dates)
            .range([0, width])
            .padding(.4)

        const categoryScale = d3.scaleBand().domain(keyMetricsBarChartObj.years).range([0, width]);

        // Scales
        const yScale = d3.scaleLinear()
            .domain([0, keyMetricsBarChartObj.maxProfitMargin])
            .range([height, 0]);

        // Draw base
        const svg = d3.select(".profitMarginBarchart-container")
            .append("svg")
            .attr("width", width + margin.right + margin.left)
            .attr("height", height + margin.top + margin.bottom)
            .append("g")
            .attr("transform", `translate(${margin.left}, ${margin.top})`);

        // Draw Bars
        const bars = svg
            .selectAll(".bar")
            .data(keyMetricsBarChartObj.barChartData)
            .enter()
            .append("rect")
            .attr("class", "bar")
            .attr("x", d => xScale(d.year + " " + d.quarter))
            .attr("y", d => yScale(d.profitMargin))
            .attr("width", d => xScale.bandwidth())
            .attr("height", d => height - yScale(d.profitMargin))
            .style("fill", "dodgerblue")

        const ticksAmount = 3;

        const yAxis = d3.axisLeft(yScale)
            .tickFormat(formatPercentage)
            .tickSizeInner(-height)
            .tickSize(0)
            .ticks(ticksAmount);

        const yAxisIsDraw = svg
            .append("g")
            .attr("class", "y axis")
            .call(yAxis);

        yAxisIsDraw.selectAll("text").attr("dx", "-.2rem");

        const xAxis = d3.axisBottom(xScale).tickFormat(removeYear).tickSize(0);

        const xAxisIsDraw = svg.append("g")
            .attr("class", "x axis")
            .attr("transform", `translate(0, ${height})`)
            .call(xAxis);

        xAxisIsDraw.selectAll("text").attr("dy", "1rem");

    }

    function createProftBarChart() {
        const margin = { top: 40, right: 100, bottom: 40, left: 40 };
        const width  = 800 - margin.left - margin.right;
        const height = 200 - margin.top - margin.bottom;

        const xScale = d3.scaleBand()
            .domain(keyMetricsBarChartObj.dates)
            .range([0, width])
            .padding(.4)

        const categoryScale = d3.scaleBand().domain(keyMetricsBarChartObj.years).range([0, width]);

        // Scales
        const yScale = d3.scaleLinear()
            .domain([0, keyMetricsBarChartObj.maxProfitAmount])
            .range([height, 0]);

        // Draw base
        const svg = d3.select(".profitBarchart-container")
            .append("svg")
            .attr("width", width + margin.right + margin.left)
            .attr("height", height + margin.top + margin.bottom)
            .append("g")
            .attr("transform", `translate(${margin.left}, ${margin.top})`);

        // Draw Bars
        const bars = svg
            .selectAll(".bar")
            .data(keyMetricsBarChartObj.barChartData)
            .enter()
            .append("rect")
            .attr("class", "bar")
            .attr("x", d => xScale(d.year + " " + d.quarter))
            .attr("y", d => yScale(d.profitAmount))
            .attr("width", d => xScale.bandwidth())
            .attr("height", d => height - yScale(d.profitAmount))
            .style("fill", "dodgerblue")

        const ticksAmount = 3;

        const yAxis = d3.axisLeft(yScale)
            .tickFormat(formatTicks)
            .tickSizeInner(-height)
            .tickSize(0)
            .ticks(ticksAmount);

        const yAxisIsDraw = svg
            .append("g")
            .attr("class", "y axis")
            .call(yAxis);

        yAxisIsDraw.selectAll("text").attr("dx", "-.2rem");

        const xAxis = d3.axisBottom(xScale).tickFormat(removeYear).tickSize(0);

        const xAxisIsDraw = svg.append("g")
            .attr("class", "x axis")
            .attr("transform", `translate(0, ${height})`)
            .call(xAxis);

        xAxisIsDraw.selectAll("text").attr("dy", "1rem");

    }

    function createSalesBarChart() {
        const margin = { top: 40, right: 100, bottom: 40, left: 40 };
        const width = 800 - margin.left - margin.right;
        const height = 200 - margin.top - margin.bottom;


        const xScale = d3.scaleBand()
            .domain(keyMetricsBarChartObj.dates)
            .range([0, width])
            .padding(.4)

        const categoryScale = d3.scaleBand().domain(keyMetricsBarChartObj.years).range([0, width]);



            // Scales
            const yScale = d3.scaleLinear()
                .domain([0, keyMetricsBarChartObj.maxSalesAmont])
                .range([height, 0]);

            // Draw base

            const svg = d3.select(".barchart-container")
                .append("svg")
                .attr("width", width + margin.right + margin.left)
                .attr("height", height + margin.top + margin.bottom)
                .append("g")
                .attr("transform", `translate(${margin.left}, ${margin.top})`);

            // Draw Bars
            console.log(keyMetricsBarChartObj.salesData);
            const bars = svg
                .selectAll(".bar")
                .data(keyMetricsBarChartObj.barChartData)
                .enter()
                .append("rect")
                .attr("class", "bar")
                .attr("x", d => xScale(d.year + " " + d.quarter))
                .attr("y", d => yScale(d.salesAmount))
                .attr("width", d => xScale.bandwidth())
                .attr("height", d => height - yScale(d.salesAmount))
                .style("fill", "dodgerblue")

            //var lels = svg.selectAll("g rect:nth-of-type(4n)");
            //svg.append("g")

            //.each(function () {

            //    var rect = this;

            //    var line = document.createElement("line");
            //    line.setAttribute("x1", "0");
            //    line.setAttribute("y1", "80");
            //    line.setAttribute("x2", "100");
            //    line.setAttribute("y2", "20");
            //    line.setAttribute("stroke", "black");
            //    line.setAttribute("stroke-width", "5");

            //    //d3.select(this)
            //    //    .insert("line", this.nextSibling);

            //    rect.parentNode.insertBefore(line, rect.nextSibling);
            //}).enter()

            const ticksAmount = 3;

            const yAxis = d3.axisLeft(yScale)
                .tickFormat(formatTicks)
                .tickSizeInner(-height)
                .tickSize(0)
                .ticks(ticksAmount);

            const yAxisIsDraw = svg
                .append("g")
                .attr("class", "y axis")
                .call(yAxis);

            yAxisIsDraw.selectAll("text").attr("dx", "-.2rem");

            const xAxis = d3.axisBottom(xScale).tickFormat(removeYear).tickSize(0);

            const xAxisIsDraw = svg.append("g")
                .attr("class", "x axis")
                .attr("transform", `translate(0, ${height})`)
                .call(xAxis);

            xAxisIsDraw.selectAll("text").attr("dy", "1rem");

            const xAxis2 = d3.axisTop(categoryScale).tickSize(0);

            const xAxisIsDraw2 = svg.append("g")
                .attr("class", "x2 axis")
                .call(xAxis2);

    }

    // format tick helper
    function removeYear(d) {
        return d.trim().substring(4, d.length);
    }

    function formatPercentage(d) {
        return d3.format(".0%")(d)
    }

    function formatTicks(d) {
        return d3.format("~s")(d)
            .replace("M", " mil")
            .replace("G", " bil")
            .replace("T", " tril")
    }

    return {
        init: init,
        scale: function () {
            const yScale = scaleLinear()
                .domain([0, xMax])
                .range([height, 0]);

            const xScale = scaleBand()
                .domain(keyMetricsBarChartObj.sort((a, b) => descending(a.quarter, b.quarter)).map(d => d.quarter))
                .range([0, width])
                .paddingInner(0.25);

            return {
                yScale: yScale,
                xScale: xScale
            }
        }
    }

})();

index_module.init();

