/**
 * BaseFetch - Utility class to centralize and standardize AJAX calls using the Fetch API.
 */
class BaseFetch {
    /**
     * Performs a generic HTTP request.
     * @param {string} url - Destination URL.
     * @param {object} options - Additional fetch options (method, headers, body, etc.).
     * @returns {Promise<object>} A promise resolving to the server response or a default { success: false, message: string } object.
     */
    static async request(url, options = {}) {
        const defaults = { headers: {} };

        if (options.body && typeof options.body === 'object' && !(options.body instanceof FormData)) {
            defaults.headers['Content-Type'] = 'application/json';

            options.body = JSON.stringify(options.body);
        }

        const mergedHeaders = { ...defaults.headers, ...options.headers };
        const mergedOptions = { ...defaults, ...options, headers: mergedHeaders };

        try {
            const response = await fetch(url, mergedOptions);
            const contentType = response.headers.get('content-type');
            
            let data;

            if (contentType && contentType.includes('application/json')) {
                data = await response.json();
            } else {
                data = await response.text();
            }

            if (!response.ok) {
                const errorMsg = (data && typeof data === 'object') 
                    ? (data.message || data.error) 
                    : (data || `HTTP Error: ${response.status}`);
                
                return {
                    success: false,
                    message: errorMsg || `Network communication failed (${response.status})`
                };
            }

            if (typeof data === 'string') {
                return {
                    success: true,
                    data: data
                };
            }

            return data;
        } catch {
            return {
                success: false,
                message: 'Network error or unable to connect to the server.'
            };
        }
    }

    /**
     * Performs a GET request.
     * @param {string} url - Destination URL.
     * @param {object} headers - Additional HTTP headers.
     * @returns {Promise<object>}
     */
    static async get(url, headers = {}) {
        return this.request(url, { method: 'GET', headers });
    }

    /**
     * Performs a POST request.
     * @param {string} url - Destination URL.
     * @param {any} body - Request payload.
     * @param {object} headers - Additional HTTP headers.
     * @returns {Promise<object>}
     */
    static async post(url, body, headers = {}) {
        return this.request(url, { method: 'POST', body, headers });
    }

    /**
     * Performs a PUT request.
     * @param {string} url - Destination URL.
     * @param {any} body - Request payload.
     * @param {object} headers - Additional HTTP headers.
     * @returns {Promise<object>}
     */
    static async put(url, body, headers = {}) {
        return this.request(url, { method: 'PUT', body, headers });
    }

    /**
     * Performs a DELETE request.
     * @param {string} url - Destination URL.
     * @param {object} headers - Additional HTTP headers.
     * @returns {Promise<object>}
     */
    static async delete(url, headers = {}) {
        return this.request(url, { method: 'DELETE', headers });
    }
}

/**
 * Bind global events on DOM ready.
 */
document.addEventListener('DOMContentLoaded', () => {
    document.body.addEventListener('input', function(e) {
        if (e.target && e.target.classList.contains('decimal-input')) {
            const cursorPosition = e.target.selectionStart;
            const originalLength = e.target.value.length;
            
            const sanitized = sanitizeDecimalInput(e.target.value);

            e.target.value = sanitized;
            
            const newLength = sanitized.length;
            const positionAdjustment = newLength - originalLength;

            e.target.setSelectionRange(cursorPosition + positionAdjustment, cursorPosition + positionAdjustment);
        }
    });

    /**
     * Sanitizes inputs to format as a decimal number.
     * @param {string} value - The raw input value.
     * @returns {string} The sanitized decimal string.
     */
    function sanitizeDecimalInput(value) {
        if (!value) return '';

        const hasMinus = value.includes('-');

        let cleaned = value.replace(/[^0-9,]/g, '');

        const parts = cleaned.split(',');

        if (parts.length > 2) {
            cleaned = parts[0] + ',' + parts.slice(1).join('');
        }

        const finalParts = cleaned.split(',');

        if (finalParts.length === 2) {
            let decimals = finalParts[1];

            if (decimals.length > 2) {
                decimals = decimals.substring(0, 2);
            }

            cleaned = finalParts[0] + ',' + decimals;
        }

        return (hasMinus ? '-' : '') + cleaned;
    }
});