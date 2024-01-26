// initialize image uploader with suitable parameters
$(function () {
    var inputImages1 = document.querySelector('.input-images-1');
    if (inputImages1 != null) {
        $('.input-images-1').imageUploader({
            multiple: 'multiple'
        });
    }

    var inputImages2 = document.querySelector('.input-images-2');
    if (inputImages2 != null) {
        $('.input-images-2').imageUploader({
            preloaded: preloaded,
            imagesInputName: 'photos',
            preloadedInputName: 'old',
            multiple: 'multiple',
            maxFiles: 10
        });
    }

    var inputImages3 = document.querySelector('.input-images-3');
    if (inputImages3 != null) {
        $('.input-images-3').imageUploader({
            preloaded: preloaded,
            imagesInputName: 'photos',
            preloadedInputName: 'old',
            maxFiles: 1
        });
    }
});

// initialize shorten with suitable parameters
$(function () {
    $(".longtext").shorten({
        "showChars": 525,
        "moreText": "Read More",
        "lessText": "Show Less",
    });

    $(".long-profile-text").shorten({
        "showChars": 150,
        "moreText": "Read More",
        "lessText": "Show Less",
    });
});

// display navbar's sub menu on hover
$(function () {

    document.querySelectorAll('.menu-item-has-children').forEach(function (everyitem) {

        everyitem.addEventListener('mouseover', function () {
            let parent = this.querySelector('a');
            if (parent != null) {
                let child = parent.nextElementSibling;
                $(child).show();
            }
        });

        everyitem.addEventListener('mouseleave', function () {
            let parent = this.querySelector('a');
            if (parent != null) {
                let child = parent.nextElementSibling;
                $(child).hide();
            }
        });

    });

});

// format datetime of user's registration date
$(function () {
    var dt = document.getElementsByClassName("registration_date");
    for (let i = 0; i < dt.length; i++) {
        var myArray = document.getElementsByClassName("registration_date")[i].innerHTML.split(" ");
        var newdate = myArray[0].split("/").reverse().join("-");
        var d = new Date(newdate);
        document.getElementsByClassName("datetime")[i].innerHTML += d.toLocaleDateString();
    }
});

// slider of minimum and maximum values
$(function () {
    var $slider = $("#slider-range");
    //Get min and max values

    var priceMin = $slider.attr("data-min"),
        priceMax = $slider.attr("data-max");

    //Set min and max values where relevant
    $("#filter-min, #filter-max").map(function () {
        $(this).attr({
            "min": priceMin,
            "max": priceMax
        });
    });

    $("#filter-min").attr({
        "placeholder": "min " + priceMin,
        "value": priceMin
    });

    $("#filter-max").attr({
        "placeholder": "max " + priceMax,
        "value": priceMax
    });

    $slider.slider({
        range: true,
        min: Math.max(priceMin, 0),
        max: priceMax,
        values: [$("#filter-min").val(), $("#filter-max").val()],
        slide: function (event, ui) {
            $("#filter-min").val(ui.values[0]);
            $("#filter-max").val(ui.values[1]);
        }
    });

    $("#filter-min, #filter-max").map(function () {
        $(this).on("input", function () {
            updateSlider();
        });
    });

    function updateSlider() {
        $slider.slider("values", [$("#filter-min").val(), $("#filter-max").val()]);
    }

});

// set modal class names on filter sidebar on smaller screens
$(function () {
    if ($(window).width() < 768) {
        $("#filterModalCenter").addClass("modal fade");
        $("#mod_dialog").addClass("modal-dialog modal-dialog-centered");
        $("#mod_content").addClass("modal-content");
        $("#mod_body").addClass("modal-body");
        $("form_footer").addClass("modal-footer");
        $(".create_btn").addClass("btn-lg");
        $(".widget--vendor-filter").hide();
    }
    else {
        $("#mod_header").hide();
    }


});

