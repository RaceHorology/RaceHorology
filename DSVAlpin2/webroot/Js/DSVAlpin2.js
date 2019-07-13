
Vue.component('dsv-startlist', {
  data: function() {
    return {
      startlist: null,
      logs: [],
      status: "disconnected",
      lastUpdate: ""
    };
  },

  created: function()
  {
    this.socket = new WebSocket("ws://" + window.location.hostname + ":" + window.location.port + "/StartList");
    this.socket.onopen = () => 
    {
      this.status = "connected";
      this.logs.push({ event: "Connected to", data: this.socket.location});

      this.socket.onmessage = ({data}) => {
        this.startlist = JSON.parse(event.data);
        this.lastUpdate = new Date().toLocaleString();
        this.logs.push({ event: "Recieved message", data: new Date().toLocaleString()});
      };

      this.sendMessage("init");
    };
  },

  methods: 
  {
    disconnect() 
    {
      this.socket.close();
      this.status = "disconnected";
      this.logs.push({ event: "Disconnected", data: new Date().toLocaleString()});
    },
    sendMessage(e) 
    {
      this.socket.send(this.message);
      this.logs.push({ event: "Sent message", data: this.message });
      this.message = "";
    }
  }

});


Vue.component('dsv-runresultslist', {
  data: function() {
    return {
      runresultslist: null,
      logs: [],
      status: "disconnected",
      lastUpdate: ""
    };
  },

  created: function()
  {
    this.socket = new WebSocket("ws://" + window.location.hostname + ":" + window.location.port + "/ResultList");
    this.socket.onopen = () => 
    {
      this.status = "connected";
      this.logs.push({ event: "Connected to", data: this.socket.location});

      this.socket.onmessage = ({data}) => {
        this.runresultslist = JSON.parse(event.data);
        this.lastUpdate = new Date().toLocaleString();
        this.logs.push({ event: "Recieved message", data: new Date().toLocaleString()});
      };

      this.sendMessage("init");
    };
  },

  methods: 
  {
    disconnect() 
    {
      this.socket.close();
      this.status = "disconnected";
      this.logs.push({ event: "Disconnected", data: new Date().toLocaleString()});
    },
    sendMessage(e) 
    {
      this.socket.send(this.message);
      this.logs.push({ event: "Sent message", data: this.message });
      this.message = "";
    }
  }

});




var app = new Vue({
  el: "#app",
  data: 
  {
    message: ""
  }

});



