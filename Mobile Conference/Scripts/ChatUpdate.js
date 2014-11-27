$(document).ready(function () {
    var urlToUpdate = $("#chatUpdate").val();
    var curDate = null;
    setInterval(function () {
        $.getJSON(urlToUpdate, function (data) {
            if (curDate == null) {
                curDate = data;
            }
            if (curDate != data) {
                var urlToLoad = $("#chatTo").val();
                curDate = data;
                $.ajax({
                    url: urlToLoad,
                    type: 'GET',
                    success: function (returndata) {
                        $("#chatBlock").html(returndata);
                        afterAjaxUpdating();
                    }
                });
            }
        });
    }, 3000);

    $(document).on("click", "#chatForm :submit", function() {
        var $updateAt = $(this).parents(".ajaxHere").first();
        var $form = $(this).parents("form").first();
        var $photoLink = $("#photoLink");
        if ($photoLink.length > 0) {
            var href = $photoLink.first().attr("href");
            var $picture = $("<input type='hidden' name='picture' value='" + href + "'/>");
            $form.append($picture);
        }
        $.post($form.attr("action"), $form.serialize, function(response) {
            $updateAt.html(response);
        });
    });
});