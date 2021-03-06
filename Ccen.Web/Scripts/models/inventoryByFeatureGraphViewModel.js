var InventoryByFeatureGraphViewModel = function (settings) {
    var self = this;

    self.chart = null;
    self.chartData = null;

    self.settings = settings;

    self.selectedType = ko.observable(0);

    self.selectedFeatureId = ko.observable(null);
    self.selectedFeatureName = ko.observable(null);

    self.types = [
        {
            label: 'units',
            value: 0,
        },
        {
            label: 'styles',
            value: 1,
        }
    ];

    $.each(self.types, function (i, n) {
        n.isSelected = ko.computed(function () {
            return self.selectedType() == n.value;
        });
        n.select = function () {
            self.selectedType(n.value);
            self.showChartData();
        }
    });

    self.onBack = function() {
        self.selectedFeatureId(null);
        self.selectedFeatureName(null);
        self.requestData();
    }

    self.prepareNAName = function(name) {
        if (name == "n/a")
        {
            if (self.selectedFeatureName() != null) {
                return "Other " + self.selectedFeatureName();
            }
        }
        return name;
    }

    self.requestData = function () {
        while (self.chart.series.length > 0)
            self.chart.series[0].remove(true);

        self.chart.showLoading();

        $.ajax({
            cache: false,
            url: self.settings.getDataUrl,
            data: {
                valueType: self.selectedType(),
                featureId: self.settings.featureId,
                selectedFeatureId: self.selectedFeatureId(),
            },
            success: function (result) {
                self.chart.hideLoading();

                if (result.IsSuccess) {
                    console.log('setCategories');
                    self.chart.xAxis[0].setCategories(result.Data.Labels, true);
                    for (var i = 0; i < result.Data.Labels.length; i++) {
                        result.Data.UnitSeries[0][i] = {
                            y: result.Data.UnitSeries[0][i],
                            name: self.prepareNAName(result.Data.Labels[i].Name),
                            id: result.Data.Labels[i].Id
                        },
                        result.Data.StyleSeries[0][i] = {
                            y: result.Data.StyleSeries[0][i],
                            name: self.prepareNAName(result.Data.Labels[i].Name),
                            id: result.Data.Labels[i].Id
                        }
                    }
                    self.chartData = result.Data;
                    self.showChartData();
                }
            }
        });
    };

    self.showChartData = function () {
        while (self.chart.series.length > 0)
            self.chart.series[0].remove(true);
        self.chart.yAxis[0].setExtremes(null, null);

        if (self.chartData == null)
            return;

        if (self.selectedType() == 0) {
            self.chart.yAxis[0].setTitle({
                text: 'units'
            });
            self.chart.addSeries({
                name: self.settings.seriesName,
                innerSize: '50%',
                data: self.chartData.UnitSeries[0],
                tooltip: {
                    pointFormat: '<b>{point.y}</b> ({point.percentage:.1f}%)'
                },
                dataLabels: {
                    format: '<span style="font-weight: normal">{point.name}</span><br/>{point.y:,.0f} ({point.percentage:.1f}%)'
                },
                point: {
                    events: {
                        click: function (e) {
                            console.log('click');
                            console.log(e);
                            console.log(this.id);
                            console.log(e.point.name);

                            if (self.settings.enableClick) {
                                if (self.selectedFeatureId() == null && this.id != null) {
                                    self.selectedFeatureId(this.id);
                                    self.selectedFeatureName(e.point.name);
                                    self.requestData();
                                }
                            }
                        }
                    }
                },
            });
        }
        if (self.selectedType() == 1) {
            self.chart.yAxis[0].setTitle({
                text: 'styles'
            });
            self.chart.addSeries({
                name: self.settings.seriesName,
                innerSize: '50%',
                data: self.chartData.StyleSeries[0],
                tooltip: {
                    pointFormat: '<b>{point.y}</b> ({point.percentage:.1f}%)'
                },
                dataLabels: {
                    format: '<span style="font-weight: normal">{point.name}</span><br/>{point.y:,.0f} ({point.percentage:.1f}%)'
                },
                point: {
                    events: {
                        click: function(e) {
                            console.log('click');
                            console.log(e);
                            console.log(this.id);
                            console.log(e.point.name);

                            if (self.settings.enableClick) {
                                if (self.selectedFeatureId() == null && this.id != null) {
                                    self.selectedFeatureId(this.id);
                                    self.selectedFeatureName(e.point.name);
                                    self.requestData();
                                }
                            }
                        }
                    }
                },
            });
        }
    }

    self.init = function () {
        self.chart = new Highcharts.Chart({
            chart: {
                renderTo: self.settings.graphId,
                plotBackgroundColor: null,
                plotBorderWidth: null,
                plotShadow: false,
                type: 'pie',
                events: {
                    load: function () {
                        self.chart = this;
                        self.requestData();
                    }
                },
                //spacing: [0,0,0,0]
            },
            credits: {
                enabled: false
            },
            title: {
                text: ""
            },
            tooltip: {
                pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>'
            },
            //legend: {
            //    layout: 'horizontal',
            //    align: 'center',
            //    verticalAlign: 'bottom',
            //    borderWidth: 0
            //},
            plotOptions: {
                pie: {
                    size: '55%',
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: true,
                        format: '<b>{point.name}</b>: {point.percentage:.1f} %',
                        style: {
                            color: (Highcharts.theme && Highcharts.theme.contrastTextColor) || 'black'
                        }
                    },
                }
            },
        });
    }

    self.init();
}