export async function downloadFileFromBytes(fileName, pData, length) {
    var arrayBuffer = new Uint8ClampedArray(Module.HEAPU8.buffer, pData, length);
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}
