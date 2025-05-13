const express = require("express");
const bodyParser = require("body-parser");
const cors = require("cors");
const session = require("express-session");
const passport = require("passport");
const OpenIDConnectStrategy = require("passport-openidconnect");
const path = require("path");

require("dotenv").config({ path: "./Docker/.env" });

const { translationsDB } = require("./src/scripts/db"); //import db

const app = express();
process.env.DEBUG = 'passport-openidconnect passport:*';

// ✅ Session Configuration
app.use(session({
    secret: process.env.OIDC_COOKIE_SECRET,
    resave: false,
    saveUninitialized: false,
    cookie: {
        secure: false,
    }
}));

passport.serializeUser((user, done) => {
    done(null, user);
});

passport.deserializeUser((obj, done) => {
    done(null, obj);
});

app.use(passport.initialize());
app.use(passport.session());


// ✅ OIDC Authentication Strategy
passport.use(new OpenIDConnectStrategy({
    issuer: process.env.OIDC_ISSUER,
    clientID: process.env.OIDC_CLIENT_ID,
    clientSecret: process.env.OIDC_CLIENT_SECRET,
    authorizationURL: process.env.OIDC_AUTHORIZATION_ENDPOINT,
    tokenURL: process.env.OIDC_TOKEN_ENDPOINT,
    userInfoURL: process.env.OIDC_USERINFO_ENDPOINT,
    callbackURL: process.env.OIDC_REDIRECT_URI,
}, (req, issuer, sub, profile, accessToken, refreshToken, done) => {
    console.log('Passport callback called');
    console.log('Issuer:', issuer);
    console.log('Subject:', sub);
    console.log('Profile:', profile);
    console.log('Access Token:', accessToken);
    console.log('Refresh Token:', refreshToken);
    profile.accessToken = accessToken;
    profile.refreshToken = refreshToken;
    return done(null, profile);
}));

// ✅ Middleware to Check Authentication Status
function authenticate(req, res, next) {
    console.log("🔍 Checking authentication...");

    if (req.isAuthenticated()) {
        console.log("✅ User is authenticated.");
        return next();
    }

    console.log("❌ User is not authenticated, redirecting to login.");
    res.redirect("/auth/login");
}

// ✅ Middleware to Protect API Routes
function ensureAuthenticated(req, res, next) {
    if (req.isAuthenticated()) {
        return next();
    }
    res.status(401).json({ message: "Unauthorized" });
}

// ✅ Static File Serving

app.use("/assets", express.static(path.join(__dirname, "src/assets")));
app.use("/scripts", express.static(path.join(__dirname, "src/scripts")));



// Serve HTML pages explicitly
const pages = ["fileDrive", "home", "settings", "translation"];

pages.forEach((page) => {
  app.get(`/${page}.html`, (req, res) => {
    const filePath = path.join(__dirname, `src/pages/${page}.html`);
    res.sendFile(filePath, (err) => {
      if (err) {
        console.error(`File not found: ${filePath}`);
        res.status(404).send("Page not found");
      }
    });
  });
});

// ✅ Parse JSON Requests
app.use(bodyParser.json());
app.use(cors());

// ✅ Logging Middleware
app.use((req, res, next) => {
    console.log(`Incoming request: ${req.method} ${req.url}`);
    next();
});

// ✅ OIDC Authentication Routes

// 🔹 Redirect to Authentik for Login
app.get("/auth/login", passport.authenticate("openidconnect", { 
    responseType: "code",
    successReturnToOrRedirect: "/dashboard",
    failureRedirect: "/"
}));

app.get("/auth/callback", (req, res, next) => {
    console.log("✅ Incoming request to /auth/callback");
    console.log("🔹 Query Parameters:", req.query); // Log the query parameters
    
    passport.authenticate("openidconnect", (err, user, info) => {
        console.log("🔍 Debugging OIDC Authentication...");
        console.log("Error:", err);
        console.log("User:", user);
        console.log("Info:", info);
        if (err) {
            console.error("❌ OIDC Authentication Error:", err);
            return res.redirect("/");
        }
        if (!user) {
            console.error("❌ No user returned from OIDC.");
            return res.redirect("/");
        }
        req.logIn(user, (err) => {
            if (err) {
                console.error("❌ Login error:", err);
                return res.redirect("/");
            }
            console.log("✅ User authenticated successfully:", user);
            req.session.save(() => {
                res.redirect("/dashboard");
            });
        });
    })(req, res, next);
});

