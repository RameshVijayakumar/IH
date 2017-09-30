/**
 * @license
 * Copyright (c) 2014 The Polymer Project Authors. All rights reserved.
 * This code may only be used under the BSD style license found at http://polymer.github.io/LICENSE.txt
 * The complete set of authors may be found at http://polymer.github.io/AUTHORS.txt
 * The complete set of contributors may be found at http://polymer.github.io/CONTRIBUTORS.txt
 * Code distributed by Google as part of the polymer project is also
 * subject to an additional IP rights grant found at http://polymer.github.io/PATENTS.txt
 */

// https://github.com/webcomponents/webcomponents-lite
// @version 0.6.0-58c8709
window.WebComponents = window.WebComponents || {};

(function (scope) {
    var flags = scope.flags || {};
    var file = "webcomponents.js";
    var script = document.querySelector('script[src*="' + file + '"]');
    if (!flags.noOpts) {
        location.search.slice(1).split("&").forEach(function (o) {
            o = o.split("=");
            o[0] && (flags[o[0]] = o[1] || true);
        });
        if (script) {
            for (var i = 0, a; a = script.attributes[i]; i++) {
                if (a.name !== "src") {
                    flags[a.name] = a.value || true;
                }
            }
        }
        if (flags.log) {
            var parts = flags.log.split(",");
            flags.log = {};
            parts.forEach(function (f) {
                flags.log[f] = true;
            });
        } else {
            flags.log = {};
        }
    }
    flags.shadow = flags.shadow || flags.shadowdom || flags.polyfill;
    if (flags.shadow === "native") {
        flags.shadow = false;
    } else {
        flags.shadow = flags.shadow || !HTMLElement.prototype.createShadowRoot;
    }
    if (flags.register) {
        window.CustomElements = window.CustomElements || {
            flags: {}
        };
        window.CustomElements.flags.register = flags.register;
    }
    scope.flags = flags;
})(WebComponents);

(function (scope) {
    "use strict";
    var hasWorkingUrl = false;
    if (!scope.forceJURL) {
        try {
            var u = new URL("b", "http://a");
            u.pathname = "c%20d";
            hasWorkingUrl = u.href === "http://a/c%20d";
        } catch (e) { }
    }
    if (hasWorkingUrl) return;
    var relative = Object.create(null);
    relative["ftp"] = 21;
    relative["file"] = 0;
    relative["gopher"] = 70;
    relative["http"] = 80;
    relative["https"] = 443;
    relative["ws"] = 80;
    relative["wss"] = 443;
    var relativePathDotMapping = Object.create(null);
    relativePathDotMapping["%2e"] = ".";
    relativePathDotMapping[".%2e"] = "..";
    relativePathDotMapping["%2e."] = "..";
    relativePathDotMapping["%2e%2e"] = "..";
    function isRelativeScheme(scheme) {
        return relative[scheme] !== undefined;
    }
    function invalid() {
        clear.call(this);
        this._isInvalid = true;
    }
    function IDNAToASCII(h) {
        if ("" == h) {
            invalid.call(this);
        }
        return h.toLowerCase();
    }
    function percentEscape(c) {
        var unicode = c.charCodeAt(0);
        if (unicode > 32 && unicode < 127 && [34, 35, 60, 62, 63, 96].indexOf(unicode) == -1) {
            return c;
        }
        return encodeURIComponent(c);
    }
    function percentEscapeQuery(c) {
        var unicode = c.charCodeAt(0);
        if (unicode > 32 && unicode < 127 && [34, 35, 60, 62, 96].indexOf(unicode) == -1) {
            return c;
        }
        return encodeURIComponent(c);
    }
    var EOF = undefined, ALPHA = /[a-zA-Z]/, ALPHANUMERIC = /[a-zA-Z0-9\+\-\.]/;
    function parse(input, stateOverride, base) {
        function err(message) {
            errors.push(message);
        }
        var state = stateOverride || "scheme start", cursor = 0, buffer = "", seenAt = false, seenBracket = false, errors = [];
        loop: while ((input[cursor - 1] != EOF || cursor == 0) && !this._isInvalid) {
            var c = input[cursor];
            switch (state) {
                case "scheme start":
                    if (c && ALPHA.test(c)) {
                        buffer += c.toLowerCase();
                        state = "scheme";
                    } else if (!stateOverride) {
                        buffer = "";
                        state = "no scheme";
                        continue;
                    } else {
                        err("Invalid scheme.");
                        break loop;
                    }
                    break;

                case "scheme":
                    if (c && ALPHANUMERIC.test(c)) {
                        buffer += c.toLowerCase();
                    } else if (":" == c) {
                        this._scheme = buffer;
                        buffer = "";
                        if (stateOverride) {
                            break loop;
                        }
                        if (isRelativeScheme(this._scheme)) {
                            this._isRelative = true;
                        }
                        if ("file" == this._scheme) {
                            state = "relative";
                        } else if (this._isRelative && base && base._scheme == this._scheme) {
                            state = "relative or authority";
                        } else if (this._isRelative) {
                            state = "authority first slash";
                        } else {
                            state = "scheme data";
                        }
                    } else if (!stateOverride) {
                        buffer = "";
                        cursor = 0;
                        state = "no scheme";
                        continue;
                    } else if (EOF == c) {
                        break loop;
                    } else {
                        err("Code point not allowed in scheme: " + c);
                        break loop;
                    }
                    break;

                case "scheme data":
                    if ("?" == c) {
                        query = "?";
                        state = "query";
                    } else if ("#" == c) {
                        this._fragment = "#";
                        state = "fragment";
                    } else {
                        if (EOF != c && "	" != c && "\n" != c && "\r" != c) {
                            this._schemeData += percentEscape(c);
                        }
                    }
                    break;

                case "no scheme":
                    if (!base || !isRelativeScheme(base._scheme)) {
                        err("Missing scheme.");
                        invalid.call(this);
                    } else {
                        state = "relative";
                        continue;
                    }
                    break;

                case "relative or authority":
                    if ("/" == c && "/" == input[cursor + 1]) {
                        state = "authority ignore slashes";
                    } else {
                        err("Expected /, got: " + c);
                        state = "relative";
                        continue;
                    }
                    break;

                case "relative":
                    this._isRelative = true;
                    if ("file" != this._scheme) this._scheme = base._scheme;
                    if (EOF == c) {
                        this._host = base._host;
                        this._port = base._port;
                        this._path = base._path.slice();
                        this._query = base._query;
                        break loop;
                    } else if ("/" == c || "\\" == c) {
                        if ("\\" == c) err("\\ is an invalid code point.");
                        state = "relative slash";
                    } else if ("?" == c) {
                        this._host = base._host;
                        this._port = base._port;
                        this._path = base._path.slice();
                        this._query = "?";
                        state = "query";
                    } else if ("#" == c) {
                        this._host = base._host;
                        this._port = base._port;
                        this._path = base._path.slice();
                        this._query = base._query;
                        this._fragment = "#";
                        state = "fragment";
                    } else {
                        var nextC = input[cursor + 1];
                        var nextNextC = input[cursor + 2];
                        if ("file" != this._scheme || !ALPHA.test(c) || nextC != ":" && nextC != "|" || EOF != nextNextC && "/" != nextNextC && "\\" != nextNextC && "?" != nextNextC && "#" != nextNextC) {
                            this._host = base._host;
                            this._port = base._port;
                            this._path = base._path.slice();
                            this._path.pop();
                        }
                        state = "relative path";
                        continue;
                    }
                    break;

                case "relative slash":
                    if ("/" == c || "\\" == c) {
                        if ("\\" == c) {
                            err("\\ is an invalid code point.");
                        }
                        if ("file" == this._scheme) {
                            state = "file host";
                        } else {
                            state = "authority ignore slashes";
                        }
                    } else {
                        if ("file" != this._scheme) {
                            this._host = base._host;
                            this._port = base._port;
                        }
                        state = "relative path";
                        continue;
                    }
                    break;

                case "authority first slash":
                    if ("/" == c) {
                        state = "authority second slash";
                    } else {
                        err("Expected '/', got: " + c);
                        state = "authority ignore slashes";
                        continue;
                    }
                    break;

                case "authority second slash":
                    state = "authority ignore slashes";
                    if ("/" != c) {
                        err("Expected '/', got: " + c);
                        continue;
                    }
                    break;

                case "authority ignore slashes":
                    if ("/" != c && "\\" != c) {
                        state = "authority";
                        continue;
                    } else {
                        err("Expected authority, got: " + c);
                    }
                    break;

                case "authority":
                    if ("@" == c) {
                        if (seenAt) {
                            err("@ already seen.");
                            buffer += "%40";
                        }
                        seenAt = true;
                        for (var i = 0; i < buffer.length; i++) {
                            var cp = buffer[i];
                            if ("	" == cp || "\n" == cp || "\r" == cp) {
                                err("Invalid whitespace in authority.");
                                continue;
                            }
                            if (":" == cp && null === this._password) {
                                this._password = "";
                                continue;
                            }
                            var tempC = percentEscape(cp);
                            null !== this._password ? this._password += tempC : this._username += tempC;
                        }
                        buffer = "";
                    } else if (EOF == c || "/" == c || "\\" == c || "?" == c || "#" == c) {
                        cursor -= buffer.length;
                        buffer = "";
                        state = "host";
                        continue;
                    } else {
                        buffer += c;
                    }
                    break;

                case "file host":
                    if (EOF == c || "/" == c || "\\" == c || "?" == c || "#" == c) {
                        if (buffer.length == 2 && ALPHA.test(buffer[0]) && (buffer[1] == ":" || buffer[1] == "|")) {
                            state = "relative path";
                        } else if (buffer.length == 0) {
                            state = "relative path start";
                        } else {
                            this._host = IDNAToASCII.call(this, buffer);
                            buffer = "";
                            state = "relative path start";
                        }
                        continue;
                    } else if ("	" == c || "\n" == c || "\r" == c) {
                        err("Invalid whitespace in file host.");
                    } else {
                        buffer += c;
                    }
                    break;

                case "host":
                case "hostname":
                    if (":" == c && !seenBracket) {
                        this._host = IDNAToASCII.call(this, buffer);
                        buffer = "";
                        state = "port";
                        if ("hostname" == stateOverride) {
                            break loop;
                        }
                    } else if (EOF == c || "/" == c || "\\" == c || "?" == c || "#" == c) {
                        this._host = IDNAToASCII.call(this, buffer);
                        buffer = "";
                        state = "relative path start";
                        if (stateOverride) {
                            break loop;
                        }
                        continue;
                    } else if ("	" != c && "\n" != c && "\r" != c) {
                        if ("[" == c) {
                            seenBracket = true;
                        } else if ("]" == c) {
                            seenBracket = false;
                        }
                        buffer += c;
                    } else {
                        err("Invalid code point in host/hostname: " + c);
                    }
                    break;

                case "port":
                    if (/[0-9]/.test(c)) {
                        buffer += c;
                    } else if (EOF == c || "/" == c || "\\" == c || "?" == c || "#" == c || stateOverride) {
                        if ("" != buffer) {
                            var temp = parseInt(buffer, 10);
                            if (temp != relative[this._scheme]) {
                                this._port = temp + "";
                            }
                            buffer = "";
                        }
                        if (stateOverride) {
                            break loop;
                        }
                        state = "relative path start";
                        continue;
                    } else if ("	" == c || "\n" == c || "\r" == c) {
                        err("Invalid code point in port: " + c);
                    } else {
                        invalid.call(this);
                    }
                    break;

                case "relative path start":
                    if ("\\" == c) err("'\\' not allowed in path.");
                    state = "relative path";
                    if ("/" != c && "\\" != c) {
                        continue;
                    }
                    break;

                case "relative path":
                    if (EOF == c || "/" == c || "\\" == c || !stateOverride && ("?" == c || "#" == c)) {
                        if ("\\" == c) {
                            err("\\ not allowed in relative path.");
                        }
                        var tmp;
                        if (tmp = relativePathDotMapping[buffer.toLowerCase()]) {
                            buffer = tmp;
                        }
                        if (".." == buffer) {
                            this._path.pop();
                            if ("/" != c && "\\" != c) {
                                this._path.push("");
                            }
                        } else if ("." == buffer && "/" != c && "\\" != c) {
                            this._path.push("");
                        } else if ("." != buffer) {
                            if ("file" == this._scheme && this._path.length == 0 && buffer.length == 2 && ALPHA.test(buffer[0]) && buffer[1] == "|") {
                                buffer = buffer[0] + ":";
                            }
                            this._path.push(buffer);
                        }
                        buffer = "";
                        if ("?" == c) {
                            this._query = "?";
                            state = "query";
                        } else if ("#" == c) {
                            this._fragment = "#";
                            state = "fragment";
                        }
                    } else if ("	" != c && "\n" != c && "\r" != c) {
                        buffer += percentEscape(c);
                    }
                    break;

                case "query":
                    if (!stateOverride && "#" == c) {
                        this._fragment = "#";
                        state = "fragment";
                    } else if (EOF != c && "	" != c && "\n" != c && "\r" != c) {
                        this._query += percentEscapeQuery(c);
                    }
                    break;

                case "fragment":
                    if (EOF != c && "	" != c && "\n" != c && "\r" != c) {
                        this._fragment += c;
                    }
                    break;
            }
            cursor++;
        }
    }
    function clear() {
        this._scheme = "";
        this._schemeData = "";
        this._username = "";
        this._password = null;
        this._host = "";
        this._port = "";
        this._path = [];
        this._query = "";
        this._fragment = "";
        this._isInvalid = false;
        this._isRelative = false;
    }
    function jURL(url, base) {
        if (base !== undefined && !(base instanceof jURL)) base = new jURL(String(base));
        this._url = url;
        clear.call(this);
        var input = url.replace(/^[ \t\r\n\f]+|[ \t\r\n\f]+$/g, "");
        parse.call(this, input, null, base);
    }
    jURL.prototype = {
        toString: function () {
            return this.href;
        },
        get href() {
            if (this._isInvalid) return this._url;
            var authority = "";
            if ("" != this._username || null != this._password) {
                authority = this._username + (null != this._password ? ":" + this._password : "") + "@";
            }
            return this.protocol + (this._isRelative ? "//" + authority + this.host : "") + this.pathname + this._query + this._fragment;
        },
        set href(href) {
            clear.call(this);
            parse.call(this, href);
        },
        get protocol() {
            return this._scheme + ":";
        },
        set protocol(protocol) {
            if (this._isInvalid) return;
            parse.call(this, protocol + ":", "scheme start");
        },
        get host() {
            return this._isInvalid ? "" : this._port ? this._host + ":" + this._port : this._host;
        },
        set host(host) {
            if (this._isInvalid || !this._isRelative) return;
            parse.call(this, host, "host");
        },
        get hostname() {
            return this._host;
        },
        set hostname(hostname) {
            if (this._isInvalid || !this._isRelative) return;
            parse.call(this, hostname, "hostname");
        },
        get port() {
            return this._port;
        },
        set port(port) {
            if (this._isInvalid || !this._isRelative) return;
            parse.call(this, port, "port");
        },
        get pathname() {
            return this._isInvalid ? "" : this._isRelative ? "/" + this._path.join("/") : this._schemeData;
        },
        set pathname(pathname) {
            if (this._isInvalid || !this._isRelative) return;
            this._path = [];
            parse.call(this, pathname, "relative path start");
        },
        get search() {
            return this._isInvalid || !this._query || "?" == this._query ? "" : this._query;
        },
        set search(search) {
            if (this._isInvalid || !this._isRelative) return;
            this._query = "?";
            if ("?" == search[0]) search = search.slice(1);
            parse.call(this, search, "query");
        },
        get hash() {
            return this._isInvalid || !this._fragment || "#" == this._fragment ? "" : this._fragment;
        },
        set hash(hash) {
            if (this._isInvalid) return;
            this._fragment = "#";
            if ("#" == hash[0]) hash = hash.slice(1);
            parse.call(this, hash, "fragment");
        },
        get origin() {
            var host;
            if (this._isInvalid || !this._scheme) {
                return "";
            }
            switch (this._scheme) {
                case "data":
                case "file":
                case "javascript":
                case "mailto":
                    return "null";
            }
            host = this.host;
            if (!host) {
                return "";
            }
            return this._scheme + "://" + host;
        }
    };
    var OriginalURL = scope.URL;
    if (OriginalURL) {
        jURL.createObjectURL = function (blob) {
            return OriginalURL.createObjectURL.apply(OriginalURL, arguments);
        };
        jURL.revokeObjectURL = function (url) {
            OriginalURL.revokeObjectURL(url);
        };
    }
    scope.URL = jURL;
})(this);

if (typeof WeakMap === "undefined") {
    (function () {
        var defineProperty = Object.defineProperty;
        var counter = Date.now() % 1e9;
        var WeakMap = function () {
            this.name = "__st" + (Math.random() * 1e9 >>> 0) + (counter++ + "__");
        };
        WeakMap.prototype = {
            set: function (key, value) {
                var entry = key[this.name];
                if (entry && entry[0] === key) entry[1] = value; else defineProperty(key, this.name, {
                    value: [key, value],
                    writable: true
                });
                return this;
            },
            get: function (key) {
                var entry;
                return (entry = key[this.name]) && entry[0] === key ? entry[1] : undefined;
            },
            "delete": function (key) {
                var entry = key[this.name];
                if (!entry || entry[0] !== key) return false;
                entry[0] = entry[1] = undefined;
                return true;
            },
            has: function (key) {
                var entry = key[this.name];
                if (!entry) return false;
                return entry[0] === key;
            }
        };
        window.WeakMap = WeakMap;
    })();
}

(function (global) {
    var registrationsTable = new WeakMap();
    var setImmediate;
    if (/Trident|Edge/.test(navigator.userAgent)) {
        setImmediate = setTimeout;
    } else if (window.setImmediate) {
        setImmediate = window.setImmediate;
    } else {
        var setImmediateQueue = [];
        var sentinel = String(Math.random());
        window.addEventListener("message", function (e) {
            if (e.data === sentinel) {
                var queue = setImmediateQueue;
                setImmediateQueue = [];
                queue.forEach(function (func) {
                    func();
                });
            }
        });
        setImmediate = function (func) {
            setImmediateQueue.push(func);
            window.postMessage(sentinel, "*");
        };
    }
    var isScheduled = false;
    var scheduledObservers = [];
    function scheduleCallback(observer) {
        scheduledObservers.push(observer);
        if (!isScheduled) {
            isScheduled = true;
            setImmediate(dispatchCallbacks);
        }
    }
    function wrapIfNeeded(node) {
        return window.ShadowDOMPolyfill && window.ShadowDOMPolyfill.wrapIfNeeded(node) || node;
    }
    function dispatchCallbacks() {
        isScheduled = false;
        var observers = scheduledObservers;
        scheduledObservers = [];
        observers.sort(function (o1, o2) {
            return o1.uid_ - o2.uid_;
        });
        var anyNonEmpty = false;
        observers.forEach(function (observer) {
            var queue = observer.takeRecords();
            removeTransientObserversFor(observer);
            if (queue.length) {
                observer.callback_(queue, observer);
                anyNonEmpty = true;
            }
        });
        if (anyNonEmpty) dispatchCallbacks();
    }
    function removeTransientObserversFor(observer) {
        observer.nodes_.forEach(function (node) {
            var registrations = registrationsTable.get(node);
            if (!registrations) return;
            registrations.forEach(function (registration) {
                if (registration.observer === observer) registration.removeTransientObservers();
            });
        });
    }
    function forEachAncestorAndObserverEnqueueRecord(target, callback) {
        for (var node = target; node; node = node.parentNode) {
            var registrations = registrationsTable.get(node);
            if (registrations) {
                for (var j = 0; j < registrations.length; j++) {
                    var registration = registrations[j];
                    var options = registration.options;
                    if (node !== target && !options.subtree) continue;
                    var record = callback(options);
                    if (record) registration.enqueue(record);
                }
            }
        }
    }
    var uidCounter = 0;
    function JsMutationObserver(callback) {
        this.callback_ = callback;
        this.nodes_ = [];
        this.records_ = [];
        this.uid_ = ++uidCounter;
    }
    JsMutationObserver.prototype = {
        observe: function (target, options) {
            target = wrapIfNeeded(target);
            if (!options.childList && !options.attributes && !options.characterData || options.attributeOldValue && !options.attributes || options.attributeFilter && options.attributeFilter.length && !options.attributes || options.characterDataOldValue && !options.characterData) {
                throw new SyntaxError();
            }
            var registrations = registrationsTable.get(target);
            if (!registrations) registrationsTable.set(target, registrations = []);
            var registration;
            for (var i = 0; i < registrations.length; i++) {
                if (registrations[i].observer === this) {
                    registration = registrations[i];
                    registration.removeListeners();
                    registration.options = options;
                    break;
                }
            }
            if (!registration) {
                registration = new Registration(this, target, options);
                registrations.push(registration);
                this.nodes_.push(target);
            }
            registration.addListeners();
        },
        disconnect: function () {
            this.nodes_.forEach(function (node) {
                var registrations = registrationsTable.get(node);
                for (var i = 0; i < registrations.length; i++) {
                    var registration = registrations[i];
                    if (registration.observer === this) {
                        registration.removeListeners();
                        registrations.splice(i, 1);
                        break;
                    }
                }
            }, this);
            this.records_ = [];
        },
        takeRecords: function () {
            var copyOfRecords = this.records_;
            this.records_ = [];
            return copyOfRecords;
        }
    };
    function MutationRecord(type, target) {
        this.type = type;
        this.target = target;
        this.addedNodes = [];
        this.removedNodes = [];
        this.previousSibling = null;
        this.nextSibling = null;
        this.attributeName = null;
        this.attributeNamespace = null;
        this.oldValue = null;
    }
    function copyMutationRecord(original) {
        var record = new MutationRecord(original.type, original.target);
        record.addedNodes = original.addedNodes.slice();
        record.removedNodes = original.removedNodes.slice();
        record.previousSibling = original.previousSibling;
        record.nextSibling = original.nextSibling;
        record.attributeName = original.attributeName;
        record.attributeNamespace = original.attributeNamespace;
        record.oldValue = original.oldValue;
        return record;
    }
    var currentRecord, recordWithOldValue;
    function getRecord(type, target) {
        return currentRecord = new MutationRecord(type, target);
    }
    function getRecordWithOldValue(oldValue) {
        if (recordWithOldValue) return recordWithOldValue;
        recordWithOldValue = copyMutationRecord(currentRecord);
        recordWithOldValue.oldValue = oldValue;
        return recordWithOldValue;
    }
    function clearRecords() {
        currentRecord = recordWithOldValue = undefined;
    }
    function recordRepresentsCurrentMutation(record) {
        return record === recordWithOldValue || record === currentRecord;
    }
    function selectRecord(lastRecord, newRecord) {
        if (lastRecord === newRecord) return lastRecord;
        if (recordWithOldValue && recordRepresentsCurrentMutation(lastRecord)) return recordWithOldValue;
        return null;
    }
    function Registration(observer, target, options) {
        this.observer = observer;
        this.target = target;
        this.options = options;
        this.transientObservedNodes = [];
    }
    Registration.prototype = {
        enqueue: function (record) {
            var records = this.observer.records_;
            var length = records.length;
            if (records.length > 0) {
                var lastRecord = records[length - 1];
                var recordToReplaceLast = selectRecord(lastRecord, record);
                if (recordToReplaceLast) {
                    records[length - 1] = recordToReplaceLast;
                    return;
                }
            } else {
                scheduleCallback(this.observer);
            }
            records[length] = record;
        },
        addListeners: function () {
            this.addListeners_(this.target);
        },
        addListeners_: function (node) {
            var options = this.options;
            if (options.attributes) node.addEventListener("DOMAttrModified", this, true);
            if (options.characterData) node.addEventListener("DOMCharacterDataModified", this, true);
            if (options.childList) node.addEventListener("DOMNodeInserted", this, true);
            if (options.childList || options.subtree) node.addEventListener("DOMNodeRemoved", this, true);
        },
        removeListeners: function () {
            this.removeListeners_(this.target);
        },
        removeListeners_: function (node) {
            var options = this.options;
            if (options.attributes) node.removeEventListener("DOMAttrModified", this, true);
            if (options.characterData) node.removeEventListener("DOMCharacterDataModified", this, true);
            if (options.childList) node.removeEventListener("DOMNodeInserted", this, true);
            if (options.childList || options.subtree) node.removeEventListener("DOMNodeRemoved", this, true);
        },
        addTransientObserver: function (node) {
            if (node === this.target) return;
            this.addListeners_(node);
            this.transientObservedNodes.push(node);
            var registrations = registrationsTable.get(node);
            if (!registrations) registrationsTable.set(node, registrations = []);
            registrations.push(this);
        },
        removeTransientObservers: function () {
            var transientObservedNodes = this.transientObservedNodes;
            this.transientObservedNodes = [];
            transientObservedNodes.forEach(function (node) {
                this.removeListeners_(node);
                var registrations = registrationsTable.get(node);
                for (var i = 0; i < registrations.length; i++) {
                    if (registrations[i] === this) {
                        registrations.splice(i, 1);
                        break;
                    }
                }
            }, this);
        },
        handleEvent: function (e) {
            e.stopImmediatePropagation();
            switch (e.type) {
                case "DOMAttrModified":
                    var name = e.attrName;
                    var namespace = e.relatedNode.namespaceURI;
                    var target = e.target;
                    var record = new getRecord("attributes", target);
                    record.attributeName = name;
                    record.attributeNamespace = namespace;
                    var oldValue = e.attrChange === MutationEvent.ADDITION ? null : e.prevValue;
                    forEachAncestorAndObserverEnqueueRecord(target, function (options) {
                        if (!options.attributes) return;
                        if (options.attributeFilter && options.attributeFilter.length && options.attributeFilter.indexOf(name) === -1 && options.attributeFilter.indexOf(namespace) === -1) {
                            return;
                        }
                        if (options.attributeOldValue) return getRecordWithOldValue(oldValue);
                        return record;
                    });
                    break;

                case "DOMCharacterDataModified":
                    var target = e.target;
                    var record = getRecord("characterData", target);
                    var oldValue = e.prevValue;
                    forEachAncestorAndObserverEnqueueRecord(target, function (options) {
                        if (!options.characterData) return;
                        if (options.characterDataOldValue) return getRecordWithOldValue(oldValue);
                        return record;
                    });
                    break;

                case "DOMNodeRemoved":
                    this.addTransientObserver(e.target);

                case "DOMNodeInserted":
                    var changedNode = e.target;
                    var addedNodes, removedNodes;
                    if (e.type === "DOMNodeInserted") {
                        addedNodes = [changedNode];
                        removedNodes = [];
                    } else {
                        addedNodes = [];
                        removedNodes = [changedNode];
                    }
                    var previousSibling = changedNode.previousSibling;
                    var nextSibling = changedNode.nextSibling;
                    var record = getRecord("childList", e.target.parentNode);
                    record.addedNodes = addedNodes;
                    record.removedNodes = removedNodes;
                    record.previousSibling = previousSibling;
                    record.nextSibling = nextSibling;
                    forEachAncestorAndObserverEnqueueRecord(e.relatedNode, function (options) {
                        if (!options.childList) return;
                        return record;
                    });
            }
            clearRecords();
        }
    };
    global.JsMutationObserver = JsMutationObserver;
    if (!global.MutationObserver) global.MutationObserver = JsMutationObserver;
})(this);