// if window size smaller than 1200px set the category items with col-sm-6 class
$(function () {
    if ($(window).width() < 1200) {
        $(".listing-category-item").removeClass("col-sm-3").addClass("col-sm-6");
    }
});

// initialize slick slider and change display items on smaller screens
$(function () {
    if ($(window).width() < 768) {

        $("#vendor-slick-slider").addClass("vendor-slider");
        $('.vendor-slider').slick({
            infinite: true,
            slidesToShow: 1
        });

        $("#event-slick-slider").addClass("event-slider");
        $('.event-slider').slick({
            infinite: true,
            slidesToShow: 1
        });

    }
    else {
        $('.vendor-slider').slick({
            infinite: true,
            slidesToShow: 3
        });

        $('.event-slider').slick({
            infinite: true,
            slidesToShow: 3
        });
    }
});

// set minimum and start dates based on selection
$(function () {
    var start = document.getElementById('start');
    var end = document.getElementById('end');

    if (start != null && end != null) {
        start.addEventListener('change', function () {
            if (start.value)
                end.min = start.value;
        }, false);

        end.addEventListener('change', function () {
            if (end.value)
                start.max = end.value;
        }, false);
    }
});

// set tags input on focus type of email
$(function () {
    $("#multidatalist").focusin(function () { $(this).attr("type", "email"); });
    $("#multidatalist").focusout(function () { $(this).attr("type", "textbox"); });
});

// remove message that says to enter a valid email address
$(function () {
    var multidatalist = document.getElementById('multidatalist');
    if (multidatalist != null) {
        multidatalist.addEventListener('keyup', () => {
            if ($('#multidatalist-error').length) {
                if ($('#multidatalist-error:contains("email")')) {
                    $("#tagsMsg").removeClass("d-block").addClass("d-none");
                }
                else {
                    $("#tagsMsg").removeClass("d-none").addClass("d-block");
                }
            }
        });
    }
});

// display progress bar's percentage based on rating
$(function () {
    var progress = document.getElementsByClassName("progressbar");
    for (let i = 0; i < progress.length; i++) {
        document.getElementsByClassName("progressbar")[i].style.width = 20 * (document.getElementsByClassName("review_rating")[i].innerHTML.replace(",", ".")) + '%';
    }
});

// get hashtag item from url and add active class to the current navigation button (highlight it)
$(function () {
    var fullHash = location.hash;
    var hash = location.hash.substr(1);
    var menuItem = document.getElementById("menu-" + hash);
    var tabContent = document.getElementById(hash);

    if (fullHash !== "") {
        scrollToSection(fullHash);
    }

    $(".nav-tabs a").click(function () {
        $(this).tab('show');
        scrollToSection($(this).attr('href'));
    });

    var navTabs = document.getElementById("ex1");
    if (navTabs != null) {

        if (hash !== "") {
            $(menuItem).addClass("menu_item--current");
            $(tabContent).addClass("show active");
        }

        else {
            $("#menu-description").addClass("menu_item--current");
            $("#description").addClass("show active");
        }

        var lis = navTabs.getElementsByClassName("menu_item");
        for (var i = 0; i < lis.length; i++) {
            lis[i].addEventListener("click", function () {
                var current = document.getElementsByClassName("menu_item--current");
                current[0].className = current[0].className.replace(" menu_item--current", "");
                this.className += " menu_item--current";
            });
        }
    }

    var navTabs2 = document.getElementById("ex2");
    if (navTabs2 != null) {

        if (hash !== "") {
            $(menuItem).addClass("active");
            $(tabContent).addClass("show active");
        }

        else {
            if ($('#menu-offersforexpert').length) {
                $("#menu-offersforexpert").addClass("active");
                $("#offersforexpert").addClass("show active");
            }
            else if ($('#menu-offersbyexpert').length) {
                $("#menu-offersbyexpert").addClass("active");
                $("#offersbyexpert").addClass("show active");
            }
        }
    }

});

