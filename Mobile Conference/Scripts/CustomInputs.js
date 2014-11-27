// Examples for drop down list:
// 1) if you used select in your html, script auto replace it on the custom element
// 2) if you need used any element as dropdown:
//      <select class='external' tag='id_external_element>
//          ......
//      </select>
//      ....
//      <any_element id='id_external_element'>
//
//  this element transform to dropDownList

$(function () {
    UpdateCustomElement();
    ConfigDropDownList();
    ConfigCheckbox();
});

function UpdateCustomElement() {
    $("select").each(function() {
        ReplaceDropDownToCustomDiv($(this));
        $(this).hide();
    });
    $("input[type='checkbox']").each(function() {
        ReplaceCheckboxToCustom($(this));
        $(this).hide();
    });
    $(".dropDownContent ul").each(function () {
        if ($(this).children("li").length>7) {
            $(this).slimScroll({
                height: 'auto'
            });
        }
    });
}

function ReplaceDropDownToCustomDiv(dropDown) {
    if (dropDown.next().hasClass("customDropDownList")) return;
    var external = dropDown.hasClass("external");
    var $customDropDown;

    if (external) {
        $customDropDown = $("#" + dropDown.attr("tag"));
        //if ($customDropDown.length > 0 && $customDropDown.first().hasClass("customDropDownList")) {
        //    return;
        //}
    } else {
        $customDropDown = $("<div><div class='dropDownArrows'><div></div></div></div>");
        $customDropDown.attr("class", dropDown.attr("class"));
        $customDropDown.addClass("customDropDownList");
    }

    //create custom dropDown list
    var $contentDiv = $("<div class='dropDownContent'></div>");
    var $list = $("<ul></ul>");
    $contentDiv.append($list);
    var $label = $("<span class='valueDropDown'></span>");
    var $content = $("<span></span>");
    $label.append($content);

    //use ready element
    if (external) {
        if ($customDropDown.children(".dropDownContent").length==0) {
            $customDropDown.append($contentDiv);
            $customDropDown.addClass("customDropDownList");
            $customDropDown.addClass("external");
        }
    } else
    // create element
    {
        dropDown.after($customDropDown);
        $customDropDown.append($label);
        $label.after($contentDiv);
    }
    $customDropDown.attr("tag", dropDown.attr("id"));

    //fill custom dropDown list
    var dropDownDOM = dropDown.get(0);
    for (var i = 0; i < dropDownDOM.options.length; i++) {
        var $option = $("<li></li>");
        $option.html(dropDownDOM.options[i].innerHTML);
        $option.attr("tag", dropDownDOM.options[i].value);
        if (dropDownDOM.options[i].getAttribute("selected") != null && !external) {
            $content.html(dropDownDOM.options[i].innerHTML);
        }
        $list.append($option);
    }
}

function ReplaceCheckboxToCustom(checkbox) {
    if (checkbox.next().hasClass("customCheckbox")) return;

    //create custom Checkbox
    var $customCheckbox = $("<div></div>");
    $customCheckbox.attr("class", checkbox.attr("class"));
    $customCheckbox.addClass("customCheckbox");
    checkbox.after($customCheckbox);
    var data_pic = checkbox.attr("data-pic");
    if (data_pic) {
        $customCheckbox.attr("data-pic", data_pic);
    }
    //set/unset custom checkbox
    if (checkbox.get(0).checked) {
        $customCheckbox.addClass("active");
    }
}

function ConfigDropDownList() {
    $(document).click(function() {
        HideAllDropDownList();
    });
    $(document).on("click", ".customDropDownList", function (event) {
        var isActive = $(this).hasClass('active');
        HideAllDropDownList();
        if (!isActive) {
            $(this).addClass('active');
        };
        event.stopPropagation();
    });
    $(document).on("click", ".customDropDownList ul li", function () {
        var $parent = $(this).parents(".customDropDownList").first();
        var external = $parent.hasClass("external");

        if (external) {
            $("#" + $parent.attr("tag")).val($(this).attr("tag"));
            $("#" + $parent.attr("tag")).change();
        } else {
            $parent.prev("select").val($(this).attr("tag"));
            var $label = $parent.children("span.valueDropDown").find("span");
            $label.html($(this).html());
            $parent.prev("select").change();
        }
    });
    
    //resolve problems with unusable text selection
    $(document).on("mousedown", ".customDropDownList", function () { return false; });
    $(document).on("selectstart", ".customDropDownList", function () { return false; });
    $(document).on("mousedown", ".customCheckbox", function () { return false; });
    $(document).on("selectstart", ".customCheckbox", function () { return false; });

}

function ConfigCheckbox() {
    $(document).on("click", ".customCheckbox", function() {
        $(this).toggleClass("active");
        $(this).prev("input[type='checkbox']").get(0).checked = $(this).hasClass("active");
        $(this).prev("input[type='checkbox']").change();
    });
    $(document).on("change", "input[type='checkbox']", function() {
        if ($(this).get(0).checked) {
            $(this).next(".customCheckbox").addClass("active");
        } else {
            $(this).next(".customCheckbox").removeClass("active");
        }
    });
}

function HideAllDropDownList() {
    $(".customDropDownList").removeClass("active");
}