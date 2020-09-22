import React, { useState, useEffect } from 'react';
import {  Link } from "react-router-dom";

function SettingsData(props) {
    const [settings, setSettings] = useState([]);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        setIsLoading(true);
        getSettings().then(_courses => {
            setSettings(_courses);
            setIsLoading(false);
        });
    }, []);

    const getSettings = () => {
        return fetch('api/settings')
            .then(response => {
                if (response.ok) return response.json();
                if (response.status === 400) {
                    throw new Error(response.text());
                }
                throw new Error("Network response was not ok.");
            })
            .catch((error) => {
                console.error("API call failed. " + error);
                throw error;
            });
    }

    return (
        <>
            <h1>Settings</h1>
            {isLoading ?
                <p><em>Loading...</em></p>
                :
                settings.map(group => <SettingsTable key={group.groupName} name={group.groupName} items={group.items} />)
            }
            <br />
            <Link to="/">Back to Home</Link>
        </>
    );
}

function SettingsTable({ name, items }) {
    return (
        <table style={{ width: 100 + '%', marginBottom: 24 }} aria-labelledby="tabelLabel">
            <thead>
                <tr><th>{name}</th></tr>
            </thead>
            <tbody>            
                { items.map(item => <tr key={item}><td>{item}</td></tr>) }
            </tbody>
        </table>
    );
}

export default SettingsData;