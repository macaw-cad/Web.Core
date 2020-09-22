import React from 'react';
import { render } from '@testing-library/react';
import App from './App';

test('renders Swagger link', () => {
  const { getByText } = render(<App />);
  const linkElement = getByText(/Swagger/i);
  expect(linkElement).toBeInTheDocument();
});
