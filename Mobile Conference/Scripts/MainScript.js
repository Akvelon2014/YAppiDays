var IsModernIElikeOld;

// disabled caching
$.ajaxSetup({
    cache: false
});

$(document).ready(function () {
    if (IsModernIElikeOld == null) {
        IsModernIElikeOld = false;
    }
    //Twitter block
    !function (d, s, id) { var js, fjs = d.getElementsByTagName(s)[0], p = /^http:/.test(d.location) ? 'http' : 'https'; if (!d.getElementById(id)) { js = d.createElement(s); js.id = id; js.src = p + "://platform.twitter.com/widgets.js"; fjs.parentNode.insertBefore(js, fjs); } }(document, "script", "twitter-wjs");

    afterAjaxUpdating();
    
    //scroll to element if it's need
    var topOfWindowsElement = $(".windowsTopHere");
    if (topOfWindowsElement.length > 0) {
        $(document).scrollTop(topOfWindowsElement.first().offset().top);
    }

    //clear elements val
    $(".clearMe").each(function() {
        $(this).val("");
    });

    //Work with popup dialog for Account
    $("#accountDialog").dialog({ autoOpen: false, draggable: false, modal: true, resizable: false, width: 426, height: 323 });
    $(document).on("change", "#profileCommand", function() {
        var $form = $(this).parent("form");
        var url = $form.attr("action");
        var data = $form.serialize();
        var isSubmit = false;
        //added loader to the popup windows
        $("#accountDialog").html("<div class='loaderIcon closeAccountDialog'><img src='/Content/img/loader.gif'></img></div>");
        $.post(url, data, function (response) {
            if (response.data == "no") {
                //$form.submit();
                isSubmit = true;
            } else {
                var $accountDialog = $("#accountDialog");
                $accountDialog.html(response);
                $accountDialog.dialog("option", "height", "auto");
                $accountDialog.dialog("option", "width", "auto");
                $accountDialog.dialog("option", "position", "{ my: 'center', at: 'center', of: window }");
                afterAjaxUpdating();
            }
        }).fail(function() {
            $form.submit();
        }).success(function() {
            if (isSubmit) {
                $form.submit();
            }
        });
        $("#accountDialog").dialog("open");
    });

    
    //Close popup account dialog
    $(document).on("click", ".closeAccountDialog", function () {
        $("#accountDialog").dialog("close");
    });

    //Ajax sending of form with class "withAjax"
    $(document).on("submit", "form.withAjax", function (event) {
        event.preventDefault();
        var url = $(this).attr("action");
        var data = $(this).serialize();
        var $loadTo = $(this).parents(".ajaxHere");
        var form = $(this);
        $.post(url, data, function (response) {
            if ($loadTo.length > 0) {
                $loadTo.first().html(response);
            } else {
                $("#mainContent").html(response);
            }
        }).success(function () {

            //submit depended on it form if it's needed
            var $addSubmit = form.children(".updateAfterFormSubmit");
            if ($addSubmit.length > 0) {
                $addSubmit.each(function() {
                    var urlChild = $(this).attr("data-url");
                    var classChild = $(this).attr("data-class");
                    if (urlChild && classChild) {
                        $.post(urlChild, null, function(responseChild) {
                            $("." + classChild).html(responseChild);
                        }).success(function () {
                            afterAjaxUpdating();
                        });
                    }
                });
            } else {
                afterAjaxUpdating();
            }
        });
    });

    //Ajax link to div.ajaxHere
    $(document).on("click", "a.withAjax", function(event) {
        event.preventDefault();
        var url = $(this).attr("href");
        var $loadTo = $(this).parents(".ajaxHere");
        if ($loadTo.length > 0) {
            $loadTo = $loadTo.first();
        } else {
            $loadTo = $("#mainContent");
        }
        $loadTo.load(url, null, function() {
            afterAjaxUpdating();
        });
    });


    //Clear textbox by special elements with class 'clearer'
    $(document).on("click", ".clearer", function (event) {
        $("#" + $(this).attr("tag")).val("").change();
        return false;
    });

    //Closing photo shower
    $(document).on("click", ".closePictureShow", function () {
        $(".pictureShow").dialog("close");
    });

    //Ajax pagination
    $(document).on("click", "#mainContent ul.pagination a", function (event) {
        event.preventDefault();
        var url = $(this).attr("href");
        var $paginationContent = $(this).parents(".paginationWrapper").first();
        if ($paginationContent == null) {
            $paginationContent = $("#mainContent");
        }
        $paginationContent.load(url, null, function () {
            afterAjaxUpdating();
            var $anchor = $paginationContent.children("a.anchorHere");
            if ($anchor.length > 0) {
                $(document).scrollTop($anchor.first().offset().top);
            } else {
                window.scrollTo(0, 0);
            }
        });
    });

    //Submit with dropdown
    $(document).on("change", ".updatingDropDown select", function () {

        var $form = $(this).parents("form").first();
        var url = $form.attr("action");
        var data = $form.serialize();
        var updateClass = $form.attr("data-update");
        if (updateClass == null) updateClass = "mainContent";
        $.post(url, data, function (response) {
            $("#" + updateClass).html(response);
            afterAjaxUpdating();
        });
    });

    //External post form
    $(document).on("submit", "form.externalUpdate", function (event) {
        event.preventDefault();
        var url = $(this).attr("action");
        var data = $(this).serialize();
        var $form = $(this);
        $.post(url, data, function (response) {
            $("." + $form.attr("id")).html(response);
            afterAjaxUpdating();
        });
    });

    //Link for submitting
    $(document).on("click", ".submit", function(event) {
        event.preventDefault();
        $(this).parents("form").first().submit();
    });

    //Submit if change (for example, checkbox)
    $(document).on("change", ".submitByChange", function() {
        $(this).parents("form").first().submit();
    });

    //Link for div clear
    $(document).on("click", ".clearBlock", function (event) {
        event.preventDefault();
        $("." + $(this).attr("id")).html("");
    });

    //textbox submit with dalay
    var timeouts = {};
    $(document).on("keyup", ".submitWithDelay", function () {
        var id = $(this).attr("id");
        var $form = $(this).parents("form").first();
        if (timeouts[id] != undefined) {
            clearTimeout(timeouts[id]);
        }
        timeouts[id] = setTimeout(function () {
            delete timeouts[id];
            $form.submit();
        }, 1500);
    });
    $(document).on("change", ".submitWithDelay", function () {
        var id = $(this).attr("id");
        var $form = $(this).parents("form").first();
        if (timeouts[id] != undefined) {
            clearTimeout(timeouts[id]);
        }
        timeouts[id] = setTimeout(function () {
            delete timeouts[id];
            $form.submit();
        }, 1500);
    });

    //set/unset border to div
    $(document).on("change", ".borderSetter", function() {
        var div = $(this).attr("data-border-to");
        if ($(this).get(0).checked) {
            $("#" + div).css("border", "solid 1px #999");
        } else {
            $("#" + div).css("border", "none");
        }
    });

    //auto exec form
    $(".autoexec").each(function () {
        var $form = $(this);
        var idTo = $form.attr("data-to");
        var url = $form.attr("action");
        var data = $form.serialize();
        $.post(url, data, function(response) {
            $("#" + idTo).html(response);
        });
    });

    //experts changing
    $("#experts").each(function() {
        var time = $(this).attr("data-time");
        var url = $(this).attr("data-url");
        var curExpert = 1;
        setInterval(function () {
            
            $("#experts" + curExpert).animate({ opacity: 0 }, {
                duration: 1500,
                complete: function() {
                    $(this).css("display", "none");
                }
            });
            curExpert = 3 - curExpert;
            $.post(url, null, function (response) {
                $("#experts" + curExpert).html(response);
            });
            $("#experts" + curExpert)
                .css("display", "block")
                .animate({ opacity: 1 }, {
                    duration: 1000
                });
        }, time);
    });
});


