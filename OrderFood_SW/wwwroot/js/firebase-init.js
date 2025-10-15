// Import Firebase SDK
import { initializeApp } from "https://www.gstatic.com/firebasejs/10.12.3/firebase-app.js";
import { getMessaging, getToken, onMessage } from "https://www.gstatic.com/firebasejs/10.12.3/firebase-messaging.js";

const firebaseConfig = {
    apiKey: "AIzaSyAVFgi1rEG2rnhrVgxz4ieJINkbQvpnYKw",
    authDomain: "restaurantsw-27ae0.firebaseapp.com",
    projectId: "restaurantsw-27ae0",
    storageBucket: "restaurantsw-27ae0.firebasestorage.app",
    messagingSenderId: "246839255069",
    appId: "1:246839255069:web:05a95543dcfdb85031a6f4",
    measurementId: "G-SZGP1KETQ9"
};

const app = initializeApp(firebaseConfig);
const messaging = getMessaging(app);

// Đăng ký Service Worker
navigator.serviceWorker.register("/firebase-messaging-sw.js")
    .then(reg => {
        console.log("Service Worker registered:", reg);

        return getToken(messaging, {
            vapidKey: "BPEY9r57_n6WTRjfMGt7KiOK-T9fO7zWlmmmRXkQTD0SxGZ_xGNOeKpPSJBqOP3vmkAtXDltDhJkHXoupjgEKt4",
            serviceWorkerRegistration: reg
        });
    })
    .then(token => {
        console.log("FCM Token:", token);
        // Gửi token lên server nếu cần
    })
    .catch(err => console.error("FCM init error:", err));

onMessage(messaging, payload => {
    console.log("Message received:", payload);
    if (payload.data?.action === "RELOAD") {
        location.reload();
    }
});

const messaging = firebase.messaging();

Notification.requestPermission()
    .then(permission => {
        if (permission === 'granted') {
            console.log('Quyền thông báo được cấp.');
            return messaging.getToken({ vapidKey: 'BPEY9r57_n6WTRjfMGt7KiOK-T9fO7zWlmmmRXkQTD0SxGZ_xGNOeKpPSJBqOP3vmkAtXDltDhJkHXoupjgEKt4' });
        } else {
            console.log('Người dùng từ chối nhận thông báo.');
            throw new Error('Permission denied');
        }
    })
    .then(token => {
        console.log("Token:", token);
        // gửi token này lên server qua AJAX
    })
    .catch(err => console.error("Lỗi khi lấy token:", err));
