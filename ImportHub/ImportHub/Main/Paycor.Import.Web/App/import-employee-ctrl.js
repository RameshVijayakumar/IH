var app = angular.module('appModule', ['ngFileUpload']);

app.controller("ImportEmployeeController", function ($scope) {
    $scope.headingCaption = "Import Employee Data";

    $scope.importTypeLists = [
        { id: 'ddl_selone', name: '--Select One--' },
        { id: 'ddl_newempimp', name: 'New Employee Import' }
    ];
    $scope.selectedItem = $scope.importTypeLists[0];
});

app.controller("FileUploadController", [
    '$scope', 'Upload', function ($scope, $upload) {

        $scope.fileSelected = function(files) {
            if (files.length) {
                $scope.$evalAsync(function() {
                    $scope.uploadMode = true;
                });
            } else {
                $scope.$evalAsync(function() {
                    $scope.uploadMode = false;
                });
            }
        };

        $scope.getDisableStatus = function() {
            if ($scope.uploadMode) {
                return false;
            } else {
                return true;
            }
        }

        $scope.uploadFiles = function (files, webapiurl) {
            $scope.loading = true;
            $scope.fileuploadurl = webapiurl;

            if (files && files.length) {
                for (var i = 0; i < files.length; i++) {
                    var file = files[i];
                    $upload.upload({
                        url: webapiurl,
                        method: 'POST',
                        file: file
                        }).progress(function(evt) {
                            var progressPercentage = parseInt(100.0 * evt.loaded / evt.total);
                            $scope.log = 'progress: ' + progressPercentage + '% ' +
                                evt.config.file.name + '\n' + $scope.log;
                        }).success(function(data, status, headers, config) {
                            console.log('success:' + status);
                    }).error(function(data, status, headers, config) {
                            console.log('Error:status=' + status + ',filename=' + config.file.name);
                        }).finally(function () {
                            $scope.loading = false;
                        $scope.uploadMode = false;
                    });
                }
            }
        };

        $scope.abortUpload = function () {
            $upload.abortUpload();
            $scope.loading = false;
        }
    }
]);