function afterAjaxUpdating() {
    UpdateCustomElement();
    UpdatePictureBlocks();

    //Utitlity for date select
    $(".calendar").datetimepicker({
        format: "d.m.Y",
        lang: 'ru',
        timepicker: false,
        scrollMonth: false,
        yearStart: 1900,
        todayButton: false
    });

    //Utitlity for date-time select
    $(".dateTime").datetimepicker({
        format: "d.m.Y H:i",
        lang: 'ru',
        timepicker: true,
        scrollMonth: false,
        yearStart: 1900,
        todayButton: false
    });

    //Utility for autocomplete
    var url = $(".autocomplete").attr("tag");
    if (url != undefined) {
        $.getJSON(url, function(data) {
            $(".autocomplete").autocomplete({ source: data, minLength: 2 });
        });
    }

}

//detect IE
function IsIE() {
    var uA = navigator.userAgent;
    if (uA.indexOf('MSIE') >= 0) {
        return true;
    }
    return false;
}

function IsIE11() {
    if (IsModernIElikeOld) {
        return false;
    }
    var uA = navigator.userAgent;
    if (uA.indexOf('Trident/7.') >= 0) {
        return true;
    }
    return false;
}

function IsFrameExists(name) {
    if ($("#" + name).length > 0) {
        return true;
    }
    return false;
}

