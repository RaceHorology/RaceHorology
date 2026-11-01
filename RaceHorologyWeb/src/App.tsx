import Container from 'react-bootstrap/Container'
import Navbar from 'react-bootstrap/Navbar'
import Tab from 'react-bootstrap/Tab'
import Tabs from 'react-bootstrap/Tabs'
import { useLiveData } from './api/liveData'
import { useMetaData } from './api/metaData'
import logo from './assets/rh-logo.svg'
import ConnectionStatus from './components/ConnectionStatus'
import LiveTab from './components/LiveTab'
import RaceResultTab from './components/RaceResultTab'
import RunResultTab from './components/RunResultTab'
import StartListTab from './components/StartListTab'

export default function App() {
  const liveData = useLiveData()
  // Reload the metadata (classes, runs, ...) whenever the current race or run changes
  const meta = useMetaData(JSON.stringify(liveData.currentRaceRun))

  return (
    <>
      <Navbar className="rh-navbar mb-3">
        <Container fluid>
          <Navbar.Brand>
            <img src={logo} alt="Race Horology" height={40} />
          </Navbar.Brand>
          <ConnectionStatus liveData={liveData} />
        </Container>
      </Navbar>
      <Container fluid className="rh-content">
        <Tabs defaultActiveKey="live" className="mb-3" mountOnEnter>
          <Tab eventKey="live" title="Live">
            <LiveTab liveData={liveData} meta={meta} />
          </Tab>
          <Tab eventKey="startlist" title="Startliste">
            <StartListTab meta={meta} />
          </Tab>
          <Tab eventKey="runresults" title="Einzelergebnisse">
            <RunResultTab liveData={liveData} meta={meta} />
          </Tab>
          <Tab eventKey="raceresults" title="Rennergebnis">
            <RaceResultTab liveData={liveData} meta={meta} />
          </Tab>
        </Tabs>
      </Container>
    </>
  )
}