// if user hasn't checked a star in his review return a message
$('#reviewForm').on('submit', function () {

    if ($("input[type=radio]:checked").length == 0) {
        document.getElementById("reviewRatingMsg").innerHTML = "The Rating field is required.";
        return false;
    }
    return true;

});

// call methods to check if form is valid
$('.form--listing-submit').on('submit', function (e) {
    if (!checkDescription()) {
        //stop form submission
        e.preventDefault();
    }

    if (!checkTags()) {
        //stop form submission
        e.preventDefault();
    }
});

// get all uploaded images from hidden inputs and set it to a new hidden input
$('#editForm').on('submit', function () {

    document.getElementById('uploadedFiles').value = "";

    var oldValues = [];
    $("input[name='oldName[]']").each(function () {
        oldValues.push($(this).val().replace(/^.*[\\\/]/, ''));
    });

    document.getElementById('uploadedFiles').value = oldValues;

});

// if the user hasn't inserted a description in form return a message
function checkDescription() {
    var description = document.getElementById('descriptionRTE');
    var descriptionMsg = document.getElementById("descriptionMsg");
    if (description.value == "") {
        descriptionMsg.innerHTML = "The Description field is required.";
        return false;
    }
    else {
        descriptionMsg.innerHTML = "";
    }
    return true;
}

// if the tags contain null values return a message
function checkTags() {
    var multidatalist = document.getElementById('multidatalist');
    var input = multidatalist.value;
    if (input != "") {
        var tagsArray = input.split(",");
        var tags = document.getElementById("tagsMsg");
        for (var i = 0; i < tagsArray.length; i++) {
            if (tagsArray.includes("")) {
                $(tags).removeClass("d-none").addClass("d-block");
                tags.removeAttribute("data-valmsg-for");
                tags.removeAttribute("data-valmsg-replace");
                tags.innerHTML = "Empty values are not allowed.";
                return false;
            }
            else {
                $(tags).removeClass("d-block").addClass("d-none");
                tags.innerHTML = "";
            }
        }
        var set = new Set(tagsArray);
        if (tagsArray.length !== set.size) {
            $(tags).removeClass("d-none").addClass("d-block");
            tags.removeAttribute("data-valmsg-for");
            tags.removeAttribute("data-valmsg-replace");
            tags.innerHTML = "Duplicated values are not allowed.";
            return false;
        }
        else {
            $(tags).removeClass("d-block").addClass("d-none");
            tags.innerHTML = "";
        }
    }
    return true;
}

$(function () {
    var descriptionParagraph = document.querySelector('#descriptionRTE_rte-edit-view p');
    if (descriptionParagraph != null) {
        let observer;

        function observeParagraph() {
            // create a new MutationObserver
            observer = new MutationObserver(function (mutations) {
                mutations.forEach(function (mutation) {
                    if (mutation.type === "characterData") {
                        console.log("Paragraph text has changed");
                        checkDescriptionParagraph();
                    }
                });
            });

            // configure the observer to watch for changes to the text content of the paragraph
            const config = { characterData: true, subtree: true };
            observer.observe(descriptionParagraph, config);
        }

        function stopObservingParagraph() {
            observer.disconnect();
        }

        // if the user hasn't inserted a description in form return a message
        function checkDescriptionParagraph() {
            var descriptionMsg = document.getElementById("descriptionMsg");
            if (descriptionParagraph.textContent.trim().length === 0) {
                $(descriptionMsg).removeClass("d-none").addClass("d-block");
                descriptionMsg.innerHTML = "The Description field is required.";
                return false;
            }
            else {
                $(descriptionMsg).removeClass("d-block").addClass("d-none");
                descriptionMsg.innerHTML = "";
            }
            return true;
        }

        observeParagraph();

        window.addEventListener("beforeunload", function () {
            stopObservingParagraph();
        });
    }
});

