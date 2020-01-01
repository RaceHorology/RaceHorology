
/** Used for grouping and filter the lists
 */
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
      type: Object, 
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

      for (let [key, value] of Object.entries(this.datalist)) {
        if (this.filterby && key != this.filterby)
          continue;

        grouped[key] = value;
      }

      return grouped;
    }
  },

  methods: 
  {
  }

}

/** Used for providing the group and filter comboboxes with data
 */
var dsvFilterAndGroupByDataMixin = {
  data: function()
  {
    return {
      groupby: "",
    };
  },
  
  props: {
    groupings:{
      type: Array, 
      required: true
    },
    categories:{
      type: Array, 
      required: true
    },
    classes:{
      type: Array, 
      required: true
    },
    groups:{
      type: Array, 
      required: true
    },
    sex:{
      type: Array, 
      required: true
    }
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

}


Vue.component('dsv-livedatalists', {

  props: {
    datafields:{
      type: Array, 
      default: () => ["Club", "Sex", "Year"] 
    },
    nextstarterslist:{
      type: Array, 
      required: true
    },
    ontracklist:{
      type: Array, 
      required: true
    },
    finishedlist:{
      type: Array, 
      required: true
    },

  },

  computed: {
    nextStartersListUI(){
      var retList = [];
      if (Array.isArray(this.nextstarterslist))
      {
        retList = [...this.nextstarterslist];
        retList.reverse();
      }

      // Ensure at least one empty item is there
      if (retList.length < 1)
      {
        retList.push({});
      }

      return retList;
    },

    onTrackListUI(){
      var retList = [];
      if (Array.isArray(this.ontracklist))
      {
        retList = [...this.ontracklist];
        retList.reverse();
      }

      // Ensure at least one empty item is there
      if (retList.length < 1)
      {
        retList.push({});
      }

      return retList;
    },

    justFinishedListUI(){
      var retList = [];
      if (Array.isArray(this.finishedlist))
      {
        retList = [...this.finishedlist]; //.reverse();
      }

      // Ensure at least one empty item is there
      if (retList.length < 1)
      {
        retList.push({});
      }

      return retList;
    }
  },


  template: `
  <div class="centered">
    <table class="dsvalpin-lists dsvalpin-livetable">
      <tr class="dsvalpin-livetable-heading">
        <th class="first-col" v-bind:rowspan="nextStartersListUI.length + 1"><em class="vertical">Am Start</em></th>

        <th class="cell-centered">StNr</th>
        <th>Name</th>
        <th>Vorname</th>
        <th v-if="datafields.includes('Sex')" class="cell-centered">Geschlecht</th>
        <th v-if="datafields.includes('Year')" class="cell-centered">Jahrgang</th>
        <th v-if="datafields.includes('Club')">Verein</th>
        <th v-if="datafields.includes('Class')">Klasse</th>
        <th v-if="datafields.includes('Group')">Gruppe</th>
        <th >Zeit</th>
        <th class="first-col" v-bind:rowspan="nextStartersListUI.length + 1"><em class="vertical">Am Start</em></th>
      </tr>

      <template v-for="item in nextStartersListUI" >
        <tr>
          <td class="cell-centered">{{ item.StartNumber == 0? "---" : item.StartNumber }}</td>
          <td>{{ item.Name }}</td>
          <td>{{ item.Firstname }}</td>
          <td v-if="datafields.includes('Sex')" class="cell-centered">{{ item.Sex }}</td>
          <td v-if="datafields.includes('Year')" class="cell-centered">{{ item.Year }}</td>
          <td v-if="datafields.includes('Club')">{{ item.Club }}</td>
          <td v-if="datafields.includes('Class')">{{ item.Class }}</td>
          <td v-if="datafields.includes('Group')">{{ item.Group }}</td>
          <td >&nbsp;</td>
        </tr>
      </template>

      <tr>
        <th class="cell-centered dsvalpin-livetable-divider" colspan="11"></th>
      </tr>

      <template v-for="(item, key) in onTrackListUI" >
        <tr>
          <th class="first-col" v-if="key == 0" v-bind:rowspan="onTrackListUI.length"><em class="vertical">Im Lauf</em></th>
          <td class="cell-centered">{{ item.StartNumber == 0? "---" : item.StartNumber }}</td>
          <td>{{ item.Name }}</td>
          <td>{{ item.Firstname }}</td>
          <td v-if="datafields.includes('Sex')" class="cell-centered">{{ item.Sex }}</td>
          <td v-if="datafields.includes('Year')" class="cell-centered">{{ item.Year }}</td>
          <td v-if="datafields.includes('Club')">{{ item.Club }}</td>
          <td v-if="datafields.includes('Class')">{{ item.Class }}</td>
          <td v-if="datafields.includes('Group')">{{ item.Group }}</td>
          <td class="cell-right">{{ item.Runtime }}</td>
          <th class="first-col" v-if="key == 0" v-bind:rowspan="onTrackListUI.length"><em class="vertical">Im Lauf</em></th>
        </tr>
      </template>

      <tr>
        <th class="cell-centered dsvalpin-livetable-divider" colspan="11"></th>
      </tr>

      <template v-for="(item, key) in justFinishedListUI" >
        <tr>
          <th class="first-col" v-if="key == 0" v-bind:rowspan="justFinishedListUI.length + 1"><em class="vertical">Im Ziel</em></th>
          <td class="cell-centered">{{ item.StartNumber == 0? "---" : item.StartNumber }}</td>
          <td>{{ item.Name }}</td>
          <td>{{ item.Firstname }}</td>
          <td v-if="datafields.includes('Sex')" class="cell-centered">{{ item.Sex }}</td>
          <td v-if="datafields.includes('Year')" class="cell-centered">{{ item.Year }}</td>
          <td v-if="datafields.includes('Club')">{{ item.Club }}</td>
          <td v-if="datafields.includes('Class')">{{ item.Class }}</td>
          <td v-if="datafields.includes('Group')">{{ item.Group }}</td>
          <td class="cell-right">{{ item.Runtime }} {{ (item.Position ? "(" + item.Position + ")" : "" ) }}</td>
          <th class="first-col" v-if="key == 0" v-bind:rowspan="justFinishedListUI.length + 1"><em class="vertical">Im Ziel</em></th>
        </tr>
      </template>

      <tr class="dsvalpin-livetable-heading">
        <th class="cell-centered">StNr</th>
        <th>Name</th>
        <th>Vorname</th>
        <th v-if="datafields.includes('Sex')" class="cell-centered">Geschlecht</th>
        <th v-if="datafields.includes('Year')" class="cell-centered">Jahrgang</th>
        <th v-if="datafields.includes('Club')">Verein</th>
        <th v-if="datafields.includes('Class')">Klasse</th>
        <th v-if="datafields.includes('Group')">Gruppe</th>
        <th >Zeit</th>
      </tr>


    </table>
  </div>
  `
});


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
    <table class="dsvalpin-lists dsvalpin-livetable" v-if="datalist">
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
          <td class="cell-right">{{ item.Runtime }}</td>
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
              <td class="cell-right">{{ item.Runtime }}</td>
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

  props: {
    datakeys:{
      type: Object, 
      default: () => {} 
    }
  },

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
          <template v-for="rt in datakeys.Runtimes" >
            <th>{{ rt }}</th>
          </template>
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
              <template v-for="rt in item.Runtimes" >
                <td class="cell-right">{{ rt }}</td>
              </template>
              <td class="cell-right">{{ item.Totaltime }}</td>
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



Vue.component('dsv-liveapp', {
  data: function()
  {
    return {
      onstartlist: [],
      ontracklist: [],
      finishedListWOResult: [],
      
      racerunresults: {},
      raceresults: {},
      currentracerun: {"run": "", "type": ""},

      categories: [],
      classes: [],
      groups: [],
      sex: [],
      groupings: [],

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
    },

    finishedlist()
    {
      var finished = [];

      this.finishedListWOResult.forEach(element => {
        var item = {...element};


        var itemsResults = null;
        for (let [key, value] of Object.entries(this.racerunresults)) 
        {
          itemsResults = value.find( x => x.StartNumber == item.StartNumber);
          if (itemsResults)
            break;
        }

        if (itemsResults)
        {
          item["Runtime"] = itemsResults["Runtime"];
          item["Position"] = itemsResults["Position"];
        }
        finished.push(item);
      });

      return finished;
    }
  },  


  watch: {
  },

  created: function()
  {
    this.socket = new WebSocket("ws://" + window.location.hostname + ":" + window.location.port + "/api/LiveData");
    this.socket.onopen = () => 
    {
      this.status = "connected";

      this.socket.onmessage = ({data}) => {
        
        parsedData = JSON.parse(event.data);

        if (parsedData["type"] == "onstart")
        {
          this.onstartlist = parsedData["data"];
        } 
        else if (parsedData["type"] == "ontrack")
        {
          this.ontracklist = parsedData["data"];
        } 
        else if (parsedData["type"] == "racerunresult")
        {
          this.racerunresults = parsedData["data"];
        } 
        else if (parsedData["type"] == "raceresult")
        {
          this.raceresultlist = parsedData["data"];
        } 
        else if (parsedData["type"] == "currentracerun")
        {
          this.currentracerun = parsedData["data"];
        }
        else if (parsedData["type"] == "event_participant")
        {
          this.updateFinishedList(parsedData["data"]);
        }

        

        this.lastUpdate = new Date().toLocaleString();
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
    },

    sendMessage(e) 
    {
      this.socket.send(this.message);
      this.message = "";
    },

    updateFinishedList(eventData)
    {
      var eventType = eventData["EventType"];
      var particpant = eventData["Participant"];

      if (eventType == "Finished")
      {
        if (!this.finishedListWOResult.find( x => x.StartNumber == particpant.StartNumber))
        {
          this.finishedListWOResult.unshift(particpant);
          this.finishedListWOResult.splice(3);
        }
      }
    }
  },




});


Vue.component('dsv-startapp', {
  mixins: [dsvFilterAndGroupByDataMixin],

  data: function()
  {
    return {
      startlist: {},
      filterby: ""
    };
  },

  watch: {
    groupby: function (newGroupBy, oldGroupBy){
      this.fetchStartList();
    }
  },


  created: function()
  {
    this.fetchStartList();
  },

  methods: 
  {
    fetchStartList()
    {
      var url = "http://" + window.location.hostname + ":" + window.location.port + "/api/v0.1" + "/races//runs//startlist";
      if (this.groupby)
        url += "?groupby="+this.groupby;

      var that = this; // To preserve the Vue context within the jQuery callback
      $.getJSON(url, function (data) {
        that.startlist = data["data"];
      });
    }
  }
});


Vue.component('dsv-raceresultapp', {
  mixins: [dsvFilterAndGroupByDataMixin],

  data: function()
  {
    return {
      raceresultlist: {},
      datakeys: {},
      filterby: ""
    };
  },

  watch: {
    groupby: function (newGroupBy, oldGroupBy){
      this.fetchRaceResultList();
    }
  },


  created: function()
  {
    this.fetchRaceResultList();
  },

  methods: 
  {
    fetchRaceResultList()
    {
      var url = "http://" + window.location.hostname + ":" + window.location.port + "/api/v0.1" + "/races//resultlist";
      if (this.groupby)
        url += "?groupby="+this.groupby;

      var that = this; // To preserve the Vue context within the jQuery callback
      $.getJSON(url, function (data) {
        let realData = data["data"]
        that.raceresultlist = realData;
        that.datakeys = data["fields"];
      });
    }
  }
});



var app = new Vue({
  el: "#app",
  data: function()
  {
    return {
      categories: [],
      classes: [],
      groups: [],
      sex: [],
      groupings: [],
    };
  },


  computed: {
  }, 
  

  watch: {
  },

  created: function()
  {
    this.fetchMetaData();
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
  },
});




