class SaveResult {
    constructor({ success, message, id, statusCode }) {
        this.success = success ?? false;
        this.message = message ?? '';
        this.id = id ?? null;
        this.statusCode = statusCode ?? 500;
    }

    static fromResponse(data) {
        return new SaveResult(data || {});
    }

    isSuccess() {
        return this.success === true;
    }

    hasId() {
        return this.id !== null && this.id !== undefined;
    }
}

async function postFromBody(url, data) {
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        const json = await response.json();
        const result = SaveResult.fromResponse(json);

        await showMessageBox(result.message, false);

        return result;
    } catch (err) {
        await showMessageBox('Erro de comunicação com o servidor.', false);
        return new SaveResult({
            success: false,
            message: 'Erro de comunicação com o servidor.',
            id: null,
            statusCode: 500
        });
    }
}

async function deleteFromUrl(url) {
    try {
        const response = await fetch(url, { method: 'DELETE' });
        const json = await response.json();
        const result = SaveResult.fromResponse(json);

        await showMessageBox(result.message, false);

        return result;
    } catch (err) {
        await showMessageBox('Erro de comunicação com o servidor.', false);

        return new SaveResult({
            success: false,
            message: 'Erro de comunicação com o servidor.',
            id: null,
            statusCode: 500
        });
    }
}

async function uploadExcelStatement(file, statementTypeId) {
    const formData = new FormData();

    formData.append("file", file);
    formData.append("statementTypeId", statementTypeId);

    try {
        const response = await fetch('/Statements/import', {
            method: 'POST',
            body: formData
        });

        const json = await response.json();
        const result = SaveResult.fromResponse(json);

        await showMessageBox(result.message, false);

        return result;
    } catch (err) {
        await showMessageBox('Erro de comunicação com o servidor.', false);

        return new SaveResult({
            success: false,
            message: 'Erro de comunicação com o servidor.',
            id: null,
            statusCode: 500
        });
    }
}

async function getFromUrl(url) {
    try {
        const response = await fetch(url);

        if (!response.ok) {
            throw new Error(`Erro ao buscar dados. Status: ${response.status}`);
        }

        const data = await response.json();

        return data;
    } catch (err) {
        await showMessageBox('Erro ao buscar dados do servidor.', false);
       
        return null;
    }
}