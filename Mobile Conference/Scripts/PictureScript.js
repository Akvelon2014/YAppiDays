var curFiles;
$(function () {
    UpdatePictureBlocks();

    if (IsIE()) {
        $(":file").removeAttr("multiple");
    }

    //invoke file dialog
    $(document).on("click", "a.imageLoaderLink[data-pic]", function () {
        var dataPic = $(this).attr("data-pic");
        var $form = $("form[data-pic='" + dataPic + "']");
        if (IsIE() && !IsIE11()) {
            var frame = CreateFrame("myFrame", false);
            $form.attr("target", "myFrame");
            OpenLoadDialogForIE($form);
            $("#submitter").on("click", function() {
                FrameEvent(frame, function (parse) {
                    ClearImageArea();
                    SetResizerByDataPic(parse, dataPic);
                });
                return true;
            });
            return;
        };
        var $loader = $form.children(":file").first();
        $loader.click();
    });

    //utility for simple load
    $(document).on("click", ".pictureJustLoadingLink", function () {
        var $form = $(this).parents("form").first();
        if (IsIE() && !IsIE11()) {
            $form = $form.clone();
            $form.children("a").remove();
            $form.children(":file").remove();
            $form.append("<input type='file' name='picture'/>");
            var frame = CreateFrame("myFrame", false);
            $form.attr("target", "myFrame");
            OpenLoadDialogForIE($form);
            var $ajax = $("." + $form.attr("data-to"));
            $("#submitter").on("click", function () {
                FrameEvent(frame, function (parse) {
                    $ajax.html(parse);
                    afterAjaxUpdating();
                    $(".pictureJustLoadingProgress").html("");
                });
                return true;
            });
            return;
        };
        var $loader = $form.children(":file").first();
        $loader.click();
    });

    //submit picture to server (like avatar for all things)
    $(document).on("change", "form[data-pic] :file", function () {
        var $form = $(this).parents("form[data-pic]");
        var dataPic = $form.attr("data-pic");

        var $img = $(".pictureLoaderDiv").first();
        $img.css("background", " url(../Content/img/loader.gif)");
        $img.css("background-position", " 50% 50%");
        $img.height("150px");
        $img.width("150px");
        $(".pictureLoaderDiv img").css("display", "none");

        //for IE only
        if (IsIE()) {
            var frame = CreateFrame("myFrame", false);
            $form.attr("target", "myFrame");
            if (IsIE11()) {
                SubmitIE($form);
            } else {
                OpenLoadDialogForIE($form);
            }
            FrameEvent(frame, function (parse) {
                ClearImageArea();
                SetResizerByDataPic(parse, dataPic);
                $(".pictureLoaderDiv img").css("display", "inline-block");
                var $pictureLoaderDiv = $(".pictureLoaderDiv");
                $pictureLoaderDiv.css("background", "none");
                $pictureLoaderDiv.css("height", "auto");
                $pictureLoaderDiv.css("width", "auto");
            });
            return;
        }


        //for other browsers
        $.ajax({
            url: $form.attr("action"),
            type: 'POST',
            data: new FormData($form[0]),
            async: true,
            cache: false,
            mimeType: "multipart/form-data",
            contentType: false,
            processData: false,
            success: function (returndata) {
                $img.height("auto");
                $img.width("auto");
                var jqeryReturnData = $(returndata);
                var parse = jqeryReturnData.html();
                ClearImageArea();
                SetResizerByDataPic(parse, dataPic);
                $(".pictureLoaderDiv img").css("display", "inline-block");
                $(".pictureLoaderDiv").css("background", "none");
            },
            error: function () {
                UnknownFormatMessage();
            }
        });
    });

    //Utitlity for ajax send of photos
    $(document).on("change", 'input[type="file"].pictureJustLoading', function () {
        var $form = $(this).parents("form");
        var $ajax = $("." + $form.attr("data-to"));
        var url = $form.attr("action");
        //animation
        var $img = $("<div class='loader' id='loaderImgForPhoto'></div>");
        $img.css("background", " url(../Content/img/loader.gif)");
        $img.css("background-position", " 50% 50%");
        $img.height(150);
        $img.width(150);
        $(".pictureJustLoadingProgress").html($img);

        //for IE only
        if (IsIE()) {
            var frame = CreateFrame("myFrame", false);
            $form.attr("target", "myFrame");
            if (IsIE11()) {
                SubmitIE($form);
            } else {
                $form = $form.clone();
                $form.children("a").remove();
                $form.children(":file").remove();
                $form.append("<input type='file' name='picture'/>");
                OpenLoadDialogForIE($form);
            }
            FrameEvent(frame, function (parse) {
                $ajax.html(parse);
                afterAjaxUpdating();
                $(".pictureJustLoadingProgress").html("");
            });
            return;
        }

        //for other browsers
        var files = $(this)[0].files;
        var tb = $(this).attr("id");

        //save hidden elements in the dictionary
        var hiddenElement = $form.children(":hidden");
        var dictionary = {};
        if (hiddenElement.length > 0) {
            hiddenElement.each(function () {
                var name = $(this).attr("name");
                var value = $(this).val();
                dictionary[name] = value;
            });
        }
        clearFileInput(tb);
        SubmitFile($(this).attr("name"), url, files, 0, $img, $ajax, dictionary);
    });

    $(".justLoader").each(function() {
        var $img = $(this).children("img").first();
        SetResizerByDataPic($img.attr("src"), $(this).attr("data-pic"));
    });
    
    //Rotate image
    $(document).on("click", ".rotateLink", function(event) {
        event.preventDefault();
        var $this = $(this);
        var dataPic = $this.attr("data-pic");
        var url = $this.attr("data-url");
        var $image = $("img[data-pic='" + dataPic + "']");
        var src = $image.attr("src");
        $.post(url + "?picture=" + src, null, function(response) {
            var jqeryReturnData = $(response);
            var parse = jqeryReturnData.html();
            $image.attr("src", parse);
            $("input#" + dataPic).val(parse);
            //ClearImageArea();
            SetResizerByDataPic(parse, dataPic);
            //var $div = $("div.pictureLoaderDiv[data-pic='" + dataPic + "']");
            //var $img = $div.children("img").first();
            //var ias = $img.imgAreaSelect({ instance: true });
            //ias.cancelSelection();
            //ias.update();
        });
    });
});