$(function () {
    var multidatalist = document.getElementById("multidatalist");
    if (multidatalist != null) {
        var datalist = document.getElementById("tags-list");
        var datalistOptions = [];
        for (var i = 0; i < datalist.options.length; i++) {
            datalistOptions.push(datalist.options[i].value);
        }

        removeElementFromDatalist(multidatalist, datalist, datalistOptions);
        addElementToDatalist(multidatalist, datalist, datalistOptions);

        multidatalist.addEventListener('keyup', () => {
            if (document.getElementById("tagsMsg") != null) {
                checkTags();
            }
            removeElementFromDatalist(multidatalist, datalist, datalistOptions);
            addElementToDatalist(multidatalist, datalist, datalistOptions);
        });

        function removeElementFromDatalist(multidatalist, datalist, datalistOptions) {
            var inputValues = getInputValues(multidatalist);
            for (var i = 0; i < inputValues.length; i++) {
                if (datalistOptions.includes(inputValues[i])) {
                    // Get the option element that you want to hide
                    var option = datalist.querySelector('[value="' + inputValues[i] + '"]');
                    // Remove the option element from the datalist
                    if (option) {
                        datalist.removeChild(option);
                    }
                }
            }
        }

        function addElementToDatalist(multidatalist, datalist, datalistOptions) {
            var inputValues = getInputValues(multidatalist);
            for (var i = 0; i < datalistOptions.length; i++) {
                if (!inputValues.includes(datalistOptions[i])) {
                    // Get the option element that you want to hide
                    var existingOption = datalist.querySelector('[value="' + datalistOptions[i] + '"]');
                    // if option doesn't exist
                    if (existingOption == null) {
                        // Create a new option element
                        var newOption = document.createElement('option');
                        // Set the option's value and text
                        newOption.value = datalistOptions[i];
                        newOption.innerText = datalistOptions[i];
                        // Append the option to the datalist
                        datalist.appendChild(newOption);
                    }
                }
            }
        }

        function getInputValues(multidatalist) {
            var inputValues = [];
            inputValues.length = 0;
            var input = multidatalist.value;
            if (input != "") {
                var tagsArray = input.split(",").filter((str) => str !== '');
                for (var j = 0; j < tagsArray.length; j++) {
                    if (!inputValues.includes(tagsArray[j])) {
                        inputValues.push(tagsArray[j]);
                    }
                }
            }
            return inputValues;
        }
    }
});

$(function () {
    var navLinks = document.querySelector('.navigation .nav-links');
    if (navLinks != null) {
        var activeLink = navLinks.querySelector('.active');
        var firstBtn = navLinks.querySelector('.first');
        var previousBtn = navLinks.querySelector('.prev');
        var previousElement = activeLink.previousElementSibling;
        var nextElement = activeLink.nextElementSibling;
        var nextBtn = navLinks.querySelector('.next');
        var lastBtn = navLinks.querySelector('.last');
        var siblingsToKeep = [];

        if ($(window).width() < 1200) {
            siblingsToKeep = [firstBtn, previousBtn, previousElement, activeLink, nextElement, nextBtn, lastBtn];
            hidePaginationNumbers();
        }

        if ($(window).width() < 425) {
            siblingsToKeep = [previousBtn, previousElement, activeLink, nextElement, nextBtn];
            hidePaginationNumbers();
        }

        function hidePaginationNumbers() {
            for (var i = 0; i < navLinks.children.length; i++) {
                var child = navLinks.children[i];
                if (!siblingsToKeep.includes(child)) {
                    $(child).hide();
                }
            }
        }
    }
});

