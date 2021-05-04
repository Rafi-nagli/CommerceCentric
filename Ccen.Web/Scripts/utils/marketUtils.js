var marketUtils = (function () {
    var allMarkets = {
        none: 0,
        amazon: 1,
        amazonUk: 3,
        eBay: 2,
        magento: 4,
        walmart: 5,
        jet: 7,
        shopify: 6,
        walmartCA: 8,
    }

    var allMarketplaceIds = {
        amazonCom: 'ATVPDKIKX0DER',
        amazonCa: 'A2EUQ1WTGCTBG2',
        amazonMx: 'A1AM78C64UM0Y8',

        amazonUk: 'A1F83G8C2ARO7P',
        amazonDe: 'A1PA6795UKMFR9',
        amazonEs: 'A1RKKUPIHCS9HS',
        amazonFr: 'A13V1IB3VIYZZH',
        amazonIt: 'APJ6JRA9NG5V4',
    }

    return {
        getAllMarkets: function() {
            return allMarkets;
        },
        getAllMarketplaceIds: function() {
            return allMarketplaceIds;
        },
        hasLinkQty: function(marketplaceId) {
            return marketplaceId == allMarketplaceIds.amazonCa
                || marketplaceId == allMarketplaceIds.amazonMx
                || marketplaceId == allMarketplaceIds.amazonDe
                || marketplaceId == allMarketplaceIds.amazonEs
                || marketplaceId == allMarketplaceIds.amazonFr
                || marketplaceId == allMarketplaceIds.amazonIt;
        }
    };
})();
