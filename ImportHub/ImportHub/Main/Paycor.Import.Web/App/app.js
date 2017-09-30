(function () {
    console.log('navigator.userAgent', navigator.userAgent);
    var
        lib = window.lib,
        isIE = navigator.userAgent.indexOf('MSIE'),
        importId,
        importPage,
        summaryPage,
        infoPage,
        errorPage,
        errorBanner,
        errorParseBanner,
        infoText,
        infoProgress,
        doneBtn,
        backBtn,
        downloadBtn,
        warningBanner,
        percentComplete,
        noErrors,
        grid,
        modelNode,
        timer,
        xhrRequest,
        importedRecordCount,
        errorRecordCount,
        importInitialized = false,
        summaryInitialized = false,
        POLL_TIME = 1000,
        statusTextDefault = 'Uploading file',
        hasErrorMessage = 'There was a problem with the file. Please fix any errors and try again.<br><br>Error: ',
        lastStatus,
        status = {
            0: 'Initiate Employee Import ',
            2: 'Retrieve Validation Results',
            1: 'Import File Data',
            3: 'Validate Record Types',
            4: 'Validate Data Types',
            5: 'Validate General Record',
            6: 'Validate Static Record',
            7: 'Validate Earning Record',
            8: 'Validate Deduction Record',
            9: 'Validate Benefit Record',
            10: 'Validate Direct Deposit Record',
            11: 'Validate Rate Record',
            12: 'Validate Tax Record',
            13: 'Employee Import Update General',
            14: 'Employee Import Update Group Term Life Earnings',
            15: 'Employee Import Update Earnings',
            16: 'Employee Import Update Default Earnings',
            17: 'Employee Import Update Deductions',
            18: 'Employee Import Update Taxes',
            19: 'Employee Import Update Direct Deposit',
            20: 'Employee Import Update Rates',
            21: 'Employee Import Update Benefits',
            22: 'Employee Import Update Time and Attendance',
            23: 'Clear Import Tables',
            24: 'Employee Import Complete',
            98: 'Queued',
            99: 'Prevalidation Failure',
            100: 'Processing Failure' // server communication error           
        },
        COMPLETE = 24,
        VALIDATION_ERROR = 99,
        SERVER_ERROR = 100,
        columns = [ {
            key: 'id',
            label: 'Employee Number'
        }, {
            key: 'name',
            label: 'Employee Name'
        },
        //{
         //   key: 'rowNumber',
         //   label: 'Row'
        //}, 
        {
            key: 'issue',
            label: 'Issue',
            css: 'no-sort'
        }, {
            key: 'status',
            label: '',
            css: 'no-sort'
        }],
        data;

    function parseSummaryData(summary) {
        var
            list = [];

        summary.forEach(function (item) {
            var row = {
                id: item.employeeNumber,
                name: item.employeeName,
                rowNumber: item.rowNumber || 0

            };
            if (item.recordUploaded) {
                row.issue = '<div class="icon-warn yellow list-item">' + item.issue + '</div>';
            } else {
                row.issue = '<div class="icon-error red list-item">' + item.issue + '</div>';
                row.status = 'This record cannot be imported';
            }
            list.push(row);
        });

        return {
            columns: columns,
            items: list
        };
    }

    function initDomNodes() {
        importPage = dom.byId('import-page');
        uploadBtn = dom.query(importPage, '.icon-ok');
        cancelBtn = dom.query(importPage, '.icon-remove');
        downloadBtn = dom.byId('download-btn');
        doneBtn = dom.byId('done-btn');
        backBtn = dom.byId('back-btn');
        drop = dom.query('drop-down');
        uploader = dom.query('dnd-upload');

        summaryPage = dom.byId('summary-page');
        errorBanner = dom.byId('errorBanner');
        errorParseBanner = dom.byId('error-parse-banner');
        warningBanner = dom.byId('warningBanner');
        noErrors = dom.byId('no-errors');
        grid = dom.byId('summary-grid');
        modelNode = dom.query(grid, 'data-grid-model');
        importedRecordCount = dom.byId('importedRecordCount');
        errorRecordCount = dom.byId('errorRecordCount');

        errorPage = dom.byId('error-page');
        infoPage = dom.byId('info-page');
        infoText = dom.byId('status-text');
        infoProgress = dom.query(infoPage, 'progress-bar');
        percentComplete = dom.byId('percent-complete');

        window.testing = {
            importPage: importPage,
            summaryPage: summaryPage,
            infoPage: infoPage,
            errorPage: errorPage,
            uploader: uploader,
            doneButton: doneBtn,
            backButton: backBtn, 
            uploadButton: uploadBtn,
            cancelButton: cancelBtn,
            downloadButton: downloadBtn,
            errorBanner: errorBanner,
            errorParseBanner:  errorParseBanner,
            warningBanner: warningBanner,
            infoProgress: infoProgress,
            percentComplete: percentComplete,
            infoText: infoText,
            noErrors: noErrors,
            grid: grid,
            importedRecordCount: importedRecordCount,
            errorRecordCount:  errorRecordCount,
            getModal: function () {
                var modal = dom.query('modal-dialog');
                return Array.isArray(modal) || !modal ? null : modal;
            },
            closeModal: function () {
                lib.dialog.close();
            },
            showingLoader: function () {
                var loader = dom.query('loading-indicator');
                return Array.isArray(loader) || !loader ? null : loader;
            },
            browse: function(){
                drop.value = 'ddl_newempimp';
                setTimeout(function () { uploader.fileBrowser.fileInput.click();  }, 300);
                
            },
            test: function () {
                drop.value = 'ddl_newempimp';
                uploader.setTestFile();
                uploadBtn.click();

            }
        };

        importPage.style.display = 'none';
        summaryPage.style.display = 'none';
        errorPage.style.display = 'none';
        infoPage.style.display = 'none';

        dom.byId('pages').style.display = 'block';

        on(backBtn, 'click', function () {
            location.hash = '';
        });

        on(cancelBtn, 'click', function () {
            reset();
        });

        cancelBtn.disabled = true;
    }

    function initImportPage() {
        
        var
            file;

        showPage('import');
        reset();
        
        if (!importInitialized) {
            on(drop, 'change', function (e) {
                // because you can't select "none", any change should show the upload
                uploader.style.display = '';
                cancelBtn.disabled = false;
            });

            on(uploader, 'change', function (e) {
                if (e.detail.value) {
                    uploadBtn.disabled = false;
                } else {
                    uploadBtn.disabled = true;
                }
                file = e.detail.value;
            });

            on(uploadBtn, 'click', function () {
                var indicator = lib.loading.show('Uploading file...', true);
                if (window.badURL) {
                    window.globalWebApiUrl += 'X';
                }
                if (file) {

                    xhr.upload(
                        window.globalWebApiUrl, {
                            file: file,
                            importType: drop.value
                        },
                        function (result) {
                            console.log('result', result);
                            importId = result.response.replace(/"/g, '');
                            location.hash = 'importId='+importId;
                            
                        },
                        function (err) {
                            console.error('FAIL', JSON.stringify(err));
                            lib.loading.hide();
                            data = err;
                            initErrorPage();
                        }, function (e) {
                            console.log('progress', e);
                            indicator.value = e.percent;
                        });
                } else {
                    console.log('no file to upload');
                }
            });
        } else {
            reset();
        }
        importInitialized = true;
    }

    function reset() {
        drop.reset();
        uploader.reset();
        abortPing();
        uploader.style.display = 'none';
        cancelBtn.disabled = true;
        uploadBtn.disabled = true;
        infoText.innerHTML = statusTextDefault;
        infoProgress.value = 0;
        percentComplete.innerHTML = '0%';
        lib.loading.hide();
        lib.dialog.close();
        importId = null;
        data = null;
    }

    function ping() {
        if (!importId) {
            console.log('no importId. ping.aborted');
            initImportPage();
            return;
        }
        timer = setTimeout(function () {
            xhrRequest = xhr(window.globalWebApiUrl + '/' + importId, {
                callback: function (result) {
                    
                    lib.loading.hide();
                    data = result;
                    if (data.status === COMPLETE) {
                        initSummaryPage();
                    }
                    else if(data.status === VALIDATION_ERROR || data.status === SERVER_ERROR){
                        initErrorPage();
                    }
                    else {
                        initProgressPage();
                        ping();  
                    }
                    xhrRequest = null;

                },
                errback: function (e) {
                    // TODO: Kill process to simulate 500
                    // kill internet to simulate timeout
                    // Currently, it catches a 401
                    xhrRequest = null;
                    data = e;
                    initErrorPage();
                }

            });
        }, POLL_TIME);
    }

    function abortPing() {
        console.log('abort the ping!');
        if (xhrRequest) {
            xhrRequest.abort();
        }
        clearTimeout(timer);
    }

    function getMessage() {
        if (!data) {
            return 'Retrieving status';
        }
        var msg = status[data.status];
        if (!msg) {
            return 'unknown status';
        }
        return msg;
    }

    function initProgressPage() {
        showPage('info');
        var
            percent = 0,
            msg = getMessage();
        if (msg === lastStatus) {
            return;
        }
        if (data) {
            console.log('ping.result', data.status, data.message, data);
        }
        lastStatus = msg;
        infoText.innerHTML = msg;
        if (data && data.currentStep) {
            percent = Math.round(data.currentStep / data.totalSteps * 100);
        }
        infoProgress.value = percent;
        percentComplete.innerHTML = percent + '%';
        
    }

    function initErrorPage() {
        abortPing();
        var
            msg = hasErrorMessage + data.message,
            modal;
        console.log('data.message', data.message);

        if (data.message.indexOf('<') === 0) {
            // an error page, probably a 404
            msg = 'Server error: "Page not found (404)"';
        }
        modal = lib.dialog.error('Upload Error', msg);

        modal.on('action', function () {
            console.log('close error dialog');
            lib.loading.hide();
            reset();
            location.hash = '';
        });
    }

    function initSummaryPage() {
        if (data) {
            console.log('ping.result', data.status, data.message, data);
        }
        var
            tableData = parseSummaryData(data.statusDetails);

        if (!data.successRecordsCount && !data.failRecordsCount) {
            data = {
                message: 'Nothing imported. Please use one of the templates for importing.'
            }
            initErrorPage();
            return;
        }
        showPage('summary');

        importedRecordCount.innerHTML = data.successRecordsCount;
        errorRecordCount.innerHTML = data.failRecordsCount;

        if (data.warnRecordCount) {
            warningBanner.style.display = '';
            warningBanner.innerHTML = data.warnRecordCount + ' record(s) have some potential problems';
        } else {
            warningBanner.style.display = 'none';
        }

        if (data.failRecordsCount) {
            errorBanner.style.display = '';
            errorBanner.innerHTML = data.failRecordsCount + ' record(s) contain errors and could not be imported';
        } else {
            errorBanner.style.display = 'none';
        }

        if (tableData.items.length) {
            noErrors.style.display = 'none';
            grid.style.display = '';
            if (!modelNode.DOMSTATE || modelNode.DOMSTATE === 'created') {
                modelNode.init = function () {
                    modelNode.setModel(tableData);
                };
            } else {
                modelNode.setModel(tableData);
            }
        }
        else {
            noErrors.style.display = '';
            grid.style.display = 'none';
        }

        if (data.resultsDownloadLink) {
            downloadBtn.disabled = false;
            downloadBtn.href = data.resultsDownloadLink;
        } else {
            downloadBtn.disabled = true;
        }
    }

    function showPage(which) {
        errorPage.style.display = 'none';
        infoPage.style.display = 'none';
        summaryPage.style.display = 'none';
        importPage.style.display = 'none';
        switch (which) {
            case 'summary': summaryPage.style.display = 'block'; break;
            case 'info': infoPage.style.display = 'block'; break;
            case 'error': errorPage.style.display = 'block'; break;
            case 'import': importPage.style.display = 'block'; break;
        }
    }

    function togglePages() {
        //var isLoggingIn = dom.query('div.Grid');
        if (data) {
            abortPing();
            initSummaryPage();
        } else if (importId) {
            initProgressPage();
            ping();
        } else {
            abortPing();
            initImportPage();
        }
    }

    function checkUrl() {
        var hash = location.hash.replace('#', '');
        if (hash) {
            importId = hash.split('=')[1];
        }
        else {
            importId = null;
            data = null;
        }
        console.log('CHANGE PAGE', hash, 'data', data);
        togglePages();
    }

    document.addEventListener('WebComponentsReady', function () {
        initDomNodes();
        checkUrl();
    });

    window.addEventListener('hashchange', function (event) {
        checkUrl();
    });

}());