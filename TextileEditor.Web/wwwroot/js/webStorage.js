export class LocalStorage {
    constructor() { }

    static create() {
        return new LocalStorage();
    }

    setItem(key, value) {
        localStorage.setItem(key, value);
    }

    getItem(key) {
        return localStorage.getItem(key);
    }

    removeItem(key) {
        localStorage.removeItem(key);
    }

    clear() {
        localStorage.clear();
    }
}