// 🔹 Logout and Redirect to Authentik Logout URL
app.get("/auth/logout", (req, res) => {
    req.session.destroy((err) => {
        res.redirect(`${process.env.OIDC_ISSUER}/end-session/`);
    });
});

// ✅ Serve the Frontend
app.get("/", (req, res) => {
    console.log("✅ Checking authentication status at root route");
    console.log("User Authenticated?", req.isAuthenticated());
    console.log("Session Data:", req.session);
    console.log("User Data:", req.user);

    res.sendFile(path.join(__dirname, "src/pages/index.html"));
});

// ✅ Protect Dashboard
app.get("/dashboard", authenticate, (req, res) => {
    res.sendFile(path.join(__dirname, "src/pages/home.html"));
});

// ✅ API to Check Auth Status
app.get("/auth/status", (req, res) => {
    if (req.isAuthenticated()) {
        res.json({ authenticated: true, user: req.user });
    } else {
        res.json({ authenticated: false });
    }
});

// ✅ Protected API Route to Fetch User Info
app.get("/api/user-info", ensureAuthenticated, (req, res) => {
    res.json({
        username: req.user.displayName || req.user.name,
        email: req.user.email || req.user._json.email || "Email not available"
    });
});

// ✅ API Endpoint to Save Translations to CouchDB
app.post("/save-translation", async (req, res) => {
    console.log("POST /save-translation endpoint hit");
    const { delphiCode, csharpCode, documentTitle } = req.body;
    try {
        const response = await translationsDB.insert({
            delphiCode,
            csharpCode,
            documentTitle,
            timestamp: new Date().toISOString()
        });
        console.log("Translation saved with ID:", response.id);
        res.status(200).json({ success: true, id: response.id, documentTitle });
    } catch (error) {
        console.error("Error saving translation:", error);
        res.status(500).json({ success: false, error: error.message });
    }
});

// ✅ API Endpoint to Fetch All Translations from CouchDB
app.get("/get-translations", async (req, res) => {
    try {
        // List all documents, including the document content
        const response = await translationsDB.list({ include_docs: true });
        const translations = response.rows.map(row => row.doc).sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp));
        res.status(200).json({ success: true, translations });
    } catch (error) {
        console.error("Error fetching translations:", error);
        res.status(500).json({ success: false, error: error.message });
    }
});

// ✅ API Endpoint to Fetch a Single Translation from CouchDB by ID
app.get("/get-translation/:id", async (req, res) => {
    try {
        const docId = req.params.id;
        const translation = await translationsDB.get(docId);
        res.status(200).json({ success: true, translation });
    } catch (error) {
        console.error("Error fetching translation:", error);
        res.status(500).json({ success: false, error: error.message });
    }
});

// ✅ API Endpoint to Delete a Translation from CouchDB by ID
app.delete("/delete-translation/:id", async (req, res) => {
    try {
        // First, get the document to retrieve its revision
        const doc = await translationsDB.get(req.params.id);
        // Delete the document using its _id and _rev
        const deleteResponse = await translationsDB.destroy(doc._id, doc._rev);
        console.log("Translation deleted with ID:", req.params.id);
        res.status(200).json({ success: true, id: req.params.id });
    } catch (error) {
        console.error("Error deleting translation:", error);
        res.status(500).json({ success: false, error: error.message });
    }
});

app.put('/update-translation/:id', async (req, res) => {
    const { id } = req.params;
    const { documentTitle } = req.body;

    if (!documentTitle) {
        return res.status(400).json({ success: false, error: "No title provided." });
    }

    try {
        const doc = await translationsDB.get(id);
        doc.documentTitle = documentTitle;
        const response = await translationsDB.insert(doc);

        res.json({ success: true, id: response.id, rev: response.rev });
    } catch (error) {
        console.error("Error updating document:", error);
        res.status(500).json({ success: false, error: error.message });
    }
});

// ✅ Start Server
const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`🚀 Server running at http://localhost:${PORT}`);
});
