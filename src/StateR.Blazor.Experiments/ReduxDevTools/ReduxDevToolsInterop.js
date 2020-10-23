window.__StateRDevTools__ = (function () {
    const reduxDevTools = window.__REDUX_DEVTOOLS_EXTENSION__;
    if (typeof reduxDevTools === 'undefined') {
        return {
            init: function () {},
            dispatch: function (action, state) {},
        };
    }

    const statorDevTools = reduxDevTools.connect({ name: 'StatoR' });
    if (typeof statorDevTools === 'undefined') {
        return {
            init: function () {},
            dispatch: function (action, state) {},
        };
    }
    statorDevTools.subscribe((message) => {
        if (window.statorDevToolsDotNetInterop) {
            const messageAsJson = JSON.stringify(message);
            window.statorDevToolsDotNetInterop.invokeMethodAsync('DevToolsCallback', messageAsJson);
        }
    });

    return {
        init: function (dotNetCallbacks, state) {
            window.statorDevToolsDotNetInterop = dotNetCallbacks;
            statorDevTools.init(state);
            if (window.statorDevToolsDotNetInterop) {
                // Notify Stator of the presence of the browser plugin
                const detectedMessage = {
                    payload: {
                        type: 'detected',
                    },
                };
                const detectedMessageAsJson = JSON.stringify(detectedMessage);
                window.statorDevToolsDotNetInterop.invokeMethodAsync('DevToolsCallback', detectedMessageAsJson);
            }
        },
        dispatch: function (action, state) {
            action = JSON.parse(action);
            statorDevTools.send(action, state);
        },
    };
})();
