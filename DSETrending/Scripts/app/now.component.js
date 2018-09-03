Vue.component('now-grid', {
        template: '#grid-template',
        props: {
            data: Array,
            columns: Array,
            filterKey: String
        },
        data: function() {
            var sortOrders = {}
            this.columns.forEach(function(key) {
                sortOrders[key] = 1;
            });
            return {
                sortKey: 'Value',
                sortOrders: sortOrders
            }
        },
        computed: {
            filteredData: function() {
                var sortKey = this.sortKey;
                var filterKey = this.filterKey && this.filterKey.toLowerCase();
                var order = this.sortOrders[sortKey] || 1;
                var data = this.data;
                if (filterKey) {
                    data = data.filter(function(row) {
                        return Object.keys(row).some(function(key) {
                            return String(row[key]).toLowerCase().indexOf(filterKey) > -1;
                        });
                    });
                }
                if (sortKey) {
                    data = data.slice().sort(function(a, b) {
                        a = a[sortKey];
                        b = b[sortKey];
                        return (a === b ? 0 : a < b ? 1 : -1) * order;
                    });
                }
                for (var i = 0; i < data.length; i++) {
                    data[i]["#"] = i + 1;
                }
                return data;
            }
        },
        filters: {
            capitalize: function(str) {
                return str.charAt(0).toUpperCase() + str.slice(1);
            }
        },
        methods: {
            sortBy: function(key) {
                this.sortKey = key;
                this.sortOrders[key] = this.sortOrders[key] * -1;
            }
        }
    });

// bootstrap the now
var now = new Vue({
    el: '#now',
    data: {
        items: [],
        searchQuery: '',
        gridColumns: ['#', 'Sector', 'Scrip', 'Sec', 'Cat', 'LTP', 'Change', 'Trade', 'Value', 'Volume', 'Spiked', 'Day 1', 'Day 2', 'Day 3', 'Day 4', 'Day 5'],
        gridData: [
        ]
    },
    mounted: function () {
        var self = this;
        $.ajax({
            url: '../api/defaultapi/now',
            method: 'GET',
            success: function (response) {
                var stocks = response.stocks;
                var data = response.now;
                var gridArray = [];
                for (var i = 0; i < data.length; i++) {
                    if (gridArray.filter(e => e.Scrip === data[i]["Scrip"]).length < 1) {
                        data[i]["Volume"] = meaningAmt(data[i]["Volume"]);
                        data[i]['Sec'] = stocks[i]['TotalSecurities'];
                        data[i]['Sector'] = stocks[i]['BusinessSegment'];
                        data[i]['Cat'] = stocks[i]['MarketCategory'];
                        if (data[i].Quotes.length > 5) {
                            data[i]["Spiked"] = data[i]['Value'] * 1000000 > (data[i].Quotes[data[i].Quotes.length - 2].Value + data[i].Quotes[data[i].Quotes.length - 3].Value)
                                || data[i]['Value'] * 1000000 > (data[i].Quotes[data[i].Quotes.length - 2].Value * 2) ? "YES" : "";

                            //data[i]["Day 1"] = meaningAmt(data[i].Quotes[data[i].Quotes.length - 2].Value) + " (" + meaningAmt(data[i].Quotes[data[i].Quotes.length - 2].Volume) + ")";
                            //data[i]["Day 2"] = meaningAmt(data[i].Quotes[data[i].Quotes.length - 3].Value) + " (" + meaningAmt(data[i].Quotes[data[i].Quotes.length - 3].Volume) + ")";
                            //data[i]["Day 3"] = meaningAmt(data[i].Quotes[data[i].Quotes.length - 4].Value) + " (" + meaningAmt(data[i].Quotes[data[i].Quotes.length - 4].Volume) + ")";
                            //data[i]["Day 4"] = meaningAmt(data[i].Quotes[data[i].Quotes.length - 5].Value) + " (" + meaningAmt(data[i].Quotes[data[i].Quotes.length - 5].Volume) + ")";
                            //data[i]["Day 5"] = meaningAmt(data[i].Quotes[data[i].Quotes.length - 6].Value) + " (" + meaningAmt(data[i].Quotes[data[i].Quotes.length - 6].Volume) + ")";

                            data[i]["Day 1"] = data[i].Quotes[data[i].Quotes.length - 2].Value;
                            data[i]["Day 2"] = data[i].Quotes[data[i].Quotes.length - 3].Value;
                            data[i]["Day 3"] = data[i].Quotes[data[i].Quotes.length - 4].Value;
                            data[i]["Day 4"] = data[i].Quotes[data[i].Quotes.length - 5].Value;
                            data[i]["Day 5"] = data[i].Quotes[data[i].Quotes.length - 6].Value;
                            //data[i]["Spiked"] = 
                        }

                        gridArray.push(data[i]);
                    }
                }
                self.gridData = gridArray;
                console.log(stocks);
            },
            error: function (error) {
                console.log(error);
            }
        });

        function meaningAmt(labelValue) {

            // Nine Zeroes for Billions
            return Math.abs(Number(labelValue)) >= 1.0e+9

                ? (Math.abs(Number(labelValue)) / 1.0e+9).toFixed(2) + "B"
                // Six Zeroes for Millions 
                : Math.abs(Number(labelValue)) >= 1.0e+6

                    ? (Math.abs(Number(labelValue)) / 1.0e+6).toFixed(2) + "M"
                    // Three Zeroes for Thousands
                    : Math.abs(Number(labelValue)) >= 1.0e+3

                        ? (Math.abs(Number(labelValue)) / 1.0e+3).toFixed(2) + "K"

                        : Math.abs(Number(labelValue));

        }
    }
})