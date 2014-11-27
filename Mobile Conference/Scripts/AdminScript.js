$(document).ready(function () {
    //change state in Material Create
    var checker = $("#externalChecker");
    if (checker.length > 0) {
        changeExternalInternal(checker.first().get(0));
    }

    $(document).on("click", ".dialogLink", function(event) {
        event.preventDefault();
        var link = $(this).attr("href");
        if (!$(".dialogUser").length) {
            $("body").append("<div class='dialogUser'></div>");
            $(".dialogUser").dialog({ autoOpen: false, draggable: false, modal: true, resizable: false, width: 400, height: 350 });
            $(".dialogUser").dialog("option", "position", "{ my: 'center', at: 'center', of: window }");
        }
        var $dialog = $(".dialogUser");
        $dialog.dialog("open");
        $dialog.html("<div class='loaderIcon closeAccountDialog'><img src='/Content/img/loader.gif'></img></div>");
        $dialog.attr("id", $(this).parents("div.borderBlock").attr("data-name"));
        $dialog.load(link, null, function () {
            afterAjaxUpdating();
            $dialog.dialog("option", "height", "auto");
            $dialog.dialog("option", "width", "auto");
            $dialog.dialog("option", "position", "{ my: 'center', at: 'center', of: window }");
        });
    });

    $(document).on("click", ".closeDialogLink", function() {
        $(".dialogUser").dialog("close");
    });

    $(document).on("submit", ".dialogUser form", function(event) {
        event.preventDefault();
        submitAdminForm($(this));
    });

    $(document).on("change", ".dialogUser select", function() {
        var $form = $(this).parents("form").first();
        submitAdminForm($form);
    });

    //Switch between external and internal types of materials
    $(document).on("change", "#externalChecker", function () {
        changeExternalInternal($(this).get(0));
    });

    //add link to text area
    $(document).on("click", "#addLink", function() {
        showAddLinkWindows();
    });

    GetPreview();
    $(document).on("keyup", ".materialArea", function () {
        GetPreview();
    });

    //Link to Today
    $(document).on("click", ".linkToToday", function() {
        var id = $(this).attr("tag");
        var today = new Date();
        var day = today.getDate();
        var month = today.getMonth() + 1;
        var year = today.getFullYear();
        $("#" + id).val(((day < 10) ? "0" : "") + day + "." + ((month < 10) ? "0" : "") + month + "." + year).change();
    });
});

function submitAdminForm(form) {
    var url = form.attr("action");
    var data = form.serialize();
    $.post(url, data, function (response) {
        var login = form.parents(".dialogUser").attr("id");
        $("div[data-name=" + login + "]").html(response);
        $(".dialogUser").dialog("close");
    });
}

function changeExternalInternal(checker) {
    $(".internalDiv input").prop('disabled', checker.checked);
    $(".externalDiv textarea").prop('disabled', !checker.checked);
    $(".externalDiv input").prop('disabled', !checker.checked);
    if (checker.checked) {
        $(".internalDiv").css("display", "none");
        $("tr.externalDiv").css("display", "table-row");
        $("a.externalDiv").css("display", "inline-block");
        $("div.externalDiv").css("display", "block");

    } else {
        $("tr.internalDiv").css("display", "table-row");
        $("a.internalDiv").css("display", "inline-block");
        $("div.internalDiv").css("display", "block");
        $(".externalDiv").css("display", "none");
    }
}

function addTextToTextArea(textArea, text) {
    var position = textArea[0].selectionStart;
    var textFromArea = textArea.val();
    if (position != undefined)
        textArea.val(textFromArea.slice(0, position) + text + textFromArea.slice(position));
    else {
        textArea.trigger('focus');
        var range = document.selection.createRange();
        range.text = text;
    }
}

function GetLinkForDialog(dialog) {
    var link = dialog.children("#linkText").first().val();
    var caption = dialog.children("#linkCaption").first().val();
    if (link.indexOf("://") < 0) {
        link = "http://" + link;
    }
    var isBlank = dialog.children("#isBlank").get(0).checked;
    return "<a href='" + link + "' " + (isBlank ? "target='_blank'":"")+">"+caption+"</a>";
}

function showAddLinkWindows() {
    var $dialog = $("#linkDialog");
    if ($dialog.length < 1) {
        $dialog = $("<div id='linkDialog'></div>");
        $("body").append($dialog);
        $dialog.append("<p id='closeAddLink'><span>X</span></p><p>Введите ссылку и нажмите вставить.</p>");
        $dialog.append("<p>Адрес</p>");
        $dialog.append("<input type='text' id='linkText'/><br/>");
        $dialog.append("<p>Подпись</p>");
        $dialog.append("<input type='text' id='linkCaption'/><br/>");
        $dialog.append("<input type='checkbox' id='isBlank'/><label for='isBlank'>Откывать в новом окне</label>");
        $dialog.append("<br/><input type='submit' value='Вставить' id='addLinkButton' class='customButton mediumSizeButton orange'/>");
        $dialog.dialog({ autoOpen: false, draggable: false, modal: true, resizable: false, width: 400, height: 330 });

        $(document).on("click", "#closeAddLink span", function() {
            $dialog.dialog("close");
        });
        $(document).on("click", "#linkDialog #addLinkButton", function () {
            addTextToTextArea($("textarea"), GetLinkForDialog($("#linkDialog")));
            GetPreview();
            $dialog.dialog("close");
        });
        afterAjaxUpdating();
    }
    $dialog.children(":text").val("");
    $dialog.dialog("open");
}

function GetPreview() {
    $("#materialPreview").html($(".materialArea").val());
}