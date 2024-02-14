import { Plugin } from "vue";

export const PermissionPlugin: Plugin<PermissionOptions> = {
    install: (app, options) => {
        if (!options || !options.permissionProvider) {
            console.warn("Permission won't work without permissionProvider")
            return;
        }

        app.config.globalProperties.$pm = {
            some: options.permissionsProvider.some,
            every: options.permissionsProvider.every
        }
    }
}

export interface PermissionOptions {
    permissionsProvider: { some: (...permissionCodes: string[]) => boolean, every: (...permissionCodes: string[]) => boolean };
}