// add load more to related cultural activities and experts
$(function () {
    $('.load-more-grid').after('<div style="display:flex; justify-content: center;" id="load-more-content">');
    var rowsShown = 6;
    var startItem = 0;
    var rowsTotal = $('.load-more-grid .row .grid_item').length;
    var endItem = rowsTotal;

    $('.load-more-grid .row .grid_item').hide();
    $('.load-more-grid .row .grid_item').slice(0, rowsShown).show();

    if (rowsTotal > rowsShown) {
        $('#load-more-content').append('<button class="btn btn-primary" id="load_more_btn">Load more</button>');

        $('#load-more-content #load_more_btn').bind('click', function (e) {
            e.preventDefault();
            $('.load-more-grid .row .grid_item').css('opacity', '0').hide().slice(startItem, endItem).
                css('display', 'block').animate({
                    opacity: 1
                }, 300);
            checkButton();
        });

        function checkButton() {
            var button = document.getElementById('load_more_btn');
            if (button.textContent.includes('Load')) {
                document.getElementById("load_more_btn").textContent = "Show less";
                endItem = rowsShown;
            }
            else {
                document.getElementById("load_more_btn").textContent = "Load more";
                endItem = rowsTotal;
            }
        }
    }

});

// add pagination to reviews and offers
$(function () {
    $('.paginated-grid').after('<nav class="navigation pagination" id="nav">');
    var rowsShown = 5;
    var rowsTotal = $('.paginated-grid .row .grid_item').length;
    var numPages = rowsTotal / rowsShown;
    var currPage = 0;

    $('#nav').append('<div class="nav-links"><a href="#" class="prev d-flex"></a><a href="#" class="next d-flex"></a></div>');

    checkPreviousAndNextPages();

    $('.paginated-grid .row .grid_item').hide();
    $('.paginated-grid .row .grid_item').slice(0, rowsShown).show();

    function clickEvent() {
        var startItem = currPage * rowsShown;
        var endItem = startItem + rowsShown;
        $('.paginated-grid .row .grid_item').css('opacity', '0').hide().slice(startItem, endItem).
            css('display', 'block').animate({
                opacity: 1
            }, 300);

        checkPreviousAndNextPages();
    }

    function checkPreviousAndNextPages() {
        if (currPage + 1 < numPages) {
            if ($(".next").hasClass("d-none")) {
                $(".next").addClass("d-flex").removeClass("d-none");
            }
        }
        else {
            if ($(".next").hasClass("d-flex")) {
                $(".next").addClass("d-none").removeClass("d-flex");
            }
        }

        if (currPage > 0) {
            if ($(".prev").hasClass("d-none")) {
                $(".prev").addClass("d-flex").removeClass("d-none");
            }
        }
        else {
            if ($(".prev").hasClass("d-flex")) {
                $(".prev").addClass("d-none").removeClass("d-flex");
            }
        }
    }

    $('#nav .prev').bind('click', function (e) {
        e.preventDefault();
        currPage = currPage - 1;
        clickEvent();
    });

    $('#nav .next').bind('click', function (e) {
        e.preventDefault();
        currPage = currPage + 1;
        clickEvent();
    });

});

// show email verification link 
$(function () {
    var manageEmail = document.getElementById("manage-email");

    if (manageEmail != null) {
        var storedEmail = manageEmail.value;
        var emailVerification = document.getElementById("email-verification");
        manageEmail.addEventListener("keyup", () => {
            if (manageEmail.value != storedEmail) {
                emailVerification.style.display = 'none';
            }
            else {
                emailVerification.style.display = 'block';
            }
        });
    }
});

// toggle visibility of new and confirm password when user changes his old one
$(function () {
    var oldPassword = document.getElementById("old-password");
    var newPassword = document.getElementById("new-password");
    var newPassWordInputs = document.getElementById("new-password-inputs");

    if (oldPassword != null) {
        function togglePasswordInputs() {
            if (oldPassword.value != "") {
                newPassWordInputs.style.display = "block";
                newPassword.required = true;
            } else {
                newPassWordInputs.style.display = "none";
                newPassword.required = false;
            }
        }

        togglePasswordInputs();

        oldPassword.addEventListener("keyup", () => {
            togglePasswordInputs();
        });
    }
});