function UpdatePictureBlocks() {
    //popup for the photo
    $(".photoGroup div").magnificPopup({
        delegate: 'a',
        type: 'image',
        gallery: {
            enabled: true
        },
        zoom: {
            enabled: true
        },
        retina: {
            ratio: 1
        },
        image: {
            verticalFit: true,
            tError: 'Фотография не найдена. Возможно она была удалена'
        }
    });

    $(".photoGroup.single div").magnificPopup({
        delegate: 'a',
        type: 'image',
        gallery: {
            enabled: false
        },
        zoom: {
            enabled: true
        },
        retina: {
            ratio: 1
        },
        image: {
            verticalFit: true,
            tError: 'Фотография не найдена. Возможно она была удалена'
        }
    });

    $("a.imageLoaderLink[data-pic]").each(function () {
        var dataPic = $(this).attr("data-pic");
        if ($("form[data-pic='" + dataPic + "'").length == 0) {
            var $form = $("<form method='post' encType = 'multipart/form-data' data-pic='" + dataPic + "'></form>");
            var $loader = $("<input type='file' name='picture'/>");
            $form.append($loader);
            $form.append("<input type='hidden' name='isForUser' value='" + $(this).attr("data-tag") + "'>");
            $form.attr("action", $(this).attr("data-action"));
            var $picture = $("img[data-pic='" + dataPic + "']");
            var altPicture = $picture.attr("src");
            if (altPicture != null) {
                $form.append("<input type='hidden' name='altImage' value='" + altPicture + "'>");
            }
            $form.css("display", "none");
            $("body").append($form);
        }
    });
   
};

