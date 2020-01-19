
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

      return retList;
    },

    onTrackListUI(){
      var retList = [];
      if (Array.isArray(this.ontracklist))
      {
        retList = [...this.ontracklist];
        retList.reverse();
      }

      return retList;
    },

    justFinishedListUI(){
      var retList = [];
      if (Array.isArray(this.finishedlist))
      {
        retList = [...this.finishedlist]; //.reverse();
      }

      return retList;
    },

    runtimeFields() {
      var fields = 0;

      if (this.nextstarterslist && Array.isArray(this.nextstarterslist) && this.nextstarterslist.length > 0 && this.nextstarterslist[0]["Runtimes"] && Array.isArray(this.nextstarterslist[0]["Runtimes"]))
        fields = this.nextstarterslist[0]["Runtimes"].length;
      else if (this.ontracklist && Array.isArray(this.ontracklist) && this.ontracklist.length > 0 &&  this.ontracklist[0]["Runtimes"] && Array.isArray(this.ontracklist[0]["Runtimes"]))
        fields = this.ontracklist[0]["Runtimes"].length;
      else if (this.finishedlist && Array.isArray(this.finishedlist) && this.finishedlist.length > 0 &&  this.finishedlist[0]["Runtimes"] && Array.isArray(this.finishedlist[0]["Runtimes"]))
        fields = this.finishedlist[0]["Runtimes"].length;

      var headings = new Array();
      for(i=0; i<fields; i++)
        headings.push("Zeit " + (i+1));
      return headings;
    }
  },


  template: `
  <div class="centered">
    <table class="dsvalpin-lists dsvalpin-livetable">
      <tr class="dsvalpin-livetable-heading">
        <th></th>

        <th class="cell-centered">StNr</th>
        <th>Name</th>
        <th>Vorname</th>
        <th v-if="datafields.includes('Sex')" class="cell-centered">Geschlecht</th>
        <th v-if="datafields.includes('Year')" class="cell-centered">Jahrgang</th>
        <th v-if="datafields.includes('Club')">Verein</th>
        <th v-if="datafields.includes('Class')">Klasse</th>
        <th v-if="datafields.includes('Group')">Gruppe</th>

        <template v-for="rt in runtimeFields" >
          <th colspan="2" class="cell-right">{{ rt }}</th>
        </template>

        <th colspan="2">Zeit</th>

        <th></th>
      </tr>

      <template v-for="(item,key) in nextStartersListUI" >
        <tr>
          <th class="first-col" v-if="key == 0" v-bind:rowspan="nextStartersListUI.length"><em class="vertical">Am Start</em></th>

          <td class="cell-centered">{{ item.StartNumber == 0? "---" : item.StartNumber }}</td>
          <td>{{ item.Name }}</td>
          <td>{{ item.Firstname }}</td>
          <td v-if="datafields.includes('Sex')" class="cell-centered">{{ item.Sex }}</td>
          <td v-if="datafields.includes('Year')" class="cell-centered">{{ item.Year }}</td>
          <td v-if="datafields.includes('Club')">{{ item.Club }}</td>
          <td v-if="datafields.includes('Class')">{{ item.Class }}</td>
          <td v-if="datafields.includes('Group')">{{ item.Group }}</td>

          <template v-if="item.Runtimes" v-for="rt in item.Runtimes" >
            <td class="dsvalpin-cell-timeBeneathPosition">{{ rt.Runtime }}</td>
            <td class="dsvalpin-cell-positionBeneathTime">{{ (rt.Position ? "(" + rt.Position + ")" : "" ) }}</td>
          </template>


          <td colspan="2"></td>

          <th class="first-col" v-if="key == 0" v-bind:rowspan="nextStartersListUI.length"><em class="vertical">Am Start</em></th>
        </tr>
      </template>

      <template v-if="nextStartersListUI.length == 0" >
        <tr>
          <th class="first-col"><em class="vertical">Am Start</em></th>

          <td class="cell-centered" v-bind:colspan="8+runtimeFields.length*2"><em>keine weiteren Starter</em></td>
          
          <th class="first-col"><em class="vertical">Am Start</em></th>
        </tr>
      </template>


      <tr>
        <th class="cell-centered dsvalpin-livetable-divider" v-bind:colspan="10+runtimeFields.length*2"></th>
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

          <template v-if="item.Runtimes" v-for="rt in item.Runtimes" >
            <td class="dsvalpin-cell-timeBeneathPosition">{{ rt.Runtime }}</td>
            <td class="dsvalpin-cell-positionBeneathTime">{{ (rt.Position ? "(" + rt.Position + ")" : "" ) }}</td>
          </template>

          <td class="dsvalpin-cell-timeBeneathPosition">{{ item.Runtime }}</td>
          <td class="dsvalpin-cell-positionBeneathTime"></td>

          <th class="first-col" v-if="key == 0" v-bind:rowspan="onTrackListUI.length"><em class="vertical">Im Lauf</em></th>
        </tr>
      </template>

      <template v-if="onTrackListUI.length == 0" >
        <tr>
          <th class="first-col"><em class="vertical">Im Lauf</em></th>

          <td class="cell-centered" v-bind:colspan="8+runtimeFields.length*2"><em>keine Läufer gestartet</em></td>
          
          <th class="first-col"><em class="vertical">Im Lauf</em></th>
        </tr>
      </template>


      <tr>
      <th class="cell-centered dsvalpin-livetable-divider" v-bind:colspan="10+runtimeFields.length*2"></th>
      </tr>

      <template v-for="(item, key) in justFinishedListUI" >
        <tr>
          <th class="first-col" v-if="key == 0" v-bind:rowspan="justFinishedListUI.length"><em class="vertical">Im Ziel</em></th>

          <td class="cell-centered">{{ item.StartNumber == 0? "---" : item.StartNumber }}</td>
          <td>{{ item.Name }}</td>
          <td>{{ item.Firstname }}</td>
          <td v-if="datafields.includes('Sex')" class="cell-centered">{{ item.Sex }}</td>
          <td v-if="datafields.includes('Year')" class="cell-centered">{{ item.Year }}</td>
          <td v-if="datafields.includes('Club')">{{ item.Club }}</td>
          <td v-if="datafields.includes('Class')">{{ item.Class }}</td>
          <td v-if="datafields.includes('Group')">{{ item.Group }}</td>

          <template v-if="item.Runtimes" v-for="rt in item.Runtimes" >
            <td class="dsvalpin-cell-timeBeneathPosition">{{ rt.Runtime }}</td>
            <td class="dsvalpin-cell-positionBeneathTime">{{ (rt.Position ? "(" + rt.Position + ")" : "" ) }}</td>
          </template>

          <td class="dsvalpin-cell-timeBeneathPosition">{{ item.Runtime }}</td>
          <td class="dsvalpin-cell-positionBeneathTime">{{ (item.Position ? "(" + item.Position + ")" : "" ) }}</td>
          
          <th class="first-col" v-if="key == 0" v-bind:rowspan="justFinishedListUI.length"><em class="vertical">Im Ziel</em></th>
        </tr>
      </template>

      <template v-if="justFinishedListUI.length == 0" >
        <tr>
          <th class="first-col"><em class="vertical">Im Ziel</em></th>
          <td class="cell-centered" v-bind:colspan="8+runtimeFields.length*2"><em>noch kein Läufer im Ziel</em></td>
          <th class="first-col"><em class="vertical">Im Ziel</em></th>
        </tr>
      </template>

      <tr class="dsvalpin-livetable-heading">
        <th></th>

        <th class="cell-centered">StNr</th>
        <th>Name</th>
        <th>Vorname</th>
        <th v-if="datafields.includes('Sex')" class="cell-centered">Geschlecht</th>
        <th v-if="datafields.includes('Year')" class="cell-centered">Jahrgang</th>
        <th v-if="datafields.includes('Club')">Verein</th>
        <th v-if="datafields.includes('Class')">Klasse</th>
        <th v-if="datafields.includes('Group')">Gruppe</th>

        <template v-for="rt in runtimeFields" >
          <th colspan="2" class="cell-right">{{ rt }}</th>
        </template>

        <th colspan="2">Zeit</th>

        <th></th>
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
          <th class="cell-centered">Diff</th>
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
          <td class="cell-right">{{ item.DiffToFirst }}</td>
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
          <th class="cell-centered">Diff</th>
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
              <td class="cell-right">{{ item.DiffToFirst }}</td>
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
            <th colspan="2">{{ rt }}</th>
          </template>
          <th class="cell-centered">Zeit</th>
          <th class="cell-centered">Diff</th>
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
                <td class="dsvalpin-cell-timeBeneathPosition">{{ rt.Runtime }}</td>
                <td class="dsvalpin-cell-positionBeneathTime">{{ (rt.Position ? "(" + rt.Position + ")" : "" ) }}</td>
              </template>
              <td class="cell-right">{{ item.Totaltime }}</td>
              <td class="cell-right">{{ item.DiffToFirst }}</td>
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


    onStartList()
    {
      var retList = [];

      if (!this.onstartlist)
        return retList;

      this.onstartlist.forEach(element => {
        var item = {...element};

        // Copy in the previous results
        var itemsResults = null;
        if (this.raceresults)
        {
          for (let [key, value] of Object.entries(this.raceresults)) 
          {
            itemsResults = value.find( x => x.StartNumber == item.StartNumber);
            if (itemsResults)
              break;
          }
        }
        if (itemsResults)
        {
          //item["Runtimes"] = itemsResults["Runtimes"].slice(0, -1); //[...itemsResults["Runtimes"]];
          item["Runtimes"] = [...itemsResults["Runtimes"]];
        }
        retList.push(item);
      });

      return retList;
    },

    onTrackList()
    {
      var retList = [];

      if (!this.ontracklist)
        return retList;

      this.ontracklist.forEach(element => {
        var item = {...element};

        // Copy in the previous results
        var itemsResults = null;
        if (this.raceresults)
        {
          for (let [key, value] of Object.entries(this.raceresults)) 
          {
            itemsResults = value.find( x => x.StartNumber == item.StartNumber);
            if (itemsResults)
              break;
          }
        }
        if (itemsResults)
        {
          //item["Runtimes"] = itemsResults["Runtimes"].slice(0, -1); //[...itemsResults["Runtimes"]];
          item["Runtimes"] = [...itemsResults["Runtimes"]];
        }
        retList.push(item);
      });

      return retList;
    },

    finishedList()
    {
      var finished = [];

      this.finishedListWOResult.forEach(element => {
        var item = {...element};


        var itemsCurResults = null;
        for (let [key, value] of Object.entries(this.racerunresults)) 
        {
          itemsCurResults = value.find( x => x.StartNumber == item.StartNumber);
          if (itemsCurResults)
            break;
        }
        if (itemsCurResults)
        {
          item["Runtime"] = itemsCurResults["Runtime"];
          item["Position"] = itemsCurResults["Position"];
        }

       
        // Copy in the previous results
        var itemsResults = null;
        if (this.raceresults)
        {
          for (let [key, value] of Object.entries(this.raceresults)) 
          {
            itemsResults = value.find( x => x.StartNumber == item.StartNumber);
            if (itemsResults)
              break;
          }
        }
        if (itemsResults)
        {
          //item["Runtimes"] = itemsResults["Runtimes"].slice(0, -1); //[...itemsResults["Runtimes"]];
          item["Runtimes"] = [...itemsResults["Runtimes"]];

          item["Runtime"] = itemsResults["Totaltime"];
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
          this.raceresults = parsedData["data"];
        } 
        else if (parsedData["type"] == "currentracerun")
        {
          this.currentracerun = parsedData["data"];
          this.finishedListWOResult = [];
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
      filterby: "",
      run: 0
    };
  },

  props: {
    runs: {
      type: Array, 
      required: true
    }
  },

  watch: {
    groupby: function (newGroupBy, oldGroupBy){
      this.fetchStartList();
    },
    run: function (newRun, oldRun){
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
      var url = "http://" + window.location.hostname + ":" + window.location.port + "/api/v0.1" + "/races//runs/"+this.run+"/startlist";

      if (this.groupby)
        url += "?groupby="+this.groupby;

      var that = this; // To preserve the Vue context within the jQuery callback
      $.getJSON(url, function (data) {
        that.startlist = data["data"];
        that.groupby = data["groupby"];
      });
    }
  }
});


Vue.component('dsv-runresultapp', {
  mixins: [dsvFilterAndGroupByDataMixin],

  data: function()
  {
    return {
      resultlist: {},
      datakeys: {},
      filterby: "",
      run: 0
    };
  },

  props: {
    runs: {
      type: Array, 
      required: true
    }
  },

  watch: {
    groupby: function (newGroupBy, oldGroupBy){
      this.fetchResultList();
    },
    run: function (newRun, oldRun){
      this.fetchResultList();
    }
  },


  created: function()
  {
    this.fetchResultList();
  },

  methods: 
  {
    fetchResultList()
    {
      var url = "http://" + window.location.hostname + ":" + window.location.port + "/api/v0.1" + "/races//runs/"+this.run+"/resultlist";

      if (this.groupby)
        url += "?groupby="+this.groupby;

      var that = this; // To preserve the Vue context within the jQuery callback
      $.getJSON(url, function (data) {
        that.resultlist = data["data"];
        that.datakeys = data["fields"];
        that.groupby = data["groupby"];
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
        that.groupby = data["groupby"];
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
      runs: [],
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
          text:"Reset to Default"
        });
        data["data"]["groupings"].forEach(function (a) {
          that.groupings.push({
            value:a, 
            text:a
          });
        });

        that.runs = [];
        var runs = data["data"]["runs"];
        for(var i=0; i<runs; i++)
        {
          that.runs.push({
            value: i,
            text: (i+1) + ". Durchgang"
          });
        }
      });
    },
  },
});




