import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, css } from '@umbraco-cms/backoffice/external/lit';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

const COGFLARE_API_BASE = '/umbraco/management/api/v1/cogflare';

class CogflareDashboard extends UmbLitElement {
    static properties = {
        _isPurging: { state: true },
        _settings: { state: true },
        _showSettings: { state: true },
    };

    static styles = [
        css`
            :host {
                display: block;
                padding: var(--uui-size-space-5);
            }
            .cogflare-icon {
                height: 150px;
                width: 150px;
                display: block;
                margin-bottom: var(--uui-size-space-4);
            }
            uui-box {
                margin-bottom: var(--uui-size-space-5);
            }
            .settings-grid {
                display: grid;
                gap: var(--uui-size-space-3);
                margin-top: var(--uui-size-space-4);
            }
            .settings-row {
                display: flex;
                gap: var(--uui-size-space-3);
            }
            .settings-label {
                font-weight: bold;
                min-width: 180px;
            }
        `,
    ];

    constructor() {
        super();
        this._isPurging = false;
        this._settings = null;
        this._showSettings = false;
        this._auth = null;
        this._notification = null;
    }

    connectedCallback() {
        super.connectedCallback();
        this.consumeContext(UMB_AUTH_CONTEXT, (auth) => {
            this._auth = auth;
        });
        this.consumeContext(UMB_NOTIFICATION_CONTEXT, (notification) => {
            this._notification = notification;
        });
    }

    async _getToken() {
        return this._auth?.getLatestToken?.() ?? null;
    }

    async _purgeCache() {
        this._isPurging = true;
        try {
            const token = await this._getToken();
            const response = await fetch(`${COGFLARE_API_BASE}/purge-cache`, {
                method: 'GET',
                headers: { Authorization: `Bearer ${token}` },
            });

            if (response.ok) {
                const result = await response.json();
                if (result === true) {
                    this._notification?.peek('positive', {
                        data: { headline: 'CogFlare', message: 'CloudFlare purged successfully! Check logs for more details' },
                    });
                } else {
                    this._notification?.peek('danger', {
                        data: { headline: 'CogFlare', message: 'Something went wrong. Possible that not all cache was purged. Check logs for more details' },
                    });
                }
            } else {
                this._notification?.peek('danger', {
                    data: { headline: 'CogFlare', message: 'Something went wrong. Possible that not all cache was purged. Check logs for more details' },
                });
            }
        } catch {
            this._notification?.peek('danger', {
                data: { headline: 'CogFlare', message: 'Something went wrong. Check logs for more details' },
            });
        } finally {
            this._isPurging = false;
        }
    }

    async _loadSettings() {
        try {
            const token = await this._getToken();
            const response = await fetch(`${COGFLARE_API_BASE}/settings`, {
                method: 'GET',
                headers: { Authorization: `Bearer ${token}` },
            });

            if (response.ok) {
                this._settings = await response.json();
                this._showSettings = true;
            }
        } catch {
            this._notification?.peek('danger', {
                data: { headline: 'CogFlare', message: 'Failed to load settings. Check logs for more details' },
            });
        }
    }

    render() {
        return html`
            <uui-box headline="CogFlare">
                <img
                    src="/App_Plugins/Cogworks.CogFlare/cogflare.png"
                    class="cogflare-icon"
                    alt="CogFlare Logo"
                />
                <h3>CogFlare</h3>
                <p>A package that helps automatically purge CloudFlare, currently handling:</p>
                <ul>
                    <li>Individual purge for <em>content node</em> changes (Published/Deleted/Unpublished) and any nodes referencing it</li>
                    <li>Individual media item changes (Saved)</li>
                    <li>Full cache purge if a changed <em>content node</em> is a <strong>Key Node</strong> or referenced by a <strong>Key Node</strong></li>
                    <li>Ability to trigger a full cache purge manually in the backoffice (below)</li>
                </ul>
            </uui-box>

            <uui-box headline="Purge">
                <p>Click here to trigger a FULL cache purge of the whole site</p>
                ${this._isPurging
                    ? html`<uui-loader></uui-loader>`
                    : html`
                        <uui-button
                            look="secondary"
                            color="warning"
                            label="Purge all cache"
                            @click=${this._purgeCache}>
                            Purge all cache
                        </uui-button>
                    `}
            </uui-box>

            <uui-box headline="Settings">
                <p>
                    Click here to view the current CogFlare Settings
                    (<strong>Warning</strong> — this may reveal sensitive information like API Keys)
                </p>
                <uui-button
                    look="secondary"
                    color="danger"
                    label="Show settings"
                    @click=${this._loadSettings}>
                    Show settings
                </uui-button>

                ${this._showSettings && this._settings
                    ? html`
                        <div class="settings-grid">
                            <div class="settings-row">
                                <span class="settings-label">CogFlare Enabled:</span>
                                <span>${this._settings.isEnabled}</span>
                            </div>
                            <div class="settings-row">
                                <span class="settings-label">Key Nodes:</span>
                                <span>${this._settings.keyNodes}</span>
                            </div>
                            <div class="settings-row">
                                <span class="settings-label">Endpoint:</span>
                                <span>${this._settings.endpoint}</span>
                            </div>
                            <div class="settings-row">
                                <span class="settings-label">Email:</span>
                                <span>${this._settings.email}</span>
                            </div>
                            <div class="settings-row">
                                <span class="settings-label">API Key:</span>
                                <span>${this._settings.apiKey}</span>
                            </div>
                            <div class="settings-row">
                                <span class="settings-label">Authentication Method:</span>
                                <span>${this._settings.authenticationMethod}</span>
                            </div>
                        </div>
                    `
                    : ''}
            </uui-box>
        `;
    }
}

customElements.define('cogworks-cogflare-dashboard', CogflareDashboard);
export default CogflareDashboard;
