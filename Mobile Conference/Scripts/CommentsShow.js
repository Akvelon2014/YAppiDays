$(document).ready(function() {
    $(document).on("click", ".showComments", function (event) {
        event.preventDefault();
        var $comment = $(this).parents(".commentWrapper").first();
        if ($comment.parent().children(".loader").length == 0) {
            var $img = $("<div class='loader miniLoader grid_8'></div>");
            $comment.after($img);
        }
        var $loader = $comment.parent().children(".loader");
        if ($(this).html() != "скрыть") {
            $loader.css("display", "block");
        }
        $.post($(this).attr("href"), null, function(response) {
            $comment.html(response);
            $loader.css("display", "none");
        });
    });

});