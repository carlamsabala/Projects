const CACHE_NAME = "act-app-cache-v6"; // Updated version to force cache refresh
const urlsToCache = [
    "/",
    "/index.html",
    "/pages/login.html",
    "/pages/signup.html",
    "/pages/forgotPassword.html",
    "/pages/translation.html",
    "/pages/FileUploadTranslation.html",
    "/manifest.json",
    "/assets/stylesheets/styleguide.css",
    "/assets/stylesheets/index.css",
    "/assets/stylesheets/login.css",
    "/assets/stylesheets/signup.css",
    "/assets/stylesheets/forgotPassword.css",
    "/scripts/login.js",
    "/scripts/signup.js",
    "/scripts/forgotPassword.js",
    "/scripts/translation.js",
    "https://fonts.googleapis.com/css2?family=Koulen&display=swap"
];

// ✅ Install service worker and cache necessary resources
self.addEventListener("install", (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => {
            console.log("Opened cache");
            return cache.addAll(urlsToCache);
        }).catch((error) => console.error("Cache addAll failed:", error))
    );
});

// ✅ Activate service worker and clean up old caches
self.addEventListener("activate", (event) => {
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames.map((cache) => {
                    if (cache !== CACHE_NAME) {
                        console.log("Deleting old cache:", cache);
                        return caches.delete(cache);
                    }
                })
            );
        })
    );
});

// ✅ Fetch event - serve from cache first, then fallback to network
self.addEventListener("fetch", (event) => {
    // Bypass cache for API calls to /couchdb/
    if (event.request.url.includes('/couchdb/')) {
        event.respondWith(fetch(event.request));
        return;
    }
    
    event.respondWith(
        caches.match(event.request).then((response) => {
            return response || fetch(event.request).catch(() => {
                // Fallback to offline page if the request is for navigation
                if (event.request.mode === "navigate") {
                    return caches.match("/index.html");
                }
            });
        })
    );
});