"use strict";
exports.__esModule = true;
var mqtt_1 = require("mqtt");
var client = mqtt_1.connect('ws://' + location.host + '/mqtt', {
    clientId: "client1"
});
window.onbeforeunload = function () {
    client.end();
};
var publishButton = document.getElementById("publish");
var topicInput = document.getElementById("topic");
var msgInput = document.getElementById("msg");
var stateParagraph = document.getElementById("state");
var msgsList = document.getElementById("msgs");
publishButton.onclick = function (click) {
    var topic = topicInput.value;
    var msg = msgInput.value;
    client.publish(topic, msg);
};
client.on('connect', function () {
    client.subscribe('#', { qos: 0 }, function (err, granted) {
        console.log(err);
    });
    client.publish('presence', 'Hello mqtt');
    stateParagraph.innerText = "connected";
});
client.on("error", function (e) {
    showMsg("error: " + e.message);
});
client.on("reconnect", function () {
    stateParagraph.innerText = "reconnecting";
});
client.on('message', function (topic, message) {
    showMsg(topic + ": " + message.toString());
});
function showMsg(msg) {
    //console.log(msg);
    var node = document.createElement("LI");
    node.appendChild(document.createTextNode(msg));
    msgsList.appendChild(node);
    if (msgsList.childElementCount > 20) {
        msgsList.removeChild(msgsList.childNodes[0]);
    }
}