window.HTMLImports = window.HTMLImports || {
    flags: {}
};

(function (scope) {
    var IMPORT_LINK_TYPE = "import";
    var useNative = Boolean(IMPORT_LINK_TYPE in document.createElement("link"));
    var hasShadowDOMPolyfill = Boolean(window.ShadowDOMPolyfill);
    var wrap = function (node) {
        return hasShadowDOMPolyfill ? ShadowDOMPolyfill.wrapIfNeeded(node) : node;
    };
    var rootDocument = wrap(document);
    var currentScriptDescriptor = {
        get: function () {
            var script = HTMLImports.currentScript || document.currentScript || (document.readyState !== "complete" ? document.scripts[document.scripts.length - 1] : null);
            return wrap(script);
        },
        configurable: true
    };
    Object.defineProperty(document, "_currentScript", currentScriptDescriptor);
    Object.defineProperty(rootDocument, "_currentScript", currentScriptDescriptor);
    var isIE = /Trident|Edge/.test(navigator.userAgent);
    function whenReady(callback, doc) {
        doc = doc || rootDocument;
        whenDocumentReady(function () {
            watchImportsLoad(callback, doc);
        }, doc);
    }
    var requiredReadyState = isIE ? "complete" : "interactive";
    var READY_EVENT = "readystatechange";
    function isDocumentReady(doc) {
        return doc.readyState === "complete" || doc.readyState === requiredReadyState;
    }
    function whenDocumentReady(callback, doc) {
        if (!isDocumentReady(doc)) {
            var checkReady = function () {
                if (doc.readyState === "complete" || doc.readyState === requiredReadyState) {
                    doc.removeEventListener(READY_EVENT, checkReady);
                    whenDocumentReady(callback, doc);
                }
            };
            doc.addEventListener(READY_EVENT, checkReady);
        } else if (callback) {
            callback();
        }
    }
    function markTargetLoaded(event) {
        event.target.__loaded = true;
    }
    function watchImportsLoad(callback, doc) {
        var imports = doc.querySelectorAll("link[rel=import]");
        var parsedCount = 0, importCount = imports.length, newImports = [], errorImports = [];
        function checkDone() {
            if (parsedCount == importCount && callback) {
                callback({
                    allImports: imports,
                    loadedImports: newImports,
                    errorImports: errorImports
                });
            }
        }
        function loadedImport(e) {
            markTargetLoaded(e);
            newImports.push(this);
            parsedCount++;
            checkDone();
        }
        function errorLoadingImport(e) {
            errorImports.push(this);
            parsedCount++;
            checkDone();
        }
        if (importCount) {
            for (var i = 0, imp; i < importCount && (imp = imports[i]) ; i++) {
                if (isImportLoaded(imp)) {
                    parsedCount++;
                    checkDone();
                } else {
                    imp.addEventListener("load", loadedImport);
                    imp.addEventListener("error", errorLoadingImport);
                }
            }
        } else {
            checkDone();
        }
    }
    function isImportLoaded(link) {
        return useNative ? link.__loaded || link.import && link.import.readyState !== "loading" : link.__importParsed;
    }
    if (useNative) {
        new MutationObserver(function (mxns) {
            for (var i = 0, l = mxns.length, m; i < l && (m = mxns[i]) ; i++) {
                if (m.addedNodes) {
                    handleImports(m.addedNodes);
                }
            }
        }).observe(document.head, {
            childList: true
        });
        function handleImports(nodes) {
            for (var i = 0, l = nodes.length, n; i < l && (n = nodes[i]) ; i++) {
                if (isImport(n)) {
                    handleImport(n);
                }
            }
        }
        function isImport(element) {
            return element.localName === "link" && element.rel === "import";
        }
        function handleImport(element) {
            var loaded = element.import;
            if (loaded) {
                markTargetLoaded({
                    target: element
                });
            } else {
                element.addEventListener("load", markTargetLoaded);
                element.addEventListener("error", markTargetLoaded);
            }
        }
        (function () {
            if (document.readyState === "loading") {
                var imports = document.querySelectorAll("link[rel=import]");
                for (var i = 0, l = imports.length, imp; i < l && (imp = imports[i]) ; i++) {
                    handleImport(imp);
                }
            }
        })();
    }
    whenReady(function (detail) {
        HTMLImports.ready = true;
        HTMLImports.readyTime = new Date().getTime();
        var evt = rootDocument.createEvent("CustomEvent");
        evt.initCustomEvent("HTMLImportsLoaded", true, true, detail);
        rootDocument.dispatchEvent(evt);
    });
    scope.IMPORT_LINK_TYPE = IMPORT_LINK_TYPE;
    scope.useNative = useNative;
    scope.rootDocument = rootDocument;
    scope.whenReady = whenReady;
    scope.isIE = isIE;
})(HTMLImports);

(function (scope) {
    var modules = [];
    var addModule = function (module) {
        modules.push(module);
    };
    var initializeModules = function () {
        modules.forEach(function (module) {
            module(scope);
        });
    };
    scope.addModule = addModule;
    scope.initializeModules = initializeModules;
})(HTMLImports);

