$(function () {
    "use strict";

    $(".header_main_head").text(resources.addDevice);
    $(".header_main_subhead").text(resources.stepOneHeader);
    $(".button_back").show();
    $(".button_back").off("click").click(function () {
        location.href = resources.redirectToIndexUrl;
    });
});