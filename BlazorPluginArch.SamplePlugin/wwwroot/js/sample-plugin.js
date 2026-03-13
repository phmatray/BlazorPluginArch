// Sample Plugin JavaScript module
// This file is served from: _content/BlazorPluginArch.SamplePlugin/js/sample-plugin.js

/**
 * Shows a browser alert with the given message.
 * @param {string} message - The message to display.
 */
export function showAlert(message) {
    alert(message);
}

/**
 * Returns the current browser window dimensions.
 * @returns {{ width: number, height: number }}
 */
export function getWindowSize() {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
}
