$(document).ready(function() {
    var idea = $("#ideaId").attr("value");
    var pathToAction = $(".openPictureDialog").attr("tag");

    $(document).on("click", ".openPictureDialog", function () {
        if (!$("#pictureDialog").length) {
            $("body").append("<div id='pictureDialog'><div id='content'></div></div>");
        } else {
            $("#pictureDialog").css("display", "block");
        }
        $("#pictureDialog #content").html("<img src='/Content/img/loader.gif' class='loaderIcon closePictureDialog'></img>");
        $("#pictureDialog #content").load(pathToAction, { ideaId: idea }, function() {
            
        });
    });
    $(document).on("click", ".closePictureDialog", function() {
        ClosePictureDialog();
    });

    //Ajax pagination
    $(document).on("click", "#pictureDialog ul.pagination a", function (event) {
        event.preventDefault();
        var url = $(this).attr("href");
        $("#pictureDialog #content").load(url, null);
    });

    //if user selected the picture this picture added to the form
    $(document).on("click", "#pictureDialog .photo", function () {
        var fileName = $(this).attr("tag");
        $("#wrapperDataForImage").html("<input type='hidden' name='store' value='"+fileName+"'/>").append($(this));
        ClosePictureDialog();
    });

    //Utitlity for ajax send of photos
    $(document).on("change", 'input[type="file"]#loadedPhoto', function () {
        var $form = $(this).parents("form");
        $.ajax({
            url: $form.attr("action"),
            type: 'POST',
            data: new FormData($form[0]),
            async: false,
            cache: false,
            mimeType: "multipart/form-data",
            contentType: false,
            processData: false,
            success: function (returndata) {
                if(returndata!=null) $("#wrapperDataForImage").html(returndata);
                ClosePictureDialog();
            }
        });
    });

});

function ClosePictureDialog() {
    $("#pictureDialog").css("display", "none");
}