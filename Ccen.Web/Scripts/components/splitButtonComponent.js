ko.components.register('split-button', {
    synchronous: true,
    viewModel: function (params) {
        var self = this;

        //self.title = params ? params.title || '-' : '-';
        self.actions = params ? params.actions || [] : [];

        self.isOpen = ko.observable(false);
        self.toggleMenu = function (data, event) {
            var sender = $(event.target);
            var root = sender.closest('.x-split-button');
            if (root.hasClass('open'))
                root.removeClass('open');
            else
                root.addClass('open');
            //self.isOpen(!self.isOpen());

            event.stopPropagation();
        }

        self.stopClose = function(data, event) {
            //event.stopPropagation();
        }
    },

    template: '<span class="x-split-button" data-bind="click: stopClose">\
                    <button class="x-button x-button-main" type="button">\
                        <!-- ko template: { nodes: $componentTemplateNodes, data: $data } --><!-- /ko -->\
                    </button>\
                    <button class="x-button x-button-drop" type="button" data-bind="click: toggleMenu">\
                       <span class="caret"></span>\
                    </button>\
                    <ul class="x-button-drop-menu" data-bind="click: stopClose, foreach: actions">\
                        <li>\
                            <a data-bind="attr: { \'href\': url }" target="_blank"><span data-bind="text: title"></span></a>\
                        </li>\
                    </ul>\
                </span>'
});


$(function () {
    console.log('split buttons');

    $('html').on('click', function () {
        var splitBtn = $('.x-split-button');
        splitBtn.removeClass('open');
    });
});