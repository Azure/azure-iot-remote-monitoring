IoTApp.createModule(
    'IoTApp.Dashboard.JobIndicators',
    function initJobIndicators() {
        'use strict';

        var init = function init(jobIndicatorsSettings) {
            loadJobDefinitions();
        };

        var loadJobDefinitions = function () {
            $.ajax({
                url: '/api/v1/jobIndicators/definitions',
                type: 'GET',
                cache: false,
                success: function (result) {
                    var $container = $('.dashboard_job_indicators');
                    var $div;

                    $.each(result, function (index, indicator) {
                        if (index == 0) {
                            $div = $('<div>', {
                                'class': 'dashboard_job_indicator_container dashboard_job_indicator_container--left'
                            });
                        } else if (index == result.length - 1) {
                            $div = $('<div>', {
                                'class': 'dashboard_job_indicator_container dashboard_job_indicator_container--right'
                            });
                        } else {
                            $div = $('<div>', {
                                'class': 'dashboard_job_indicator_container dashboard_job_indicator_container--center'
                            });
                        }

                        $('<p>', {
                            'class': 'dashboard_job_indicator_title',
                            text: indicator.title
                        }).appendTo($div);

                        $('<p>', {
                            'data-indicator': indicator.id,
                            'class': 'dashboard_job_indicator_value',
                            text: '0'
                        }).appendTo($div);

                        $container.append($div);
                    });

                    updateJobIndicatorsData();
                },
                error: function () {
                    setTimeout(loadJobDefinitions, 60000)
                }
            });
        };

        var updateJobIndicatorsData = function updateJobIndicatorsData() {
            var indicators = $(".dashboard_job_indicator_value").map(function () {
                return $(this).attr('data-indicator');
            }).get();

            $.ajax({
                url: '/api/v1/jobIndicators/values',
                type: 'GET',
                data: { 'indicators': indicators },
                dataType: 'json',
                cache: false,
                success: function (result) {
                    if (result.length == $(".dashboard_job_indicator_value").length) {
                        $(".dashboard_job_indicator_value").each(function (index) {
                            $(this).text(result[index]);
                        });
                    }
                    setTimeout(updateJobIndicatorsData, 600000);
                },
                error: function () {
                    setTimeout(updateJobIndicatorsData, 600000);
                }
            });
        };

        return {
            init: init
        };
    },
    [jQuery, resources]);