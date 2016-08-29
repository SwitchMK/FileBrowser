(function() {
    "use strict";

    angular
        .module("fileBrowserApp")
        .controller("fileBrowserController", fileBrowserController);

    fileBrowserController.$inject = ["$q", "$scope", "fileBrowserService", "toaster"];

    function fileBrowserController($q, $scope, fileBrowserService, toaster) {
        $scope.filesAndDirectories = [];

        var tree;
        $scope.myTree = tree = {};
        $scope.myData = [];
        $scope.amount = [0, 0, 0];
        var canceller;

        $scope.getLogicalDrives = function() {
            fileBrowserService.getLogicalDrives()
                .success(function(logicalDrives) {
                    logicalDrives.forEach(function(drive) {
                        var model = {
                            'label': drive.name,
                            'data': {
                                'path': drive.path
                            }
                        };

                        $scope.myData.push(model);
                    });

                    $scope.filesAndDirectories = logicalDrives;
                });
        };

        $scope.refreshDirectories = function() {
            $scope.filesAndDirectories.splice(0);
            $scope.myData.splice(0);

            $scope.currentDirectory = "";
            $scope.parsedCurrentDirectory = "";

            $scope.getLogicalDrives();
        };

        $scope.getFilesInDirectory = function(path) {
            var request = {
                path: JSON.stringify(path)
            };

            $scope.currentDirectory = request.path;
            $scope.parsedCurrentDirectory = JSON.parse($scope.currentDirectory);

            getFilesInDirectory(request.path);
        };

        $scope.getDirectories = function(branch) {
            var request = {
                path: JSON.stringify(branch.data.path)
            };

            $scope.currentDirectory = request.path;
            $scope.parsedCurrentDirectory = JSON.parse($scope.currentDirectory);

            getFilesInDirectory(request.path);

            if (branch.children.length > 0)
                return;

            fileBrowserService.getDirectories(request.path)
                .success(function(directories) {
                    directories.forEach(function(directory) {
                        var model = {
                            'label': directory.name,
                            'data': {
                                'path': directory.path
                            }
                        };

                        $scope.lessThanTen = directory.less10coun;

                        tryAddABranch(model);
                    });
                });
        };

        $scope.goTopDirectory = function() {
            fileBrowserService.goTopDirectory($scope.currentDirectory)
                .success(function(directory) {
                    var request = {
                        path: JSON.stringify(directory.path)
                    };

                    $scope.currentDirectory = request.path;
                    $scope.parsedCurrentDirectory = JSON.parse($scope.currentDirectory);

                    getFilesInDirectory(request.path);
                });
        };

        var getAmountOfFiles = function () {
            var canceller = $q.defer();

            $scope.getAmountOfFilesPromise = fileBrowserService.getAmountOfFiles($scope.currentDirectory, canceller)
                .success(function(amount) {
                    $scope.amount = amount;
                });

            return canceller;
        };

        var getFilesInDirectory = function (path) {
            if (canceller)
                canceller.resolve();

            $scope.amount = [0, 0, 0];

            $scope.getGetFilesInDirectoryPromise = fileBrowserService.getFilesInDirectory(path)
                .success(function (files) {
                    if (files) {
                        $scope.filesAndDirectories = files;
                    } else {
                        toaster.pop('error', "Error", "Access denied!");
                    }
                    canceller = getAmountOfFiles();
                });
        };

        var tryAddABranch = function(model) {
            var branch = tree.get_selected_branch();
            return tree.add_branch(branch, model);
        };
    }
})();