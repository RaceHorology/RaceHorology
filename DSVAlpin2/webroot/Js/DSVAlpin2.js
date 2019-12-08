
var dsvFilterAndGroupByMixin = {

  props: {
    groupby:{
      type: String,
      default: null
    },
    filterby:{
      type: String,
      default: null
    },
    datalist:{
      type: Array, 
      required: true
    },
    datafields:{
      type: Array, 
      default: () => ["Club", "Sex", "Year"] 
    },
    maxitems:{
      type: Number, 
      required: false
    },

  },


  computed: {
    renderDataList(){
      let grouped = {}
      if (Array.isArray(this.datalist))
      {
        var counter = 0;
        for( item of this.datalist)
        {

          if (this.filterby && item[this.groupby] != this.filterby)
            continue;

          counter++;

          if (this.maxitems && counter > this.maxitems)
            break;

          groupName = "";
          if (this.groupby)
            groupName = item[this.groupby];

          grouped[groupName] = grouped[groupName] || [];
          grouped[groupName].push(item);
        }
      }
      return grouped;
    }
  },

  methods: 
  {
  }

}


Vue.component('dsv-startlist', {
  mixins: [dsvFilterAndGroupByMixin],

  data: function() {
    return {
    };
  },

  created: function()
  {
  },


  template: `
  <div>
    <table class="dsvalpin-lists" v-if="renderDataList">
      <thead>
        <tr>
          <th class="cell-centered">StNr</th>
          <th>Name</th>
          <th>Vorname</th>
          <th v-if="datafields.includes('Sex')" class="cell-centered">Geschlecht</th>
          <th v-if="datafields.includes('Year')" class="cell-centered">Jahrgang</th>
          <th v-if="datafields.includes('Club')">Verein</th>
          <th v-if="datafields.includes('Class')">Klasse</th>
          <th v-if="datafields.includes('Group')">Gruppe</th>
        </tr>
      </thead>
      <tbody>
      <template v-for="(items, group) in renderDataList">
        <tr v-if="group">
          <td colspan="8" class="cell-groupheader">{{ group }}</td>
        </tr>
        <template v-for="item in items" >
          <tr v-bind:class="{ just_modified: item.JustModified }">
            <td class="cell-centered">{{ item.StartNumber == 0? "---" : item.StartNumber }}</td>
            <td>{{ item.Name }}</td>
            <td>{{ item.Firstname }}</td>
            <td v-if="datafields.includes('Sex')" class="cell-centered">{{ item.Sex }}</td>
            <td v-if="datafields.includes('Year')" class="cell-centered">{{ item.Year }}</td>
            <td v-if="datafields.includes('Club')">{{ item.Club }}</td>
            <td v-if="datafields.includes('Class')">{{ item.Class }}</td>
            <td v-if="datafields.includes('Group')">{{ item.Group }}</td>
          </tr>
        </template>
      </template>
      </tbody>
    </table>
  </div>
`

});


Vue.component('dsv-ontracklist', {
  props: {
    datalist:{
      type: Array, 
      required: true
    },
    datafields:{
      type: Array, 
      default: () => ["Club", "Sex", "Year"] 
    }

  },


  template: `
  <div>
    <table class="dsvalpin-lists" v-if="datalist">
      <thead>
        <tr>
          <th class="cell-centered">StNr</th>
          <th>Name</th>
          <th>Vorname</th>
          <th v-if="datafields.includes('Sex')" class="cell-centered">Geschlecht</th>
          <th v-if="datafields.includes('Year')" class="cell-centered">Jahrgang</th>
          <th v-if="datafields.includes('Club')">Verein</th>
          <th v-if="datafields.includes('Class')">Klasse</th>
          <th v-if="datafields.includes('Group')">Gruppe</th>
          <th class="cell-centered">Zeit</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in datalist">
          <td class="cell-centered">{{ item.StartNumber == 0? "---" : item.StartNumber }}</td>
          <td>{{ item.Name }}</td>
          <td>{{ item.Firstname }}</td>
          <td v-if="datafields.includes('Sex')" class="cell-centered">{{ item.Sex }}</td>
          <td v-if="datafields.includes('Year')" class="cell-centered">{{ item.Year }}</td>
          <td v-if="datafields.includes('Club')">{{ item.Club }}</td>
          <td v-if="datafields.includes('Class')">{{ item.Class }}</td>
          <td v-if="datafields.includes('Group')">{{ item.Group }}</td>
          <td class="cell-centered">{{ item.Runtime }}</td>
        </tr>
      </tbody>
    </table>
  </div>`

});



