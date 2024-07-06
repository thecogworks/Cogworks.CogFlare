angular.module("umbraco.resources").factory("cogflareResources", function ($http) {

    var API_ROOT = '/umbraco/backoffice/api/CloudFlareCachePurgeApi/';

    return {
        purgeCache: function () {
            return $http.get(API_ROOT + 'PurgeCache');
        },
        getSettings: function () {
            return $http.get(API_ROOT + 'GetSettings');
        }
    };
});