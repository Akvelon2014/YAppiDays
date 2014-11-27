//style for before and after
var styleForTips;

$(function () {
    var $tip = $("#tip1");
    var $tipNews = $("#tip0");
    
    if ($tip.length > 0 || $tipNews.length > 0) {
        styleForTips = $("<style/>");
        $("head").append(styleForTips);
        setTimeout(function () {
            if ($tip.length > 0) {
                ShowTip(1);
            } else {
                ShowTip(0);
            }
        }, 500);
    } 

    //close tip
    $(document).on("click", ".tip", function() {
        $(this).animate({ opacity: 0 }, { duration: 1000, complete: function() {
            $(this).css("display", "none");
            var id = $(this).attr("id");
            if (id != "tip0") {
                ShowTip(parseInt(id.substr(3)) + 1);
            }
        } });
    });
});

function ShowTip(tipNumber) {
    $("#tip" + tipNumber).each(function () {
        var $tip = $(this);
        var width = $tip.attr("data-width");
        if (width == undefined) {
            width = 150;
        }
        var offset = $tip.attr("data-offset");
        if (offset == undefined) {
            offset = 15;
        }
        var offsetX = $tip.attr("data-offset-x");
        if (offsetX == undefined) {
            offsetX = 0;
        }
        var offsetY = $tip.attr("data-offset-y");
        if (offsetY == undefined) {
            offsetY = 0;
        }
        var element = $("#" + $tip.attr("data-for"));
        var position = element.first().offset();
        var id = $tip.attr("id");

        $tip.css("width", width);
        $tip.css("display", "block");
        //top triangle
        if ($tip.hasClass("topTriangle")) {
            $tip.css("left", position.left + parseInt(offsetX)+ "px");
            $tip.css("top", (position.top + element.outerHeight()+ parseInt(offsetY)) + "px");

            styleForTips.append("#" + id + ":after{left:" + (parseInt(offset) + 2) + "px;}");
            styleForTips.append("#" + id + ":before{left:" + offset + "px;}");
        }

        //bottom triangle
        if ($tip.hasClass("bottomTriangle")) {
            $tip.css("left", position.left + parseInt(offsetX) + "px");
            $tip.css("top", (position.top - $tip.outerHeight() + parseInt(offsetY)) + "px");

            styleForTips.append("#" + id + ":after{left:" + (parseInt(offset) + 2) + "px;}");
            styleForTips.append("#" + id + ":before{left:" + offset + "px;}");
        }

        //left triangle
        if ($tip.hasClass("leftTriangle")) {
            $tip.css("left", position.left +parseInt(element.outerWidth()) + parseInt(offsetX) + "px");
            $tip.css("top", (position.top + parseInt(offsetY)) + "px");

            styleForTips.append("#" + id + ":after{top:" + (parseInt(offset)+2) + "px;}");
            styleForTips.append("#" + id + ":before{top:" + (parseInt(offset)) + "px;}");
        }

        //right triangle
        if ($tip.hasClass("rightTriangle")) {
            $tip.css("left", position.left - parseInt($tip.outerWidth()) + parseInt(offsetX) + "px");
            $tip.css("top", (position.top + parseInt(offsetY)) + "px");

            styleForTips.append("#" + id + ":after{top:" + (parseInt(offset) + 2) + "px;}");
            styleForTips.append("#" + id + ":before{top:" + (parseInt(offset)) + "px;}");
        }

        //scroll screen to the top px
        var top;
        if (($tip.offset().top+$tip.outerHeight()) > (position.top+element.outerHeight())) {
            top = $tip.offset().top + $tip.outerHeight() - $("body").height() + 10;
        } else {
            top = element.offset().top + element.outerHeight() - $("body").height() + 10;
        }

        //show tip
        setTimeout(function () {
            $tip.delay(500).animate({ opacity: 1 }, {
                duration: 1000
            });
            if ($tip.attr("id") != "tip0") {
                var body = $("html, body");
                body.animate({ scrollTop: top }, 1000, 'swing');
            }
        }, 500);
        
    });
}