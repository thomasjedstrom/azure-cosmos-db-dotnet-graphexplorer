import 'isomorphic-fetch';
import { Aurelia } from 'aurelia-framework';
import { HttpClient } from "aurelia-fetch-client";
import { OAuthService, OAuthTokenService } from 'aurelia-oauth';
import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap';
declare const IS_DEV_BUILD: boolean; // The value is supplied by Webpack during the build

export function configure(aurelia: Aurelia) {
    const httpClient = <HttpClient>aurelia.container.get(HttpClient);

    aurelia.use
        .standardConfiguration()
        .plugin('aurelia-oauth', (oauthService, oauthTokenService, configureClient) =>
            configureOauth(oauthService, oauthTokenService, configureClient, httpClient));

    if (IS_DEV_BUILD) {
        aurelia.use.developmentLogging();
    }

    aurelia.start().then(() => aurelia.setRoot('app/components/app/app'));
}

function configureOauth(oauthService: OAuthService, oauthTokenService: OAuthTokenService, configureClient: (client: any) => void, client: any) {
    oauthService.configure(
        {
            loginUrl: 'https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47/oauth2/authorize',
            logoutUrl: 'https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47/oauth2/authorize',
            clientId: '6af40fda-042a-4106-98d3-5483c64b13b6',
            alwaysRequireLogin: false
        });

    configureClient(client);
}