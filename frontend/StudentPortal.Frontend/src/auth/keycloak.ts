import Keycloak from 'keycloak-js';

const keycloak = new Keycloak({
  url: 'http://localhost:8080',
  realm: 'StudentPortal',
  clientId: 'studentportal-frontend',
});

export default keycloak;