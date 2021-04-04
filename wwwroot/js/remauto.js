(function remauto(window, document) {
    var docEl = document.documentElement;
    var dpr = window.devicePixelRatio || 1;
    function setRemUnit() {
        var width=docEl.clientWidth;
        if(width>docEl.clientHeight){
            width=docEl.clientHeight;
        }
        var rem = width / 25;
        docEl.style.fontSize = rem + 'px';
    }
    setRemUnit();
    // reset rem unit on page resize
    window.addEventListener('resize', setRemUnit);
    window.addEventListener('pageshow', function (e) {
        if (e.persisted) {
            setRemUnit();
        }
    });
}(window, document));