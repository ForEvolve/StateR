// Source based on https://github.com/mrpmorris/Fluxor/blob/master/Source/Fluxor.Blazor.Web.ReduxDevTools/ReduxDevToolsInterop.cs
'use strict';

window.__StateRDevTools__ = (function () {
    var reduxDevTools = window.__REDUX_DEVTOOLS_EXTENSION__;
    if (typeof reduxDevTools === 'undefined') {
        return {
            init: function init() {},
            dispatch: function dispatch(action, state) {}
        };
    }

    var statorDevTools = reduxDevTools.connect({ name: 'StatoR' });
    if (typeof statorDevTools === 'undefined') {
        return {
            init: function init() {},
            dispatch: function dispatch(action, state) {}
        };
    }
    statorDevTools.subscribe(function (message) {
        if (window.statorDevToolsDotNetInterop) {
            var messageAsJson = JSON.stringify(message);
            window.statorDevToolsDotNetInterop.invokeMethodAsync('DevToolsCallback', messageAsJson);
        }
    });

    return {
        init: function init(dotNetCallbacks, state) {
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
        },
        dispatch: function dispatch(action, state) {
            action = JSON.parse(action);
            statorDevTools.send(action, state);
        }
    };
})();

