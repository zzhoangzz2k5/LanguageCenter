(function ($) {
  "use strict";

  var windowOn = $(window);
  let device_width = window.innerWidth;

  let dynamicyearElm = $(".xc-dynamic-year");
  if (dynamicyearElm.length) {
    let currentYear = new Date().getFullYear();
    dynamicyearElm.html(currentYear);
  }

  /* -----------------------------------------------------
    Counter 
  ----------------------------------------------------- */
  if ($(".xc-count-box").length) {
    $(".xc-count-box").appear(
      function () {
        var $t = $(this),
          n = $t.find(".xc-count-number").attr("data-stop"),
          r = parseInt($t.find(".xc-count-number").attr("data-speed"), 10);

        if (!$t.hasClass("counted")) {
          $t.addClass("counted");
          $({
            countNum: $t.find(".xc-count-number").text()
          }).animate(
            {
              countNum: n
            },
            {
              duration: r,
              easing: "linear",
              step: function () {
                $t.find(".xc-count-number").text(Math.floor(this.countNum));
              },
              complete: function () {
                $t.find(".xc-count-number").text(this.countNum);
              }
            }
          );
        }
      },
      {
        accY: 0
      }
    );
  }

  /* -----------------------------------------------------
    Vide popup 
  ----------------------------------------------------- */
  if ($(".video-popup").length) {
    $(".video-popup").magnificPopup({
      type: "iframe",
      mainClass: "mfp-fade",
      removalDelay: 160,
      preloader: true,

      fixedContentPos: false
    });
  }

  /* -----------------------------------------------------
    Image popup 
  ----------------------------------------------------- */
  if ($(".img-popup").length) {
    var groups = {};
    $(".img-popup").each(function () {
      var id = parseInt($(this).attr("data-group"), 10);

      if (!groups[id]) {
        groups[id] = [];
      }

      groups[id].push(this);
    });

    $.each(groups, function () {
      $(this).magnificPopup({
        type: "image",
        closeOnContentClick: true,
        closeBtnInside: false,
        gallery: {
          enabled: true
        }
      });
    });
  }

  /* -----------------------------------------------------
    Mobile Menu 
  ----------------------------------------------------- */
  $('#mobile-menu').meanmenu({
    meanMenuContainer: '.xc-mobile-nav__menu',
    meanScreenWidth: "1199",
    meanExpand: ['<i class="fal fa-plus"></i>'],
  });

  function dynamicCurrentMenuClass(selector) {
    let FileName = window.location.href.split("/").reverse()[0];

    selector.find("li").each(function () {
      let anchor = $(this).find("a");
      if ($(anchor).attr("href") == FileName) {
        $(this).addClass("current");
      }
    });
    // if any li has .current elmnt add class
    selector.find("li").each(function () {
      if ($(this).find(".current").length) {
        $(this).addClass("current");
      }
    });
    // if no file name return
    if ("" == FileName) {
      selector.find("li").eq(0).addClass("current");
    }
  }

  /* -----------------------------------------------------
     Menu Active 
   ----------------------------------------------------- */
  if ($(".xc-main-menu__list").length) {
    // dynamic current class
    let mainNavUL = $(".xc-main-menu__list");
    dynamicCurrentMenuClass(mainNavUL);
  }

  /* -----------------------------------------------------
      Offcanvas 
    ----------------------------------------------------- */
  $(".xc-offcanvas-btn").on("click", function () {
    $(".xc-mobile-nav__wrapper").toggleClass("opened");
    $("body").toggleClass("scoroll-locked");
  });

  if ($(".xc-close-toggler").length) {
    $(".xc-close-toggler").on("click", function (e) {
      e.preventDefault();
      $(".xc-search-popup").removeClass("open");
      $(".xc-mobile-nav__wrapper").removeClass("opened");
      $("body").removeClass("scoroll-locked");
      $(".xc-body-overlay").removeClass("active");
    });
  }

  if ($(".odometer").length) {
    $(".odometer").appear(function (e) {
      var odo = $(".odometer");
      odo.each(function () {
        var countNumber = $(this).attr("data-count");
        $(this).html(countNumber);
      });
    });
  }

  /* -----------------------------------------------------
    WOW 
  ----------------------------------------------------- */
  if ($(".wow").length) {
    var wow = new WOW({
      boxClass: "wow", // animated element css class (default is wow)
      animateClass: "animated", // animation css class (default is animated)
      mobile: true, // trigger animations on mobile devices (default is true)
      live: true // act on asynchronously loaded content (default is true)
    });
    wow.init();
  }

  /* -----------------------------------------------------
    Accordion 
  ----------------------------------------------------- */
  if ($(".swiftcart-accordion-active").length) {
    var accordionGrp = $(".swiftcart-accordion-active");
    accordionGrp.each(function () {
      var accordionName = $(this).data("grp-name");
      var Self = $(this);
      var accordion = Self.find(".xc-accordion");
      Self.addClass(accordionName);
      Self.find(".xc-accordion .xc-accordion-content").hide();
      Self.find(".xc-accordion.active").find(".xc-accordion-content").show();
      accordion.each(function () {
        $(this)
          .find(".xc-accordion-title")
          .on("click", function () {
            if ($(this).parent().hasClass("active") === false) {
              $(".swiftcart-accordion-active." + accordionName)
                .find(".xc-accordion")
                .removeClass("active");
              $(".swiftcart-accordion-active." + accordionName)
                .find(".xc-accordion")
                .find(".xc-accordion-content")
                .slideUp();
              $(this).parent().addClass("active");
              $(this).parent().find(".xc-accordion-content").slideDown();
            }
          });
      });
    });
  }

  /* -----------------------------------------------------
    Add 
  ----------------------------------------------------- */
  $(".add").on("click", function () {
    if ($(this).prev().val() < 999) {
      $(this)
        .prev()
        .val(+$(this).prev().val() + 1);
    }
  });

  /* -----------------------------------------------------
    Sub
  ----------------------------------------------------- */
  $(".sub").on("click", function () {
    if ($(this).next().val() > 0) {
      if ($(this).next().val() > 0)
        $(this)
          .next()
          .val(+$(this).next().val() - 1);
    }
  });

  /* -----------------------------------------------------
    Tab
  ----------------------------------------------------- */
  if ($(".tabs-box").length) {
    $(".tabs-box .tab-buttons .tab-btn").on("click", function (e) {
      e.preventDefault();
      var target = $($(this).attr("data-tab"));

      if ($(target).is(":visible")) {
        return false;
      } else {
        target
          .parents(".tabs-box")
          .find(".tab-buttons")
          .find(".tab-btn")
          .removeClass("active-btn");
        $(this).addClass("active-btn");
        target
          .parents(".tabs-box")
          .find(".tabs-content")
          .find(".tab")
          .fadeOut(0);
        target
          .parents(".tabs-box")
          .find(".tabs-content")
          .find(".tab")
          .removeClass("active-tab");
        $(target).fadeIn(300);
        $(target).addClass("active-tab");
      }
    });
  }

  /* -----------------------------------------------------
    Range
  ----------------------------------------------------- */
  if ($(".range-slider-price").length) {
    var priceRange = document.getElementById("range-slider-price");

    noUiSlider.create(priceRange, {
      start: [30, 150],
      limit: 200,
      behaviour: "drag",
      connect: true,
      range: {
        min: 10,
        max: 200
      }
    });

    var limitFieldMin = document.getElementById("min-value-rangeslider");
    var limitFieldMax = document.getElementById("max-value-rangeslider");

    priceRange.noUiSlider.on("update", function (values, handle) {
      (handle ? $(limitFieldMax) : $(limitFieldMin)).text(values[handle]);
    });
  }

  /* -----------------------------------------------------
    Owl Slider
  ----------------------------------------------------- */
  function thmOwlInit() {
    // owl slider
    let swiftcartowlCarousel = $(".swiftcart-owl__carousel");
    if (swiftcartowlCarousel.length) {
      swiftcartowlCarousel.each(function () {
        let elm = $(this);
        let options = elm.data("owl-options");
        let thmOwlCarousel = elm.owlCarousel(
          "object" === typeof options ? options : JSON.parse(options)
        );
        elm.find("button").each(function () {
          $(this).attr("aria-label", "carousel button");
        });
      });
    }
    let swiftcartowlCarouselNav = $(".swiftcart-owl__carousel--custom-nav");
    if (swiftcartowlCarouselNav.length) {
      swiftcartowlCarouselNav.each(function () {
        let elm = $(this);
        let owlNavPrev = elm.data("owl-nav-prev");
        let owlNavNext = elm.data("owl-nav-next");
        $(owlNavPrev).on("click", function (e) {
          elm.trigger("prev.owl.carousel");
          e.preventDefault();
        });

        $(owlNavNext).on("click", function (e) {
          elm.trigger("next.owl.carousel");
          e.preventDefault();
        });
      });
    }
  }

  /* -----------------------------------------------------
    Sicky Header
  ----------------------------------------------------- */
  windowOn.on('scroll', function () {
    var scroll = $(window).scrollTop();
    if (scroll < 100) {
      $("#xc-header-sticky").removeClass("xc-header-sticky");
    } else {
      $("#xc-header-sticky").addClass("xc-header-sticky");
    }
  });

  // 07. Back Top Top 
  var btn = $('#xc_back-to-top');
  var btn_wrapper = $('.xc-back-to-top-wrapper');

  windowOn.scroll(function () {
    if (windowOn.scrollTop() > 300) {
      btn_wrapper.addClass('xc-back-to-top-btn-show');
    } else {
      btn_wrapper.removeClass('xc-back-to-top-btn-show');
    }
  });

  btn.on('click', function (e) {
    e.preventDefault();
    $('html, body').animate({
      scrollTop: 0
    }, '300');
  });


  //Strech Column
  function swiftcart_stretch() {
    var i = $(window).width();
    $(".row .swiftcart-stretch-element-inside-column").each(function () {
      var $this = $(this),
        row = $this.closest(".row"),
        cols = $this.closest('[class^="col-"]'),
        colsheight = $this.closest('[class^="col-"]').height(),
        rect = this.getBoundingClientRect(),
        l = row[0].getBoundingClientRect(),
        s = cols[0].getBoundingClientRect(),
        r = rect.left,
        d = i - rect.right,
        c = l.left + (parseFloat(row.css("padding-left")) || 0),
        u = i - l.right + (parseFloat(row.css("padding-right")) || 0),
        p = s.left,
        f = i - s.right,
        styles = {
          "margin-left": 0,
          "margin-right": 0
        };
      if (Math.round(c) === Math.round(p)) {
        var h = parseFloat($this.css("margin-left") || 0);
        styles["margin-left"] = h - r;
      }
      if (Math.round(u) === Math.round(f)) {
        var w = parseFloat($this.css("margin-right") || 0);
        styles["margin-right"] = w - d;
      }
      $this.css(styles);
    });
  }
  swiftcart_stretch();

  function swiftcart_cuved_circle() {
    let circleTypeElm = $(".curved-circle--item");
    if (circleTypeElm.length) {
      circleTypeElm.each(function () {
        let elm = $(this);
        let options = elm.data("circle-text-options");
        elm.circleType(
          "object" === typeof options ? options : JSON.parse(options)
        );
      });
    }
  }

  /*-- Price Range --*/
  function priceFilter() {
    if ($(".price-ranger").length) {
      $(".price-ranger #slider-range").slider({
        range: true,
        min: 50,
        max: 1000,
        values: [11, 500],
        slide: function (event, ui) {
          $(".price-ranger .ranger-min-max-block .min").val("$" + ui.values[0]);
          $(".price-ranger .ranger-min-max-block .max").val("$" + ui.values[1]);
        }
      });
      $(".price-ranger .ranger-min-max-block .min").val(
        "$" + $(".price-ranger #slider-range").slider("values", 0)
      );
      $(".price-ranger .ranger-min-max-block .max").val(
        "$" + $(".price-ranger #slider-range").slider("values", 1)
      );
    }
  }

  if ($('#marker').length > 0) {
    function xc_tab_line() {
      var marker = document.querySelector('#marker');
      var item = document.querySelectorAll('.xc-tab-menu button');
      var itemActive = document.querySelector('.xc-tab-menu .nav-link.active');

      // rtl settings
      var xc_rtl = localStorage.getItem('xc_dir');
      let rtl_setting = xc_rtl == 'rtl' ? 'right' : 'left';

      function indicator(e) {
        marker.style.left = e.offsetLeft + "px";
        marker.style.width = e.offsetWidth + "px";
      }


      item.forEach(link => {
        link.addEventListener('click', (e) => {
          indicator(e.target);
        });
      });

      var activeNav = $('.nav-link.active');
      var activewidth = $(activeNav).width();
      var activePadLeft = parseFloat($(activeNav).css('padding-left'));
      var activePadRight = parseFloat($(activeNav).css('padding-right'));
      var totalWidth = activewidth + activePadLeft + activePadRight;

      var precedingAnchorWidth = anchorWidthCounter();


      $(marker).css('display', 'block');

      $(marker).css('width', totalWidth);

      function anchorWidthCounter() {
        var anchorWidths = 0;
        var a;
        var aWidth;
        var aPadLeft;
        var aPadRight;
        var aTotalWidth;
        $('.xc-tab-menu button').each(function (index, elem) {
          var activeTest = $(elem).hasClass('active');
          marker.style.left = elem.offsetLeft + "px";
          if (activeTest) {
            // Break out of the each function.
            return false;
          }

          a = $(elem).find('button');
          aWidth = a.width();
          aPadLeft = parseFloat(a.css('padding-left'));
          aPadRight = parseFloat(a.css('padding-right'));
          aTotalWidth = aWidth + aPadLeft + aPadRight;

          anchorWidths = anchorWidths + aTotalWidth;

        });

        return anchorWidths;
      }
    }
    xc_tab_line();
  }

  // window load event

  $(window).on("load", function () {
    thmOwlInit();
    priceFilter();
    swiftcart_cuved_circle();

    if ($(".xc-preloader").length) {
      $(".xc-preloader").fadeOut(500);
    }
    if ($(".circle-progress").length) {
      $(".circle-progress").appear(function () {
        let circleProgress = $(".circle-progress");
        circleProgress.each(function () {
          let progress = $(this);
          let progressOptions = progress.data("options");
          progress.circleProgress(progressOptions);
        });
      });
    }
    if ($(".xc-masonry-layout").length) {
      $(".xc-masonry-layout").imagesLoaded(function () {
        $(".xc-masonry-layout").isotope({
          layoutMode: "masonry",
          gutter: 20
        });
      });
    }
    if ($(".fitRow-layout").length) {
      $(".fitRow-layout").imagesLoaded(function () {
        $(".fitRow-layout").isotope({
          layoutMode: "fitRows"
        });
      });
    }

    if ($(".xc-post-filter").length) {
      var postFilterList = $(".xc-post-filter li");
      // for first init
      $(".xc-filter-layout").isotope({
        filter: ".xc-filter-item",
        animationOptions: {
          duration: 500,
          easing: "linear",
          queue: false
        }
      });
      // on click filter links
      postFilterList.on("click", function () {
        var Self = $(this);
        var selector = Self.attr("data-filter");
        postFilterList.removeClass("active");
        Self.addClass("active");

        $(".filter-layout").isotope({
          filter: selector,
          animationOptions: {
            duration: 500,
            easing: "linear",
            queue: false
          }
        });
        return false;
      });
    }

    if ($(".post-filter.has-dynamic-filter-counter").length) {
      // var allItem = $('.single-filter-item').length;

      var activeFilterItem = $(".post-filter.has-dynamic-filter-counter").find(
        "li"
      );

      activeFilterItem.each(function () {
        var filterElement = $(this).data("filter");
        var count = $(".filter-layout").find(filterElement).length;
        $(this).append("<sup>[" + count + "]</sup>");
      });
    }
  });

  $(document).ready(function () {
    if ($("select").length) {
      $('select').niceSelect();
    }
  });
  $(window).on("scroll", function () {
    if ($(".sticky-header--one-page").length) {
      var headerScrollPos = 130;
      var stricky = $(".sticky-header--one-page");
      if ($(window).scrollTop() > headerScrollPos) {
        stricky.addClass("active");
      } else if ($(this).scrollTop() <= headerScrollPos) {
        stricky.removeClass("active");
      }
    }
    var scrollToTopBtn = ".scroll-to-top";
    if (scrollToTopBtn.length) {
      if ($(window).scrollTop() > 500) {
        $(scrollToTopBtn).addClass("show");
      } else {
        $(scrollToTopBtn).removeClass("show");
      }
    }
  });

  $(window).on("resize", function () {
    swiftcart_stretch();
  });


  // 05. Data CSS Js
  if ($("[data-bg]").length) {
    $("[data-bg]").each(function () {
      $(this).css("background-image", "url( " + $(this).attr("data-bg") + "  )");
      $(this).removeAttr("data-bg").addClass("background-image");
    });
  };
  // 06. Data CSS Js
  if ($("[data-bg-color]").length) {
    $("[data-bg-color]").each(function () {
      $(this).css("background-color", "" + $(this).attr('data-bg-color') + " ");
      $(this).removeAttr("data-bg-color").addClass("background-color");
    });
  };

  function dynamicCurrentMenuClass(selector) {
    let FileName = window.location.href.split("/").reverse()[0];

    selector.find("li").each(function () {
      let anchor = $(this).find("a");
      if ($(anchor).attr("href") == FileName) {
        $(this).addClass("current");
      }
    });
    // if any li has .current elmnt add class
    selector.children("li").each(function () {
      if ($(this).find(".current").length) {
        $(this).addClass("current");
      }
    });
    // if no file name return
    if ("" == FileName) {
      selector.find("li").eq(0).addClass("current");
    }
  }

  if ($(".xc-main-menu ul").length) {
    // dynamic current class
    let mainNavUL = $(".xc-main-menu ul");
    dynamicCurrentMenuClass(mainNavUL);
  }

  if ($(".service-sidebar__nav ul").length) {
    // dynamic current class
    let mainNavUL = $(".service-sidebar__nav ul");
    dynamicCurrentMenuClass(mainNavUL);
  }

  if ($(".xc-popup").length) {
    GLightbox({
      selector: '.xc-popup',
      descPosition: 'right',
    })
  }
  Splitting();
  ScrollOut({
    targets: '[data-splitting]'
  });


})(jQuery);