Vue.component('dsv-runresultslist', {
  mixins: [dsvFilterAndGroupByMixin],

  template: `
  <div>
    <table class="dsvalpin-lists" v-if="datalist">
      <thead>
        <tr>
          <th class="cell-centered">Position</th>
          <th class="cell-centered">StNr</th>
          <th>Name</th>
          <th>Vorname</th>
          <th v-if="datafields.includes('Sex')" class="cell-centered">Geschlecht</th>
          <th v-if="datafields.includes('Year')" class="cell-centered">Jahrgang</th>
          <th v-if="datafields.includes('Club')">Verein</th>
          <th v-if="datafields.includes('Class')">Klasse</th>
          <th v-if="datafields.includes('Group')">Gruppe</th>
          <th class="cell-centered">Zeit</th>
          <th>&nbsp;</th>
        </tr>
      </thead>
      <tbody>
        <template v-for="(items, group) in renderDataList">
          <tr v-if="group">
            <td colspan="11" class="cell-groupheader">{{ group }}</td>
          </tr>
          <template v-for="item in items" >
            <tr v-bind:class="{ just_modified: item.JustModified }">
              <td class="cell-centered">{{ item.Position ==0 ? "---" : item.Position }}</td>
              <td class="cell-centered">{{ item.StartNumber == 0? "---" : item.StartNumber }}</td>
              <td>{{ item.Name }}</td>
              <td>{{ item.Firstname }}</td>
              <td v-if="datafields.includes('Sex')" class="cell-centered">{{ item.Sex }}</td>
              <td v-if="datafields.includes('Year')" class="cell-centered">{{ item.Year }}</td>
              <td v-if="datafields.includes('Club')">{{ item.Club }}</td>
              <td v-if="datafields.includes('Class')">{{ item.Class }}</td>
              <td v-if="datafields.includes('Group')">{{ item.Group }}</td>
              <td class="cell-centered">{{ item.Runtime }}</td>
              <td>{{ item.DisqualText }}</td>
            </tr>
          </template>
        </template>
      </tbody>
    </table>
  </div>`

});