// scroll to selected element
function scrollToSection(section) {
    setTimeout(() => {
        var sectionTo = $(section);
        $('html, body').animate({
            scrollTop: $(sectionTo).offset().top
        }, 1500);
    }, 200);
}

// open and close navbar menu
function toggleMenu() {
    var headerNavBurger = document.querySelector('.header-navbar_burger');
    var menu = headerNavBurger.querySelector('#menu-header');
    var body = document.querySelector('#body');
    if ($(menu).is(':hidden')) {
        menu.style.display = "block";
        body.style.overflow = "hidden";
        $(".fa-bars").addClass("fa-x").removeClass("fa-bars");
        $("#burger-action").addClass("burger-link");
    } else {
        menu.style.display = "none";
        body.style.overflow = "scroll";
        $(".fa-x").addClass("fa-bars").removeClass("fa-x");
        $("#burger-action").removeClass("burger-link");
    }
}

// open filter modal on button click
function showModal() {
    $(".widget--vendor-filter").show();
}

// decode text
function decodeEntities(input) {
    var y = document.createElement('textarea');
    y.innerHTML = input;
    return y.value;
}

// set message values
function setMessageValues(modalTitle, listingId, expertId) {
    document.getElementById("modalTitle").textContent = modalTitle;
    document.getElementById("listingId").value = listingId;
    document.getElementById("expertId").value = expertId;
}

// set offer values
function setOfferValues(modalTitle, requestId, offerId, offerText) {
    document.getElementById("offerModalTitle").textContent = modalTitle;
    document.getElementById("requestId").value = requestId;
    document.getElementById("offerId").value = offerId;
    document.getElementById("offerText").value = decodeEntities(offerText);
}

// set review values
function setReviewValues(modalTitle, listingId, reviewId, reviewRating, reviewMessage) {
    document.getElementById("reviewModalTitle").textContent = modalTitle;
    document.getElementById("rewiewListingId").value = listingId;
    document.getElementById("reviewId").value = reviewId;
    document.getElementById("reviewRating").value = reviewRating;
    var ratingVal = document.getElementById("reviewRating").value;
    if (ratingVal != "") {
        document.getElementById(ratingVal).checked = true;
    }
    document.getElementById("reviewMessage").value = decodeEntities(reviewMessage);
}

// set delete values
function setDeleteValues(modalTitle, modalBody, id) {
    document.getElementById("deleteModalTitle").textContent = modalTitle;
    document.getElementById("modalBody").textContent = modalBody;
    document.getElementById("idVal").value = id;
}

// change visibility, icon and text of listing
function changeVisibility() {
    if (document.getElementById("visibilityText").innerHTML == "Hide") {
        $(".visibility-icon").removeClass("fa-eye-slash").addClass("fa-eye");
        document.getElementById("visibilityText").innerHTML = "Unhide";
        document.getElementById("visibilityInput").value = false;
    }
    else {
        $(".visibility-icon").removeClass("fa-eye").addClass("fa-eye-slash");
        document.getElementById("visibilityText").innerHTML = "Hide";
        document.getElementById("visibilityInput").value = true;
    }
}

// display expert profile's registration fields
function showExpertProfile() {
    var expertProfile = document.getElementById("expert-profile");
    if ($(expertProfile).hasClass("d-none")) {
        $(expertProfile).removeClass("d-none").addClass("d-block");
        scrollToSection(expertProfile);
    }
    else {
        $(expertProfile).removeClass("d-block").addClass("d-none");
    }
}

// format date
function formatDate(date) {
    var newdate = date.split("/").reverse().join("-");
    const newDate = new Date(newdate);
    const yearDate = newDate.getFullYear();
    const monthDate = String(newDate.getMonth() + 1).padStart(2, '0');
    const dayDate = String(newDate.getDate()).padStart(2, '0');
    const joinedDateStart = [yearDate, monthDate, dayDate].join('-');
    return joinedDateStart;
}