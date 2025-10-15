importScripts("https://www.gstatic.com/firebasejs/9.22.2/firebase-app-compat.js");
importScripts("https://www.gstatic.com/firebasejs/9.22.2/firebase-messaging-compat.js");

firebase.initializeApp({
    apiKey: "AIzaSyAVFgi1rEG2rnhrVgxz4ieJINkbQvpnYKw",
    authDomain: "restaurantsw-27ae0.firebaseapp.com",
    projectId: "restaurantsw-27ae0",
    storageBucket: "restaurantsw-27ae0.appspot.com",
    messagingSenderId: "246839255069",
    appId: "1:246839255069:web:05a95543dcfdb85031a6f4"
});

const messaging = firebase.messaging();

messaging.onBackgroundMessage(payload => {
    console.log("📬 Background message:", payload);
    const notificationTitle = payload.notification.title;
    const notificationOptions = {
        body: payload.notification.body,
        icon: "/images/icons/icon-192x192.png"
    };
    self.registration.showNotification(notificationTitle, notificationOptions);
});