function SetResizerByDataPic(parse, dataPic) {
    var $div = $("div.pictureLoaderDiv[data-pic='" + dataPic + "']");
    var $img = $div.children("img").first();
    var $name = $div.children(":hidden[name=" + dataPic + "]");

    if ($name.length == 0) {
        $name = $("<input type='hidden' name='" + dataPic + "' value='" + parse + "'>");
        $div.append($name);
    } else {
        $name = $name.first();
        $name.attr("value", parse);
    }
    var ias;
    var $x1 = $('input.size_' + dataPic + "[name='x1']");
    var $x2 = $('input.size_' + dataPic + "[name='x2']");
    var $y1 = $('input.size_' + dataPic + "[name='y1']");
    var $y2 = $('input.size_' + dataPic + "[name='y2']");
    if ($(".imgareaselect-selection").length < 1) {
        $img.imgAreaSelect({
            aspectRatio: GetRatioForClass($div),
            handles: true,
            onSelectEnd: function(img, selection) {
                $x1.val(selection.x1);
                $x2.val(selection.x2);
                $y1.val(selection.y1);
                $y2.val(selection.y2);
                $(".previewSize[data-from='" + dataPic + "']").html(selection.width + "x" + selection.height);
                $(".previewCoordinates[data-from='" + dataPic + "']").html(selection.x1 + "x" + selection.y1);
            },
            onSelectChange: Preview
        });
    } else {
        ias = $img.imgAreaSelect({ instance: true });
        ias.cancelSelection();
        ias.setOptions({
            aspectRatio: GetRatioForClass($div),
            handles: true,
            onSelectEnd: function(img, selection) {
                $x1.val(selection.x1);
                $x2.val(selection.x2);
                $y1.val(selection.y1);
                $y2.val(selection.y2);
                $(".previewSize[data-from='" + dataPic + "']").html(selection.width + "x" + selection.height);
                $(".previewCoordinates[data-from='" + dataPic + "']").html(selection.x1 + "x" + selection.y1);
            },
            onSelectChange: Preview
        });
        ias.update();
    }
    //resolve resizer display
    var $tools = $(".imageTools");
    $( ".imgareaselect-outer").on("mouseenter", function () {
        $tools.css("display", "inline-block");
    });
    $(".imageLabel[data-pic='" + dataPic + "']").show();
    UpdateCustomElement();
    $("a.customButton.imageLabel[data-pic='" + dataPic + "']").css("display", "inline-block");
    $(".customCheckbox.imageLabel[data-pic='" + dataPic + "']").css("display", "inline-block");
    $img.attr("src", parse);
    var preview = $(".imagePreview[data-pic='preview_" + dataPic + "']").children("img");
    preview.attr("src", parse);
    preview.css("display", "inline-block");
    $name.attr("value", parse);
}

//helper function for get ratio for the different images
function GetRatioForClass($div) {
    if ($div.hasClass("ideaRatio")) return "22:13";
    if ($div.hasClass("userRatio")) return "11:15";
    if ($div.hasClass("awardRatio")) return "1:1";
    return "22:13";
}

//helper function for get preview
function Preview(img, selection) {
    var dataPic = img.getAttribute("data-pic");
    $(".imagePreview[data-pic='preview_" + dataPic + "']").each(function () {
        var scaleX = $(this).width() / (selection.width || 1);
        var scaleY = $(this).height() / (selection.height || 1);
        $(this).children("img").css({
            width: Math.round(scaleX * img.width) + 'px',
            height: Math.round(scaleY * img.height) + 'px',
            marginLeft: '-' + Math.round(scaleX * selection.x1) + 'px',
            marginTop: '-' + Math.round(scaleY * selection.y1) + 'px'
        });
    });
}

function ClearImageArea() {
    $(".imageLabel").css("display", "none");
    $("form[data-pic] :file").val("");
    $("div.imgareaselect-outer").css("display", "none");
    $("div.imgareaselect-selection").parent("div").css("display", "none");
}

//Popup for error image format
function UnknownFormatMessage(name, url, files, i, $img, $dataTo, dictionary) {
    var $dialog = $("#dialogForFormat");
    curFiles = files;
    if ($dialog.length < 1) {
        $dialog = $("<div id='dialogForFormat'></div>");
        $dialog.dialog({ autoOpen: false, draggable: false, modal: true, resizable: false, width: 370, height: 240 });
        $dialog.append("<p id='closeFormatDialog'><span>X</span></p><p>Не удалось загрузить фотографию. Проверьте параметры изображения: <br/>поддерживаемые форматы: jpg, gif, png <br/>размер должен быть не более 6 Мб</p>");
        $("#closeFormatDialog").on("click", function () {
            $('.cancelLoad').click();
            $dialog.dialog("close");
        });

        $(document).on("click", "#cancel", function () {
            $('.cancelLoad').click();
            $dialog.dialog("close");
        });

        $(document).on("click", "#cancelContinue", function () {
            SubmitFile(name, url, curFiles, parseInt($(this).attr("data-i")), $img, $dataTo, dictionary);
            $dialog.dialog("close");
        });

        $dialog.append("<br><a class='closeFormatDialog customButton grey mediumSizeButton'>ОК</a>");
    }
    if (name == null) {
        $("a.closeFormatDialog").attr("id", "cancel");
        $("#cancel").html("ОК");
    } else {
        $("a.closeFormatDialog").attr("id", "cancelContinue");
        $("#cancelContinue").html("Продолжить");
        $("#cancelContinue").attr("data-i", i);
    }
    $dialog.dialog("open");
}