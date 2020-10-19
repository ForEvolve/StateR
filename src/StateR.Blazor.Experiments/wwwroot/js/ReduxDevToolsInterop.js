// Source based on https://github.com/mrpmorris/Fluxor/blob/master/Source/Fluxor.Blazor.Web.ReduxDevTools/ReduxDevToolsInterop.cs
'use strict';

window.__StateRDevTools__ = new function () {
    var _this = this;

    var reduxDevTools = window.__REDUX_DEVTOOLS_EXTENSION__;
    this.init = function () {};
    if (reduxDevTools) {
        (function () {
            var statorDevTools = reduxDevTools.connect({ name: 'StatoR' });
            if (statorDevTools) {
                statorDevTools.subscribe(function (message) {
                    if (window.statorDevToolsDotNetInterop) {
                        var messageAsJson = JSON.stringify(message);
                        window.statorDevToolsDotNetInterop.invokeMethodAsync('DevToolsCallback', messageAsJson);
                    }
                });
            }
            _this.init = function (dotNetCallbacks, state) {
                window.statorDevToolsDotNetInterop = dotNetCallbacks;
                statorDevTools.init(state);
                if (window.statorDevToolsDotNetInterop) {
                    // Notify StatoR of the presence of the browser plugin
                    var detectedMessage = {
                        payload: {
                            type: 'detected'
                        }
                    };
                    var detectedMessageAsJson = JSON.stringify(detectedMessage);
                    window.statorDevToolsDotNetInterop.invokeMethodAsync('DevToolsCallback', detectedMessageAsJson);
                }
            };
            _this.dispatch = function (action, state) {
                action = JSON.parse(action);
                statorDevTools.send(action, state);
            };
        })();
    }
}();

