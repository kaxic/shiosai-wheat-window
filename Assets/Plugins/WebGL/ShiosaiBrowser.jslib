mergeInto(LibraryManager.library, {
    ShiosaiSync: function () {
        if (Module.shiosaiWriting) { Module.shiosaiDirty = true; return; }
        function flush() {
            Module.shiosaiWriting = true;
            Module.shiosaiDirty = false;
            FS.syncfs(false, function (error) {
                Module.shiosaiWriting = false;
                if (error) console.warn("Save sync", error);
                if (Module.shiosaiDirty) flush();
                else if (Module.shiosaiClosing) location.reload();
            });
        }
        flush();
    },
    ShiosaiImage: function (pointer) {
        var path = UTF8ToString(pointer);
        window.shiosaiPictures([{name:path.split('/').pop(), bytes:FS.readFile(path)}]);
    },
    ShiosaiAlbum: function (pointer) {
        var dir = UTF8ToString(pointer);
        var names = FS.readdir(dir).filter(function(n){return /\.png$/i.test(n);}).sort().reverse();
        window.shiosaiPictures(names.map(function(n){return {name:n,bytes:FS.readFile(dir+'/'+n)};}));
    },
    ShiosaiExit__deps: ['ShiosaiSync'],
    ShiosaiExit: function () { Module.shiosaiClosing = true; _ShiosaiSync(); }
});
