/*
 * echotest.js
 *
 * Derived from Echo Test of WebSocket.org (http://www.websocket.org/echo.html).
 *
 * Copyright (c) 2012 Kaazing Corporation.
 */

var url = "ws://" + window.location.hostname + ":" + window.location.port + "/StartList";

var output;

function init () {
  output = document.getElementById ("output");
  doWebSocket ();
}

function doWebSocket () {
  websocket = new WebSocket (url);

  websocket.onopen = function (e) {
    onOpen (e);
  };

  websocket.onmessage = function (e) {
    onMessage (e);
  };

  websocket.onerror = function (e) {
    onError (e);
  };

  websocket.onclose = function (e) {
    onClose (e);
  };
}

function onOpen (event) {
  writeToScreen ("CONNECTED");
  send ("WebSocket rocks");
}

function onMessage (event) {
  //writeToScreen ('<span style="color: blue;">RESPONSE: ' + event.data + '</span>');
  //websocket.close ();

  var json = JSON.parse(event.data);
  var table = json2table(json);

  var pre = document.createElement("p");
  pre.innerHTML = table;

  while (output.firstChild) { output.removeChild(output.firstChild); }

  output.appendChild(pre);
}

function onError (event) {
  writeToScreen ('<span style="color: red;">ERROR: ' + event.data + '</span>');
}

function onClose (event) {
  writeToScreen ("DISCONNECTED");
}

function send (message) {
  writeToScreen ("SENT: " + message);
  websocket.send (message);
}

function writeToScreen (message) {
  var pre = document.createElement ("p");
  pre.style.wordWrap = "break-word";
  pre.innerHTML = message;
  output.appendChild (pre);
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