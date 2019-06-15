/*
 * echotest.js
 *
 * Derived from Echo Test of WebSocket.org (http://www.websocket.org/echo.html).
 *
 * Copyright (c) 2012 Kaazing Corporation.
 */

// Test for Bernd
var urlStartList = "ws://" + window.location.hostname + ":" + window.location.port + "/StartList";
var urlResultList = "ws://" + window.location.hostname + ":" + window.location.port + "/ResultList";

var outputSL, outputRL, outputInfo;

function init () {
  outputInfo = document.getElementById("outputInfo");
  outputSL = document.getElementById("outputSL");
  outputRL = document.getElementById("outputRL");
  doWebSocket ();
}

function doWebSocket () {
  websocketSL = new WebSocket(urlStartList);
  websocketRL = new WebSocket(urlResultList);

  websocketSL.onopen = function (e) {
    onOpen(e);
  };
  websocketRL.onopen = function (e) {
    onOpen(e);
  };

  websocketSL.onmessage = function (e) {
    onMessageSL(e);
  };

  websocketRL.onmessage = function (e) {
    onMessageRL(e);
  };

  websocketSL.onerror = function (e) {
    onError(e);
  };
  websocketRL.onerror = function (e) {
    onError(e);
  };

  websocketSL.onclose = function (e) {
    onClose(e);
  };
  websocketRL.onclose = function (e) {
    onClose(e);
  };
}

function onOpen (event) {
  writeToScreen("CONNECTED");
  event.srcElement.send("WebSocket rocks");
}

function onMessageSL(event) {
  //writeToScreen ('<span style="color: blue;">RESPONSE: ' + event.data + '</span>');
  //websocket.close ();

  var json = JSON.parse(event.data);
  var table = json2table(json);

  var pre = document.createElement("p");
  pre.innerHTML = table;

  while (outputSL.firstChild) { outputSL.removeChild(outputSL.firstChild); }

  outputSL.appendChild(pre);
}

function onMessageRL(event) {
  //writeToScreen ('<span style="color: blue;">RESPONSE: ' + event.data + '</span>');
  //websocket.close ();

  var json = JSON.parse(event.data);
  var table = json2table(json);

  var pre = document.createElement("p");
  pre.innerHTML = table;

  while (outputRL.firstChild) { outputRL.removeChild(outputRL.firstChild); }

  outputRL.appendChild(pre);
}

function onError (event) {
  writeToScreen ('<span style="color: red;">ERROR: ' + event.data + '</span>');
}

function onClose (event) {
  writeToScreen ("DISCONNECTED");
}

function send (message) {
  writeToScreen ("SENT: " + message);
  //websocket.send (message);
}

function writeToScreen (message) {
  var pre = document.createElement ("p");
  pre.style.wordWrap = "break-word";
  pre.innerHTML = message;
  outputInfo.appendChild (pre);
}



function json2table(json, classes) {
  // Everything goes in here

  var cols = Object.keys(json[0]);
  var headerRow = '';
  var bodyRows = '';

  classes = classes || '';

  cols.map(function (col) {
    headerRow += '<th>' + col + '</th>';
  });

  json.map(function (row) {
    bodyRows += '<tr>';
    // To do: Loop over object properties and create cells
    cols.map(function (colName) {
      bodyRows += '<td>' + row[colName] + '</td>';
    });
    bodyRows += '</tr>';
  });



  return '<table class="' +
  classes +
    '"><thead><tr>' +
    headerRow +
    '</tr></thead><tbody>' +
    bodyRows +
    '</tbody></table>';
}


window.addEventListener ("load", init, false);