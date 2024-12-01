export class FileDownloader {
    constructor() {
        this.urls = new Set();
    }

    static create() {
        return new FileDownloader();
    }

    createBlobUrl(pData, length) {
        const arrayBuffer = new Uint8ClampedArray(Module.HEAPU8.buffer, pData, length);
        const blob = new Blob([arrayBuffer]);
        const url = URL.createObjectURL(blob);
        this.urls.add(url);
        return url;
    }

    downloadFile(fileName, url) {
        const anchorElement = document.createElement('a');
        anchorElement.href = url;
        anchorElement.download = fileName ?? '';
        anchorElement.click();
        anchorElement.remove();
    }

    revokeUrl(url) {
        if (this.urls.has(url)) {
            URL.revokeObjectURL(url);
            this.urls.delete(url);
        }
    }

    clear() {
        for (const url of this.urls) {
            URL.revokeObjectURL(url);
        }
        this.urls.clear();
    }

    async downloadFileFromBytes(fileName, pData, length) {
        const url = this.createBlobUrl(pData, length);
        this.downloadFile(fileName, url);
        this.revokeUrl(url);
    }
}