import React from 'react';
import logo from './logo.svg';
import './App.css';
import { BrowserRouter as Router, Switch, Route, Link } from "react-router-dom";
import SettingsData from './SettingsData';

function App() {
    return (
        <div className="App">
            <Router>
                <Switch>
                    <Route path='/' exact component={HomePage} />
                    <Route path='/settings' component={SettingsData} />
                    <Route component={NotFoundPage} />
                </Switch>
            </Router>
        </div>
    );
}

function HomePage(props) {
    return (
        <>
            <header className="App-header">
                <img src={logo} className="App-logo" alt="logo" />
                <p>
                    Edit <code>src/App.js</code> and save to reload.
                </p>
                <Link className="App-link" to="/settings">Settings</Link>
                <br />
                <a className="App-link" href="/swagger" target="_blank" rel="noopener noreferrer">Swagger</a>
            </header>
        </>
    );
}

function NotFoundPage(props) {
    return (
        <>
            <h2>Page Not Found</h2>
            <p>
                <Link to="/">Back to Home</Link>
            </p>
        </>
    );
}

export default App;
