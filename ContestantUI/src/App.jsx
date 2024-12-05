import { useState } from 'react';
import { Route, BrowserRouter as Router, Routes } from 'react-router-dom';
import './App.css';
import LoginComponent from './components/auth/LoginComponent';
import RegistrationForm from './components/auth/RegisterComponent';
import TeamComponent from './components/auth/TeamConfirm';
import ChallengeDetail from './components/challenges/ChallengeDetail';
import ChallengeList from './components/challenges/ChallengeList';
import ChallengeTopics from './components/challenges/ChallengeTopics';
import HomePage from './components/home/HomePage';
import Scoreboard from './components/scoreboard/Scoreboard';
import CreateTeamComponent from './components/team/CreateNewTeam';
import JoinTeamComponent from './components/team/JoinTeam';
import Template from './components/Template';
import TicketDetailPage from './components/ticket/TicketDetailPage';
import TicketList from './components/ticket/TicketListPage';
import UserProfile from './components/user/UserProfile';
import LockScreen from './template/Forbidden';

function App() {
  const [count, setCount] = useState(0)

  return (
    <Router>
      <Routes>
        <Route path="/" element={<Template><HomePage /></Template>} />
        <Route path="/register" element= {<RegistrationForm></RegistrationForm>} />
        <Route path="/login" element={<LoginComponent />} />
        <Route path='/team-confirm' element={<TeamComponent></TeamComponent>}></Route>
        <Route path= '/team-create' element= {<CreateTeamComponent></CreateTeamComponent>}></Route>
        <Route path='/team-join' element= {<JoinTeamComponent></JoinTeamComponent>}></Route>
        <Route path="/rankings" element={
          <Template>
            <Scoreboard />
          </Template>} />
        <Route path="/topics" element={
          <Template>
            <ChallengeTopics />
          </Template>} />
          <Route path="/tickets" element= {<Template><TicketList></TicketList></Template>} />
        <Route path="/challenge/:id" element={<Template><ChallengeDetail /></Template>} />
        <Route path="/ticket/:id" element= {<Template><TicketDetailPage /></Template>}></Route>
        <Route path="/topic/:categoryName" element={<Template><ChallengeList /></Template>} />
        <Route path="/profile" element={<Template><UserProfile /></Template>} />
        <Route path='/forbidden' element= {<Template><LockScreen></LockScreen></Template>}></Route>
      </Routes>
    </Router>
  )
}

export default App