HTMLImports.addModule(function (scope) {
    var CSS_URL_REGEXP = /(url\()([^)]*)(\))/g;
    var CSS_IMPORT_REGEXP = /(@import[\s]+(?!url\())([^;]*)(;)/g;
    var path = {
        resolveUrlsInStyle: function (style, linkUrl) {
            var doc = style.ownerDocument;
            var resolver = doc.createElement("a");
            style.textContent = this.resolveUrlsInCssText(style.textContent, linkUrl, resolver);
            return style;
        },
        resolveUrlsInCssText: function (cssText, linkUrl, urlObj) {
            var r = this.replaceUrls(cssText, urlObj, linkUrl, CSS_URL_REGEXP);
            r = this.replaceUrls(r, urlObj, linkUrl, CSS_IMPORT_REGEXP);
            return r;
        },
        replaceUrls: function (text, urlObj, linkUrl, regexp) {
            return text.replace(regexp, function (m, pre, url, post) {
                var urlPath = url.replace(/["']/g, "");
                if (linkUrl) {
                    urlPath = new URL(urlPath, linkUrl).href;
                }
                urlObj.href = urlPath;
                urlPath = urlObj.href;
                return pre + "'" + urlPath + "'" + post;
            });
        }
    };
    scope.path = path;
});

HTMLImports.addModule(function (scope) {
    var xhr = {
        async: true,
        ok: function (request) {
            return request.status >= 200 && request.status < 300 || request.status === 304 || request.status === 0;
        },
        load: function (url, next, nextContext) {
            var request = new XMLHttpRequest();
            if (scope.flags.debug || scope.flags.bust) {
                url += "?" + Math.random();
            }
            request.open("GET", url, xhr.async);
            request.addEventListener("readystatechange", function (e) {
                if (request.readyState === 4) {
                    var locationHeader = request.getResponseHeader("Location");
                    var redirectedUrl = null;
                    if (locationHeader) {
                        var redirectedUrl = locationHeader.substr(0, 1) === "/" ? location.origin + locationHeader : locationHeader;
                    }
                    next.call(nextContext, !xhr.ok(request) && request, request.response || request.responseText, redirectedUrl);
                }
            });
            request.send();
            return request;
        },
        loadDocument: function (url, next, nextContext) {
            this.load(url, next, nextContext).responseType = "document";
        }
    };
    scope.xhr = xhr;
});

HTMLImports.addModule(function (scope) {
    var xhr = scope.xhr;
    var flags = scope.flags;
    var Loader = function (onLoad, onComplete) {
        this.cache = {};
        this.onload = onLoad;
        this.oncomplete = onComplete;
        this.inflight = 0;
        this.pending = {};
    };
    Loader.prototype = {
        addNodes: function (nodes) {
            this.inflight += nodes.length;
            for (var i = 0, l = nodes.length, n; i < l && (n = nodes[i]) ; i++) {
                this.require(n);
            }
            this.checkDone();
        },
        addNode: function (node) {
            this.inflight++;
            this.require(node);
            this.checkDone();
        },
        require: function (elt) {
            var url = elt.src || elt.href;
            elt.__nodeUrl = url;
            if (!this.dedupe(url, elt)) {
                this.fetch(url, elt);
            }
        },
        dedupe: function (url, elt) {
            if (this.pending[url]) {
                this.pending[url].push(elt);
                return true;
            }
            var resource;
            if (this.cache[url]) {
                this.onload(url, elt, this.cache[url]);
                this.tail();
                return true;
            }
            this.pending[url] = [elt];
            return false;
        },
        fetch: function (url, elt) {
            flags.load && console.log("fetch", url, elt);
            if (!url) {
                setTimeout(function () {
                    this.receive(url, elt, {
                        error: "href must be specified"
                    }, null);
                }.bind(this), 0);
            } else if (url.match(/^data:/)) {
                var pieces = url.split(",");
                var header = pieces[0];
                var body = pieces[1];
                if (header.indexOf(";base64") > -1) {
                    body = atob(body);
                } else {
                    body = decodeURIComponent(body);
                }
                setTimeout(function () {
                    this.receive(url, elt, null, body);
                }.bind(this), 0);
            } else {
                var receiveXhr = function (err, resource, redirectedUrl) {
                    this.receive(url, elt, err, resource, redirectedUrl);
                }.bind(this);
                xhr.load(url, receiveXhr);
            }
        },
        receive: function (url, elt, err, resource, redirectedUrl) {
            this.cache[url] = resource;
            var $p = this.pending[url];
            for (var i = 0, l = $p.length, p; i < l && (p = $p[i]) ; i++) {
                this.onload(url, p, resource, err, redirectedUrl);
                this.tail();
            }
            this.pending[url] = null;
        },
        tail: function () {
            --this.inflight;
            this.checkDone();
        },
        checkDone: function () {
            if (!this.inflight) {
                this.oncomplete();
            }
        }
    };
    scope.Loader = Loader;
});

HTMLImports.addModule(function (scope) {
    var Observer = function (addCallback) {
        this.addCallback = addCallback;
        this.mo = new MutationObserver(this.handler.bind(this));
    };
    Observer.prototype = {
        handler: function (mutations) {
            for (var i = 0, l = mutations.length, m; i < l && (m = mutations[i]) ; i++) {
                if (m.type === "childList" && m.addedNodes.length) {
                    this.addedNodes(m.addedNodes);
                }
            }
        },
        addedNodes: function (nodes) {
            if (this.addCallback) {
                this.addCallback(nodes);
            }
            for (var i = 0, l = nodes.length, n, loading; i < l && (n = nodes[i]) ; i++) {
                if (n.children && n.children.length) {
                    this.addedNodes(n.children);
                }
            }
        },
        observe: function (root) {
            this.mo.observe(root, {
                childList: true,
                subtree: true
            });
        }
    };
    scope.Observer = Observer;
});

HTMLImports.addModule(function (scope) {
    var path = scope.path;
    var rootDocument = scope.rootDocument;
    var flags = scope.flags;
    var isIE = scope.isIE;
    var IMPORT_LINK_TYPE = scope.IMPORT_LINK_TYPE;
    var IMPORT_SELECTOR = "link[rel=" + IMPORT_LINK_TYPE + "]";
    var importParser = {
        documentSelectors: IMPORT_SELECTOR,
        importsSelectors: [IMPORT_SELECTOR, "link[rel=stylesheet]", "style", "script:not([type])", 'script[type="text/javascript"]'].join(","),
        map: {
            link: "parseLink",
            script: "parseScript",
            style: "parseStyle"
        },
        dynamicElements: [],
        parseNext: function () {
            var next = this.nextToParse();
            if (next) {
                this.parse(next);
            }
        },
        parse: function (elt) {
            if (this.isParsed(elt)) {
                flags.parse && console.log("[%s] is already parsed", elt.localName);
                return;
            }
            var fn = this[this.map[elt.localName]];
            if (fn) {
                this.markParsing(elt);
                fn.call(this, elt);
            }
        },
        parseDynamic: function (elt, quiet) {
            this.dynamicElements.push(elt);
            if (!quiet) {
                this.parseNext();
            }
        },
        markParsing: function (elt) {
            flags.parse && console.log("parsing", elt);
            this.parsingElement = elt;
        },
        markParsingComplete: function (elt) {
            elt.__importParsed = true;
            this.markDynamicParsingComplete(elt);
            if (elt.__importElement) {
                elt.__importElement.__importParsed = true;
                this.markDynamicParsingComplete(elt.__importElement);
            }
            this.parsingElement = null;
            flags.parse && console.log("completed", elt);
        },
        markDynamicParsingComplete: function (elt) {
            var i = this.dynamicElements.indexOf(elt);
            if (i >= 0) {
                this.dynamicElements.splice(i, 1);
            }
        },
        parseImport: function (elt) {
            if (HTMLImports.__importsParsingHook) {
                HTMLImports.__importsParsingHook(elt);
            }
            if (elt.import) {
                elt.import.__importParsed = true;
            }
            this.markParsingComplete(elt);
            if (elt.__resource && !elt.__error) {
                elt.dispatchEvent(new CustomEvent("load", {
                    bubbles: false
                }));
            } else {
                elt.dispatchEvent(new CustomEvent("error", {
                    bubbles: false
                }));
            }
            if (elt.__pending) {
                var fn;
                while (elt.__pending.length) {
                    fn = elt.__pending.shift();
                    if (fn) {
                        fn({
                            target: elt
                        });
                    }
                }
            }
            this.parseNext();
        },
        parseLink: function (linkElt) {
            if (nodeIsImport(linkElt)) {
                this.parseImport(linkElt);
            } else {
                linkElt.href = linkElt.href;
                this.parseGeneric(linkElt);
            }
        },
        parseStyle: function (elt) {
            var src = elt;
            elt = cloneStyle(elt);
            src.__appliedElement = elt;
            elt.__importElement = src;
            this.parseGeneric(elt);
        },
        parseGeneric: function (elt) {
            this.trackElement(elt);
            this.addElementToDocument(elt);
        },
        rootImportForElement: function (elt) {
            var n = elt;
            while (n.ownerDocument.__importLink) {
                n = n.ownerDocument.__importLink;
            }
            return n;
        },
        addElementToDocument: function (elt) {
            var port = this.rootImportForElement(elt.__importElement || elt);
            port.parentNode.insertBefore(elt, port);
        },
        trackElement: function (elt, callback) {
            var self = this;
            var done = function (e) {
                if (callback) {
                    callback(e);
                }
                self.markParsingComplete(elt);
                self.parseNext();
            };
            elt.addEventListener("load", done);
            elt.addEventListener("error", done);
            if (isIE && elt.localName === "style") {
                var fakeLoad = false;
                if (elt.textContent.indexOf("@import") == -1) {
                    fakeLoad = true;
                } else if (elt.sheet) {
                    fakeLoad = true;
                    var csr = elt.sheet.cssRules;
                    var len = csr ? csr.length : 0;
                    for (var i = 0, r; i < len && (r = csr[i]) ; i++) {
                        if (r.type === CSSRule.IMPORT_RULE) {
                            fakeLoad = fakeLoad && Boolean(r.styleSheet);
                        }
                    }
                }
                if (fakeLoad) {
                    elt.dispatchEvent(new CustomEvent("load", {
                        bubbles: false
                    }));
                }
            }
        },
        parseScript: function (scriptElt) {
            var script = document.createElement("script");
            script.__importElement = scriptElt;
            script.src = scriptElt.src ? scriptElt.src : generateScriptDataUrl(scriptElt);
            scope.currentScript = scriptElt;
            this.trackElement(script, function (e) {
                script.parentNode.removeChild(script);
                scope.currentScript = null;
            });
            this.addElementToDocument(script);
        },
        nextToParse: function () {
            this._mayParse = [];
            return !this.parsingElement && (this.nextToParseInDoc(rootDocument) || this.nextToParseDynamic());
        },
        nextToParseInDoc: function (doc, link) {
            if (doc && this._mayParse.indexOf(doc) < 0) {
                this._mayParse.push(doc);
                var nodes = doc.querySelectorAll(this.parseSelectorsForNode(doc));
                for (var i = 0, l = nodes.length, p = 0, n; i < l && (n = nodes[i]) ; i++) {
                    if (!this.isParsed(n)) {
                        if (this.hasResource(n)) {
                            return nodeIsImport(n) ? this.nextToParseInDoc(n.import, n) : n;
                        } else {
                            return;
                        }
                    }
                }
            }
            return link;
        },
        nextToParseDynamic: function () {
            return this.dynamicElements[0];
        },
        parseSelectorsForNode: function (node) {
            var doc = node.ownerDocument || node;
            return doc === rootDocument ? this.documentSelectors : this.importsSelectors;
        },
        isParsed: function (node) {
            return node.__importParsed;
        },
        needsDynamicParsing: function (elt) {
            return this.dynamicElements.indexOf(elt) >= 0;
        },
        hasResource: function (node) {
            if (nodeIsImport(node) && node.import === undefined) {
                return false;
            }
            return true;
        }
    };
    function nodeIsImport(elt) {
        return elt.localName === "link" && elt.rel === IMPORT_LINK_TYPE;
    }
    function generateScriptDataUrl(script) {
        var scriptContent = generateScriptContent(script);
        return "data:text/javascript;charset=utf-8," + encodeURIComponent(scriptContent);
    }
    function generateScriptContent(script) {
        return script.textContent + generateSourceMapHint(script);
    }
    function generateSourceMapHint(script) {
        var owner = script.ownerDocument;
        owner.__importedScripts = owner.__importedScripts || 0;
        var moniker = script.ownerDocument.baseURI;
        var num = owner.__importedScripts ? "-" + owner.__importedScripts : "";
        owner.__importedScripts++;
        return "\n//# sourceURL=" + moniker + num + ".js\n";
    }
    function cloneStyle(style) {
        var clone = style.ownerDocument.createElement("style");
        clone.textContent = style.textContent;
        path.resolveUrlsInStyle(clone);
        return clone;
    }
    scope.parser = importParser;
    scope.IMPORT_SELECTOR = IMPORT_SELECTOR;
});

HTMLImports.addModule(function (scope) {
    var flags = scope.flags;
    var IMPORT_LINK_TYPE = scope.IMPORT_LINK_TYPE;
    var IMPORT_SELECTOR = scope.IMPORT_SELECTOR;
    var rootDocument = scope.rootDocument;
    var Loader = scope.Loader;
    var Observer = scope.Observer;
    var parser = scope.parser;
    var importer = {
        documents: {},
        documentPreloadSelectors: IMPORT_SELECTOR,
        importsPreloadSelectors: [IMPORT_SELECTOR].join(","),
        loadNode: function (node) {
            importLoader.addNode(node);
        },
        loadSubtree: function (parent) {
            var nodes = this.marshalNodes(parent);
            importLoader.addNodes(nodes);
        },
        marshalNodes: function (parent) {
            return parent.querySelectorAll(this.loadSelectorsForNode(parent));
        },
        loadSelectorsForNode: function (node) {
            var doc = node.ownerDocument || node;
            return doc === rootDocument ? this.documentPreloadSelectors : this.importsPreloadSelectors;
        },
        loaded: function (url, elt, resource, err, redirectedUrl) {
            flags.load && console.log("loaded", url, elt);
            elt.__resource = resource;
            elt.__error = err;
            if (isImportLink(elt)) {
                var doc = this.documents[url];
                if (doc === undefined) {
                    doc = err ? null : makeDocument(resource, redirectedUrl || url);
                    if (doc) {
                        doc.__importLink = elt;
                        this.bootDocument(doc);
                    }
                    this.documents[url] = doc;
                }
                elt.import = doc;
            }
            parser.parseNext();
        },
        bootDocument: function (doc) {
            this.loadSubtree(doc);
            this.observer.observe(doc);
            parser.parseNext();
        },
        loadedAll: function () {
            parser.parseNext();
        }
    };
    var importLoader = new Loader(importer.loaded.bind(importer), importer.loadedAll.bind(importer));
    importer.observer = new Observer();
    function isImportLink(elt) {
        return isLinkRel(elt, IMPORT_LINK_TYPE);
    }
    function isLinkRel(elt, rel) {
        return elt.localName === "link" && elt.getAttribute("rel") === rel;
    }
    function hasBaseURIAccessor(doc) {
        return !!Object.getOwnPropertyDescriptor(doc, "baseURI");
    }
    function makeDocument(resource, url) {
        var doc = document.implementation.createHTMLDocument(IMPORT_LINK_TYPE);
        doc._URL = url;
        var base = doc.createElement("base");
        base.setAttribute("href", url);
        if (!doc.baseURI && !hasBaseURIAccessor(doc)) {
            Object.defineProperty(doc, "baseURI", {
                value: url
            });
        }
        var meta = doc.createElement("meta");
        meta.setAttribute("charset", "utf-8");
        doc.head.appendChild(meta);
        doc.head.appendChild(base);
        doc.body.innerHTML = resource;
        if (window.HTMLTemplateElement && HTMLTemplateElement.bootstrap) {
            HTMLTemplateElement.bootstrap(doc);
        }
        return doc;
    }
    if (!document.baseURI) {
        var baseURIDescriptor = {
            get: function () {
                var base = document.querySelector("base");
                return base ? base.href : window.location.href;
            },
            configurable: true
        };
        Object.defineProperty(document, "baseURI", baseURIDescriptor);
        Object.defineProperty(rootDocument, "baseURI", baseURIDescriptor);
    }
    scope.importer = importer;
    scope.importLoader = importLoader;
});

HTMLImports.addModule(function (scope) {
    var parser = scope.parser;
    var importer = scope.importer;
    var dynamic = {
        added: function (nodes) {
            var owner, parsed, loading;
            for (var i = 0, l = nodes.length, n; i < l && (n = nodes[i]) ; i++) {
                if (!owner) {
                    owner = n.ownerDocument;
                    parsed = parser.isParsed(owner);
                }
                loading = this.shouldLoadNode(n);
                if (loading) {
                    importer.loadNode(n);
                }
                if (this.shouldParseNode(n) && parsed) {
                    parser.parseDynamic(n, loading);
                }
            }
        },
        shouldLoadNode: function (node) {
            return node.nodeType === 1 && matches.call(node, importer.loadSelectorsForNode(node));
        },
        shouldParseNode: function (node) {
            return node.nodeType === 1 && matches.call(node, parser.parseSelectorsForNode(node));
        }
    };
    importer.observer.addCallback = dynamic.added.bind(dynamic);
    var matches = HTMLElement.prototype.matches || HTMLElement.prototype.matchesSelector || HTMLElement.prototype.webkitMatchesSelector || HTMLElement.prototype.mozMatchesSelector || HTMLElement.prototype.msMatchesSelector;
});

(function (scope) {
    var initializeModules = scope.initializeModules;
    var isIE = scope.isIE;
    if (scope.useNative) {
        return;
    }
    if (isIE && typeof window.CustomEvent !== "function") {
        window.CustomEvent = function (inType, params) {
            params = params || {};
            var e = document.createEvent("CustomEvent");
            e.initCustomEvent(inType, Boolean(params.bubbles), Boolean(params.cancelable), params.detail);
            return e;
        };
        window.CustomEvent.prototype = window.Event.prototype;
    }
    initializeModules();
    var rootDocument = scope.rootDocument;
    function bootstrap() {
        HTMLImports.importer.bootDocument(rootDocument);
    }
    if (document.readyState === "complete" || document.readyState === "interactive" && !window.attachEvent) {
        bootstrap();
    } else {
        document.addEventListener("DOMContentLoaded", bootstrap);
    }
})(HTMLImports);

window.CustomElements = window.CustomElements || {
    flags: {}
};

(function (scope) {
    var flags = scope.flags;
    var modules = [];
    var addModule = function (module) {
        modules.push(module);
    };
    var initializeModules = function () {
        modules.forEach(function (module) {
            module(scope);
        });
    };
    scope.addModule = addModule;
    scope.initializeModules = initializeModules;
    scope.hasNative = Boolean(document.registerElement);
    scope.useNative = !flags.register && scope.hasNative && !window.ShadowDOMPolyfill && (!window.HTMLImports || HTMLImports.useNative);
})(CustomElements);

CustomElements.addModule(function (scope) {
    var IMPORT_LINK_TYPE = window.HTMLImports ? HTMLImports.IMPORT_LINK_TYPE : "none";
    function forSubtree(node, cb) {
        findAllElements(node, function (e) {
            if (cb(e)) {
                return true;
            }
            forRoots(e, cb);
        });
        forRoots(node, cb);
    }
    function findAllElements(node, find, data) {
        var e = node.firstElementChild;
        if (!e) {
            e = node.firstChild;
            while (e && e.nodeType !== Node.ELEMENT_NODE) {
                e = e.nextSibling;
            }
        }
        while (e) {
            if (find(e, data) !== true) {
                findAllElements(e, find, data);
            }
            e = e.nextElementSibling;
        }
        return null;
    }
    function forRoots(node, cb) {
        var root = node.shadowRoot;
        while (root) {
            forSubtree(root, cb);
            root = root.olderShadowRoot;
        }
    }
    var processingDocuments;
    function forDocumentTree(doc, cb) {
        processingDocuments = [];
        _forDocumentTree(doc, cb);
        processingDocuments = null;
    }
    function _forDocumentTree(doc, cb) {
        doc = wrap(doc);
        if (processingDocuments.indexOf(doc) >= 0) {
            return;
        }
        processingDocuments.push(doc);
        var imports = doc.querySelectorAll("link[rel=" + IMPORT_LINK_TYPE + "]");
        for (var i = 0, l = imports.length, n; i < l && (n = imports[i]) ; i++) {
            if (n.import) {
                _forDocumentTree(n.import, cb);
            }
        }
        cb(doc);
    }
    scope.forDocumentTree = forDocumentTree;
    scope.forSubtree = forSubtree;
});

CustomElements.addModule(function (scope) {
    var flags = scope.flags;
    var forSubtree = scope.forSubtree;
    var forDocumentTree = scope.forDocumentTree;
    function addedNode(node) {
        return added(node) || addedSubtree(node);
    }
    function added(node) {
        if (scope.upgrade(node)) {
            return true;
        }
        attached(node);
    }
    function addedSubtree(node) {
        forSubtree(node, function (e) {
            if (added(e)) {
                return true;
            }
        });
    }
    function attachedNode(node) {
        attached(node);
        if (inDocument(node)) {
            forSubtree(node, function (e) {
                attached(e);
            });
        }
    }
    var hasPolyfillMutations = !window.MutationObserver || window.MutationObserver === window.JsMutationObserver;
    scope.hasPolyfillMutations = hasPolyfillMutations;
    var isPendingMutations = false;
    var pendingMutations = [];
    function deferMutation(fn) {
        pendingMutations.push(fn);
        if (!isPendingMutations) {
            isPendingMutations = true;
            setTimeout(takeMutations);
        }
    }
    function takeMutations() {
        isPendingMutations = false;
        var $p = pendingMutations;
        for (var i = 0, l = $p.length, p; i < l && (p = $p[i]) ; i++) {
            p();
        }
        pendingMutations = [];
    }
    function attached(element) {
        if (hasPolyfillMutations) {
            deferMutation(function () {
                _attached(element);
            });
        } else {
            _attached(element);
        }
    }
    function _attached(element) {
        if (element.__upgraded__ && (element.attachedCallback || element.detachedCallback)) {
            if (!element.__attached && inDocument(element)) {
                element.__attached = true;
                if (element.attachedCallback) {
                    element.attachedCallback();
                }
            }
        }
    }
    function detachedNode(node) {
        detached(node);
        forSubtree(node, function (e) {
            detached(e);
        });
    }
    function detached(element) {
        if (hasPolyfillMutations) {
            deferMutation(function () {
                _detached(element);
            });
        } else {
            _detached(element);
        }
    }
    function _detached(element) {
        if (element.__upgraded__ && (element.attachedCallback || element.detachedCallback)) {
            if (element.__attached && !inDocument(element)) {
                element.__attached = false;
                if (element.detachedCallback) {
                    element.detachedCallback();
                }
            }
        }
    }
    function inDocument(element) {
        var p = element;
        var doc = wrap(document);
        while (p) {
            if (p == doc) {
                return true;
            }
            p = p.parentNode || p.nodeType === Node.DOCUMENT_FRAGMENT_NODE && p.host;
        }
    }
    function watchShadow(node) {
        if (node.shadowRoot && !node.shadowRoot.__watched) {
            flags.dom && console.log("watching shadow-root for: ", node.localName);
            var root = node.shadowRoot;
            while (root) {
                observe(root);
                root = root.olderShadowRoot;
            }
        }
    }
    function handler(mutations) {
        if (flags.dom) {
            var mx = mutations[0];
            if (mx && mx.type === "childList" && mx.addedNodes) {
                if (mx.addedNodes) {
                    var d = mx.addedNodes[0];
                    while (d && d !== document && !d.host) {
                        d = d.parentNode;
                    }
                    var u = d && (d.URL || d._URL || d.host && d.host.localName) || "";
                    u = u.split("/?").shift().split("/").pop();
                }
            }
            console.group("mutations (%d) [%s]", mutations.length, u || "");
        }
        mutations.forEach(function (mx) {
            if (mx.type === "childList") {
                forEach(mx.addedNodes, function (n) {
                    if (!n.localName) {
                        return;
                    }
                    addedNode(n);
                });
                forEach(mx.removedNodes, function (n) {
                    if (!n.localName) {
                        return;
                    }
                    detachedNode(n);
                });
            }
        });
        flags.dom && console.groupEnd();
    }
    function takeRecords(node) {
        node = wrap(node);
        if (!node) {
            node = wrap(document);
        }
        while (node.parentNode) {
            node = node.parentNode;
        }
        var observer = node.__observer;
        if (observer) {
            handler(observer.takeRecords());
            takeMutations();
        }
    }
    var forEach = Array.prototype.forEach.call.bind(Array.prototype.forEach);
    function observe(inRoot) {
        if (inRoot.__observer) {
            return;
        }
        var observer = new MutationObserver(handler);
        observer.observe(inRoot, {
            childList: true,
            subtree: true
        });
        inRoot.__observer = observer;
    }
    function upgradeDocument(doc) {
        doc = wrap(doc);
        flags.dom && console.group("upgradeDocument: ", doc.baseURI.split("/").pop());
        addedNode(doc);
        observe(doc);
        flags.dom && console.groupEnd();
    }
    function upgradeDocumentTree(doc) {
        forDocumentTree(doc, upgradeDocument);
    }
    var originalCreateShadowRoot = Element.prototype.createShadowRoot;
    if (originalCreateShadowRoot) {
        Element.prototype.createShadowRoot = function () {
            var root = originalCreateShadowRoot.call(this);
            CustomElements.watchShadow(this);
            return root;
        };
    }
    scope.watchShadow = watchShadow;
    scope.upgradeDocumentTree = upgradeDocumentTree;
    scope.upgradeSubtree = addedSubtree;
    scope.upgradeAll = addedNode;
    scope.attachedNode = attachedNode;
    scope.takeRecords = takeRecords;
});

CustomElements.addModule(function (scope) {
    var flags = scope.flags;
    function upgrade(node) {
        if (!node.__upgraded__ && node.nodeType === Node.ELEMENT_NODE) {
            var is = node.getAttribute("is");
            var definition = scope.getRegisteredDefinition(is || node.localName);
            if (definition) {
                if (is && definition.tag == node.localName) {
                    return upgradeWithDefinition(node, definition);
                } else if (!is && !definition.extends) {
                    return upgradeWithDefinition(node, definition);
                }
            }
        }
    }
    function upgradeWithDefinition(element, definition) {
        flags.upgrade && console.group("upgrade:", element.localName);
        if (definition.is) {
            element.setAttribute("is", definition.is);
        }
        implementPrototype(element, definition);
        element.__upgraded__ = true;
        created(element);
        scope.attachedNode(element);
        scope.upgradeSubtree(element);
        flags.upgrade && console.groupEnd();
        return element;
    }
    function implementPrototype(element, definition) {
        if (Object.__proto__) {
            element.__proto__ = definition.prototype;
        } else {
            customMixin(element, definition.prototype, definition.native);
            element.__proto__ = definition.prototype;
        }
    }
    function customMixin(inTarget, inSrc, inNative) {
        var used = {};
        var p = inSrc;
        while (p !== inNative && p !== HTMLElement.prototype) {
            var keys = Object.getOwnPropertyNames(p);
            for (var i = 0, k; k = keys[i]; i++) {
                if (!used[k]) {
                    Object.defineProperty(inTarget, k, Object.getOwnPropertyDescriptor(p, k));
                    used[k] = 1;
                }
            }
            p = Object.getPrototypeOf(p);
        }
    }
    function created(element) {
        if (element.createdCallback) {
            element.createdCallback();
        }
    }
    scope.upgrade = upgrade;
    scope.upgradeWithDefinition = upgradeWithDefinition;
    scope.implementPrototype = implementPrototype;
});

CustomElements.addModule(function (scope) {
    var upgradeDocumentTree = scope.upgradeDocumentTree;
    var upgrade = scope.upgrade;
    var upgradeWithDefinition = scope.upgradeWithDefinition;
    var implementPrototype = scope.implementPrototype;
    var useNative = scope.useNative;
    function register(name, options) {
        var definition = options || {};
        if (!name) {
            throw new Error("document.registerElement: first argument `name` must not be empty");
        }
        if (name.indexOf("-") < 0) {
            throw new Error("document.registerElement: first argument ('name') must contain a dash ('-'). Argument provided was '" + String(name) + "'.");
        }
        if (isReservedTag(name)) {
            throw new Error("Failed to execute 'registerElement' on 'Document': Registration failed for type '" + String(name) + "'. The type name is invalid.");
        }
        if (getRegisteredDefinition(name)) {
            throw new Error("DuplicateDefinitionError: a type with name '" + String(name) + "' is already registered");
        }
        if (!definition.prototype) {
            definition.prototype = Object.create(HTMLElement.prototype);
        }
        definition.__name = name.toLowerCase();
        definition.lifecycle = definition.lifecycle || {};
        definition.ancestry = ancestry(definition.extends);
        resolveTagName(definition);
        resolvePrototypeChain(definition);
        overrideAttributeApi(definition.prototype);
        registerDefinition(definition.__name, definition);
        definition.ctor = generateConstructor(definition);
        definition.ctor.prototype = definition.prototype;
        definition.prototype.constructor = definition.ctor;
        if (scope.ready) {
            upgradeDocumentTree(document);
        }
        return definition.ctor;
    }
    function overrideAttributeApi(prototype) {
        if (prototype.setAttribute._polyfilled) {
            return;
        }
        var setAttribute = prototype.setAttribute;
        prototype.setAttribute = function (name, value) {
            changeAttribute.call(this, name, value, setAttribute);
        };
        var removeAttribute = prototype.removeAttribute;
        prototype.removeAttribute = function (name) {
            changeAttribute.call(this, name, null, removeAttribute);
        };
        prototype.setAttribute._polyfilled = true;
    }
    function changeAttribute(name, value, operation) {
        name = name.toLowerCase();
        var oldValue = this.getAttribute(name);
        operation.apply(this, arguments);
        var newValue = this.getAttribute(name);
        if (this.attributeChangedCallback && newValue !== oldValue) {
            this.attributeChangedCallback(name, oldValue, newValue);
        }
    }
    function isReservedTag(name) {
        for (var i = 0; i < reservedTagList.length; i++) {
            if (name === reservedTagList[i]) {
                return true;
            }
        }
    }
    var reservedTagList = ["annotation-xml", "color-profile", "font-face", "font-face-src", "font-face-uri", "font-face-format", "font-face-name", "missing-glyph"];
    function ancestry(extnds) {
        var extendee = getRegisteredDefinition(extnds);
        if (extendee) {
            return ancestry(extendee.extends).concat([extendee]);
        }
        return [];
    }
    function resolveTagName(definition) {
        var baseTag = definition.extends;
        for (var i = 0, a; a = definition.ancestry[i]; i++) {
            baseTag = a.is && a.tag;
        }
        definition.tag = baseTag || definition.__name;
        if (baseTag) {
            definition.is = definition.__name;
        }
    }
    function resolvePrototypeChain(definition) {
        if (!Object.__proto__) {
            var nativePrototype = HTMLElement.prototype;
            if (definition.is) {
                var inst = document.createElement(definition.tag);
                var expectedPrototype = Object.getPrototypeOf(inst);
                if (expectedPrototype === definition.prototype) {
                    nativePrototype = expectedPrototype;
                }
            }
            var proto = definition.prototype, ancestor;
            while (proto && proto !== nativePrototype) {
                ancestor = Object.getPrototypeOf(proto);
                proto.__proto__ = ancestor;
                proto = ancestor;
            }
            definition.native = nativePrototype;
        }
    }
    function instantiate(definition) {
        return upgradeWithDefinition(domCreateElement(definition.tag), definition);
    }
    var registry = {};
    function getRegisteredDefinition(name) {
        if (name) {
            return registry[name.toLowerCase()];
        }
    }
    function registerDefinition(name, definition) {
        registry[name] = definition;
    }
    function generateConstructor(definition) {
        return function () {
            return instantiate(definition);
        };
    }
    var HTML_NAMESPACE = "http://www.w3.org/1999/xhtml";
    function createElementNS(namespace, tag, typeExtension) {
        if (namespace === HTML_NAMESPACE) {
            return createElement(tag, typeExtension);
        } else {
            return domCreateElementNS(namespace, tag);
        }
    }
    function createElement(tag, typeExtension) {
        var definition = getRegisteredDefinition(typeExtension || tag);
        if (definition) {
            if (tag == definition.tag && typeExtension == definition.is) {
                return new definition.ctor();
            }
            if (!typeExtension && !definition.is) {
                return new definition.ctor();
            }
        }
        var element;
        if (typeExtension) {
            element = createElement(tag);
            element.setAttribute("is", typeExtension);
            return element;
        }
        element = domCreateElement(tag);
        if (tag.indexOf("-") >= 0) {
            implementPrototype(element, HTMLElement);
        }
        return element;
    }
    function cloneNode(deep) {
        var n = domCloneNode.call(this, deep);
        upgrade(n);
        return n;
    }
    var domCreateElement = document.createElement.bind(document);
    var domCreateElementNS = document.createElementNS.bind(document);
    var domCloneNode = Node.prototype.cloneNode;
    var isInstance;
    if (!Object.__proto__ && !useNative) {
        isInstance = function (obj, ctor) {
            var p = obj;
            while (p) {
                if (p === ctor.prototype) {
                    return true;
                }
                p = p.__proto__;
            }
            return false;
        };
    } else {
        isInstance = function (obj, base) {
            return obj instanceof base;
        };
    }
    document.registerElement = register;
    document.createElement = createElement;
    document.createElementNS = createElementNS;
    Node.prototype.cloneNode = cloneNode;
    scope.registry = registry;
    scope.instanceof = isInstance;
    scope.reservedTagList = reservedTagList;
    scope.getRegisteredDefinition = getRegisteredDefinition;
    document.register = document.registerElement;
});

(function (scope) {
    var useNative = scope.useNative;
    var initializeModules = scope.initializeModules;
    var isIE11OrOlder = /Trident/.test(navigator.userAgent);
    if (isIE11OrOlder) {
        (function () {
            var importNode = document.importNode;
            document.importNode = function () {
                var n = importNode.apply(document, arguments);
                if (n.nodeType == n.DOCUMENT_FRAGMENT_NODE) {
                    var f = document.createDocumentFragment();
                    f.appendChild(n);
                    return f;
                } else {
                    return n;
                }
            };
        })();
    }
    if (useNative) {
        var nop = function () { };
        scope.watchShadow = nop;
        scope.upgrade = nop;
        scope.upgradeAll = nop;
        scope.upgradeDocumentTree = nop;
        scope.upgradeSubtree = nop;
        scope.takeRecords = nop;
        scope.instanceof = function (obj, base) {
            return obj instanceof base;
        };
    } else {
        initializeModules();
    }
    var upgradeDocumentTree = scope.upgradeDocumentTree;
    if (!window.wrap) {
        if (window.ShadowDOMPolyfill) {
            window.wrap = ShadowDOMPolyfill.wrapIfNeeded;
            window.unwrap = ShadowDOMPolyfill.unwrapIfNeeded;
        } else {
            window.wrap = window.unwrap = function (node) {
                return node;
            };
        }
    }
    function bootstrap() {
        upgradeDocumentTree(wrap(document));
        if (window.HTMLImports) {
            HTMLImports.__importsParsingHook = function (elt) {
                upgradeDocumentTree(wrap(elt.import));
            };
        }
        CustomElements.ready = true;
        setTimeout(function () {
            CustomElements.readyTime = Date.now();
            if (window.HTMLImports) {
                CustomElements.elapsed = CustomElements.readyTime - HTMLImports.readyTime;
            }
            document.dispatchEvent(new CustomEvent("WebComponentsReady", {
                bubbles: true
            }));
        });
    }
    if (isIE11OrOlder && typeof window.CustomEvent !== "function") {
        window.CustomEvent = function (inType, params) {
            params = params || {};
            var e = document.createEvent("CustomEvent");
            e.initCustomEvent(inType, Boolean(params.bubbles), Boolean(params.cancelable), params.detail);
            return e;
        };
        window.CustomEvent.prototype = window.Event.prototype;
    }
    if (document.readyState === "complete" || scope.flags.eager) {
        bootstrap();
    } else if (document.readyState === "interactive" && !window.attachEvent && (!window.HTMLImports || window.HTMLImports.ready)) {
        bootstrap();
    } else {
        var loadEvent = window.HTMLImports && !HTMLImports.ready ? "HTMLImportsLoaded" : "DOMContentLoaded";
        window.addEventListener(loadEvent, bootstrap);
    }
})(window.CustomElements);

if (typeof HTMLTemplateElement === "undefined") {
    (function () {
        var TEMPLATE_TAG = "template";
        HTMLTemplateElement = function () { };
        HTMLTemplateElement.prototype = Object.create(HTMLElement.prototype);
        HTMLTemplateElement.decorate = function (template) {
            if (!template.content) {
                template.content = template.ownerDocument.createDocumentFragment();
                var child;
                while (child = template.firstChild) {
                    template.content.appendChild(child);
                }
            }
        };
        HTMLTemplateElement.bootstrap = function (doc) {
            var templates = doc.querySelectorAll(TEMPLATE_TAG);
            for (var i = 0, l = templates.length, t; i < l && (t = templates[i]) ; i++) {
                HTMLTemplateElement.decorate(t);
            }
        };
        addEventListener("DOMContentLoaded", function () {
            HTMLTemplateElement.bootstrap(document);
        });
    })();
}

(function (scope) {
    var style = document.createElement("style");
    style.textContent = "" + "body {" + "transition: opacity ease-in 0.2s;" + " } \n" + "body[unresolved] {" + "opacity: 0; display: block; overflow: hidden; position: relative;" + " } \n";
    var head = document.querySelector("head");
    head.insertBefore(style, head.firstChild);
})(window.WebComponents);

/* UMD.define */ (function (root, factory) {
    if (typeof define === 'function' && define.amd) { define([], factory); } else if (typeof exports === 'object') { module.exports = factory(); } else { root.returnExports = factory(); window.dom = factory(); }
}(this, function () {
    //  convenience library for common DOM methods
    //      dom()
    //          create dom nodes
    //      dom.style()
    //          set/get node style
    //      dom.attr()
    //          set/get attributes
    //      dom.destroy()
    //          obliterates a node
    //      dom.box()
    //          get node dimensions
    //      dom.uid()
    //          get a unique ID (not dom specific)
    //
    var
        isDimension = {
            width: 1,
            height: 1,
            top: 1,
            left: 1,
            right: 1,
            bottom: 1,
            maxWidth: 1,
            'max-width': 1,
            minWidth: 1,
            'min-width': 1,
            maxHeight: 1,
            'max-height': 1
        },
        uids = {},
        destroyer = document.createElement('div');

    function uid(type) {
        if (!uids[type]) {
            uids[type] = [];
        }
        var id = type + '-' + (uids[type].length + 1);
        uids[type].push(id);
        return id;
    }

    function isNode(item) {
        // safer test for custom elements in FF (with wc shim)
        return typeof item === 'object' && typeof item.innerHTML === 'string';
    }

    function getNode(item) {

        if (!item) { return item; }
        if (typeof item === 'string') {
            return document.getElementById(item);
        }
        // de-jqueryify
        return item.get ? item.get(0) :
            // item is a dom node
            item;
    }

    function byId(id) {
        return getNode(id);
    }

    function style(node, prop, value) {
        // get/set node style(s)
        //      prop: string or object
        //
        var key, computed;
        if (typeof prop === 'object') {
            // object setter
            for (key in prop) {
                if (prop.hasOwnProperty(key)) {
                    style(node, key, prop[key]);
                }
            }
            return null;
        } else if (value !== undefined) {
            // property setter
            if (typeof value === 'number' && isDimension[prop]) {
                value += 'px';
            }
            node.style[prop] = value;

            if (prop === 'userSelect') {
                value = !!value ? 'text' : 'none';
                style(node, {
                    webkitTouchCallout: value,
                    webkitUserSelect: value,
                    khtmlUserSelect: value,
                    mozUserSelect: value,
                    msUserSelect: value
                });
            }
        }

        // getter, if a simple style
        if (node.style[prop]) {
            if (isDimension[prop]) {
                return parseInt(node.style[prop], 10);
            }
            return node.style[prop];
        }

        // getter, computed
        computed = getComputedStyle(node, prop);
        if (computed[prop]) {
            if (/\d/.test(computed[prop])) {
                return parseInt(computed[prop], 10);
            }
            return computed[prop];
        }
        return '';
    }

    function attr(node, prop, value) {
        // get/set node attribute(s)
        //      prop: string or object
        //
        var key;
        if (typeof prop === 'object') {
            for (key in prop) {
                if (prop.hasOwnProperty(key)) {
                    attr(node, key, prop[key]);
                }
            }
            return null;
        }
        else if (value !== undefined) {
            if (prop === 'text' || prop === 'html' || prop === 'innerHTML') {
                node.innerHTML = value;
            } else {
                node.setAttribute(prop, value);
            }
        }

        return node.getAttribute(prop);
    }

    function box(node) {
        if (node === window) {
            return {
                width: node.innerWidth,
                height: node.innerHeight
            };
        }
        // node dimensions
        // returned object is immutable
        // add scroll positioning and convenience abbreviations
        var
            dimensions = getNode(node).getBoundingClientRect(),
            box = {
                top: dimensions.top,
                right: dimensions.right,
                bottom: dimensions.bottom,
                left: dimensions.left,
                height: dimensions.height,
                h: dimensions.height,
                width: dimensions.width,
                w: dimensions.width,
                scrollY: window.scrollY,
                scrollX: window.scrollX,
                x: dimensions.left + window.pageXOffset,
                y: dimensions.top + window.pageYOffset
            };

        return box;
    }

    function query(node, selector) {
        if (!selector) {
            selector = node;
            node = document;
        }
        var nodes = node.querySelectorAll(selector);

        // none found; return [] or null?
        if (!nodes.length) { return []; }

        // only one found, return single node
        if (nodes.length === 1) { return nodes[0]; }

        // multiple found; convert to Array and return it
        return Array.prototype.slice.call(nodes);

    }

    function toDom(html, options, parent) {
        // create a node from an HTML string
        var node = dom('div', { html: html });
        parent = byId(parent || options);
        if (parent) {
            while (node.firstChild) {
                parent.appendChild(node.firstChild);
            }
            return node.firstChild;
        }
        if (html.indexOf('<') !== 0) {
            return node;
        }
        return node.firstChild;
    }

    function toFrag(html) {
        var frag = document.createDocumentFragment();
        frag.innerHTML = html;
        return frag;
    }

    function dom(nodeType, options, parent, prepend) {
        // create a node
        // if first argument is a string and starts with <, it is assumed
        // to use toDom, and creates a node from HTML. Optional second arg is
        // parent to append to
        // else:
        //      nodeType: string, type of node to create
        //      options: object with style, className, or attr properties
        //          (can also be objects)
        //      parent: Node, optional node to append to
        //      prepend: truthy, to append node as the first child
        //
        if (nodeType.indexOf('<') === 0) {
            return toDom(nodeType, options, parent);
        }

        options = options || {};
        var
            className = options.css || options.className,
            node = document.createElement(nodeType);

        parent = getNode(parent);

        if (className) {
            node.className = className;
        }

        if (options.html || options.innerHTML) {
            node.innerHTML = options.html || options.innerHTML;
        }

        if (options.cssText) {
            node.style.cssText = options.cssText;
        }

        if (options.id) {
            node.id = options.id;
        }

        if (options.style) {
            style(node, options.style);
        }

        if (options.attr) {
            attr(node, options.attr);
        }

        if (parent && isNode(parent)) {
            if (prepend && parent.hasChildNodes()) {
                parent.insertBefore(node, parent.children[0]);
            } else {
                parent.appendChild(node);
            }
        }

        return node;
    }

    function destroy(node) {
        // destroys a node completely
        //
        if (node) {
            destroyer.appendChild(node);
            destroyer.innerHTML = '';
        }
    }

    function clean(node, dispose) {
        //	Removes all child nodes
        //		dispose: destroy child nodes
        if (dispose) {
            while (node.children.length) {
                destroy(node.children[0]);
            }
            return;
        }
        while (node.children.length) {
            node.removeChild(node.children[0]);
        }
    }

    function ancestor(node, selector) {
        // gets the ancestor of node based on selector criteria
        // useful for getting the target node when a child node is clicked upon
        //
        // USAGE
        //      on.selector(childNode, '.app.active');
        //      on.selector(childNode, '#thinger');
        //      on.selector(childNode, 'div');
        //	DOES NOT SUPPORT:
        //		combinations of above
        var
            test,
            parent = node;

        if (selector.indexOf('.') === 0) {
            // className
            selector = selector.replace('.', ' ').trim();
            test = function (n) {
                return n.classList.contains(selector);
            };
        }
        else if (selector.indexOf('#') === 0) {
            // node id
            selector = selector.replace('#', '').trim();
            test = function (n) {
                return n.id === selector;
            };
        }
        else if (selector.indexOf('[') > -1) {
            // attribute
            console.error('attribute selectors are not yet supported');
        }
        else {
            // assuming node name
            selector = selector.toUpperCase();
            test = function (n) {
                return n.nodeName === selector;
            };
        }

        while (parent) {
            if (parent === document.body || parent === document) { return false; }
            if (test(parent)) { break; }
            parent = parent.parentNode;
        }

        return parent;
    }

    dom.classList = {
        remove: function (node, names) {
            if (!node || !names) {
                console.error('dom.classList.remove should include a node and a className');
                return;
            }
            names = Array.isArray(names) ? names : names.indexOf(' ') > -1 ? names.trim().split(' ') : [names];
            names.forEach(function (name) {
                if (name) {
                    node.classList.remove(name.trim());
                }
            });
        },
        add: function (node, names) {
            if (!node || !names) {
                return;
            }
            names = Array.isArray(names) ? names : names.indexOf(' ') > -1 ? names.trim().split(' ') : [names];
            names.forEach(function (name) {
                if (name) {
                    node.classList.add(name.trim());
                }
            });
        },
        contains: function (node, name) {
            if (!node || !name) {
                return false;
            }
            return node.classList.contains(name);
        },
        toggle: function (node, name) {
            if (!node || !name) {
                return null;
            }
            return node.classList.toggle(name);
        }
    };

    if (!window.requestAnimationFrame) {
        dom.requestAnimationFrame = function (callback) {
            setTimeout(callback, 0);
        };
    } else {
        dom.requestAnimationFrame = function (cb) {
            window.requestAnimationFrame(cb);
        };
    }

    dom.clean = clean;
    dom.query = query;
    dom.byId = byId;
    dom.attr = attr;
    dom.box = box;
    dom.style = style;
    dom.destroy = destroy;
    dom.uid = uid;
    dom.isNode = isNode;
    dom.ancestor = ancestor;
    dom.toDom = toDom;
    dom.toFrag = toFrag;

    return dom;
}));

;/* UMD.define */ (function (root, factory) {
    if (typeof define === 'function' && define.amd) { define([], factory); } else if (typeof exports === 'object') { module.exports = factory(); } else { root.returnExports = factory(); window.on = factory(); }
}(this, function () {
    // `on` is a simple library for attaching events to nodes. Its primary feature
    // is it returns a handle, from which you can pause, resume and remove the
    // event. Handles are much easier to manipulate than using removeEventListener
    // and recreating (sometimes complex or recursive) function signatures.
    //
    // `on` is touch-friendly and will normalize touch events.
    //
    // `on` also supports a custom `clickoff` event, to detect if you've clicked
    // anywhere in the document other than the passed node
    //
    // USAGE
    //      var handle = on(node, 'clickoff', callback);
    //      //  callback fires if something other than node is clicked
    //
    // USAGE
    //      var handle = on(node, 'mousedown', onStart);
    //      handle.pause();
    //      handle.resume();
    //      handle.remove();
    //
    //  `on` also supports multiple event types at once. The following example is
    //  useful for handling both desktop mouseovers and tablet clicks:
    //
    // USAGE
    //      var handle = on(node, 'mouseover,click', onStart);
    //
    //
    // `on` has an optional context parameter. The fourth argument can be 'this'
    // (or some other object) to conveniently avoid the use of var `self = this;`
    //
    //  USAGE
    //      var handle = on(this.node, 'mousedown', 'onStart', this);
    //
    // `on.multi` allows for connecting multiple events to a node at the same
    // time. Note this requires a context (I think), so it is not applicable for
    // anonymous functions.
    //
    //  USAGE
    //      handle = on.multi(document, {
    //          "touchend":"onEnd",
    //          "touchcancel":"onEnd",
    //          "touchmove":this.method
    //      }, this);
    //
    // `on.bind` is a convenience method for binding context to a method.
    //
    // USAGE
    //      callback = on.bind(this, 'myCallback');
    //
    // `on` supports an optional ID that can be used to track connections to be
    // disposed later.
    //
    // USAGE
    //      on(node, 'click', callback, 'uid-a');
    //      on(node, 'mouseover', callback, 'uid-a');
    //      on(otherNode, 'click', callback, 'uid-a');
    //      on(document, 'load', callback, 'uid-a');
    //      on.remove('uid-a');
    //
    // `on` supports selectors, seperated from the event by a space:
    //
    // USAGE
    //      on(node, 'click div.tab span', callback);
    //

    function hasWheel() {
        var
			isIE = navigator.userAgent.indexOf('Trident') > -1,
			div = document.createElement('div');
        return "onwheel" in div || "wheel" in div ||
			(isIE && document.implementation.hasFeature("Events.wheel", "3.0")); // IE feature detection
    }

    function has(what) {
        switch (what) {
            case 'wheel': return hasWheel();
        }
        return false;
    }

    var
		isWin = navigator.userAgent.indexOf('Windows') > -1,
        FACTOR = isWin ? 10 : 0.1,
        XLR8 = 0,
        mouseWheelHandle,
        //numCalls = 0,
        keyCodes = (function () {
            // 48-57 0-9
            // 65 - 90 a-z
            var keys = new Array(46);
            keys = keys.concat([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);
            keys = keys.concat([0, 0, 0, 0, 0, 0, 0, 0, 0]);
            keys = keys.concat('a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z'.split(','));
            return keys;
        }()),
        registry = {};

    function normalizeKeyEvent(callback) {
        // Add alphanumeric property (the letter typed) to the KeyEvent
        //
        return function (e) {
            // 48-57 0-9
            // 65 - 90 a-z
            if (keyCodes[e.keyCode]) {
                e.alphanumeric = keyCodes[e.keyCode];
            }
            callback(e);
        };
    }

    function register(id, handle) {
        if (!registry[id]) {
            registry[id] = [];
        }
        registry[id].push(handle);
    }

    function makeMultiHandle(handles) {
        return {
            remove: function () {
                handles.forEach(function (h) {
                    // allow for a simple function in the list
                    if (h.remove) {
                        h.remove();
                    } else if (typeof h === 'function') {
                        h();
                    }
                });
                handles = [];
                this.remove = this.pause = this.resume = function () { };
            },
            pause: function () {
                handles.forEach(function (h) { if (h.pause) { h.pause(); } });
            },
            resume: function () {
                handles.forEach(function (h) { if (h.resume) { h.resume(); } });
            }
        };
    }

    function onClickoff(node, callback) {
        var
			isOver = false,
			mHandle,
			handle,
			lHandle = on(node, 'mouseleave', function () {
			    isOver = false;
			}),
			eHandle = on(node, 'mouseenter', function () {
			    isOver = true;
			}),
			bHandle = on(document.body, 'click', function (event) {
			    if (!isOver) {
			        callback(event);
			    }
			});

        mHandle = makeMultiHandle([lHandle, eHandle, bHandle]);

        handle = {
            resume: function () {
                setTimeout(function () {
                    mHandle.resume();
                }, 100);
            },
            pause: function () {
                isOver = false;
                mHandle.pause();
            },
            remove: function () {
                isOver = false;
                mHandle.remove();
            }
        };

        handle.pause();

        return handle;
    }

    function getNode(str) {
        if (typeof str !== 'string') {
            return str;
        }
        var node;
        if (/\#|\.|\s/.test(str)) {
            node = document.body.querySelector(str);
        } else {
            node = document.getElementById(str);
        }
        if (!node) {
            console.error('localLib/on Could not find:', str);
        }
        return node;
    }

    function normalizeWheelEvent(callback) {
        // normalizes all browsers' events to a standard:
        // delta, wheelY, wheelX
        // also adds acceleration and deceleration to make
        // Mac and Windows behave similarly
        return function (e) {
            XLR8 += FACTOR;
            var
				deltaY = Math.max(-1, Math.min(1, (e.wheelDeltaY || e.deltaY))),
				deltaX = Math.max(-10, Math.min(10, (e.wheelDeltaX || e.deltaX)));

            deltaY = deltaY <= 0 ? deltaY - XLR8 : deltaY + XLR8;

            e.delta = deltaY;
            e.wheelY = deltaY;
            e.wheelX = deltaX;

            clearTimeout(mouseWheelHandle);
            mouseWheelHandle = setTimeout(function () {
                XLR8 = 0;
            }, 300);
            callback(e);
        };
    }

    function on(node, eventType, callback, optionalContext, id) {
        //  USAGE
        //      var handle = on(this.node, 'mousedown', this, 'onStart');
        //      handle.pause();
        //      handle.resume();
        //      handle.remove();
        //
        var
			handles,
			handle,
			targetCallback,
			childTarget = false;

        if (/,/.test(eventType)) {
            // handle multiple event types, like:
            // on(node, 'mouseup, mousedown', callback);
            //
            handles = [];
            eventType.split(',').forEach(function (eStr) {
                handles.push(on(node, eStr.trim(), callback, optionalContext, id));
            });
            return makeMultiHandle(handles);
        }

        if (typeof optionalContext === 'string') {
            // no context. Last argument is handle id
            id = optionalContext;
            optionalContext = null;
        }

        node = getNode(node);
        callback = !!optionalContext ? bind(optionalContext, callback) : callback;

        if (/\s/.test(eventType)) {
            // handle child selectors, like:
            // on(node, 'click .tab span', callback);
            //
            childTarget = eventType.substring(eventType.indexOf(' ') + 1, eventType.length);
            eventType = eventType.substring(0, eventType.indexOf(' '));
            targetCallback = callback;
            callback = function (e) {
                var i, nodes, parent = on.ancestor(e.target, childTarget);
                if (parent) {
                    e.selectorTarget = parent;
                    targetCallback(e);
                } else {
                    nodes = node.querySelectorAll(childTarget);
                    for (i = 0; i < nodes.length; i++) {
                        if (nodes[i] === e.target || on.isAncestor(nodes[i], e.target)) {
                            e.selectorTarget = nodes[i];
                            targetCallback(e);
                            break;
                        }
                    }
                }
            };

        }

        if (eventType === 'clickoff') {
            // custom - used for popups 'n stuff
            return onClickoff(node, callback);
        }

        if (eventType === 'wheel') {
            // mousewheel events, natch
            if (has('wheel')) {
                // pass through, but first curry callback to wheel events
                callback = normalizeWheelEvent(callback);
            } else {
                // old Firefox, old IE, Chrome
                return on.multi(node, {
                    DOMMouseScroll: normalizeWheelEvent(callback),
                    mousewheel: normalizeWheelEvent(callback)
                }, optionalContext);
            }
        }

        if (eventType.indexOf('key') > -1) {
            callback = normalizeKeyEvent(callback);
        }


        node.addEventListener(eventType, callback, false);

        handle = {
            remove: function () {
                node.removeEventListener(eventType, callback, false);
                node = callback = null;
                this.remove = this.pause = this.resume = function () { };
            },
            pause: function () {
                node.removeEventListener(eventType, callback, false);
            },
            resume: function () {
                node.addEventListener(eventType, callback, false);
            }
        };

        if (id) {
            // If an ID has been passed, register it so it can be used to
            // remove multiple events by id
            register(id, handle);
        }

        return handle;
    }

    on.multi = function (node, map, context, id) {
        //  USAGE
        //      handle = on.multi(document, {
        //          "touchend":"onEnd",
        //          "touchcancel":"onEnd",
        //          "touchmove":this.method
        //      }, this);
        //
        var eventType,
            handles = [];

        for (eventType in map) {
            if (map.hasOwnProperty(eventType)) {
                handles.push(on(node, eventType, map[eventType], context, id));
            }
        }

        return makeMultiHandle(handles);
    };

    on.remove = function (handles) {
        // convenience function;
        // removes one or more handles;
        // accepts one handle or an array of handles;
        // accepts different types of handles (dispose/remove/topic token)
        //
        var i, h, idHandles;
        if (typeof handles === 'string') {
            idHandles = registry[handles];
            if (idHandles) {
                idHandles.forEach(function (h) {
                    h.remove();
                });
                idHandles = registry[handles] = null;
                delete registry[handles];
            }

            return [];
        }
        handles = Array.isArray(handles) ? handles : [handles];

        for (i = 0; i < handles.length; i++) {
            h = handles[i];

            if (h) { // check for nulls / already removed handles
                if (h.remove) {
                    // on handle, or AOP
                    h.remove();
                }
                else if (h.dispose) {
                    // knockout
                    h.dispose();
                }
                else if (typeof h === 'function') {
                    // custom clean up
                    h();
                }
            }
        }
        return [];
    };

    on.ancestor = function (node, selector) {
        // gets the ancestor of node based on selector criteria
        // useful for getting the target node when a child node is clicked upon
        //
        // USAGE
        //      on.selector(childNode, '.app.active');
        //      on.selector(childNode, '#thinger');
        //      on.selector(childNode, 'div');
        //	DOES NOT SUPPORT:
        //		combinations of above
        var
            test,
            parent = node;

        if (selector.indexOf('.') === 0) {
            // className
            selector = selector.replace('.', ' ').trim();
            test = function (n) {
                return n.classList.contains(selector);
            };
        }
        else if (selector.indexOf('#') === 0) {
            // node id
            selector = selector.replace('#', '').trim();
            test = function (n) {
                return n.id === selector;
            };
        }
        else if (selector.indexOf('[') > -1) {
            // attribute
            console.error('attribute selectors are not yet supported');
        }
        else {
            // assuming node name
            selector = selector.toUpperCase();
            test = function (n) {
                return n.nodeName === selector;
            };
        }

        while (parent) {
            if (parent === document.body || parent === document) { return false; }
            if (test(parent)) { break; }
            parent = parent.parentNode;
        }

        return parent;
    };

    on.isAncestor = function (parent, child) {
        // determines if parent is an ancestor of child
        // returns boolean
        //
        if (parent === child) { return false; } // do we always want the same node to be false?
        while (child) {
            if (child === parent) {
                return true;
            }
            child = child.parentNode;
        }
        return false;
    };

    on.makeMultiHandle = makeMultiHandle;

    return on;

}));

; (function () {
    // xhr.js
    //      based on: https://github.com/clubajax/base/blob/master/core/xhr.js


    function toQuery(obj) {
        var key, i, params = [];
        for (key in obj) {
            if (obj.hasOwnProperty(key)) {
                if (Array.isArray(obj[key])) {
                    for (i = 0; i < obj[key].length; i++) {
                        params.push(key + '=' + obj[key][i]);
                    }
                } else {
                    params.push(key + '=' + obj[key]);
                }
            }
        }
        return params.join('&');
    }

    function xhr(url, options) {
        var
            handleAs,
            req = new XMLHttpRequest();

        // CORS check
        //console.log('CORS:', "withCredentials" in req);

        if (typeof options === 'function') {
            options = {
                callback: options
            };
        }
        handleAs = options.handleAs || 'json';
        options.type = options.type || 'GET';
        if (options.params) {
            url += '?' + toQuery(options.params);
        }

        function callback(result) {
            if (options.callback) {
                options.callback(result, req);
            }
        }

        function errback(err) {
            console.error('XHR ERROR:', err);
            if (options.errback) {
                options.errback(err, req);
            }
            else {
                setTimeout(function () {
                    callback(err);
                }, 1);
            }
        }

        function onload(request) {
            var
                result,
                req = request.currentTarget, result, err;

            //if(req.status !== 200){
            if (!/^2/.test(req.status)) {
                err = {
                    status: req.status,
                    message: req.message || req.responseText || req.statusText,
                    request: req
                };
                try {
                    err.message = JSON.parse(err.message);
                    err.message = err.message.message || err.message;
                } catch (e) { }

                errback(err);
            }
            else {
                if (handleAs === 'json') {
                    try {
                        result = JSON.parse(req.responseText);
                    } catch (e) {
                        console.error('XHR PARSE ERROR:', req.responseText);
                        errback(e);
                    }
                    callback(result);
                }
            }
        }

        req.onload = onload;
        req.open(options.type, url, true);


        req.setRequestHeader('Content-Type', 'text/html');
        req.setRequestHeader('Accept', 'text/html');
        if (handleAs === 'json') {
            req.setRequestHeader('Accept', 'application/json');
        }

        req.withCredentials = true;

        req.send();
    }

    function upload(url, data, callback, errback, progback) {
        var
            fData = new FormData(),
            req = new XMLHttpRequest();

        Object.keys(data).forEach(function (key) {
            fData.append(key, data[key]);
        });
        if (progback) {
            req.addEventListener('progress', function (event) {

                if (event.lengthComputable) {
                    var
                        percent = Math.round(event.loaded * 100 / event.total),
                        progEvent = {
                            lengthComputable: true,
                            percent: percent,
                            loaded: event.loaded,
                            total: event.total
                        };
                    console.log('upload.progress:', progEvent);
                    progback(progEvent);
                } else {
                    progback({
                        lengthComputable: false
                    });
                }
            });
        }
        req.open("POST", url, true);
        req.setRequestHeader('Accept', 'application/json');
        req.onload = function (event) {
            event = event.currentTarget;
            if (/^2/.test(req.status)) {
                callback(event);
            }
            else {
                event.message = event.message || event.response || event.responseText || event.statusText;
                try {
                    event.message = JSON.parse(event.message);
                    event.message = event.message.message || event.message.Message;
                } catch (e) { }
                errback(event);
            }
        };

        req.send(fData);
    }

    function get(url, options) {
        return xhr(url, options);
    }

    function post(url, options) {
        options = options || {};
        options.type = 'POST';
        return xhr(url, options);
    }

    xhr.get = get;
    xhr.post = post;
    xhr.toQuery = toQuery;
    xhr.upload = upload;
    window.xhr = xhr;
}());
; (function () {
    // dates.js
    //  date helper lib
    //
    var
        // tests that it is a date string, not a valid date. 88/88/8888 would be true
        dateRegExp = /^(\d{1,2})[\/-](\d{1,2})[\/-](\d{4})/,
        // 2015-05-26T00:00:00
        tsRegExp = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/,
        daysOfWeek = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
        days = ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
        months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"],
        monthAbbr = months.map(function (month) {
            return month.substring(0, 3);
        }),
        monthLengths = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31],
        dates;

    function pad(num) {
        return num < 10 ? '0' + num : num;
    }
    function getMonth(dateOrIndex) {
        if (typeof dateOrIndex === 'number') {
            return dateOrIndex;
        }
        return dateOrIndex.getMonth();
    }
    function getMonthIndex(name) {
        var i;
        for (i = 0; i < months.length; i++) {
            if (name === months[i] || name === monthAbbr[i]) {
                return i;
            }
        }
    }
    function getMonthName(dateObject) {
        return months[getMonth(dateObject)];
    }
    function getFirstSunday(d) {
        var dt = new Date(d.getTime());
        dt.setDate(1);
        return dt.getDay() * -1;
    }
    function isLeapYear(dateObject) {
        var year = dateObject.getFullYear();
        return !(year % 400) || (!(year % 4) && !!(year % 100)); // Boolean
    }
    function getDaysInPrevMonth(dateObject) {
        var d = new Date(dateObject);
        d.setMonth(d.getMonth() - 1);
        return getDaysInMonth(d);
    }
    function getDaysInMonth(dateObject) {
        var month = dateObject.getMonth();
        if (month == 1 && isLeapYear(dateObject)) return 29;
        return monthLengths[month];
    }
    function strToDate(str) {
        var
            parts = str.split(/\/|-/),
            d = new Date(+parts[2], parseInt(parts[0], 10) - 1, +parts[1]);
        return d;
    }
    function dateToStr(d) {
        return [pad(d.getMonth() + 1), pad(d.getDate()), d.getFullYear()].join('/');
    }

    function isDateType(value) {
        if (typeof value === 'object') {
            return value instanceof Date;
        }
        // will catch timestamps as well
        return dateRegExp.test(value) || tsRegExp.test(value);
    }

    function isValidDate(d) {
        if (Object.prototype.toString.call(d) !== "[object Date]")
            return false;
        return !isNaN(d.getTime());
    }

    function isValid(value) {
        if (isDateType(value)) {
            if (typeof value === 'string') {
                var
                    bits = value.split('T')[0].split(/\/|\-/),
                    year,
                    month,
                    day,
                    date;

                if (bits[0].length === 4) {
                    // timestamp 2015-05-26T00:00:00
                    year = parseInt(bits[0], 0);
                    month = parseInt(bits[1], 0) - 1;
                    day = parseInt(bits[2], 0);
                } else {
                    // date string // 11/20/1982
                    year = parseInt(bits[2], 0);
                    month = parseInt(bits[0], 0) - 1;
                    day = parseInt(bits[1], 0);
                }

                // construct a date and ensure the numbers match, which catches
                // problems like: new Date(2000, 55, 55)
                date = new Date(year, month, day);

                return date.getFullYear() === year && date.getMonth() === month && date.getDate() === day;
            }
            else {
                if (Object.prototype.toString.call(value) !== "[object Date]")
                    return false;
                return !isNaN(value.getTime());
            }
        }
        return false;
    }

    dates = {
        months: {
            full: months,
            abbr: monthAbbr
        },
        isValid: isValid,
        isDateType: isDateType,
        getMonthIndex: getMonthIndex,
        getMonthName: getMonthName,
        getFirstSunday: getFirstSunday,
        isLeapYear: isLeapYear,
        getDaysInMonth: getDaysInMonth,
        getDaysInPrevMonth: getDaysInPrevMonth,
        strToDate: strToDate,
        dateToStr: dateToStr,
        timestamp: {
            to: function (date) {
                console.error('TO TIMESTAMP NOT IMPLEMENTED');
            },
            from: function (str) {
                // 2015-05-26T00:00:00
                var
                    parts = str.split(/\-|T|:/),
                    year = parseInt(parts[0], 0),
                    month = parseInt(parts[1], 0) - 1,
                    day = parseInt(parts[2], 0),
                    hours = parseInt(parts[3], 0),
                    minutes = parseInt(parts[4], 0),
                    seconds = parseInt(parts[5], 0),
                    date = new Date(year, month, day);

                date.setHours(hours);
                date.setMinutes(minutes);
                date.setSeconds(seconds);
                return date;
            },
            natural: function (date) {
                if (typeof date == 'string') {
                    date = this.from(date);
                }

                var
                    year = date.getFullYear().toString().substring(2),
                    month = date.getMonth() + 1,
                    day = date.getDate(),
                    hours = date.getHours(),
                    minutes = date.getMinutes(),
                    seconds = date.getSeconds(),
                    ampm = 'AM';

                if (month < 10) {
                    month = '0' + month;
                }
                if (hours > 12) {
                    hours -= 12;
                    ampm = 'PM';
                }

                return hours + ':' + minutes + ' ' + ampm + ' on ' + [month, day, year].join('/');
            }
        }
    };

    window.lib = window.lib || {};
    window.lib.dates = dates;

}());
; (function () {
    // restrict.js
    //  Adds key-restricting functionality to inputs
    //  So that letters cannot be added to a zipcode type
    //
    window.lib = window.lib || {};

    var
        pattern = 'MM/DD/YYYY',
        controlKeys = {
            8: 'backspace',
            9: 'tab',
            13: 'enter', // TODO return key
            16: 'shift',
            18: 'alt/option',
            20: 'caps',
            35: 'end',
            36: 'home',
            37: 'left',
            38: 'up',
            39: 'right',
            40: 'down',
            46: 'delete',

            116: 'f5'
        },
        keys = {
            // second nums are keypad
            dash: { 189: true, 109: true },
            slash: { 191: true, 111: true },
            period: { 190: true, 110: true },
            backspace: { 8: true, 46: true } // 46 is also fn-backspace on Mac
        };

    function paren(e) {
        //57 - 48
        return (e.target.value.indexOf('(') === -1 && e.shiftKey && e.keyCode === 57) || (e.target.value.indexOf(')') === -1 && e.shiftKey && e.keyCode === 48);
    }

    function dollarSign(e) {
        return e.target.value.indexOf('$') === -1 && e.shiftKey && e.keyCode === 52;
    }

    function checkDecimals(e) {
        // assuming 2 places for now
        return e.target.value.indexOf('.') === -1 && (e.keyCode === 190 || e.keyCode === 110);
    }

    function isAction(e) {
        return controlKeys[e.keyCode] || e.metaKey || e.ctrlKey;
    }

    function isRemoveChars(e) {
        //console.log('rem.e', e);
        // removed chars via backspace or cut
        return keys.backspace[e.keyCode] || (e.keyCode === 88 && (e.ctrlKey || e.metaKey));
    }

    window.lib.restrict = {
        currency: function (obj) {
            var node = obj.inputNode || obj;

            return on(node, 'keydown', function (e) {
                //console.log('key', e.keyCode, e.alphanumeric);
                if (isNaN(e.alphanumeric) && !dollarSign(e) && !isAction(e) && !controlKeys[e.keyCode] && !checkDecimals(e)) {
                    e.preventDefault();
                    return false;
                }
                if (e.alphanumeric !== undefined) {
                    var
                        value = node.value.toString(),
                        dotpos = value.indexOf('.'),
                        curpos = node.selectionStart,
                        declen = value.split('.')[1];

                    if (dotpos > -1) {
                        // there is a decimal-point
                        if (declen) { declen = declen.length }
                        // there are some decimal-digits
                        if (curpos - dotpos > 2) {
                            // prevent typing at the end if already 2 decimals
                            e.preventDefault();
                            return false;
                        }
                        if (curpos > dotpos && declen >= 2) {
                            on.once(node, 'keyup', function (e) {
                                node.value = node.value.substring(0, node.value.length - 1);
                                node.selectionStart = curpos; node.selectionEnd = curpos;
                            });
                        }
                    }

                }
            });
        },
        // social security number
        numericWithDash: function (obj) {
            var node = obj.inputNode || obj;
            return on(node, 'keydown', function (e) {
                if (isNaN(e.alphanumeric) && !keys.dash[e.keyCode] && !isAction(e)) {
                    e.preventDefault();
                    return false;
                }
            });
        },
        date: function (obj) {
            var node = obj.inputNode || obj;

            function adjustSelection(e) {
                var
                    key = e.alphanumeric,
                    isLeft = e.keyCode === 8 || e.keyCode === 37,
                    isDelete = e.keyCode === 46,
                    isRight = e.keyCode === 39,
                    regexp = isLeft ? /\d|\// : /\d/,
                    char,
                    beg = node.selectionStart,
                    end = node.selectionEnd;

                if (beg === end) {
                    if (isDelete) {
                        return;
                    }
                    char = node.value.charAt(beg - 1);
                    while (beg > -1 && !regexp.test(node.value.charAt(beg - 1))) {
                        //console.log('adj', beg, node.value.charAt(beg-1));
                        beg--;
                    }
                    //console.log('adj-d', node.value.charAt(beg));
                    if (node.value.charAt(beg) === '/') {
                        beg++;
                    }
                    node.selectionStart = node.selectionEnd = beg;
                }
            }

            function reformat(value) {
                var
                    i,
                    val = '',
                    reval;

                reval = value.replace(/\D/g, '');
                if (reval.length > 2) {
                    reval = reval.substring(0, 2) + '/' + reval.substring(2);
                }
                if (reval.length > 5) {
                    reval = reval.substring(0, 5) + '/' + reval.substring(5);
                }

                for (i = 0; i < pattern.length; i++) {
                    if (reval.charAt(i)) {
                        val += reval.charAt(i);
                    } else {
                        val += pattern.charAt(i);
                    }
                }
                return val;
            }

            function handleEntry(key, e) {
                var
                    val,
                    reval,
                    hasSelection = false,
                    beg = node.selectionStart,
                    end = node.selectionEnd,
                    current = node.value || pattern;

                if (beg === end) {
                    // if at the end, prevent typing
                    if (end > 9 && key !== '') {
                        e.preventDefault();
                        return false;
                    }
                    end++;
                }
                else {
                    hasSelection = true;
                }

                if (e.keyCode === 8) {
                    beg--;
                }
                val = current.substring(0, beg);
                val += key;

                val += current.substring(end);

                node.value = reformat(val);

                if (end === 2 || end === 5) {
                    end++;
                }
                if (key === '') {
                    end--;
                    end--;

                }
                node.selectionStart = node.selectionEnd = end;


                if (hasSelection) {
                    adjustSelection(e);
                }
                e.preventDefault();
                return false;
            }

            function removeBackward(e) {
                // backspace key
                var
                    val = '',
                    current = node.value,
                    beg = node.selectionStart,
                    end = node.selectionEnd;
                if (beg === end) {
                    if (current.charAt(beg - 1) === '/') {
                        beg--;
                    }
                    val += current.substring(0, beg - 1);
                    val += pattern.charAt(beg - 1);
                    val += current.substring(beg);
                    node.value = val;
                }
                else {
                    for (var i = 0; i < pattern.length; i++) {
                        if (i < beg || i >= end) {
                            val += current.charAt(i);
                        } else {
                            val += pattern.charAt(i);
                        }
                    }
                    node.value = val;
                }

                node.selectionStart = node.selectionEnd = beg - 1;
            }

            function removeForward(e) {
                // delete key
                var
                    val = '',
                    current = node.value,
                    beg = node.selectionStart,
                    end = node.selectionEnd;

                if (beg === end) {
                    if (current.charAt(beg) === '/') {
                        beg++;
                    }
                    val += current.substring(0, beg);
                    val += current.substring(beg + 1);
                    node.value = reformat(val);
                    node.selectionStart = node.selectionEnd = beg;
                }
                else {
                    for (var i = 0; i < pattern.length; i++) {
                        if (i < beg || i >= end) {
                            val += current.charAt(i);
                        } else {
                            val += pattern.charAt(i);
                        }
                    }
                    node.value = val;
                    node.selectionStart = node.selectionEnd = end;
                }
            }

            return on.makeMultiHandle([
                on(node, 'mouseup', function (e) {
                    adjustSelection(e);
                }),
                on(node, 'focus', function (e) {
                    setTimeout(function () {
                        if (node.selectionStart !== node.selectionEnd) {
                            node.selectionStart = node.selectionEnd = 100;
                            adjustSelection(e);
                        }
                    }, 1);
                }),
                on(node, 'blur', function () {
                    // if no numbers, remove the text-based placeholder
                    if (!node.value.replace(/\D/g, '')) {
                        node.value = '';
                    }
                }),
                on(node, 'paste', function (e) {
                    var
                        data,
                        dataTransfer = e.clipboardData || e.dataTransfer;

                    if (dataTransfer) {
                        data = dataTransfer.getData('text/plain');
                    } else if (window.clipboardData) {
                        data = window.clipboardData.getData('text');
                    }
                    return handleEntry(data, e);
                }),
                on(node, 'keydown', function (e) {


                    if (isRemoveChars(e)) {
                        if (e.keyCode === 8) {
                            removeBackward(e);
                        }
                        else {
                            removeForward(e);
                        }
                        e.preventDefault();
                        return false;
                    }

                    // allow meta key behavior
                    if (isAction(e)) {
                        return true;
                    }

                    // block letters and symbols
                    if (isNaN(e.alphanumeric) && !isAction(e)) {
                        e.preventDefault();
                        return false;
                    }
                    return handleEntry(e.alphanumeric, e);
                }),
                on(node, 'keyup', function (e) {

                    if (isRemoveChars(e)) {
                        // fix selection
                        adjustSelection(e);
                        e.preventDefault();
                        return false;
                    }

                    var
                        beg = node.selectionStart,
                        end = node.selectionEnd;
                    if (beg === end) {
                        // if arrowing left, skip the slash
                        if (e.keyCode === 37) {
                            if (end === 2) {
                                node.selectionStart = node.selectionEnd = 1;
                            }
                            if (end === 5) {
                                node.selectionStart = node.selectionEnd = 4;
                            }
                        }
                        // if arrowing right, skip the slash
                        if (e.keyCode === 39) { // right
                            if (end === 2) {
                                node.selectionStart = node.selectionEnd = 3;
                            }
                            if (end === 5) {
                                node.selectionStart = node.selectionEnd = 6;
                            }
                        }
                    }



                    // if meta key, allow action
                    if (isAction(e)) {
                        return true;
                    }
                    // all others, block default
                    e.preventDefault();
                    return false;
                })
            ]);
        },
        numericWithDashOrSlash: function (obj) {
            var node = obj.inputNode || obj;
            return on(node, 'keydown', function (e) {
                //console.log('key', e.keyCode, e);
                if (isNaN(e.alphanumeric) && !isAction(e) && !(keys.dash[e.keyCode] || keys.slash[e.keyCode])) {
                    e.preventDefault();
                    return false;
                }
            });
        },
        // phone
        numericWithDashOrParens: function (obj) {
            var node = obj.inputNode || obj;
            return on(node, 'keydown', function (e) {
                //console.log('key', e.keyCode, e);
                if (isNaN(e.alphanumeric) && !isAction(e) && !keys.dash[e.keyCode] && !paren(e)) {
                    e.preventDefault();
                    return false;
                }
            });
        },
        // numbers
        numeric: function (obj) {
            var node = obj.inputNode || obj;
            return on(node, 'keydown', function (e) {
                //console.log('numbers.key', e.keyCode, e);
                if (isNaN(e.alphanumeric) && !isAction(e)) {
                    e.preventDefault();
                    return false;
                }
            });
        },
        // max characters
        max: function (obj) {
            var
                node = obj.inputNode || obj,
                max = dom.attr(node, 'max');
            return on(node, 'keydown', function (e) {

                if (node.value.length >= max && !isAction(e)) {
                    e.preventDefault();
                    return false;
                }
            });
        }
    };

    window.lib.restrict.ssn = window.lib.restrict.numericWithDash;
    window.lib.restrict.zipcode = window.lib.restrict.numericWithDash;
    window.lib.restrict.phone = window.lib.restrict.numericWithDashOrParens;
}());
; (function () {
    // validation.js
    //  Adds validation capabilities to inputs
    //
    window.lib = window.lib || {};

    var
        regexps = {
            date: /^(0[1-9]|1[0-2])[\/-](0[1-9]|1\d|2\d|3[01])[\/-](19|20)\d{2}$/,
            timestamp: /\d{4}-\d{2}-\d{2}/,
            zipcode: /(^\d{5}$)|(^\d{5}-\d{4}$)|(^\d{9}$)/,
            // HR ssn:
            ssn: /^(?!\d{3}-\d{6})(?!\d{5}-\d{4})(?!000)(?!9)(?!666)[0-9]{3}-?(?!00)[0-9]{2}-?(?!0000)[0-9]{4}$/,
            // ssn SHOULD match (allowing ***):
            ssnPos: /^[\d\*]{3}-?[\d\*]{2}-?[\d\*]{4}$/,
            // ssn SHOULD match (number and dashes only):
            ssnStrict: /^\d{3}-?\d{2}-?\d{4}$/,
            // ssn should NOT match:
            ssnNeg: /^000|^666|^9|^\d{3}-?00|^\d{3}-?\d{2}-?0000/,
            // phone, yo
            phone: /\(?\d{3}\)?-?\s?\d{3}-?\d{4}/,
            // characters that should not appear in simple words, like a user name
            illegalChars: /[\!`~@#\$%\^&\*\(\)\+=\\|\]\}\[\{\"\'\:\;\/\?\>\<]/,

            // untested, unused
            email: /^[a-z0-9]+(?:[\.-]?[a-z0-9]+)*@[a-z0-9]+([-]?[a-z0-9]+)*[\.-]?[a-z0-9]+([-]?[a-z0-9]+)*([\.-]?[a-z]{2,})*(\.[a-z]{2,5})+$/i,
            // untested, unused
            website: /^http(s)?:\/\/(www\.)?[a-z0-9]+([-]?[a-z0-9]+)*[\.-]?[a-z0-9]+([-]?[a-z0-9]+)*([\.-]?[a-z]{2,})*(\.[a-z]{2,5})+$/i,

            // TODO
            // illegal HTML chars
            html: /<>\(\)=/
        },
        defaultMessages = {
            zipcode: 'Please enter a valid zip code: XXXXX [-XXXX]',
            ssn: 'Please enter a valid social security number: XXX-XX-XXXX',
            date: 'Please enter a valid date: MM/DD/YYYY',
            phone: 'Please enter a valid phone number: (XXX) XXX-XXXX',
            illegalCharacters: 'This field contains illegal characters.',
            currency: 'Please enter a valid currency: $X.XX',
            required: 'This field is required',
            max: 'Please use {{MAX}} characters or less'
        };

    function refreshing(e) {
        return e.alphanumeric === 'r' && e.metaKey;
    }

    function getErrorNode(sibling) {
        var node = dom.query(sibling.parentNode, '.field-error.icon-error');
        if (!node || (Array.isArray(node) && !node.length)) {
            node = dom('div', { css: 'field-error icon-error' }, sibling.parentNode);
        }
        return node;
    }

    function destroy(nodes) {
        if (nodes) {
            nodes = Array.isArray(nodes) ? nodes : [nodes];
            nodes.forEach(function (node) {
                dom.destroy(node);
            });
        }
    }

    function getMessage(a, b, def) {
        return typeof a === 'string' ? a : typeof b === 'string' ? b : defaultMessages[def];
    }

    function isPopulated(node) {
        var value = node.value.toString().trim();
        if (dom.attr(node, 'required') !== null) {
            if (!!value && value !== '--') {
                return true;
            }
            return defaultMessages.required;
        }
        return !!value;
    }

    function createExtValidation(node, validate) {
        node = node.inputNode || node;
        function clear() {
            node.classList.remove('invalid');
            // for components, need to remove class from data-field element
            node.parentNode.classList.remove('invalid');
            setTimeout(function () {
                destroy(getErrorNode(node));
            }, 200);
        }
        var
            handle = on(node, 'blur', function (e) {
                var
                    result = isPopulated(node);
                if (result === true) {
                    result = validate(e.target.value);
                    if (typeof result === 'string') {
                        getErrorNode(node).innerHTML = result;
                        node.classList.add('invalid');
                    }
                    else {
                        clear();
                    }
                }
                else {
                    clear();
                }
            });
        return {
            remove: handle.remove,
            validate: validate
        };
    }

    function createValidation(a, b, validate) {
        if (typeof a !== 'object') {
            // components validate DEPRECATED
            return validate;
        }
        // Ember validate
        return createExtValidation(a, validate);
    }



    window.lib.validation = {
        zipcode: function (a, b) {
            return createValidation(a, b, function (value) {
                var valid = regexps.zipcode.test(value);
                return valid ? true : getMessage(a, b, 'zipcode');
            });
        },
        ssn: function (a, b, orgValue) {
            return createValidation(a, b, function (value) {
                var valid;
                valid = regexps.ssnPos.test(value) && !regexps.ssnNeg.test(value);

                // FIXME: Does not seem to work if value was set later
                //                if(orgValue === value) {
                //                    // validation that allows ***
                //                    valid = regexps.ssnPos.test(value) && !regexps.ssnNeg.test(value);
                //                }
                //                else{
                //                    // validation that ensures numbers
                //                    valid = regexps.ssnStrict.test(value) && !regexps.ssnNeg.test(value);
                //                }
                return valid ? true : getMessage(a, b, 'ssn');
            });

        },
        date: function (a, b) {
            return createValidation(a, b, function (value) {
                var valid = lib.dates.isValid(value);
                return valid ? true : getMessage(a, b, 'date');
            });
        },
        phone: function (a, b) {
            return createValidation(a, b, function validate(value) {
                var valid = regexps.phone.test(value) && value.match(/\d/g).length === 10;
                return valid ? true : getMessage(a, b, 'phone');
            });
        },
        legalCharacters: function (a, b) {
            return createValidation(a, b, function validate(value) {
                var invalid = regexps.illegalChars.test(value);
                return !invalid ? true : getMessage(a, b, 'illegalCharacters');
            });

        },
        currency: function (a, b) {
            return createValidation(a, b, function validate(value) {
                value = ('' + value).replace('$', '');
                var
                    valid = !isNaN(parseFloat(value));
                return valid ? true : getMessage(a, b, 'currency');
            });
        },
        max: function (a, b) {
            var max = dom.attr(a, 'max');
            return createValidation(a, b, function validate(value) {
                var
                    valid = value.length <= max;
                return valid ? true : getMessage(a, b, 'max').replace('{{MAX}}', max);
            });
        },
        regexps: regexps
    };

}());
; (function () {
    // format.js
    //  Adds auto-formatting to inputs
    //
    window.lib = window.lib || {};

    function createFormatter(obj, format) {
        var
            node = obj.inputNode || obj,
            handle = on(node, 'blur', function (e) {
                format(node);
            });
        return {
            remove: handle.remove,
            format: format
        };
    }

    window.lib.format = {
        currency: function (obj) {
            return createFormatter(obj, function (node) {
                if (!node.classList.contains('invalid')) {
                    var value = node.value.toString().replace('$', '');
                    if (value) {
                        value = (parseFloat(value)).toFixed(2);
                        node.value = '$' + value;
                    }
                }
            })
        },
        date: function (obj) {
            return createFormatter(obj, function (node) {
                if (!node.classList.contains('invalid')) {
                    if (lib.validation.regexps.timestamp.test(node.value)) {
                        // 2014-12-31T05:00:00
                        node.value = lib.dates.dateToStr(lib.dates.timestamp.from(node.value));
                    }
                }
            })
        },
        ssn: function (obj) {
            return createFormatter(obj, function (node) {
                if (!node.classList.contains('invalid')) {
                    var value = node.value.toString().replace(/-/g, '');
                    if (value) {
                        node.value = [value.substring(0, 3), value.substring(3, 5), value.substring(5)].join('-');
                    }
                }
            })
        },
        phone: function (obj) {
            //(xxx) xxx-xxxx
            return createFormatter(obj, function (node) {
                if (!node.classList.contains('invalid')) {
                    var value = node.value.toString().replace(/\s|-|\.|\(|\)|\\|\//g, '');
                    if (value) {
                        node.value = '(' + value.substring(0, 3) + ') ' + value.substring(3, 6) + '-' + value.substring(6);
                    }
                }
            });
        },
        zipcode: function (obj) {
            //(xxx)-xxx-xxxx
            return createFormatter(obj, function (node) {
                if (!node.classList.contains('invalid')) {
                    var value = node.value.toString().replace(/-/g, '');
                    if (value) {
                        if (value.length === 5) {
                            return;
                        }
                        node.value = value.substring(0, 5) + '-' + value.substring(5);
                    }
                }
            });
        }
    };
}());
; (function () {

    var idMap = {};

    //    var observer = new MutationObserver(function(mutations) {
    //        mutations.forEach(function(mutation) {
    //            console.log(mutation.type);
    //        });
    //    });

    function uid(type) {
        if (!idMap[type]) {
            idMap[type] = 0;
        }
        idMap[type]++;
        return type + '-' + idMap[type];
    }

    function findMatch(contentNodes, node) {
        // match node to a content node, based on content node's sel attribute
        // Currently only supports single classNames and nodeNames
        //
        var i, noSelNode, sel;
        for (i = 0; i < contentNodes.length; i++) {
            sel = contentNodes[i].getAttribute('sel');
            if (!sel) {
                noSelNode = contentNodes[i];
            }
            else {
                if (sel.indexOf('[') > -1) {
                    throw new Error('Attribute selectors are not supported in content nodes');
                }
                if (sel.indexOf('#') > -1) {
                    throw new Error('IDs are not supported in content nodes');
                }

                if (sel.indexOf('.') > -1 && node.classList.contains(sel.substring(1))) {
                    return contentNodes[i];
                }
                else if (sel.indexOf('.') === -1 && node.nodeName.toLowerCase() === sel.toLowerCase()) {
                    return contentNodes[i];
                }
            }
        }
        return noSelNode;
    }

    function sortContentNodes(contentNodes) {
        // content nodes with the sel attribute need to be
        // ordered in priority of their selector types
        var s1, s2;
        function isClass(sel) {
            return sel.indexOf('.') > -1;
        }
        function isTag(sel) {
            return !!sel;
        }
        return Array.prototype.slice.call(contentNodes).sort(function (a, b) {
            s1 = a.getAttribute('sel');
            s2 = b.getAttribute('sel');
            if (isClass(s1)) { return -1; }
            if (isClass(s2)) { return 1; }
            if (isTag(s1)) { return -1; }
            if (isTag(s2)) { return 1; }
            return 0;
        });
    }

    function stripContentNodes(clonedTemplate) {
        // find all template  content nodes, add their children to the template,
        // then remove content nodes
        var frag, i, contentNodes = clonedTemplate.querySelectorAll('content');
        for (i = 0; i < contentNodes.length; i++) {
            frag = document.createDocumentFragment();
            while (contentNodes[i].children.length) {
                frag.appendChild(contentNodes[i].children[0]);
            }
            contentNodes[i].parentNode.insertBefore(frag, contentNodes[i]);
            contentNodes[i].parentNode.removeChild(contentNodes[i]);
        }
    }

    function convertOptionsToDefinition(options) {
        var def = {};
        Object.keys(options).forEach(function (key) {
            if (typeof options[key] === 'function') {
                def[key] = {
                    value: options[key],
                    writable: true
                };

            } else if (key === 'properties') {
                options[key].forEach(function (_prop) {
                    var
                        prop = _prop;

                    def[prop] = {
                        get: function () {
                            return this['__' + prop] || this.getAttribute(prop);
                        },
                        set: function (val) {
                            this['__' + prop] = val;
                            // will trigger attributeChanged
                            if (typeof val !== 'object') {
                                this.setAttribute(prop, this['__' + prop]);
                            }
                        }
                    };
                });

            } else if (typeof options[key] === 'object') {
                // propertyDefinition (getter/setter)
                def[key] = {};
                Object.keys(options[key]).forEach(function (k) {
                    def[key][k] = options[key][k];
                });

            } else {
                (function () {
                    var prop = options[key];
                    def[key] = {
                        get: function () {
                            return prop;
                        },
                        set: function (value) {
                            prop = value;
                        }
                    }
                }());
            }
        });
        return def;
    }

    function attachNodes(selector, context, template) {
        var
            i, name,
            attachedNodes = template.querySelectorAll('[' + selector + ']');
        for (i = 0; i < attachedNodes.length; i++) {
            name = attachedNodes[i].getAttribute(selector);
            context[name] = attachedNodes[i];
        }
    }

    function clone(template) {
        if (template.content) {
            return document.importNode(template.content, true);
        }
        var
            frag = document.createDocumentFragment(),
            clone = template.cloneNode(true);
        while (clone.children.length) {
            frag.appendChild(clone.children[0]);
        }
        return frag;
    }

    function createComponent(options) {

        var
        // Private variables for setup only
        // for registration of node
        // lifecycle variables would be shared
        //
            ATTACH_ATTR = 'data-attach',
            PLUGIN_DOMREADY = 'onDomReady',
            PLUGIN_ATTACHED = 'onAttached',
            PLUGIN_CREATED = 'onCreated',
            PLUGIN_ATTR = 'onAttributeChanged',
            template,
            tempClone,
            styleNode,
            importDoc = window.globalImportDoc || (document._currentScript || document.currentScript).ownerDocument,
            def = convertOptionsToDefinition(options),
            element,
            extOptions,
            constructor,
            callbacks = {},
            pluginAttrCallbacks = [],
            pluginAttachedCallbacks = [],
            pluginCreatedCallbacks = [],
            pluginDomReadyCallbacks = [];

        function createCallbacks(uid) {
            callbacks[uid] = {
                ready: [],
                init: null,
                properties: {},
                domstate: 'notready'
            }
        }
        if (options.templateId) {
            // get and clone the template
            template = importDoc.getElementById(options.templateId);
            tempClone = clone(template);//document.importNode(template.content, true);
            styleNode = tempClone.querySelector('style');
            if (styleNode) {
                document.head.appendChild(styleNode);
            }
        }



        extOptions = {

            insertChildrenByContent: {
                value: function (force) {
                    // pulling the children (innerHTML of this node's markup)
                    // and inserting it into the template, relative to the
                    // content element
                    //

                    if (this.noTemplate || !this.tempContentNodes) {
                        return;
                    }

                    if (this._innerNodes.length) {
                        //console.log(this._uid, this.DOMSTATE, 'abort, already done');
                        return;
                    }

                    if (!this.children.length && !force) {
                        //console.log(this._uid, this.DOMSTATE, 'abort, not forced');
                        return;

                    }

                    //console.log(this._uid, this.DOMSTATE, ' **insert**', this.children.length);
                    //console.log('this.tempContentNodes', this.tempContentNodes.length);
                    var
                    // content is a property, not the <content> element
                        content;

                    if (this.tempContentNodes.length === 1) {
                        content = this.tempContentNodes[0];
                        while (this.children.length) {
                            this._innerNodes.push(this.children[0]);
                            content.appendChild(this.children[0]);
                        }
                    }
                    else if (this.tempContentNodes.length > 1) {
                        // multiple contents with the "sel" attribute

                        while (this.children.length) {
                            this._innerNodes.push(this.children[0]);
                            content = findMatch(this.tempContentNodes, this.children[0]);
                            content.appendChild(this.children[0]);
                        }
                    }

                    stripContentNodes(this.clonedTemplate);

                    // then insert the template into this node
                    // clone is a fragment, so there will be no wrappers
                    this.appendChild(this.clonedTemplate);

                    // DEV NOTE
                    // By leaving the style in the node and inserting it, the style gets applied
                    // we can now remove the style node and the styles will remain
                    styleNode = this.querySelector('style');
                    if (styleNode) {
                        this.removeChild(styleNode);
                    }
                    //console.log('_innerNodes', this._innerNodes.length);
                    delete this.tempContentNodes;
                    delete this.clonedTemplate;
                }
            },

            createdCallback: {

                // TODO - can this be called when a child node is added?
                value: function () {

                    this._uid = uid(options.tag);
                    createCallbacks(this._uid);
                    callbacks[this._uid].domstate = 'created';

                    // Possibly make a hash map using uid to access private variables
                    this._innerNodes = [];

                    // good for debugging:
                    //console.log(' **** created', this._uid);
                    //this.setAttribute('uid', this._uid);

                    if (template) {
                        this.clonedTemplate = clone(template);//document.importNode(template.content, true);
                        this.tempContentNodes = sortContentNodes(this.clonedTemplate.querySelectorAll('content'));
                        // MOVE DOWN? or into method?
                        attachNodes(ATTACH_ATTR, this, this.clonedTemplate);
                        this.insertChildrenByContent();

                        var self = this;
                        window.requestAnimationFrame(function () {
                            self.insertChildrenByContent(true);
                        });

                    } else {
                        this.noTemplate = true;
                    }

                    if (this.created) {
                        this.created();
                    }

                    pluginCreatedCallbacks.forEach(function (callback) {
                        callback.value.call(this);
                    }, this);
                }
            },

            attachedCallback: {
                value: function () {
                    if (this.DOMSTATE === 'attached' || this.DOMSTATE === 'domready') {
                        return;
                    }
                    callbacks[this._uid].domstate = 'attached';
                    var self = this;
                    this.insertChildrenByContent(true);
                    if (element.attached) {
                        self.attached();
                    }

                    window.requestAnimationFrame(function () {

                        callbacks[self._uid].domstate = 'domready';

                        //self.insertChildrenByContent(true);

                        // Chrome will fire this version of init()
                        //console.log('Chrome INIT', self.init);
                        if (self.init) {
                            self.init(this);
                            delete self.init;
                        }

                        if (self.domReady) {
                            // WARNING! Bad!
                            // Causes illegal invocation error
                            // element.domReady.call(element);
                            self.domReady();
                        }

                        // initialize plugins
                        pluginDomReadyCallbacks.forEach(function (callback) {
                            callback.value.call(self);
                        });
                        pluginDomReadyCallbacks = [];

                        self.fire('ready');

                        callbacks[self._uid].ready.forEach(function (cb) {
                            cb();
                        });
                        callbacks[self._uid].ready = [];

                    });

                    if (options.properties) {
                        options.properties.forEach(function (prop) {
                            if (this.getAttribute(prop)) {
                                this[prop] = this.getAttribute(prop);
                            }
                        }, this);
                    }

                    this.fire('attached');
                }
            },

            attributeChangedCallback: {
                value: function (attrName, oldVal, newVal) {
                    pluginAttrCallbacks.forEach(function (callback) {
                        callback.value.call(this, attrName, oldVal, newVal);
                    }, this);
                    if (this.attributeChanged) {
                        this.attributeChanged(attrName, newVal, oldVal);
                    }
                }
            },

            detachedCallback: {
                value: function () {
                    if (this.detach) {
                        callbacks[this._uid].domstate = 'detached';
                        this.detach();
                    }
                    this.fire('detached');
                }
            },

            importDoc: {
                get: function () {
                    return importDoc;
                }
            },
            getContentNodes: {
                value: function (nodeName) {
                    if (nodeName) {
                        // filter to only nodes with this nodeName. Works around a bug that they are not filtered in the template.
                        return this._innerNodes.filter(function (node) {
                            return node.nodeName === nodeName.toUpperCase();
                        });
                    }
                    return this._innerNodes;
                }
            },
            fire: {
                value: function (eventName, eventDetail, bubbles) {
                    var event = new CustomEvent(eventName, { 'detail': eventDetail, bubbles: bubbles });
                    this.dispatchEvent(event);
                }
            },
            on: {
                value: function (eventName, callback) {
                    return on(this, eventName, callback);
                }
            },
            once: {
                value: function (eventName, callback) {
                    on.once(this, eventName, callback);
                }
            },
            onReady: {
                value: function (cb) {
                    if (this.DOMSTATE === 'domready') {
                        cb();
                        return;
                    }
                    callbacks[this._uid].ready.push(cb);
                }
            },
            DOMSTATE: {
                get: function () {
                    return callbacks[this._uid].domstate;
                }
            }
        };

        createComponent.plugins.forEach(function (plugin) {
            Object.keys(plugin).forEach(function (key) {
                if (key === PLUGIN_DOMREADY) {
                    pluginDomReadyCallbacks.push(plugin[key]);
                }
                else if (key === PLUGIN_CREATED) {
                    pluginCreatedCallbacks.push(plugin[key]);
                }
                else if (key === PLUGIN_ATTACHED) {
                    pluginAttachedCallbacks.push(plugin[key]);
                }
                else if (key === PLUGIN_ATTR) {
                    pluginAttrCallbacks.push(plugin[key]);
                }
                else {
                    extOptions[key] = plugin[key];
                }
            });
        });

        element = Object.create(HTMLElement.prototype, extOptions);

        element = Object.create(element, def);


        constructor = document.registerElement(options.nodeName || options.tag, {
            prototype: element
        });

        return constructor;
    }

    createComponent.plugins = [];

    window.createComponent = createComponent;

}());
; (function () {
    // plugin-i18n.js
    //  routes the i18n adapter into components so they have an internl this.i18n('internationalize') method
    //
    window.createComponent.plugins.push({
        i18n: {
            value: function (word) {
                return word;
            },
            writable: true
        },
        onCreated: {
            value: function () {


                var
                    tempSetContent,
                    called = false,
                    args,
                    adapter = document.querySelector('i18n-adapter');

                this.hasAdapter = !!adapter;

                if (this.setContent) {
                    if (adapter) {

                        if (!adapter.isReady()) {

                            tempSetContent = this.setContent;
                            this.setContent = function () {
                                called = true;
                                args = arguments;
                            };

                            adapter.onReady(function onAdapterReady() {
                                this.i18n = adapter.getTranslation();
                                this.setContent = tempSetContent;
                                if (called) {
                                    this.setContent.apply(this, args);
                                }
                            }.bind(this));
                        }
                        else {
                            this.i18n = adapter.getTranslation();
                        }
                    }
                }
            }
        }
    });
}());
; (function () {
    // plugin-model.js
    //      Handles a model (data object) passed to a component
    //      (the usage of this plugin is dubious and under review)
    //
    function notify(self) {
        if (this.onModel) {
            this.onModel(self.model);
        }
        if (self.model) {
            self.fire('model', { model: self.model });
        }
    }

    window.createComponent.plugins.push({
        onCreated: {
            value: function () {
                this.model = this.getModel();
                notify(this);
            }
        },
        onAttached: {
            value: function () {
                if (!this.model) {
                    this.model = this.getModel();
                    notify(this);
                }
            }
        },
        onDomReady: {
            value: function () {
                if (!this.model) {
                    this.model = this.getModel();
                    notify(this);
                }
            }
        },
        onAttributeChanged: {
            value: function (attrName, oldVal, newVal) {
                if (attrName === 'model') {
                    this.model = this.getModel();
                    notify(this);
                }
            }
        },
        getModel: {
            value: function () {
                if (this.model) {
                    return this.model;
                }
                var str = this.getAttribute('model');

                if (!str) {
                    return false;
                }
                if (str.indexOf('{') === 0 || str.indexOf('[') === 0) {
                    return JSON.parse(str);
                }
                if (this.parentNode && this.parentNode.getModel && this.parentNode.getModel(str)) {
                    // if a node gets in between the parent and the child (a popup,
                    // or an inert div) this won't work
                    return this.parentNode.getModel(str);
                }
                if (this.parentNode && this.parentNode[str]) {
                    // same problem as above
                    return this.parentNode[str];
                }
                if (window[str]) {
                    return window[str];
                }
                return false;
            }
        },

        setModel: {
            value: function (model) {
                this.model = model;
                notify(this);
            }
        }
    });
}());
; (function () {
    // keyBind.js
    //  Binds certain keyboard entries to inputs
    //  Used primarily with popups for menus/drop-downs
    //
    window.lib = window.lib || {};

    var on = window.on;

    window.lib.keyBind = function (btn, popup, show, hide) {

        function getHighlighted() {
            var node = dom.query(popup, '.hover');
            if (node.length === 0) {
                return false;
            }
            return node;

        }
        function arrowKeys(dir) {
            var
                items = dom.query(popup, 'menu-item'),
                selected = getHighlighted();//dom.query(popup, '.hover');

            if (!selected) {
                if (dir === 1) {
                    items[items.length - 1].emitHighlighted();
                } else {
                    items[0].emitHighlighted();
                }
            }
            else {
                if (dir === 1) {
                    if (selected.index === 0) {
                        items[items.length - 1].emitHighlighted();
                    } else {
                        items[selected.index - 1].emitHighlighted();
                    }
                } else {
                    if (selected.index === items.length - 1) {
                        items[0].emitHighlighted();
                    } else {
                        items[selected.index + 1].emitHighlighted();
                    }
                }
            }
        }


        return on(btn, 'keydown', function (e) {
            if (e.keyCode === 13) {
                if (popup.isOpen) {
                    var node = getHighlighted();
                    if (node) {
                        node.emitSelected();
                        setTimeout(hide, 300);
                    } else {
                        hide();
                    }
                } else {
                    show();
                }
            }
            if (e.keyCode === 27) {
                hide();
            }
            if (e.keyCode === 38) {
                arrowKeys(1);
                e.preventDefault();
                return false;
            }
            if (e.keyCode === 40) {
                arrowKeys(-1);
                e.preventDefault();
                return false;
            }
        });


    };
}());
; (function () {
    // tooltip.js
    //  Helper so that a tool-tip component can be made programmtically
    //
    window.lib = window.lib || {};

    window.lib.tooltip = function (btn, html) {
        var tooltip = dom('tool-tip', { html: html }, document.body);
        tooltip.setParent(btn);
        return tooltip;
    };
}());
; (function () {
    // loading.js
    //  Helper so that a loading-indicator component can be made programmtically
    //
    window.lib = window.lib || {};

    window.lib.loading = {
        show: function (text, showProgress) {
            if (!this.loader) {
                var options = { html: text };
                if (showProgress) {
                    options.attr = { progress: true };
                }
                this.loader = dom('loading-indicator', options, document.body);
            }
            return this.loader;
        },
        hide: function () {
            if (this.loader) {
                dom.destroy(this.loader);
                this.loader = null;
            }
        }
    }
}());
; (function () {
    // dialog.js
    //  Helper so that a modal-dialog component can be made programmtically
    //
    window.lib = window.lib || {};

    window.lib.dialog = {
        error: function (title, msg, css) {
            title = title || 'Error';
            css = 'error ' + css || 'error size350';
            this.node = dom('modal-dialog', {
                html: msg,
                attr: {
                    title: title,
                    'class': css
                }
            }, document.body);
            return this.node;
        },
        info: function () {
            console.warn('Info dialog not implemented');
        },
        show: function () {
            console.warn('Genric dialog not implemented');
        },
        hide: function () {
            this.close();
        },
        close: function () {
            if (this.node) {
                dom.destroy(this.node);
            }
        }
    }
}());

(function () {
    window.globalImportDoc = window.globalImportDoc || document.implementation.createHTMLDocument("Template Document");
    var
        htmlStr = '<template id="dnd-upload-template">' +
'    <div class="icon-documents"></div>' +
'    <div class="upload-instructions">' +
'        <div class="file-name" data-attach="filenameNode">file name</div>' +
'        <span class="drop-here" data-attach="instructionsNode">{{uploadInstructions}}</span><file-upload data-attach="fileBrowser">browse</file-upload>' +
'        <div class="file-types" data-attach="fileTypesNode">TXT</div>' +
'    </div>' +
'</template>' +
'<template id="data-grid-template">' +
'    <table data-attach="table" tabindex="1" foo="bar">' +
'        <thead data-attach="thead"></thead>' +
'        <tbody data-attach="tbody"></tbody>' +
'    </table>' +
'    <content></content>' +
'</template>' +
'<template id="menu-item-template">' +
'    <content></content>' +
'</template>',
        node = document.createElement('div');
    node.innerHTML = htmlStr;
    window.globalImportDoc.body.appendChild(node);
}());

(function () {

    function uploadFile(options) {
        var
            xhr = new XMLHttpRequest(),
            fd = new FormData(),
            fieldName = options.fieldName || 'upload_file';

        xhr.open("POST", options.url, true);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                // Every thing ok, file uploaded
                options.callback(xhr.responseText); // handle response.
            }
        };
        // the field name associated with the uploading file
        fd.append(fieldName, options.file);
        xhr.send(fd);
    }

    createComponent({
        tag: 'file-upload',

        attached: function () {
            var
                label = this.innerHTML.trim() || (this.getAttribute('label') || '').trim();

            this.innerHTML = this.i18n(label);
            this.buildInput();
        },

        reset: function () {
            if (this.fileInput) {
                this.fileInput.files = null;
                this.fileInput.value = null;
            }
        },

        buildInput: function () {
            var
                accept = this.getAttribute('accept'),
                fieldName = this.getAttribute('fieldName'),
                url = this.getAttribute('url');

            this.fileInput = dom('input', { attr: { type: 'file', accept: '' }, css: 'hidden-file-input' }, document.body);
            if (accept) {
                this.fileInput.setAttribute('accept', accept);
            }

            this.handles = [
                on(this, 'click', function () {
                    this.fileInput.click();
                }.bind(this)),
                on(this.fileInput, 'change', function () {
                    var files = this.fileInput.files;
                    if (url) {
                        for (var i = 0; i < files.length; i++) {
                            uploadFile({
                                file: files[i],
                                url: url,
                                fieldName: fieldName,
                                callback: function (result) {
                                    console.log('UPLOAD COMPLETE', result);
                                }
                            });
                        }
                    }
                    this.fire('change', { files: files });

                }.bind(this), false)
            ];
        },

        detached: function () {
            this.handles.forEach(function (h) {
                h.remove();
            });
            dom.destroy(this.fileInput);
        }
    });

}());

;
(function () {

    var
        MAX_FILE_LENGTH = 50,
        on = window.on,
        dom = window.dom,
        doc = document.documentElement;

    function createCsv(filename, rows) {
        var processRow = function (row) {
            var finalVal = '';
            for (var j = 0; j < row.length; j++) {
                var innerValue = row[j] === null ? '' : row[j].toString();
                if (row[j] instanceof Date) {
                    innerValue = row[j].toLocaleString();
                };
                var result = innerValue.replace(/"/g, '""');
                if (result.search(/("|,|\n)/g) >= 0)
                    result = '"' + result + '"';
                if (j > 0)
                    finalVal += ',';
                finalVal += result;
            }
            return finalVal + '\n';
        };

        var csvFile = '';
        for (var i = 0; i < rows.length; i++) {
            csvFile += processRow(rows[i]);
        }

        var blob = new Blob([csvFile], { type: 'text/csv;charset=utf-8;' });

        console.log('blob', blob);
        return blob;
    }

    function extension(fileName) {
        var ext = fileName.split('.');
        ext = ext[ext.length - 1];
        return ext;
    }

    function isAccepted(fileName, ac) {
        var
            valid = ac.test(extension(fileName));
        return valid;
    }

    function truncateFilename(name) {
        if (name.length <= MAX_FILE_LENGTH) {
            return name;
        }
        var
            beg = name.substring(0, 23),
            end = name.substring(name.length - 23);
        return beg + '...' + end;
    }

    createComponent({
        tag: 'dnd-upload',
        templateId: 'dnd-upload-template',

        //accept: ".csv, .txt, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.ms-excel",
        defaultFileTypes: 'TXT',
        importType: 'NA',
        uploadInstructions: 'Drop file here or',
        browseText: 'browse',
        errorMessage: 'We cannot import "{{EXT}}" file types.<br>Please select a different file.',

        setTestFile: function (name, multiArray) {
            name = name || 'test.txt';
            multiArray = multiArray || [['name1', 'city1'], ['name2', 'city2']];
            var file = createCsv(name, multiArray);
            file.name = name;
            this.onFilesSelected([file]);
        },

        reset: function () {
            if (this.fileBrowser) {
                this.fileBrowser.reset();
            }
            this.onFilesSelected();
        },

        onFilesSelected: function (files) {
            if (files && isAccepted(files[0].name, this.regExp)) {
                this.classList.add('valid');
                this.file = files[0];
            } else {
                if (files) {
                    lib.dialog.error('Upload Error', this.errorMessage.replace('{{EXT}}', extension(files[0].name)), 'size350');
                }
                this.file = null;
                this.classList.remove('valid');
            }
            this.fire('change', { value: this.file });
            this.setFileState();

        },

        attached: function () {
            function cancel(event) {
                event.stopPropagation();
                event.preventDefault();
            }

            if (navigator.userAgent.indexOf('MSIE') > -1) {
                // drag and drop does not work in IE
                this.uploadInstructions = '';
                this.browseText = 'Browse for template...';
            }

            this.instructionsNode.innerHTML = this.uploadInstructions;

            this.setFileTypes();

            this.fileBrowser.init = function () {
                dom.attr(this.fileBrowser.fileInput, 'accept', this.accept);
                this.setFileState();
            }.bind(this);

            this.handles = [
                on(this.fileBrowser, 'change', function (event) {
                    this.onFilesSelected(event.detail.files);
                }.bind(this)),
                on(doc, 'dragover', function (event) {
                    this.classList.add('hover');
                    cancel(event);
                }.bind(this)),
                on(doc, 'dragleave', function (event) {
                    this.classList.remove('hover');
                    cancel(event);
                }.bind(this)),
                on(this, 'drop', function (event) {
                    this.classList.remove('hover');
                    this.onFilesSelected(event.dataTransfer.files);
                    cancel(event);
                    return false;
                }.bind(this))
            ];

            this.initialized = true;
        },

        setFileState: function () {
            // this is a verbose way of managing state because of
            // dancing around the clickable file uploader
            // otherwise it would be a simple className change
            if (this.file) {
                this.fileBrowser.innerHTML = 'Select a different file';
                this.fileTypesNode.style.display = 'none';
                this.filenameNode.style.display = '';
                this.filenameNode.innerHTML = truncateFilename(this.file.name);
                this.instructionsNode.style.display = 'none';
            }
            else {
                this.fileBrowser.innerHTML = this.browseText;
                this.fileTypesNode.style.display = '';
                this.filenameNode.style.display = 'none';
                this.instructionsNode.style.display = '';
            }

            this.fileTypesNode.innerHTML = this.fileTypes;
        },

        setFileTypes: function () {
            this.accept = dom.attr(this, 'accept');
            if (!this.accept) {
                this.accept = this.defaultFileTypes;
            }

            // instructions text
            this.fileTypes = this.accept.toUpperCase().split(',').map(function (type, i, arr) {
                type = type.replace('.', '').trim();
                if (i === arr.length - 1 && arr.length > 1) {
                    type = 'or ' + type;
                }
                return type;
            }).join(', ');

            // the file masking test (only works for browsing, not dnd)
            this.accept = this.accept.toLowerCase().split(',').map(function (type) {
                if (!type.indexOf('.') > -1) {
                    type = '.' + type;
                }
                return type;
            }).join(',');

            // RegExp, yo
            this.regExp = new RegExp(this.accept.toUpperCase().split(',').map(function (type) {
                return type.replace('.', '').trim();
            }).join('|'), 'i');
        },

        detached: function () {
            this.handles.forEach(function (h) {
                h.remove();
            });
        }
    });

}());

;
(function () {

    createComponent({
        tag: 'data-grid',
        templateId: 'data-grid-template',

        render: function (items, columns, data) {
            this.setContent(items, columns, data);
        },

        setContent: function (items, columns, data) {
            this.fire('pre-render');
            //console.log('GRID ITEMS', items);

            this.data = data || items;
            this.items = items;


            var exclude = this.exclude = this.data.exclude || [];

            if (columns !== false) {
                columns = this.columns = columns || Object.keys(this.items[0]);
                this.renderHeader(columns);
            }
            else if (!this.columns) {
                this.columns = [];
            }

            this.renderBody(items);

            this.fire('render', { table: this.table, thead: this.thead, tbody: this.tbody });

            if (this.nextrendercallbacks) {
                this.nextrendercallbacks.forEach(function (cb) {
                    cb();
                });
                this.nextrendercallbacks = null;
            }
        },

        resize: function (box) {
            this.fire('render', { table: this.table, thead: this.thead, tbody: this.tbody });
        },

        addDataFilter: function (cb) {
            this.datafilters = this.datafilters || [];
            this.datafilters.push(cb);
        },

        // TODO i18n in closure
        renderHeader: function (columns) {
            dom.clean(this.thead, true);
            var
                self = this,
                css,
                key, label,
                thead = this.thead,
                tr = dom('tr', {}, thead);

            columns.forEach(function (col) {
                key = col.key || col;
                label = col.label === undefined ? col : col.label;
                css = col.css || col.className || '';
                dom('th', { html: '<span>' + self.i18n(label) + '</span>', css: css, attr: { 'data-field': key } }, tr);
            });
            this.fire('render-header', { thead: thead });
        },

        renderBody: function (items) {
            var
                exclude = this.exclude || [],
                columns = this.columns,
                tbody = this.tbody;

            dom.clean(tbody, true);

            if (!columns || !columns.length) {
                columns = [];
                Object.keys(items[0]).forEach(function (key) {
                    if (exclude.indexOf(key) === -1) {
                        columns.push(key);
                    }
                })
            }

            items.forEach(function (item, i) {
                var
                    html, css, key,
                    tr = dom('tr', { attr: { 'data-index': i } }, tbody);
                columns.forEach(function (col) {
                    key = col.key || col;
                    html = item[key];
                    css = key;
                    // was using some kind of convention for this - but then it breaks innerHTML with an icon
                    //                        if((''+html).indexOf('icon-') > -1 ){
                    //                            css = html;
                    //                            html = '';
                    //                        }
                    dom('td', { html: html, attr: { 'data-field': key, tabIndex: 1 }, css: css }, tr)
                });
            });
            this.fire('render-body', { tbody: tbody });

        },

        onNextRender: function (callback) {
            this.nextrendercallbacks = this.nextrendercallbacks || [];
            this.nextrendercallbacks.push(callback);
        },

        domReady: function () {
            var i, contentNodes = this.getContentNodes();
            if (contentNodes.length) {
                for (i = 0; i < contentNodes.length; i++) {
                    if (contentNodes[i].nodeType === 1 && contentNodes[i].setGrid) {
                        contentNodes[i].setGrid(this);
                    }
                }
            }

            this.parentNode.classList.add('content-self-scrolls');
        }
    });
}());


;
(function () {

    createComponent({
        tag: 'data-grid-click-handler',
        setGrid: function (grid) {
            this.grid = grid;
            this.grid.addEventListener('render', this.handleClicks.bind(this));
        },

        onBodyClick: function (event) {
            var
                index,
                item,
                emitEvent,
                cell = dom.ancestor(event.target, 'TD'),
                field = cell.getAttribute('data-field'),
                row = dom.ancestor(event.target, 'TR');
            if (!row) { return; }

            index = +(row.getAttribute('data-index'));
            item = this.grid.items[index];

            emitEvent = {
                index: index,
                cell: cell,
                row: row,
                item: item,
                field: field,
                value: item[field]
            };

            this.grid.fire('row-click', emitEvent);
        },

        onHeaderClick: function (event) {
            var
                cell = dom.ancestor(event.target, 'TH'),
                field = cell && cell.getAttribute('data-field'),
                emitEvent = {
                    field: field,
                    cell: cell
                };

            if (cell) {
                this.grid.fire('header-click', emitEvent);
            }
        },

        handleClicks: function (event) {
            if (this.bodyHandle) {
                this.bodyHandle.remove();
                this.headHandle.remove();
            }

            this.bodyHandle = on(event.detail.tbody, 'click', this.onBodyClick.bind(this));
            this.headHandle = on(event.detail.thead, 'click', this.onHeaderClick.bind(this));
        }
    });
}());


;
(function () {
    createComponent({
        tag: 'data-grid-model',
        setGrid: function (grid) {
            this.grid = grid;
            this.grid.addEventListener('sort', this.onSort.bind(this));
            this.grid.addEventListener('pagination', this.onPaginate.bind(this));
            if (this.initialData) {
                this.parseData(this.initialData);
                this.initialData = null;
            }
        },
        domReady: function () {
            this.pagination = {};
            this.sort = {};

            this.url = this.getAttribute('url');
            this.restURL = this.getAttribute('restURL');
            this.dataPropertyName = this.getAttribute('dataPropertyName');

            if (this.dataPropertyName) {
                this.data = window[this.dataPropertyName];

                // time out needed to prevent content flash
                setTimeout(function () {
                    this.parseData(this.data);
                }.bind(this), 1);
            }
            else if (this.restURL) {
                throw new Error('Not implemented');
            }
            else if (this.url) {
                this.get(this.url);
            }
        },

        get: function (url) {
            xhr(url, function (data) {
                this.data = data;
                this.parseData(this.data);
            }.bind(this));
        },

        setModel: function (data) {
            if (this.grid) {
                this.parseData(data);
            } else {
                this.initialData = data;
            }
        },

        parseData: function (data) {

            var
                defaultSort = this.grid.getAttribute('defaultSort'),
                defaultDir = this.grid.getAttribute('defaultDir') || 'desc',
                exclude = [],
                columns;

            if (data.items) {
                this.data = data;
                this.items = this.data.items || data.Items;
                exclude = this.data.exclude || exclude;
                columns = this.data.columns;
            }
            else {
                this.data = data;
                this.items = this.data;
            }
            if (defaultSort) {
                this.items = this.memorySort(defaultSort, defaultDir, this.items);
            }
            this.orgItems = [].concat(this.items);


            if (this.grid.datafilters) {
                this.grid.datafilters.forEach(function (filter) {
                    this.items = filter(this.items);
                }, this);
            }

            if (!columns) {
                columns = [];
                Object.keys(this.items[0]).forEach(function (key) {
                    if (exclude.indexOf(key) === -1) {
                        columns.push(key);
                    }
                })
            }


            if (!this.grid) {
                console.error('No grid');
                return;
            }
            this.grid.render(this.items, columns, this.data);   /// pass data as third option <---------------
            this.grid.fire('data', { data: this.data });

        },

        onSort: function (event) {
            this.grid.render(this.memorySort(event.detail.sort, event.detail.dir, this.items), false);
        },

        memorySort: function (prop, dir, items) {
            var
                bLess = dir === 'desc' ? -1 : 1,
                aLess = dir === 'desc' ? 1 : -1;

            if (!prop && !dir) {
                //console.log('item.');
                return this.orgItems;
            }

            function wordSort(a, b) {
                var n1 = a[prop].toLowerCase(), n2 = b[prop].toLowerCase();
                if (n1 < n2) {
                    return aLess;
                }
                else if (n1 > n2) {
                    return bLess;
                }
                return 0;
            }

            function numSort(a, b) {
                var n1 = +a[prop], n2 = +b[prop];
                if (n1 < n2) {
                    return aLess;
                }
                else if (n1 > n2) {
                    return bLess;
                }
                return 0;
            }

            return items.sort(function (a, b) {
                if (isNaN(+a[prop]) || isNaN(+b[prop])) {
                    return wordSort(a, b);
                }
                return numSort(a, b);
            });

        },
        onPaginate: function (event) {
            this.pagination.start = event.detail.params.start;
            this.pagination.count = event.detail.params.count;
        },
        created: function () {
            //console.log('~created callback', this.getAttribute('url'));
        },
        attached: function () {
            //console.log('~attached callback', this);
        }
    });
}());

;
(function () {

    createComponent({
        tag: 'data-grid-action',
        setGrid: function (grid) {
            this.grid = grid;
            this.grid.addDataFilter(function (items) {
                items.forEach(function (item) {
                    item.action = 'icon-pencil';
                });
                return items;
            });

            this.grid.on('row-click', function (event) {
                if (event.detail.field === 'action') {
                    this.grid.fire('action', event.detail);
                }
            }.bind(this));
        }
    });
}());

;
(function () {
    var
        icons = {
            'desc': 'icon-caret-down',
            'asc': 'icon-caret-up'
        },
        dom = window.dom;

    createComponent({
        tag: 'data-grid-sortable',
        setGrid: function (grid) {
            this.grid = grid;
            this.grid.classList.add('sortable');
            this.grid.on('render', this.handleClicks.bind(this));
            this.defaultSort = this.grid.getAttribute('defaultSort') || false;
            if (this.defaultSort) {
                this.defaultDir = this.grid.getAttribute('defaultDir') || 'desc';
                this.grid.onNextRender(this.setDefaultSort.bind(this));
            }
        },

        handleClicks: function () {
            if (this.clickHandle) {
                this.clickHandle.remove();
            }
            this.clickHandle = this.grid.on('header-click', this.onHeaderClick.bind(this));
        },

        setDefaultSort: function () {
            var node = this.getNodeByField(this.defaultSort);
            this.currentDir = this.defaultDir;
            this.currentCell = node;
            this.currentField = this.defaultSort;
            this.currentCell.classList.add(icons[this.currentDir]);
        },


        setSort: function (nodeOrField, dir) {
            var
                field,
                node,
                sortEvent = {};

            if (typeof nodeOrField === 'string') {
                field = nodeOrField;
                node = this.getNodeByField(nodeOrField);
            } else {
                node = nodeOrField;
                field = node.getAttribute('data-field');
            }
            if (this.currentCell && this.currentDir) {
                this.currentCell.classList.remove(icons[this.currentDir]);
            }

            this.currentDir = dir || '';
            if (!this.currentDir) {
                this.currentDir = this.defaultDir;
                field = this.defaultSort;
                node = this.getNodeByField(field);
            }
            this.currentCell = node;
            this.currentField = field;
            this.grid.onNextRender(function () {
                if (this.currentCell) {
                    this.currentCell = this.getNodeByField(dom.attr(this.currentCell, 'data-field'));
                    if (this.currentDir) {
                        this.currentCell.classList.add(icons[this.currentDir]);
                    }
                }
            }.bind(this));

            sortEvent = {
                dir: this.currentDir,
                sort: this.currentDir ? field : ''
            };
            this.grid.fire('sort', sortEvent);
        },

        onHeaderClick: function (event) {
            var
                sort = 'desc',
                field = event.detail.field,
                target = event.detail.cell;

            if (!target || target.className.indexOf('no-sort') > -1) {
                return;
            }
            if (field === this.currentField) {
                if (this.currentField === this.defaultSort) {
                    if (this.currentDir === 'desc') {
                        sort = 'asc';
                    } else {
                        sort = 'desc';
                    }
                } else {
                    if (this.currentDir === 'desc') {
                        sort = 'asc';
                    }
                    else if (this.currentDir === 'asc') {
                        sort = '';
                    }
                    else {
                        sort = 'desc';
                    }
                }
            }
            this.setSort(target, sort);
        },

        getNodeByField: function (field) {
            this.cells = this.getHeaderCells();
            return this.cells.map[field];
        },

        getHeaderCells: function () {
            this.cells = [];
            this.cells.map = {};
            var
                i,
                field,
                cells = this.grid.thead.getElementsByTagName('TH');
            for (i = 0; i < cells.length; i++) {
                field = cells[i].getAttribute('data-field');
                if (field) {
                    this.cells.push(cells[i]);
                    this.cells.map[field] = cells[i];
                }
            }
            return this.cells;
        }
    });
}());


;
(function () {

    function normalize(attr, emptyIsTrue) {
        if (typeof attr === 'string') {
            if (!attr) {
                return !!emptyIsTrue;
            }
            if (attr === 'true') {
                return true;
            }
            if (attr === 'false') {
                return false;
            }
            return +attr;
        }
        return attr;
    }
    createComponent({
        tag: 'menu-item',
        properties: ['label', 'value', 'selected'], //////////////???????????
        created: function () {
            on(this, 'model', function (event) {
                if (this.model) {
                    this.link = this.createByModel();
                }
            }.bind(this));
        },

        attached: function () {

            if (this.model) {
                return;
            }
            var
                value = this.getAttribute('value'),
                selected = normalize(this.getAttribute('selected'), true),
                content = this.innerHTML.trim() || this.getAttribute('label'),
                src = this.getAttribute('src') || this.getAttribute('url') || this.getAttribute('href');

            this.setContent(content, src, selected);

            this.handle = on.makeMultiHandle([
                this.on('click', this.emitSelected.bind(this)),
                this.overHandle = this.on('mouseover', this.emitHighlighted.bind(this)),
                this.outHandle = this.on('mouseout', function () {
                    this.fire('highlighted', { value: false }, true);
                }.bind(this))
            ]);
        },

        emitSelected: function () {
            var
                value = this.getAttribute('value'),
                val = this.model ? this.model : value ? value : this;
            this.fire('selected', { value: val }, true);
        },

        emitHighlighted: function () {
            var
                value = this.getAttribute('value'),
                val = this.model ? this.model : value ? value : this;
            this.fire('highlighted', { value: val }, true);
        },

        setContent: function (content, src, selected, newWindowIcon) {

            // wipe out the potential non-model or pre-parse content
            this.innerHTML = '';

            if (content) {
                content = this.i18n(content);
            }

            if (src) {
                this.link = dom('a', {
                    html: content,
                    attr: {
                        'href': src === '#' ? 'javascript:void(0);' : src
                    }
                }, this);


                if (newWindowIcon) {
                    dom('a', {
                        css: 'icon-share',
                        attr: {
                            'href': this.model.Url === '#' ? 'javascript:void(0);' : this.model.Url,
                            target: 'blank',
                            title: 'Open this link in a new window'
                        }
                    }, this);
                    this.parentNode.classList.add('has-new-window');
                }
            }
            else {
                this.link = dom('span', { html: content }, this);
            }

            this.setSelected(selected);
        },

        setSelected: function (selected) {
            if (selected) {
                this.setAttribute('selected', selected);
                this.classList.add('selected');
            } else {
                this.removeAttribute('selected', selected);
                this.classList.remove('selected');
            }
        },

        setHighlighted: function (hover) {
            if (hover) {
                this.classList.add('hover');
            } else {
                this.classList.remove('hover');
            }
        },

        createByModel: function () {

            var
                i,
                child;

            this.setContent(this.model.Title, this.model.Url, this.model.selected, this.model.ShowNewWindowIcon);

            // better if this was created on the fly?
            if (this.model.Children && this.model.Children.length) {
                this.popup = dom('pop-up');
                for (i = 0; i < this.model.Children.length; i++) {
                    child = dom('menu-item', {}, this.popup);
                    child.setModel(this.model.Children[i]);
                }
                document.body.appendChild(this.popup);
                this.popup.bindButton(this);

                this.setPopupClass();
            }
        },

        setPopupClass: function (className) {
            this.popupClassName = className || this.popupClassName;
            if (this.popup && this.popupClassName) {
                this.popup.classList.add(this.popupClassName);
            }
        },

        detached: function () {
            this.handle.remove();
            if (this.popup) {
                this.popup.detach();
            }
        }
    });
}());

;
(function () {

    function getSizes(popup, btn, callback) {
        popup.classList.remove('animateable');
        popup.classList.remove('animateable-up');
        popup.style.height = 'auto';
        popup.style.display = 'block';
        window.requestAnimationFrame(function () {
            callback(dom.box(popup), dom.box(btn));
        });
    }

    function addScrollers(popup, popSize) {
        var timer, speed = 30;
        function scroll(dir) {
            if (!dir && timer) {
                clearInterval(timer);
                timer = null;
            }
            else if (dir === 1 && !timer) {
                timer = setInterval(function () {
                    popup.scrollTop = popup.scrollTop - 10;
                }, speed);
            }
            else if (dir === -1 && !timer) {
                timer = setInterval(function () {
                    popup.scrollTop = popup.scrollTop + 10;
                }, speed);
            }
        }

        popup.scrollHandle = on(popup, 'mousemove', function (e) {
            var
                node = e.target.parentNode,
                // y includes page scroll
                popY = popSize.y,
                // pageY is top of page, not viewport (so it includes scroll)
                y = e.pageY;

            if (y - popY < 20) {
                scroll(1);
            }
            else if ((popY + popSize.scrollHeight) - y < 20) {
                scroll(-1);
            }
            else {
                scroll(0);
            }
        });

        popup.offHandle = on(popup, 'mouseleave', function (e) {
            scroll(0);
        });

        return {
            remove: function () {
                popup.scrollHandle.remove();
                popup.scrollHandle = null;
                popup.offHandle.remove();
                popup.offHandle = null;
                scroll(0);
            }
        }
    }

    function handlePopupToggle(btn, popup, options) {

        var
            scroller,
            popSize,
            btnSize,
            clickoffHandle,
            handles,
            multiHandle,
            overBtn = false,
            overPopup = false,
            showing = false;

        function show() {
            if (showing) {
                return;
            }

            function onReadyToShow() {
                if (!overBtn && !overPopup) {
                    // mouse leave during timeout
                    return;
                }
                var
                    scrollPad = 60,
                    selected,
                    style,
                    height = popSize.height,
                    minBotHeight = 300,
                    isTop = false,
                    win = dom.box(window),
                    box = dom.box(btn),
                    x = box.x,
                    y = box.y,
                    btnBtm = y + box.height,
                    topSpace = y,
                    botSpace = win.height - btnBtm;

                if (popSize.width + x > win.width) {
                    // if going off screen to the right...
                    x = box.x + box.width - popSize.width;
                }

                if (botSpace < minBotHeight && topSpace > botSpace) {
                    // pop up on top of button
                    isTop = true;
                }

                if (isTop) {
                    y = box.y - box.height - popSize.height - 3;
                    if (height > y) {
                        // popup is too large, make it scroll
                        height = y - scrollPad;
                        scroller = addScrollers(popup, popSize);
                    }
                    popup.classList.add('animateable-up');
                }
                else {
                    if (height > (win.height - btnBtm)) {
                        // popup is too large, make it scroll
                        height = win.height - btnBtm - scrollPad;
                        scroller = addScrollers(popup, popSize);
                    }
                    popup.classList.add('animateable');
                }

                popSize.scrollHeight = height;

                style = {
                    height: height,
                    opacity: 1,
                    left: x,
                    top: y + box.h
                };

                if (options.minWidth !== false) {
                    style.minWidth = btnSize.width;
                }

                dom.style(popup, style);
                showing = true;
                if (clickoffHandle) {
                    clickoffHandle.resume();
                }
                multiHandle.isOpen = true;
                if (scroller) {
                    selected = dom.query(popup, '.selected');
                    if (selected && !Array.isArray(selected)) {
                        selected.scrollIntoView();
                    }
                }
                popup.fire('open');
            }

            // size needs to be async for FF
            getSizes(popup, btn, function (_popSize, _btnSize) {
                popSize = _popSize;
                btnSize = _btnSize;
                dom.style(popup, {
                    height: 0,
                    opacity: 0
                });
                window.requestAnimationFrame(onReadyToShow);
            });
        }

        function hide(force) {
            if (window.keepPopupOpen) { return; }
            dom.style(popup, {
                height: 0,
                opacity: 0
            });
            showing = false;
            overBtn = false;
            overPopup = false;
            if (clickoffHandle) {
                clickoffHandle.pause();
            }

            setTimeout(function () {
                dom.style(popup, {
                    display: 'none'
                });
            }, 300);
            if (scroller) {
                scroller.remove();
            }
            multiHandle.isOpen = false;
            popup.fire('close');
        }


        if (options.clickToOpen) {
            clickoffHandle = on(popup, 'clickoff', function () {
                hide(true);
            });
            handles = [
                on(btn, 'click', function () {
                    if (showing) {
                        hide(true);
                    } else {
                        overBtn = true;
                        show();
                    }
                }),
                lib.keyBind(btn, popup, function () {
                    overBtn = true;
                    show();
                }, hide),
                on(popup, 'click', function () {
                    if (options.clickPopupClose !== false) {
                        overPopup = false;
                        hide();
                    }
                }),
                clickoffHandle
            ];
        }
        else {
            handles = [
                on(btn, 'mouseenter', function () {
                    overBtn = true;
                    show();
                }),
                on(btn, 'mouseleave', function () {
                    overBtn = false;
                    setTimeout(function () {
                        if (!overBtn && !overPopup) {
                            hide();
                        }
                    }, 150);
                }),
                on(popup, 'mouseenter', function () {
                    overPopup = true;
                    show();
                }),
                on(popup, 'mouseleave', function () {
                    overPopup = false;
                    setTimeout(function () {
                        if (!overBtn && !overPopup) {
                            hide();
                        }
                    }, 150);
                })
            ];
        }

        multiHandle = on.makeMultiHandle(handles);
        multiHandle.close = hide;
        multiHandle.isOpen = false;
        return multiHandle;
    }

    createComponent({
        tag: 'pop-up',

        bindButton: function (btn, options) {
            this.opener = btn;
            this.handle = handlePopupToggle(this.opener, this, options || {});

            var children = dom.query(this, 'menu-item');
            children = Array.isArray(children) ? children : [children];
            children.forEach(function (menuItem, i) {
                menuItem.index = i;
            });
        },

        close: function () {
            this.handle.close(true);
        },

        isOpen: {
            get: function () {
                return this.handle.isOpen;
            }
        },

        disabled: {
            get: function () {
                return this.__disabled;
            },
            set: function (value) {
                this.__disabled = value;
                //console.warn('disable popup not implemented');
                //                    if(value){
                //                        this.handle.pause();
                //                    }else{
                //                        this.handle.resume();
                //                    }
            }
        },

        detached: function () {
            this.handle.remove();
        }
    });
}());

;
(function () {

    function isEqual(v1, v2) {
        // check if equal as strings or numbers
        return ('' + v1) === ('' + v2) || (+v1) === (+v2);
    }

    createComponent({
        tag: 'pop-up-list',

        setModel: function (items) {
            this.render(items);
        },

        render: function (items) {
            var i, menuItem;
            if (this.popup) {
                this.popup.detached();
            }
            this.popup = dom('pop-up', {}, document.body);
            this.menuItems = [];

            for (i = 0; i < items.length; i++) {
                menuItem = dom('menu-item', {
                    attr: {
                        label: items[i].label,
                        value: items[i].value,
                        selected: items[i].selected
                    }
                }, this.popup);
                this.menuItems.push(menuItem);
            }

            this.handles = [
                this.popup.on('selected', this.onSelect.bind(this)),
                this.popup.on('highlighted', this.onHighlight.bind(this)),
                this.popup.on('close', this.onHighlight.bind(this))
            ];
        },

        reset: function () {
            this.menuItems.forEach(function (node) {
                node.setSelected(false);
            });
        },

        setSelected: function (value) {
            this.menuItems.forEach(function (menuItem) {
                menuItem.setSelected(menuItem.value === value);
            }, this);
        },

        onSelect: function (e) {
            // this doesn't bubble because pop-up-list is a CHILD of pop-up
            this.fire('selected', { value: e.detail.value });
        },

        onHighlight: function (event) {
            this.highlighted = null;
            var value = event.detail ? event.detail.value : false;
            this.menuItems.forEach(function (menuItem, i) {
                menuItem.setHighlighted(isEqual(menuItem.value, value));
                if (isEqual(menuItem.value, value)) {
                    this.highlighted = menuItem;
                    this.highlightedValue = value;
                }
            }, this);
        },

        bindButton: function (btn, options) {
            return this.popup.bindButton(btn, options);
        },

        close: function () {
            //this.handle.close(true);
        },

        isOpen: {
            get: function () {
                return this.popup.isOpen;
            }
        },

        detached: function () {
            if (this.popup) {
                this.popup.detached();
            }
            if (this.handles) {
                this.handles.forEach(function (h) {
                    h.remove();
                });
            }
        }
    });
}());

;
(function () {

    var defaultPlaceholderText = 'Select One';

    function isEqual(v1, v2) {
        // check if equal as strings or numbers
        return ('' + v1) === ('' + v2) || (+v1) === (+v2);
    }

    createComponent({
        tag: 'drop-down',
        properties: ['value', 'name'],
        created: function () {

            this.items = [];
            this.on('model', this.render.bind(this));
        },

        value: {
            get: function () {
                return this.getSelected().value;
            },
            set: function (val) {
                this.setSelected(val);
            }
        },

        disabled: {
            get: function () {
                return this.__disabled;
            },
            set: function (value) {
                this.__disabled = value;
                if (value) {
                    this.classList.add('disabled');
                } else {
                    this.classList.remove('disabled');
                }
                if (this.popup) {
                    this.popup.disabled = value;
                }
            }
        },

        setModel: function (data) {
            this.items = data;
            this.render();
        },

        setContent: function (label) {
            this.innerHTML = label;
        },

        attached: function () {
            this.render();
        },

        domReady: function () {
            this.render();
        },

        render: function () {
            if (this.initialized) { return; }
            if (!this.items.length && this.children.length) {
                // collect items array from child nodes
                var i, items = [], label;
                for (i = 0; i < this.children.length; i++) {
                    label = dom.attr(this.children[i], 'label') || this.children[i].innerHTML;
                    items.push({
                        label: label,
                        value: this.children[i].value,
                        selected: this.children[i].selected
                    });
                }
                this.items = items;
            }

            if (!this.items.length) {
                return;
            }

            this.placeholder = dom.attr(this, 'placeholder');
            this.innerHTML = '';

            this.createPopup(this.items);
            var selected = this.getSelected();
            selected = !selected ? false : selected.value;
            this.setSelected(selected);
            this.initialized = true;
            if (this.initialValue) {
                this.setSelected(this.initialValue);
            }
            if (!dom.attr(this, 'tabindex')) {
                dom.attr(this, 'tabindex', 0);
            }
        },

        reset: function () {
            this.items.forEach(function (item) {
                item.selected = false;
            });
            this.popup.reset();
            this.setSelected(false, true);
        },

        setSelected: function (value, silent) {
            var selected;

            if (value === undefined || value === null || value === false) {
                // no value (not zero)
                if (this.placeholder) {
                    // use the placeholder!
                    this.setContent(this.placeholder);
                    return;
                }
                // default to first item
                value = this.items[0].value;
            }
            else if (!this.initialized) {
                // not ready
                this.initialValue = value;
                this.setContent(defaultPlaceholderText);
                return;
            }

            // find selected item based on value
            this.items.forEach(function (item) {
                item.selected = isEqual(item.value, value);
                if (isEqual(item.value, value)) {
                    selected = item;
                }
            });

            // set popup selection
            this.popup.setSelected(value);

            // set button to selected item label
            this.setContent(selected.label);

            // fire event
            if (this.initialized && !silent) {
                this.fire('change', { value: this.value });
            }
        },

        getSelected: function () {
            // side-effecty...
            var selected = this.items.filter(function (item) { return item.selected; })[0];
            if (!selected && this.placeholder) {
                return false;
            }
            if (!selected && !this.placeholder && this.items.length) {
                this.items[0].selected = true;
                selected = this.items[0];
            }
            return selected || {};
        },

        createPopup: function (items) {

            if (this.popup) {
                this.popup.detached();
            }

            this.getSelected();

            if (items && items.length) {
                this.popup = dom('pop-up-list');
                this.popup.setModel(items);
                this.popup.bindButton(this, { clickToOpen: true });
            }
            this.handles = [
                this.popup.on('selected', function (event) {
                    var value = event.detail.value;
                    this.setSelected(value);
                }.bind(this))
            ];
        },

        attributeChanged: function (name, value) {
            this[name] = value;
        },

        detached: function () {
            if (this.popup) {
                this.popup.detached();
            }
            if (this.handles) {
                this.handles.forEach(function (h) {
                    h.remove();
                });
            }
        }
    });
}());

;
(function () {

    var aniTime;
    function getAnimationTime(node) {
        if (!aniTime) {
            aniTime = parseFloat(getComputedStyle(node)["transition-duration"]);
            aniTime *= 1000;
        }
        return aniTime;
    }
    createComponent({
        tag: 'modal-dialog',
        // The class applied to the executable button; whether it be
        // Save, Ok, etc.
        execClassName: 'blue',

        created: function () {
            this.isError = false;
            this.buttons = ['Cancel', 'Save'];
            this.buttonClasses = ['cancel icon-remove', 'blue icon-ok'];
            this.buttonActions = ['cancel', 'exec'];
        },

        domReady: function () {
            this.setContent();
        },

        setContent: function () {
            var
                disableWhenInvalid = this.getAttribute('disableWhenInvalid') === "true",
                buttons = this.getAttribute('buttons'),
                buttonClasses = this.getAttribute('buttonClasses'),
                buttonActions = this.getAttribute('buttonActions'),
                className = this.className,
                title = this.getAttribute('title'),
                description = this.getAttribute('description'),
                child = this.children[0],
                content = this.innerHTML.trim();

            this.removeAttribute('title');
            this.className = '';

            if (className.indexOf('error') > -1) {
                buttons = 'OK';
                buttonClasses = 'blue';
                buttonActions = 'exec';
                this.isError = true;
            }
            if (buttons) {
                buttons = buttons.split(',');
            }
            if (buttonClasses) {
                buttonClasses = buttonClasses.split(',');
            }
            if (buttonActions) {
                buttonActions = buttonActions.split(',');
            }
            this.buttonActions = buttonActions = buttonActions || this.buttonActions;
            this.buttonClasses = buttonClasses = buttonClasses || this.buttonClasses;
            this.buttons = buttons = buttons || this.buttons;

            if (content && child && content.indexOf('<') === 0) {
                content = child;
            }
            else if (content) {
                this.innerHTML = '';
                if (content.indexOf('<') === 0) {
                    content = dom.toDom(content);
                }
            }
            else {
                // stop. Expecting to be called imparitively
                this.style.display = 'none';
                return;
            }
            this.build({
                description: description,
                buttons: buttons,
                buttonClasses: buttonClasses || this.buttonClasses,
                content: content,
                title: title,
                className: className,
                disableWhenInvalid: disableWhenInvalid
            });
            this.open();
        },

        build: function (options) {
            var
                modalCss = 'modal-box ' + (options.className || '');

            if (this.buttonHandlers) {
                this.buttonHandlers.remove();
            }
            this.buttonHandlers = [];
            this.modal = dom('div', { css: modalCss }, this);
            if (options.title) {
                this.titleNode = dom('div', { css: 'modal-title' }, this.modal);
                dom('span', { html: options.title }, this.titleNode);
                dom('span', { css: 'close icon-remove' }, this.titleNode);
            }
            if (options.description) {
                dom('div', { css: 'modal-description', html: options.description }, this.modal);
            }
            this.bodyNode = dom('div', { css: 'modal-body' }, this.modal);
            this.buttonsNode = dom('div', { css: 'modal-button-bar' }, this.modal);
            this.buildButtons(options.buttons, options.buttonClasses);
            if (this.isError) {
                dom('div', { css: 'icon-error large' }, this.bodyNode);
            }
            if (typeof options.content === 'string') {
                this.bodyNode.appendChild(dom.toDom(options.content));
            } else {
                this.bodyNode.appendChild(options.content);
            }
            this.contentNode = this.bodyNode.children[0];
            if (options.disableWhenInvalid) {
                this.watchValidation();
            }

            this.buttonHandlers.push(on(this, 'click', function (event) {
                var
                    canCloseAttr = this.getAttribute('canClose'),
                    canClose = canCloseAttr !== false && canCloseAttr !== 'false',
                    close = /close|cancel/,
                    css = event.target.className,
                    isButton = this.buttonNodes.indexOf(event.target) > -1,
                    isCloseButton = close.test(css),
                    isActionButton = this.execButtons.indexOf(event.target) > -1,
                    exec = event.target.classList.contains(this.execClassName);

                if (isButton || isCloseButton) {
                    this.fire('action');
                    if (isCloseButton) {
                        this.fire('cancel');
                        this.close();
                    }
                    else if (isActionButton) {
                        this.fire(this.getActionForButton(event.target));
                        if (canClose) {
                            this.close();
                        }
                    }
                }

            }));

            this.classList.add('modal-overlay');

        },

        buildButtons: function (buttons, classes) {
            buttons = buttons || this.buttons;
            classes = classes || this.buttonClasses;
            this.buttonNodes = [];
            console.log('buttons', buttons);
            buttons.forEach(function (btn, i) {
                var css = 'btn ' + classes[i];
                this.buttonNodes.push(dom('button', { css: css, html: this.i18n(btn) }, this.buttonsNode));
            }, this);
            this.getExecButtons();
        },

        getExecButtons: function () {
            if (!this.execButtons) {
                var
                    btnNodes = this.buttonNodes,
                    buttons = [];
                if (this.buttonActions.length !== btnNodes.length) {
                    console.warn('If not using 2 buttons, a buttonsAttribute should be used');
                }
                this.buttonActions.forEach(function (action, i) {
                    if (action) {
                        buttons.push(btnNodes[i]);
                    }
                });
                this.execButtons = buttons;
            }
            return this.execButtons;
        },

        getActionForButton: function (node) {
            for (var i = 0; i < this.buttonNodes.length; i++) {
                if (this.buttonNodes[i] === node) {
                    return this.buttonActions[i];
                }
            }
        },

        watchValidation: function () {
            var execButton = dom.query(this.buttonsNode, '.' + this.execClassName);
        },

        open: function () {
            this.style.display = 'block';
            window.requestAnimationFrame(function () {
                this.classList.add('modal-animate-in');
            }.bind(this));
        },

        close: function () {
            this.classList.remove('modal-animate-in');
            setTimeout(function () {
                //this.style.display = 'none';
                dom.destroy(this);
            }.bind(this), getAnimationTime(this.modal));
        },

        detached: function () {
            if (this.buttonHandlers) {
                this.buttonHandlers.remove();
            }
        }
    });

}());

;
(function () {

    createComponent({
        tag: 'busy-button',
        properties: ['busy'],
        created: function () {

        },

        attached: function () {
            var
                className = this.className,
                html = this.innerHTML || dom.attr(this, 'label');
            this.innerHTML = '';
            this.className = '';
            this.buttonNode = dom('button', { html: html, css: className }, this);
        },

        setBusyState: function () {
            if (this.busy) {
                this.classList.add('isBusy');
                this.buttonNode.classList.add('icon-spinner');
                this.buttonNode.disabled = true;
            }
            else {
                this.classList.remove('isBusy');
                this.buttonNode.classList.remove('icon-spinner');
                this.buttonNode.disabled = false;
            }
        },

        busy: {
            set: function (val) {
                this.__busy = val;
                if (!this._isAtrrSync) {
                    this._isAtrrSync = true;
                    this.setAttribute('busy', val);
                    this._isAtrrSync = false;
                }
                this.setBusyState();
            },
            get: function () {
                return this.__busy;
            }
        },

        attributeChanged: function (attName, value) {
            if (attName === 'busy') {
                if (!this._isAtrrSync) {
                    this._isAtrrSync = true;
                    this.busy = value;
                    this._isAtrrSync = false;
                }
            }
        },

        domReady: function () {

        },

        detached: function () {

        }
    });

}());

;
(function () {

    createComponent({
        tag: 'loading-indicator',
        loadingText: 'loading...',
        attached: function () {
            console.log('ATRR', dom.attr(this, 'progress'));
            var
                progress = dom.attr(this, 'progress') !== null,
                txt = this.innerHTML.trim() || this.loadingText,
                displayNode = dom('div', { css: 'loading-display' });

            this.innerHTML = '';

            dom('div', { css: 'icon-spinner' }, displayNode);
            dom('div', { css: 'loading-text', html: txt }, displayNode);

            if (progress) {
                this.progressBar = dom('progress-bar', {}, displayNode);
            }
            this.appendChild(displayNode);

            window.requestAnimationFrame(function () {
                if (this.DOMSTATE !== 'detached') {
                    this.classList.add('animate');
                }
            }.bind(this));
        },
        value: {
            set: function (value) {
                if (this.progressBar) {
                    this.progressBar.value = value;
                }
            },

            get: function () {
                return this.progressBar ? this.progressBar.value : null;
            }
        }

    });

}());

;
(function () {

    createComponent({
        tag: 'progress-bar',

        created: function () {
            this.bar = dom('div', { css: 'indicator' }, this);
        },

        value: {
            set: function (value) {
                value = Math.max(0, value || 0);
                value = Math.min(100, parseInt(value, 10));
                this.__value = value;
                dom.style(this.bar, 'width', this.__value + '%');
            },

            get: function () {
                return this.__value || 0;
            }
        }
    });

}());
