ko.components.register('grid-pager', {
    synchronous: true,
    viewModel: function (params) {
        var self = this;

        self.refreshCallback = params.refreshCallback;
        self.itemCount = params.itemCount;
        self.pageIndex = params.pageIndex;

        console.log("init grid-pager");
        console.log(params.itemCount() + "-" + params.pageIndex());

        self.itemsPerPage = params.itemsPerPage;
        self.pageCount = ko.computed(function () {
            return Math.ceil(self.itemCount() / self.itemsPerPage);
        });

        console.log('pageCount: ' + self.pageCount());

        self.itemBegin = ko.computed(function () {
            if (self.itemCount() == 0)
                return 0;
            return self.itemsPerPage * (self.pageIndex() - 1) + 1;
        });
        self.itemEnd = ko.computed(function () {
            return Math.min(self.itemCount(), self.itemsPerPage * (self.pageIndex()));
        });

        self.pageNumbers = ko.computed(function () {
            var pages = [];
            var minIndex = Math.max(1, self.pageIndex() - 4);
            var maxIndex = Math.min(self.pageCount(), self.pageIndex() + 4);
            if (maxIndex - minIndex < 9) {
                if (minIndex == 1)
                    maxIndex = Math.min(9, self.pageCount());
                if (maxIndex == self.pageCount())
                    minIndex = Math.max(1, self.pageCount() - 8);
            }

            console.log(minIndex + "-" + maxIndex);
            if (maxIndex < self.pageCount())
                maxIndex = maxIndex - 1;

            for (var i = minIndex; i <= maxIndex; i++) {
                var number = i;
                console.log(number);
                pages.push({
                    label: number,
                    page: number,
                    isCurrentPage: self.pageIndex() === number,
                });
            }
            if (maxIndex < self.pageCount()) {
                pages.push({
                    label: "...",
                    page: maxIndex + 1,
                    isCurrentPage: false,
                });
            }
            return pages;
        });

        self.isFirstPage = ko.computed(function () {
            return self.pageIndex() == 1;
        });
        self.hasPreviousPage = ko.computed(function () {
            return self.pageIndex() > 1;
        });
        self.isLastPage = ko.computed(function () {
            return self.pageIndex() == self.pageCount();
        });
        self.hasNextPage = ko.computed(function () {
            return self.pageIndex() < self.pageCount();
        });

        self.firstPage = function () {
            self.pageIndex(1);
        };
        self.previousPage = function () {
            if (self.pageIndex() > 1)
                self.pageIndex(self.pageIndex() - 1);
        };
        self.lastPage = function () {
            self.pageIndex(self.pageCount());
        };
        self.nextPage = function () {
            if (self.pageIndex() < self.pageCount())
                self.pageIndex(self.pageIndex() + 1);
        };

        self.setPage = function (newPage) {
            self.pageIndex(newPage);
        };

        self.refresh = function () {
            if (typeof self.refreshCallback === "function")
                self.refreshCallback();
        }
    },
    template: { element: 'grid-pager-template' },
});