function CreateFrame(name, debug) {
    //remove frame if it's exists
    if (IsFrameExists(name)) {
        return $("#" + name).get(0);
    }

    // for old versions Internet Explorer it's neccessary create div firstly
    var tmpElem = document.createElement('div');
    tmpElem.innerHTML = '<iframe name="' + name + '" id="' + name + '">';
    var iframe = tmpElem.firstChild;
    if (!debug) {
        iframe.style.display = 'none';
    }
    document.body.appendChild(iframe);
    return iframe;
};

//clear value in the input type='file'
function clearFileInput(id) {
    var $newFile = $("<input type='file' multiple/>");
    var $oldFile = $("#" + id);
    $oldFile.after($newFile);
    $newFile.attr("id", $oldFile.attr("id"));
    $newFile.attr("name", $oldFile.attr("name"));
    $oldFile.remove();
}

//submit photos in browser else IE
function SubmitFile(name, url, files, i, $img, $dataTo, dictionary) {
    var formData = new FormData();
    for (var key in dictionary) {
        var val = dictionary[key];
        formData.append(key, val);
    }

    formData.append(name, files[i]);
    $.ajax({
        type: "POST",
        url: url,
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            if (response != "") {
                $dataTo.html(response);
                afterAjaxUpdating();
            }

            if ((i + 1) < files.length) {
                if (response == "" || $(".loadError").length > 0) {
                    UnknownFormatMessage(name, url, files, i + 1, $img, $dataTo, dictionary);
                } else {
                    $(".pictureJustLoadingProgress").html($img);
                    SubmitFile(name, url, files, i + 1, $img, $dataTo, dictionary);
                }
            } else {
                if (response == "" || $(".loadError").length>0) {
                    $(".pictureJustLoadingProgress").html("");
                    UnknownFormatMessage();
                } else {
                    $(".pictureJustLoadingProgress").html("");
                }
            }
        },
        error: function () {
            if ((i + 1) < files.length) {
                UnknownFormatMessage(name, url, files, i + 1, $img, $dataTo, dictionary);
            } else {
                $(".pictureJustLoadingProgress").html("");
                UnknownFormatMessage();
            }
        }
    });
}

//Submit form with file select in IE
function SubmitIE($form) {
    try {
        $form.submit();
    } catch (e) {
        OpenLoadDialogForIE($form);
    }
}

//dialog for select file in IE older then IE 11
function OpenLoadDialogForIE($form) {
    var $dialog = $("#dialogForIE");
    if ($dialog.length < 1) {
        $dialog = $("<div id='dialogForIE'></div>");
        $dialog.dialog({ autoOpen: false, draggable: false, modal: true, resizable: false, width: 330, height: 240 });
        $dialog.append("<p id='closeSelectFile'><span>X</span></p><p>Выберите изображение и нажмите загрузить.</p>");
        if ($form.children("#submitter").length < 1) {
            $form.append("<br/><br/><br/><input type='submit' id='submitter' value='загрузить' class='orange customButton mediumSizeButton right'/>");
            $("#closeSelectFile").on("click", function () {
                $(".pictureJustLoadingProgress").html($("#loaderImgForPhoto"));
                var $formLoad = $(this).siblings("form");
                $dialog.dialog("close");
                $formLoad.attr("data-pic", $form.attr("data-old-pic"));
                $formLoad.removeAttr("data-old-pic");
            });
        }
        $form.css("display", "block");
        $form.attr("data-old-pic", $form.attr("data-pic"));
        $form.removeAttr("data-pic");
        $form.attr("class", "");
        $dialog.append($form);
    } 
    $dialog.dialog("open");
}

//frame event
function FrameEvent(frame, functionAfterComplete) {
    frame.onreadystatechange = function () {
        if (frame.readyState == "complete") {
            var content = frame.contentWindow.document.body.innerHTML;
            var parse = $(content).html();
            functionAfterComplete(parse);
            var $dialog = $("#dialogForIE");
            if ($dialog.length > 0 && content != "") {
                $dialog.dialog("close");
            }
        }
    };
}