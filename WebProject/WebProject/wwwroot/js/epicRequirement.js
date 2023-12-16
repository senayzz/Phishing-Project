$(".textbox input").focusout(function(){
    if($(this).val() == ""){
        $(this).siblings().removeClass("hidden");
        $(this).css("background","#554343");
    }else{
        $(this).siblings().addClass("hidden");
        $(this).css("background","#484848");
    }
});
 



