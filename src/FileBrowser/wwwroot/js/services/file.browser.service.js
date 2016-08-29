(function () {
    'use strict';

    angular
        .module('fileBrowserApp')
        .service('fileBrowserService', fileBrowserService);

    fileBrowserService.$inject = ['$http'];

    function fileBrowserService($http) {
        var url = "/api/FileBrowser";

        this.getLogicalDrives = function () {
            return $http.get(url + "/GetLogicalDrives");
        };

        this.getDirectories = function (data) {
            return $http({
                method: 'POST',
                url: url + "/GetDirectories",
                data: data,
                headers: { 'Content-Type': 'application/json' }
            });
        };

        this.getFilesInDirectory = function (data) {
            return $http({
                method: 'POST',
                url: url + "/GetFilesInDirectory",
                data: data,
                headers: { 'Content-Type': 'application/json' }
            });
        };

        this.goTopDirectory = function (data) {
            return $http({
                method: 'POST',
                url: url + "/GoTopDirectory",
                data: data,
                headers: { 'Content-Type': 'application/json' }
            });
        };

        this.getAmountOfFiles = function (data, canceller) {
            return $http({
                method: 'POST',
                url: url + "/GetAmountOfFiles",
                data: data,
                headers: { 'Content-Type': 'application/json' },
                timeout: canceller.promise 
            });
        };
    }
})();