Vue.component('dsv-raceresultslist', {
  mixins: [dsvFilterAndGroupByMixin],

  template: `
  <div>
    <table class="dsvalpin-lists" v-if="datalist">
      <thead>
        <tr>
          <th class="cell-centered">Position</th>
          <th class="cell-centered">StNr</th>
          <th>Name</th>
          <th>Vorname</th>
          <th v-if="datafields.includes('Sex')" class="cell-centered">Geschlecht</th>
          <th v-if="datafields.includes('Year')" class="cell-centered">Jahrgang</th>
          <th v-if="datafields.includes('Club')">Verein</th>
          <th v-if="datafields.includes('Class')">Klasse</th>
          <th v-if="datafields.includes('Group')">Gruppe</th>
          <th class="cell-centered">Zeit</th>
          <th>&nbsp;</th>
        </tr>
      </thead>
      <tbody>
        <template v-for="(items, group) in renderDataList">
          <tr v-if="group">
            <td colspan="11" class="cell-groupheader">{{ group }}</td>
          </tr>
          <template v-for="item in items" >
            <tr v-bind:class="{ just_modified: item.JustModified }">
              <td class="cell-centered">{{ item.Position ==0 ? "---" : item.Position }}</td>
              <td class="cell-centered">{{ item.StartNumber == 0? "---" : item.StartNumber }}</td>
              <td>{{ item.Name }}</td>
              <td>{{ item.Firstname }}</td>
              <td v-if="datafields.includes('Sex')" class="cell-centered">{{ item.Sex }}</td>
              <td v-if="datafields.includes('Year')" class="cell-centered">{{ item.Year }}</td>
              <td v-if="datafields.includes('Club')">{{ item.Club }}</td>
              <td v-if="datafields.includes('Class')">{{ item.Class }}</td>
              <td v-if="datafields.includes('Group')">{{ item.Group }}</td>
              <td class="cell-centered">{{ item.Totaltime }}</td>
              <td>{{ item.DisqualText }}</td>
            </tr>
          </template>
        </template>
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
      startlist: [],
      runlist: [],
      onstartlist: [],
      ontracklist: [],
      raceresultlist: [],
      categories: [],
      classes: [],
      groups: [],
      sex: [],
      groupings: [],
      currentracerun: {"run": "", "type": ""},
      logs: [],
      status: "disconnected",
      lastUpdate: "",
      message: "",
      groupby: "",
      filterby: ""
    };
  },


  computed: {
    itemsForGrouping(){
      if (this.groupby == "Class")
      {
        return this.classes;
      }
      if (this.groupby == "Group")
      {
        return this.groups;
      }
      if (this.groupby == "Sex")
      {
        return this.sex;
      }
    }
  },  


  watch: {
    groupby: function (newGroupBy, oldGroupBy){
      this.fetchStartList();
      this.fetchRunResultList();      
      this.fetchRaceResultList();
    }
  },

  created: function()
  {
    this.socket = new WebSocket("ws://" + window.location.hostname + ":" + window.location.port + "/api/LiveData");
    this.socket.onopen = () => 
    {
      this.status = "connected";
      this.logs.push({ event: "Connected to", data: this.socket.location});

      this.socket.onmessage = ({data}) => {
        
        parsedData = JSON.parse(event.data);

        /*if (parsedData["type"] == "startlist")
        {
          this.startlist = parsedData["data"];
        } 
        else*/ if (parsedData["type"] == "onstart")
        {
          this.onstartlist = parsedData["data"];
        } 
        else if (parsedData["type"] == "ontrack")
        {
          this.ontracklist = parsedData["data"];
        } 
        else /*if (parsedData["type"] == "racerunresult")
        {
          this.runlist = parsedData["data"];
        } 
        else if (parsedData["type"] == "raceresult")
        {
          this.raceresultlist = parsedData["data"];
        } 
        else */if (parsedData["type"] == "currentracerun")
        {
          this.currentracerun = parsedData["data"];
        }

        this.lastUpdate = new Date().toLocaleString();
        this.logs.push({ event: "Recieved message", data: new Date().toLocaleString()});
      };

      this.sendMessage("init");
    };

    this.fetchMetaData();
    this.fetchStartList();
    this.fetchRunResultList();
    this.fetchRaceResultList();
  },

  methods: 
  {

    fetchMetaData()
    {
      var url = "http://" + window.location.hostname + ":" + window.location.port + "/api/v0.1" + "/races//metadata";

      var that = this; // To preserve the Vue context within the jQuery callback
      $.getJSON(url, function (data) {
        that.classes = [];
        that.classes.push({
          value:"", 
          text:"Alle"
        });
        data["data"]["classes"].forEach(function (a) {
          that.classes.push({
            value:a.Name, 
            text:a.Name
          });
        });        

        that.groups = [];
        that.groups.push({
          value:"", 
          text:"Alle"
        });
        data["data"]["groups"].forEach(function (a) {
          that.groups.push({
            value:a.Name, 
            text:a.Name
          });
        });        

        that.sex = [];
        that.sex.push({
          value:"", 
          text:"Alle"
        });
        data["data"]["sex"].forEach(function (a) {
          that.sex.push({
            value:a, 
            text:a
          });
        });        

        that.groupings = [];
        that.groupings.push({
          value:"", 
          text:"..."
        });
        data["data"]["groupings"].forEach(function (a) {
          that.groupings.push({
            value:a, 
            text:a
          });
        });        

      });
    },

    fetchStartList()
    {
      var url = "http://" + window.location.hostname + ":" + window.location.port + "/api/v0.1" + "/races//runs//startlist";
      if (this.groupby)
        url += "?groupby="+this.groupby;

      var that = this; // To preserve the Vue context within the jQuery callback
      $.getJSON(url, function (data) {
        that.startlist = data["data"];
      });
    },

    fetchRunResultList()
    {
      var url = "http://" + window.location.hostname + ":" + window.location.port + "/api/v0.1" + "/races//runs//resultlist";
      if (this.groupby)
        url += "?groupby="+this.groupby;

      var that = this; // To preserve the Vue context within the jQuery callback
      $.getJSON(url, function (data) {
        that.runlist = data["data"];
      });
    },

    fetchRaceResultList()
    {
      var url = "http://" + window.location.hostname + ":" + window.location.port + "/api/v0.1" + "/races//resultlist";
      if (this.groupby)
        url += "?groupby="+this.groupby;

      var that = this; // To preserve the Vue context within the jQuery callback
      $.getJSON(url, function (data) {
        that.raceresultlist = data["data"];
      });
    },

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
    },
  },




});



