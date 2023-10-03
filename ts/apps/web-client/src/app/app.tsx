import { Tabs, Tab } from '@mui/material';
import styles from './app.module.css';

export function App() {
  return (
    <div>
      
      <Tabs
        value={"123"}
        variant="scrollable"
        scrollButtons="auto"
        aria-label="scrollable auto tabs example"
      >
        <Tab label="Item One" />
        <Tab label="Item Two" />
        <Tab label="Item Five" />
        <Tab label="Item Six" />
        <Tab label="Item Seven" />
      </Tabs>
    </div>
  );
}

export default App;
