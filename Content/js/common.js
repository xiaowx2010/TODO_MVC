//JS关闭jquery dialog
function closeDlg(id) {
	$("#" + id).dialog('close');
}

//显示仅能输入数字 
function onlyNum(obj) {
	$(obj).val($.trim($(obj).val().replace(/\D/gi, "")));
}

var enter_goto = function(element, href){
    element.keydown(function(event) {
         if (event.which == 13) {
            event.preventDefault();
            window.location.href=href+$.trim(element.val());
        }
    });
};

var funPlaceholder = function(element) {
    var placeholder = '';
    if (element && !("placeholder" in document.createElement("input")) && (placeholder = element.getAttribute("placeholder"))) {
        element.onfocus = function() {
            if (this.value === placeholder) {
                this.value = "";
            }
            this.style.color = '';
        };
        element.onblur = function() {
            if (this.value === "") {
                this.value = placeholder;
                this.style.color = 'graytext';    
            }
        };
        
        //样式初始化

        if (element.value === "") {
            element.value = placeholder;
            element.style.color = 'graytext';    
        }
    }
};
