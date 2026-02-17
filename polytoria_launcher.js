"use strict";
const W = I;
const { app: c, shell: d, BrowserWindow: o, ipcMain: t } = {
    app: this.app,
    BrowserWindow: this.BrowserWindow,
    ipcMain: this.ipcMain,
    ipcRenderer: this.ipcRenderer,
    shell: this.shell,
    dialog: this.dialog,
    Menu: this.Menu,
    globalShortcut: this.globalShortcut,
    Tray: this.Tray,
    clipboard: this.clipboard,
    net: this.net
};
false === c.requestSingleInstanceLock() && c.quit();
true === c.isPackaged && false === c.isDefaultProtocolClient("polytoria") && c.setAsDefaultProtocolClient("polytoria");
const k = {
    readFile: this.readFile,
    readFileSync: this.readFileSync,
    writeFile: this.writeFile,
    writeFileSync: this.writeFileSync,
    readdir: this.readdir,
    readdirSync: this.readdirSync,
    mkdir: this.mkdir,
    mkdirSync: this.mkdirSync,
    exists: this.exists,
    existsSync: this.existsSync,
    createReadStream: this.createReadStream,
    createWriteStream: this.createWriteStream,
    promises: this.promises
};
const n = {
    resolve: this.resolve,
    join: this.join,
    normalize: this.normalize,
    isAbsolute: this.isAbsolute,
    relative: this.relative,
    dirname: this.dirname,
    basename: this.basename,
    extname: this.extname,
    parse: this.parse,
    format: this.format,
    sep: this.sep,
    delimiter: this.delimiter
};
const e = {
    arch: this.arch,
    platform: this.platform,
    release: this.release,
    type: this.type,
    hostname: this.hostname,
    homedir: this.homedir,
    tmpdir: this.tmpdir,
    totalmem: this.totalmem,
    freemem: this.freemem,
    cpus: this.cpus,
    networkInterfaces: this.networkInterfaces,
    EOL: this.EOL,
    uptime: this.uptime
};
const a = {
    spawn: this.spawn,
    spawnSync: this.spawnSync,
    exec: this.exec,
    execSync: this.execSync,
    fork: this.fork
};
const { getProxySettings: m } = require("get-proxy-settings");
const C = require("undici");
const s = require("node-7z");
const r = require("7zip-bin");
let R = e.platform();
const P = c.getVersion();
const S = "PolytoriaLauncher/" + P;
const O = c.getAppPath();
const Q = c.getPath("userData");
const l = n.join(c.getPath("temp"), "Polytoria");
const i = n.join(Q, "Updater");
const N = n.join(Q, "Client");
const f = n.join(Q, "Creator");
const G = r.path7za.replace("app.asar" + n.sep, "");
const h = n.join(Q, "Settings.json");
let J;
let u;
let H;
let q;
let y;
let p;
R === "win32" ? (R = "windows", J = true === c.isPackaged ? n.dirname(c.getPath("exe")) : O, u = n.join(J, "Polytoria.exe"), H = "Updater.exe", q = "Polytoria Client.exe", y = "Polytoria Creator.exe") : R === "darwin" ? (R = "macos", J = true === c.isPackaged ? n.join(c.getPath("exe"), "..", "..", "..") : O, u = n.join(J, "Contents", "MacOS", "Polytoria"), H = "Updater", q = n.join("Polytoria Client.app", "Contents", "MacOS", "Polytoria Client"), y = n.join("Polytoria Creator.app", "Contents", "MacOS", "Polytoria Creator")) : (R = "linux", J = true === c.isPackaged ? n.dirname(c.getPath("exe")) : O, u = n.join(J, "Polytoria"), H = "Updater", q = "Polytoria Client.x86_64", y = "Polytoria Creator.x86_64");
const K = {};
K.type = null;
K.token = null;
const M = K;
let x = null;
async function B() {
    const t = W;
    for (const W of process.argv){
        if (false === W.startsWith("polytoria://")) {
            continue;
        }
        const c = W.replace("polytoria://", "").split("/");
        M.type = c[0] ?? null;
        M.token = c[1] ?? null;
        M.map = c.slice(2).join("/");
        break;
    }
    if (null === M.type || null === M.token) {
        await d.openExternal("https://polytoria.com/places");
        return c.quit();
    }
    const k = (await m())?.https?.toString();
    void 0 !== k && C.setGlobalDispatcher(new ProxyAgent(k));
    x = new o({
        width: 600,
        height: 240,
        center: true,
        fullscreenable: false,
        title: "Polytoria Launcher",
        show: false,
        frame: false,
        transparent: true,
        webPreferences: {
            devTools: false,
            preload: n.join(O, "src", "preload", "index.js"),
            webgl: false,
            spellcheck: false,
            enableWebSQL: false,
            v8CacheOptions: "none"
        }
    });
    x.setResizable(false);
    x.setMenu(null);
    x.once("closed", ()=>{
        x = null;
    });
    await x.loadFile("src/renderer/index.html");
    x.show();
    process.on("uncaughtException", (W)=>v(void 0, W));
    process.on("unhandledRejection", (W)=>v(void 0, W));
    await E();
}
async function Z(c) {
    const d = W;
    await (x?.webContents.send("status", c));
}
async function L(c) {
    const d = W;
    x?.setProgressBar(1);
    await (x?.webContents.send("success", c));
}
async function v(c, d, o) {
    const t = W;
    console.error(d);
    let k = c ?? "An unknown error has occurred";
    void 0 !== d && (k += e.EOL + d.message);
    const n = {
        mode: "error"
    };
    x?.setProgressBar(1, n);
    await (x?.webContents.send("error", k, o));
}
async function V(c) {
    const d = W;
    const o = Math.min(Math.max(c, 0), 100);
    const t = Math.min(Math.max(o / 100, 0), 1);
    x?.setProgressBar(t);
    await (x?.webContents.send("progress", o));
}
async function w() {
    const c = W;
    x?.setProgressBar(-1);
    await (x?.webContents.send("progress", 0));
}
function b(c) {
    const d = W;
    return c?.constructor === Object && Object.keys(c).length > 0;
}
function T(c) {
    const d = W;
    return true === Array.isArray(c) && c.length > 0;
}
function I(W, c) {
    const d = X();
    I = function(c, o) {
        c = c - 421;
        let t = d[c];
        if (void 0 === I.HuHlbB) {
            const c = function(W, c) {
                let d;
                let o;
                let t = [];
                let k = 0;
                let n = "";
                for(W = function(W) {
                    let c = "";
                    let d = "";
                    for(let o, t, k = 0, n = 0; t = W.charAt(n++); ~t && (o = k % 4 ? 64 * o + t : t, k++ % 4) ? c += String.fromCharCode(255 & o >> (-2 * k & 6)) : 0){
                        t = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789+/=".indexOf(t);
                    }
                    for(let o = 0, t = c.length; o < t; o++){
                        d += "%" + ("00" + c.charCodeAt(o).toString(16)).slice(-2);
                    }
                    return decodeURIComponent(d);
                }(W), o = 0; o < 256; o++){
                    t[o] = o;
                }
                for(o = 0; o < 256; o++){
                    k = (k + t[o] + c.charCodeAt(o % c.length)) % 256;
                    d = t[o];
                    t[o] = t[k];
                    t[k] = d;
                }
                o = 0;
                k = 0;
                for(let e = 0; e < W.length; e++){
                    o = (o + 1) % 256;
                    k = (k + t[o]) % 256;
                    d = t[o];
                    t[o] = t[k];
                    t[k] = d;
                    n += String.fromCharCode(W.charCodeAt(e) ^ t[(t[o] + t[k]) % 256]);
                }
                return n;
            };
            I.GayGaP = c;
            W = arguments;
            I.HuHlbB = true;
        }
        const k = c + d[0];
        const n = W[k];
        n ? t = n : (void 0 === I.vMrFfw && (I.vMrFfw = true), t = I.GayGaP(t, o), W[k] = t);
        return t;
    };
    return I(W, c);
}
function U(c) {
    const d = W;
    return typeof c === "string" && c.trim().length > 0;
}
function z(c) {
    const d = W;
    return c?.hostname === "api.polytoria.com" ? M.token : void 0;
}
function A() {
    const c = W;
    const d = {
        Client: [],
        Creator: [],
        Updater: []
    };
    const o = {
        Manifest: d,
        SkipUpdates: false
    };
    const t = o;
    let n = null;
    if (true === k.existsSync(h)) {
        let W;
        try {
            n = k.readFileSync(h, "utf8");
            W = JSON.parse(n);
        } catch  {}
        if (true === b(W)) {
            true === b(W.Manifest) ? (true === T(W.Manifest.Client) && t.Manifest.Client.push(...g(W.Manifest.Client)), true === T(W.Manifest.Creator) && t.Manifest.Creator.push(...g(W.Manifest.Creator)), true === T(W.Manifest.Updater) && t.Manifest.Updater.push(...g(W.Manifest.Updater))) : (true === U(W.ClientVersion) && t.Manifest.Client.push({
                Release: "Stable",
                Version: W.ClientVersion,
                InstalledAt: (new Date).toJSON()
            }), true === U(W.CreatorVersion) && t.Manifest.Creator.push({
                Release: "Stable",
                Version: W.CreatorVersion,
                InstalledAt: (new Date).toJSON()
            }), true === U(W.UpdaterVersion) && t.Manifest.Updater.push({
                Release: "Stable",
                Version: W.UpdaterVersion,
                InstalledAt: (new Date).toJSON()
            }));
            typeof W.SkipUpdates === "boolean" && (t.SkipUpdates = W.SkipUpdates);
        }
    }
    const a = JSON.stringify(t);
    a !== n && k.writeFileSync(h, a + e.EOL);
    return t;
}
function g(c) {
    const d = W;
    const o = [];
    for (const W of c)if (true === b(W) && true === U(W.Release) && true === U(W.Version) && true === U(W.InstalledAt)) {
        const c = {
            Release: W.Release,
            Version: W.Version,
            InstalledAt: W.InstalledAt
        };
        o.push(c);
    }
    return o;
}
function F(c, d) {
    const o = W;
    const t = A();
    return false === Object.hasOwn(t.Manifest, c) ? null : t.Manifest[c].find((W)=>W.Release === d) ?? null;
}
function D(c, d, o) {
    const t = W;
    const n = A();
    if (false === Object.hasOwn(n.Manifest, c)) {
        return false;
    }
    n.Manifest[c] = n.Manifest[c].filter((W)=>W.Release !== d);
    n.Manifest[c].push({
        Release: d,
        Version: o,
        InstalledAt: (new Date).toJSON()
    });
    k.writeFileSync(h, JSON.stringify(n) + e.EOL);
}
async function E() {
    const d = W;
    ((async function(c) {
        const d = W;
        await (x?.webContents.send("version", c));
    })(P));
    Z("Loading launcher settings...");
    const o = M.type === "client" || M.type === "clientbeta";
    const t = M.type === "test" || M.type === "testbeta";
    const e = M.type === "creator" || M.type === "creatorbeta";
    if (false === o && false === t && false === e) {
        return v("Invalid launch type argument");
    }
    const m = M.type === "clientbeta" || M.type === "testbeta" || M.type === "creatorbeta" ? "Beta" : "Stable";
    let r;
    let O;
    let Q;
    let h;
    try {
        r = A();
        O = F("Updater", "Stable")?.Version ?? null;
        Q = F("Client", m)?.Version ?? null;
        h = F("Creator", m)?.Version ?? null;
    } catch (sW) {
        return v("Failed to parse launcher settings", sW);
    }
    Z("Checking for outdated files...");
    try {
        B = Q;
        b = h;
        true === U(K = O) && $(i, K, H);
        true === U(B) && $(N, B, q);
        true === U(b) && $(f, b, y);
    } catch (rW) {
        return v("Failed to migrate legacy installations", rW);
    }
    var K;
    var B;
    var b;
    if (true === r.SkipUpdates) {
        return void (true === o || true === t ? j(Q, t) : true === e && Y(h));
    }
    let T;
    Z("Checking for updates...");
    try {
        const W = new URL("https://api.polytoria.com/v1/launcher/updates");
        const { body: c } = await C.request(W, {
            headers: {
                authorization: z(W),
                "user-agent": S
            },
            query: {
                os: R,
                release: m.toLowerCase()
            },
            throwOnError: true
        });
        T = await c.json();
    } catch (RW) {
        return v("Failed to check for updates", RW.response?.data.Errors?.length > 0 ? RW.response.data.Errors[0] : RW);
    }
    if (true === T.Maintenance) {
        return v("We are currently undergoing maintenance");
    }
    if (true === U(T.Updater?.Version) && true === U(T.Updater?.Download) && (p === "updater" || null === O || O !== T.Updater.Version)) {
        try {
            await (X = new URL(T.Updater.Download), g = T.Updater.Release, E = T.Updater.Version, _ = O, new Promise(async (W, c)=>{
                const d = I;
                if (Z("Downloading the latest updater..."), true === U(_)) {
                    const W = n.join(i, _);
                    if (true === k.existsSync(W)) {
                        const c = {
                            recursive: true
                        };
                        k.rmSync(W, c);
                    }
                }
                const o = n.join(i, E);
                if (true === k.existsSync(o)) {
                    const W = {
                        recursive: true
                    };
                    k.rmSync(o, W);
                }
                const t = {
                    recursive: true,
                    mode: 493
                };
                k.mkdirSync(o, t);
                const e = n.join(l, "Updater" + n.extname(X.pathname));
                if (false === k.existsSync(l)) {
                    const W = {
                        recursive: true,
                        mode: 493
                    };
                    k.mkdirSync(l, W);
                } else {
                    true === k.existsSync(e) && k.rmSync(e);
                }
                let a;
                let m;
                try {
                    const { headers: W, body: c } = await C.request(X, {
                        headers: {
                            authorization: z(X),
                            "user-agent": S
                        },
                        throwOnError: true
                    });
                    a = W["content-length"];
                    m = c;
                } catch (O) {
                    return c(O ?? "Failed to download the updater");
                }
                let r = 0;
                m.on("data", (W)=>{
                    r += W.length;
                    V(r / a * 100);
                });
                const P = k.createWriteStream(e);
                P.once("close", async ()=>{
                    const t = d;
                    try {
                        await (a = e, m = o, C = g, r = E, new Promise((W, c)=>{
                            const d = I;
                            w();
                            Z("Extracting updater...");
                            const o = {
                                $progress: true,
                                $bin: G
                            };
                            const t = s.extractFull(a, m, o);
                            t.on("progress", (W)=>{
                                V(W.percent);
                            });
                            t.once("error", (W)=>{
                                const o = d;
                                t.removeAllListeners("end");
                                true === k.existsSync(a) && k.rmSync(a);
                                c(W);
                            });
                            t.once("end", ()=>{
                                const c = d;
                                if (true === k.existsSync(a) && k.rmSync(a), R !== "windows") {
                                    const W = n.join(m, H);
                                    true === k.existsSync(W) && k.chmodSync(W, 493);
                                }
                                D("Updater", C, r);
                                W();
                            });
                        }));
                        W();
                    } catch (P) {
                        c(P ?? "Failed to extract the updater");
                    }
                    var a;
                    var m;
                    var C;
                    var r;
                });
                m.pipe(P);
            }));
            O = T.Updater.Version;
        } catch (PW) {
            return v("Failed to download the updater", PW);
        }
    }
    var X;
    var g;
    var E;
    var _;
    var WW;
    var cW;
    var dW;
    var oW;
    var tW;
    var kW;
    var nW;
    var eW;
    var aW;
    var mW;
    var CW;
    if (true === U(T.Launcher?.Version) && true === U(T.Launcher?.Download) && (p === "launcher" || null === P || P !== T.Launcher.Version)) {
        try {
            return await (WW = new URL(T.Launcher.Download), cW = T.Launcher.Version, dW = O, new Promise(async (d, o)=>{
                const t = I;
                Z("Downloading the latest launcher...");
                const e = n.join(l, "Launcher");
                if (true === k.existsSync(e)) {
                    const W = R === "macos" ? n.join(e, "Contents", "Resources", "app.asar") : n.join(e, "resources", "app.asar");
                    true === k.existsSync(W) && k.unlinkSync(W);
                    const c = {
                        recursive: true
                    };
                    k.rmSync(e, c);
                }
                const m = {
                    recursive: true,
                    mode: 493
                };
                k.mkdirSync(e, m);
                const r = n.join(l, "Launcher" + n.extname(WW.pathname));
                if (false === k.existsSync(l)) {
                    const W = {
                        recursive: true,
                        mode: 493
                    };
                    k.mkdirSync(l, W);
                } else {
                    true === k.existsSync(r) && k.rmSync(r);
                }
                let P;
                let O;
                try {
                    const { headers: W, body: c } = await C.request(WW, {
                        headers: {
                            authorization: z(WW),
                            "user-agent": S
                        },
                        throwOnError: true
                    });
                    P = W["content-length"];
                    O = c;
                } catch (f) {
                    return o(f ?? "Failed to download the launcher");
                }
                let Q = 0;
                O.on("data", (W)=>{
                    Q += W.length;
                    V(Q / P * 100);
                });
                const N = k.createWriteStream(r);
                N.once("close", async ()=>{
                    const m = t;
                    try {
                        await (C = r, P = e, S = cW, O = dW, new Promise((d, o)=>{
                            const t = I;
                            w();
                            Z("Extracting launcher update...");
                            const e = {
                                $progress: true,
                                $bin: G
                            };
                            const m = s.extractFull(C, P, e);
                            m.on("progress", (W)=>{
                                V(W.percent);
                            });
                            m.once("error", (W)=>{
                                const c = t;
                                m.removeAllListeners("end");
                                true === k.existsSync(C) && k.rmSync(C);
                                o(W);
                            });
                            m.once("end", ()=>{
                                const o = t;
                                true === k.existsSync(C) && k.rmSync(C);
                                (function(d, o, t) {
                                    const e = W;
                                    if (null === d) {
                                        return v("Unable to locate the updater service", void 0, "updater");
                                    }
                                    const m = n.join(i, d, H);
                                    if (false === k.existsSync(m)) {
                                        return v("Unable to locate the updater service", void 0, "updater");
                                    }
                                    L("Restarting the launcher...");
                                    const C = true === c.isPackaged ? J : R !== "macos" ? n.join(J, "testing") : n.join(J, "testing", "Polytoria.app");
                                    const s = R !== "macos" ? n.join(C, n.basename(u)) : n.join(C, "Contents", "MacOS", n.basename(u));
                                    const r = process.argv.slice(true === c.isPackaged ? 1 : 2);
                                    const P = [
                                        m,
                                        "--kill",
                                        process.pid,
                                        "--input",
                                        o,
                                        "--output",
                                        C,
                                        "--version",
                                        t,
                                        "--relaunchPath",
                                        s,
                                        "--relaunchArgs",
                                        ...r
                                    ];
                                    setTimeout(()=>{
                                        const W = e;
                                        process.once("exit", ()=>{
                                            const c = W;
                                            if (R === "windows") {
                                                const W = {
                                                    detached: true,
                                                    stdio: "ignore",
                                                    windowsHide: true
                                                };
                                                a.spawn(P[0], P.slice(1), W).unref();
                                            } else if (R === "macos") {
                                                const W = {
                                                    detached: true,
                                                    stdio: "ignore"
                                                };
                                                a.spawn("osascript", [
                                                    "-e",
                                                    'do shell script "' + P.join(" ") + '" with prompt "Polytoria launcher needs to update." with administrator privileges'
                                                ], W).unref();
                                            } else {
                                                const W = {
                                                    detached: true,
                                                    stdio: "ignore"
                                                };
                                                a.spawn("pkexec", [
                                                    "--disable-internal-agent",
                                                    ...P
                                                ], W).unref();
                                            }
                                        });
                                        c.quit();
                                    }, 3e3);
                                }(O, P, S));
                                d();
                            });
                        }));
                        d();
                    } catch (Q) {
                        o(Q ?? "Failed to extract the launcher");
                    }
                    var C;
                    var P;
                    var S;
                    var O;
                });
                O.pipe(N);
            }));
        } catch (SW) {
            return v("Failed to download the launcher", SW);
        }
    }
    if (true === o || true === t) {
        if (true === U(T.Client?.Version) && true === U(T.Client?.Download) && (p === "client" || null === Q || Q !== T.Client.Version)) {
            try {
                await (eW = new URL(T.Client.Download), aW = T.Client.Release, mW = T.Client.Version, CW = Q, new Promise(async (W, c)=>{
                    const d = I;
                    if (Z("Downloading the latest client..."), true === U(CW)) {
                        const W = n.join(N, CW);
                        if (true === k.existsSync(W)) {
                            const c = {
                                recursive: true
                            };
                            k.rmSync(W, c);
                        }
                    }
                    const o = n.join(N, mW);
                    if (true === k.existsSync(o)) {
                        const W = {
                            recursive: true
                        };
                        k.rmSync(o, W);
                    }
                    const t = {
                        recursive: true,
                        mode: 493
                    };
                    k.mkdirSync(o, t);
                    const e = n.join(l, "Client" + n.extname(eW.pathname));
                    if (false === k.existsSync(l)) {
                        const W = {
                            recursive: true,
                            mode: 493
                        };
                        k.mkdirSync(l, W);
                    } else {
                        true === k.existsSync(e) && k.rmSync(e);
                    }
                    let a;
                    let m;
                    try {
                        const { headers: W, body: c } = await C.request(eW, {
                            headers: {
                                authorization: z(eW),
                                "user-agent": S
                            },
                            throwOnError: true
                        });
                        a = W["content-length"];
                        m = c;
                    } catch (O) {
                        return c(O ?? "Failed to download the client");
                    }
                    let r = 0;
                    m.on("data", (W)=>{
                        r += W.length;
                        V(r / a * 100);
                    });
                    const P = k.createWriteStream(e);
                    P.once("close", async ()=>{
                        const t = d;
                        try {
                            await (a = e, m = o, C = aW, r = mW, new Promise((W, c)=>{
                                const d = I;
                                w();
                                Z("Extracting client update...");
                                const o = {
                                    $progress: true,
                                    $bin: G
                                };
                                const t = s.extractFull(a, m, o);
                                t.on("progress", (W)=>{
                                    V(W.percent);
                                });
                                t.once("error", (W)=>{
                                    const o = d;
                                    t.removeAllListeners("end");
                                    true === k.existsSync(a) && k.rmSync(a);
                                    c(W);
                                });
                                t.once("end", ()=>{
                                    const c = d;
                                    if (true === k.existsSync(a) && k.rmSync(a), R !== "windows") {
                                        const W = n.join(m, q);
                                        true === k.existsSync(W) && k.chmodSync(W, 493);
                                    }
                                    D("Client", C, r);
                                    W();
                                });
                            }));
                            W();
                        } catch (P) {
                            c(P ?? "Failed to extract the client");
                        }
                        var a;
                        var m;
                        var C;
                        var r;
                    });
                    m.pipe(P);
                }));
                Q = T.Client.Version;
            } catch (OW) {
                return v("Failed to download the client", OW);
            }
        }
        j(Q, t);
    } else if (true === e) {
        if (true === U(T.Creator?.Version) && true === U(T.Creator?.Download) && (p === "creator" || null === h || h !== T.Creator.Version)) {
            try {
                await (oW = new URL(T.Creator.Download), tW = T.Creator.Release, kW = T.Creator.Version, nW = h, new Promise(async (W, c)=>{
                    const d = I;
                    if (Z("Downloading the latest creator..."), true === U(nW)) {
                        const W = n.join(f, nW);
                        if (true === k.existsSync(W)) {
                            const c = {
                                recursive: true
                            };
                            k.rmSync(W, c);
                        }
                    }
                    const o = n.join(f, kW);
                    if (true === k.existsSync(o)) {
                        const W = {
                            recursive: true
                        };
                        k.rmSync(o, W);
                    }
                    const t = {
                        recursive: true,
                        mode: 493
                    };
                    k.mkdirSync(o, t);
                    const e = n.join(l, "Creator" + n.extname(oW.pathname));
                    if (false === k.existsSync(l)) {
                        const W = {
                            recursive: true,
                            mode: 493
                        };
                        k.mkdirSync(l, W);
                    } else {
                        true === k.existsSync(e) && k.rmSync(e);
                    }
                    let a;
                    let m;
                    try {
                        const { headers: W, body: c } = await C.request(oW, {
                            headers: {
                                authorization: z(oW),
                                "user-agent": S
                            },
                            throwOnError: true
                        });
                        a = W["content-length"];
                        m = c;
                    } catch (O) {
                        return c(O ?? "Failed to download the creator");
                    }
                    let r = 0;
                    m.on("data", (W)=>{
                        r += W.length;
                        V(r / a * 100);
                    });
                    const P = k.createWriteStream(e);
                    P.once("close", async ()=>{
                        const t = d;
                        try {
                            await (a = e, m = o, C = tW, r = kW, new Promise((W, c)=>{
                                const d = I;
                                w();
                                Z("Extracting creator update...");
                                const o = {
                                    $progress: true,
                                    $bin: G
                                };
                                const t = s.extractFull(a, m, o);
                                t.on("progress", (W)=>{
                                    V(W.percent);
                                });
                                t.once("error", (W)=>{
                                    const o = d;
                                    t.removeAllListeners("end");
                                    true === k.existsSync(a) && k.rmSync(a);
                                    c(W);
                                });
                                t.once("end", ()=>{
                                    const c = d;
                                    if (true === k.existsSync(a) && k.rmSync(a), R !== "windows") {
                                        const W = n.join(m, y);
                                        true === k.existsSync(W) && k.chmodSync(W, 493);
                                    }
                                    D("Creator", C, r);
                                    W();
                                });
                            }));
                            W();
                        } catch (P) {
                            c(P ?? "Failed to extract the creator");
                        }
                        var a;
                        var m;
                        var C;
                        var r;
                    });
                    m.pipe(P);
                }));
                h = T.Creator.Version;
            } catch (QW) {
                return v("Failed to download the creator", QW);
            }
        }
        Y(h);
    }
}
function $(c, d, o) {
    const t = W;
    if (false === k.existsSync(c)) {
        return;
    }
    const e = {
        withFileTypes: true
    };
    const a = k.readdirSync(c, e);
    if (false === a.every((W)=>true === W.isDirectory())) {
        const W = n.join(c, d);
        if (false === k.existsSync(W)) {
            const c = {
                recursive: true,
                mode: 493
            };
            k.mkdirSync(W, c);
        }
        for (const o of a){
            if (o.name === d) {
                continue;
            }
            const e = n.join(c, o.name);
            const a = n.join(W, o.name);
            if (true === k.existsSync(a)) {
                const W = {
                    recursive: true
                };
                k.rmSync(a, W);
            }
            k.renameSync(e, a);
        }
        if (R !== "windows") {
            const c = n.join(W, o);
            true === k.existsSync(c) && k.chmodSync(c, 493);
        }
    }
}
function j(d, o) {
    const t = W;
    if (null === d) {
        return v("Unable to locate the client installation", void 0, "client");
    }
    const e = n.join(N, d, q);
    if (false === k.existsSync(e)) {
        return v("Unable to locate the client installation", void 0, "client");
    }
    L("Launching the client...");
    setTimeout(()=>{
        const W = t;
        const d = n.parse(e);
        let k;
        if (R === "windows") {
            const c = {
                windowsHide: true,
                stdio: "ignore"
            };
            k = a.spawn("taskkill", [
                "/f",
                "/im",
                d.base
            ], c);
        } else {
            const c = {
                stdio: "ignore"
            };
            k = a.spawn("pkill", [
                "-9",
                d.name
            ], c);
        }
        k.on("exit", ()=>{
            const d = W;
            const t = true === o ? [
                "-solo",
                M.map
            ] : [
                "-network",
                "client",
                "-token",
                M.token
            ];
            process.once("exit", ()=>{
                const W = d;
                const c = {
                    detached: true,
                    stdio: "ignore"
                };
                a.spawn(e, t, c).unref();
            });
            c.quit();
        });
    }, 3e3);
}
function Y(d) {
    const o = W;
    if (null === d) {
        return v("Unable to locate the creator installation", void 0, "creator");
    }
    const t = n.join(f, d, y);
    if (false === k.existsSync(t)) {
        return v("Unable to locate the creator installation", void 0, "creator");
    }
    L("Launching the creator...");
    setTimeout(()=>{
        const W = o;
        process.once("exit", ()=>{
            const c = W;
            const d = {
                detached: true,
                stdio: "ignore"
            };
            a.spawn(t, [
                "-token",
                M.token
            ], d).unref();
        });
        c.quit();
    }, 3e3);
}
c.on("activate", ()=>{
    const c = W;
    0 === o.getAllWindows().length && B();
});
c.once("window-all-closed", ()=>{
    c.quit();
});
c.once("open-url", (c, d)=>{
    const o = W;
    c.preventDefault();
    process.argv.push(d);
});
c.whenReady().then(()=>{
    B();
});
t.on("reinstall", async (c, d)=>{
    await async function() {
        const c = W;
        await (x?.webContents.send("resetUI"));
    }();
    p = d;
    await E();
    p = void 0;
});
