var MessageStatus = Object.freeze({ "SUCCESS": 0, "ERROR": 1, "WARNING": 5, "INFO": 10 });

var MessageString = function (message, status) {
    var self = this;

    self.Message = message;
    self.Status = status;

    self.IsSuccess = status == MessageStatus.SUCCESS;
    self.IsError = status == MessageStatus.ERROR;
    self.IsWarning = status == MessageStatus.WARNING;
    self.IsInfo = status == MessageStatus.INFO;
}

ko.components.register('messages', {
    synchronous: true,
    viewModel: function (params) {
        var self = this;

        self.messages = params.messages;
        self.hasMessages = ko.computed(function() {
            return self.messages() != null && self.messages().length > 0;
        });
    },
    //text-success / alert-success
    template: '<div data-bind="visible: hasMessages">\
                <ul class="messages" data-bind="foreach: messages">\
                    <li class="message-string">\
                        <div class="alert" data-bind="html: Message, css: { \'alert-danger\': IsError, \'alert-success\': IsSuccess, \'alert-warning\': IsWarning, \'alert-info\': IsInfo }"></div>\
                    </li>\
                </ul>\
                </div>'
});