$(function () {
    "use strict";

    $(".header_main__head").text(resources.addDevice);
    $(".header_main__subhead").text(resources.stepOneHeader);
    $(".header_main__button_back").show();
    $(".header_main__button_back").off("click").click(function () {
        location.href = resources.redirectToIndexUrl;
    });
});