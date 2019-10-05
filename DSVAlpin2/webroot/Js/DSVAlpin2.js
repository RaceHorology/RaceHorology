
Vue.component('dsv-startlist', {
  props: ['datalist'],

  data: function() {
    return {
    };
  },

  created: function()
  {
  },

  methods: 
  {
  },


  template: `
  <div>
    <table class="dsvalpin-lists" v-if="datalist">
      <thead>
        <tr>
          <th class="cell-centered">StNr</th>
          <th>Name</th>
          <th>Vorname</th>
          <th class="cell-centered">Geschlecht</th>
          <th class="cell-centered">Jahrgang</th>
          <th>Verein</th>
          <th>Klasse</th>
          <th>Gruppe</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in datalist" v-bind:class="{ just_modified: item.JustModified }">
          <td class="cell-centered">{{ item.StartNumber == 0? "---" : item.StartNumber }}</td>
          <td>{{ item.Name }}</td>
          <td>{{ item.Firstname }}</td>
          <td class="cell-centered">{{ item.Sex }}</td>
          <td class="cell-centered">{{ item.Year }}</td>
          <td>{{ item.Club }}</td>
          <td>{{ item.Class }}</td>
          <td>{{ item.Group }}</td>
        </tr>
      </tbody>
    </table>
  </div>
`

});


Vue.component('dsv-runresultslist', {
  props: ['datalist'],

  template: `
  <div>
    <table class="dsvalpin-lists" v-if="datalist">
      <thead>
        <tr>
          <th class="cell-centered">Position</th>
          <th class="cell-centered">StNr</th>
          <th>Name</th>
          <th>Vorname</th>
          <th class="cell-centered">Geschlecht</th>
          <th class="cell-centered">Jahrgang</th>
          <th>Verein</th>
          <th>Klasse</th>
          <th>Gruppe</th>
          <th class="cell-centered">Zeit</th>
          <th>&nbsp;</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in datalist" v-bind:key="item.Id" v-bind:class="{ just_modified: item.JustModified }">
          <td class="cell-centered">{{ item.Position ==0 ? "---" : item.Position }}</td>
          <td class="cell-centered">{{ item.StartNumber == 0? "---" : item.StartNumber }}</td>
          <td>{{ item.Name }}</td>
          <td>{{ item.Firstname }}</td>
          <td class="cell-centered">{{ item.Sex }}</td>
          <td class="cell-centered">{{ item.Year }}</td>
          <td>{{ item.Club }}</td>
          <td>{{ item.Class }}</td>
          <td>{{ item.Group }}</td>
          <td class="cell-centered">{{ item.Runtime }}</td>
          <td>{{ item.DisqualText }}</td>
        </tr>
      </tbody>
    </table>
  </div>`

});


Vue.component('dsv-raceresultslist', {
  props: ['datalist'],

  template: `
  <div>
    <table class="dsvalpin-lists" v-if="datalist">
      <thead>
        <tr>
          <th class="cell-centered">Position</th>
          <th class="cell-centered">StNr</th>
          <th>Name</th>
          <th>Vorname</th>
          <th class="cell-centered">Geschlecht</th>
          <th class="cell-centered">Jahrgang</th>
          <th>Verein</th>
          <th>Klasse</th>
          <th>Gruppe</th>
          <th class="cell-centered">Zeit</th>
          <th>&nbsp;</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in datalist" v-bind:key="item.Id" v-bind:class="{ just_modified: item.JustModified }">
          <td class="cell-centered">{{ item.Position ==0 ? "---" : item.Position }}</td>
          <td class="cell-centered">{{ item.StartNumber == 0? "---" : item.StartNumber }}</td>
          <td>{{ item.Name }}</td>
          <td>{{ item.Firstname }}</td>
          <td class="cell-centered">{{ item.Sex }}</td>
          <td class="cell-centered">{{ item.Year }}</td>
          <td>{{ item.Club }}</td>
          <td>{{ item.Class }}</td>
          <td>{{ item.Group }}</td>
          <td class="cell-centered">{{ item.Totaltime }}</td>
          <td>{{ item.DisqualText }}</td>
        </tr>
      </tbody>
    </table>
  </div>`

});

Vue.component('dsv-racedata', {
  props: ['racedata'],

  template: `<div>Rennen: {{ racedata["type"] }} Durchgang: {{ racedata["run"] }}</div>`

});



var app = new Vue({
  el: "#app",
  data: function()
  {
    return {
      startlist: null,
      runlist: null,
      raceresultlist: null,
      currentracerun: {"run": "", "type": ""},
      logs: [],
      status: "disconnected",
      lastUpdate: "",
      message: ""
    };
  },

  created: function()
  {
    this.socket = new WebSocket("ws://" + window.location.hostname + ":" + window.location.port + "/LiveData");
    this.socket.onopen = () => 
    {
      this.status = "connected";
      this.logs.push({ event: "Connected to", data: this.socket.location});

      this.socket.onmessage = ({data}) => {
        
        parsedData = JSON.parse(event.data);

        if (parsedData["type"] == "startlist")
        {
          this.startlist = parsedData["data"];
        } 
        else if (parsedData["type"] == "racerunresult")
        {
          this.runlist = parsedData["data"];
        } 
        else if (parsedData["type"] == "raceresult")
        {
          this.raceresultlist = parsedData["data"];
        } 
        else if (parsedData["type"] == "currentracerun")
        {
          this.currentracerun = parsedData["data"];
        }

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
  },




});



