import React from 'react';
import { render } from '@testing-library/react';
import { BrowserRouter as Router } from "react-router-dom";
import SettingsData from './SettingsData';

test('renders Back to Home link', () => {
  const { getByText } = render(<Router><SettingsData /></Router>);
  const linkElement = getByText(/Back to Home/i);
  expect(linkElement).toBeInTheDocument();
});
