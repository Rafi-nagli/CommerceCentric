var AcceptTermsAndConditionsModel = function (model, settings) {
    var self = this;

    self.settings = settings;
    self.isAcceptedTerms = model.IsAcceptedTerms;
    self.userId = model.UserId;

    self.showPopup = function () {
        console.log("showPopup. acceptTerms");
        Message.popupAsync({
            title: 'Confirm',
            message: 'Terms and conditions text',
            type: Message.YES_NO
        }).done(function () {
            $.ajax({
                url: self.settings.urls.acceptTerms,
                data: {                    
                },
                success: function (result) {
                    console.log("accepted");
                }
            })
        }).fail(function () {
            document.location = self.settings.urls.logout;
        });

    };

    console.log("isAcceptedTerms: " + self.isAcceptedTerms);
    if (!self.isAcceptedTerms)
        self.showPopup();
}