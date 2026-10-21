// Lets the user pick a folder (e.g. C:\data) and returns the text of every .csv file in it.
export async function pickFolderCsvs() {
    if (!window.showDirectoryPicker) {
        throw new Error("This browser cannot open folders. Please use Chrome or Edge on desktop.");
    }

    const dir = await window.showDirectoryPicker({ mode: "read" });
    const files = [];
    for await (const [name, handle] of dir.entries()) {
        if (handle.kind === "file" && name.toLowerCase().endsWith(".csv")) {
            const file = await handle.getFile();
            files.push({ name: name.replace(/\.csv$/i, ""), text: await file.text() });
        }
    }
    return files;
}
